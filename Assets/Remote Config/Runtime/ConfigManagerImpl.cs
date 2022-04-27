using System;
using System.Text;
using UnityEngine;
#if !UNITY_SWITCH && !UNITY_PS4
using UnityEngine.Analytics;
#endif
using UnityEngine.Networking;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.IO;
using System.Collections.Generic;
using Unity.RemoteConfig.Core;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("Unity.RemoteConfig.Tests")]

namespace Unity.RemoteConfig
{
    public class ConfigManagerImpl
    {
        /// <summary>
        /// Returns the status of the current configuration request from the service.
        /// </summary>
        /// <returns>
        /// An enum representing the status of the current Remote Config request.
        /// </returns>
        public ConfigRequestStatus requestStatus { get;  internal set; }
        /// <summary>
        /// This event fires when the configuration manager successfully fetches settings from the service.
        /// </summary>
        /// <returns>
        /// A struct representing the response of a Remote Config fetch.
        /// </returns>
        public event Action<ConfigResponse> FetchCompleted;
        /// <summary>
        /// Retrieves the <c>RuntimeConfig</c> object for handling Remote Config settings.
        /// </summary>
        /// <remarks>
        /// <para> Use this property to access the following <c>RuntimeConfig</c> methods and classes:</para>
        /// <para><c>public string assignmentID</c> is a unique string identifier used for reporting and analytic purposes. The Remote Config service generate this ID upon configuration requests.</para>
        /// <para><c>public bool GetBool (string key, bool defaultValue)</c> retrieves the boolean value of a corresponding key from the remote service, if one exists.</para>
        /// <para><c>public float GetFloat (string key, float defaultValue)</c> retrieves the float value of a corresponding key from the remote service, if one exists.</para>
        /// <para><c>public long GetLong (string key, long defaultValue)</c> retrieves the long value of a corresponding key from the remote service, if one exists.</para>
        /// <para><c>public int GetInt (string key, int defaultValue)</c> retrieves the integer value of a corresponding key from the remote service, if one exists.</para>
        /// <para><c>public string GetString (string key, string defaultValue)</c> retrieves the string value of a corresponding key from the remote service, if one exists.</para>
        /// <para><c>public bool HasKey (string key)</c> checks if a corresponding key exists in your remote settings.</para>
        /// <para><c>public string[] GetKeys ()</c> returns all keys in your remote settings, as an array.</para>
        /// <para><c>public string[] GetJson ()</c> returns string representation of the JSON value of a corresponding key from the remote service, if one exists.</para>
        /// </remarks>
        /// <returns>
        /// A class representing a single runtime settings configuration.
        /// </returns>
        public RuntimeConfig appConfig { get; internal set; }

        internal Delivery deliveryPayload;
        internal Common commonPayload;
        internal DeviceInfo deviceInfoPayload;

        internal event Action<ConfigResponse, JObject> ResponseParsed;
        internal event Action<ConfigOrigin, Dictionary<string, string>, string> RawResponseReturned;
        internal event Action<ConfigOrigin, Dictionary<string, string>, string> RawResponseValidated;

        internal List<Func<JObject>> requestPayloadProviders = new List<Func<JObject>>();
        internal List<Func<RequestHeaderTuple>> requestHeaderProviders = new List<Func<RequestHeaderTuple>>();
        internal List<Func<Dictionary<string, string>, string, bool>> rawResponseValidators = new List<Func<Dictionary<string, string>, string, bool>>();

        internal string cacheFile;
        internal string cacheHeadersFile;
        internal string originService;
        internal string attributionMetadataStr;

        public ConfigManagerImpl(string originService, string attributionMetadataStr = "", string cacheFileRC = "RemoteConfig.json", string cacheHeadersFileRC = "RemoteConfigHeaders.json")
        {
            cacheFile = cacheFileRC;
            cacheHeadersFile = cacheHeadersFileRC;

            appConfig = new RuntimeConfig(this, "settings");
            deliveryPayload.packageVersion = RemoteConfigEnvConf.pluginVersion;
            deliveryPayload.originService = originService;
            if (!string.IsNullOrEmpty(attributionMetadataStr))
            {
                try
                {
                    deliveryPayload.attributionMetadata = JObject.Parse(attributionMetadataStr);
                }
                catch (Exception e)
                {
                    Debug.LogWarning("attributionMetadata is not valid JSON:\n" + attributionMetadataStr + "\n" + e);
                }
            }

            commonPayload = new Common()
            {
#if !UNITY_SWITCH && !UNITY_PS4
                appid = Application.cloudProjectId,
                userid = AnalyticsSessionInfo.userId,
                sessionid = AnalyticsSessionInfo.sessionId,
                session_count = AnalyticsSessionInfo.sessionCount,
#endif
                platform = Application.platform.ToString(),
                platform_id = (int)Application.platform,
                sdk_ver = Application.unityVersion,
                debug_device = Debug.isDebugBuild,
                device_id = SystemInfo.deviceUniqueIdentifier
            };

            deviceInfoPayload = new DeviceInfo();

            requestStatus = ConfigRequestStatus.None;
            RawResponseReturned += OnRawResponseReturned;
            RawResponseValidated += SaveCache;
            LoadFromCache();
        }

        internal void OnRawResponseReturned(ConfigOrigin origin, Dictionary<string, string> headers, string body)
        {
            if(body == null || headers == null)
            {
                return;
            }
            var configResponse = new ConfigResponse() {
                requestOrigin = origin,
                status = ConfigRequestStatus.Pending
            };
            foreach (var validationFunc in rawResponseValidators)
            {
                if(validationFunc(headers, body) == false)
                {
                    configResponse.status = ConfigRequestStatus.Failed;
                    requestStatus = configResponse.status;
                    FetchCompleted?.Invoke(configResponse);
                    return;
                }
            }

            RawResponseValidated?.Invoke(origin, headers, body);

            JObject responseJObj = null;
            try
            {
                responseJObj = JObject.Parse(body);
                configResponse.status = ConfigRequestStatus.Success;
            }
            catch
            {
                configResponse.status = ConfigRequestStatus.Failed;
            }
            ResponseParsed?.Invoke(configResponse, responseJObj);

            requestStatus = configResponse.status;
            FetchCompleted?.Invoke(configResponse);
        }

        /// <summary>
        /// Sets a custom user identifier for the Remote Config delivery request payload.
        /// </summary>
        /// <param name="customUserID">Custom user identifier.</param>
        public void SetCustomUserID(string customUserID)
        {
            deliveryPayload.customUserId = customUserID;
        }

        /// <summary>
        /// Sets an environment identifier in the Remote Config delivery request payload.
        /// </summary>
        /// <param name="environmentID">Environment unique identifier.</param>
        public void SetEnvironmentID(string environmentID)
        {
            deliveryPayload.environmentId = environmentID;
        }

        /// <summary>
        /// Fetches an app configuration settings from the remote server.
        /// </summary>
        /// <param name="userAttributes">A struct containing custom user attributes. If none apply, use an empty struct.</param>
        /// <param name="appAttributes">A struct containing custom app attributes. If none apply, use an empty struct.</param>
        /// <typeparam name="T">The type of the <c>userAttributes</c> struct.</typeparam>
        /// <typeparam name="T2">The type of the <c>appAttributes</c> struct.</typeparam>
        public void FetchConfigs<T, T2>(T userAttributes, T2 appAttributes) where T : struct where T2 : struct
        {
            PostConfig(userAttributes, appAttributes);
        }

        /// <summary>
        /// Fetches an app configuration settings from the remote server.
        /// </summary>
        /// <param name="userAttributes">A struct containing custom user attributes. If none apply, use null.</param>
        /// <param name="appAttributes">A struct containing custom app attributes. If none apply, use null.</param>
        public void FetchConfigs(object userAttributes, object appAttributes)
        {
            PostConfig(userAttributes, appAttributes);
        }

        internal void PostConfig(object userAttributes, object appAttributes)
        {
            var jsonText = PreparePayload(userAttributes, appAttributes);
            DoRequest(jsonText);
        }

        internal string PreparePayload(object userAttributes, object appAttributes)
        {
            requestStatus = ConfigRequestStatus.Pending;
            long timestamp = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeMilliseconds();
            long rtSinceStart = (long)(Time.realtimeSinceStartup * 1000000);
            var commonJobj = JObject.FromObject(commonPayload);
            commonJobj.Add("t_since_start", rtSinceStart);
            JObject json = new JObject();
            json.Add("common", commonJobj);
            List<JObject> items = new List<JObject>();
            items.Add(json);
            items.Add(CreatePayloadJObjectFromValuesJObject(JObject.FromObject(deliveryPayload), "analytics.delivery.v1", timestamp));
            var deviceInfoJObj = JObject.FromObject(deviceInfoPayload);
            deviceInfoJObj.Add("t_since_start", rtSinceStart);
            items.Add(CreatePayloadJObjectFromValuesJObject(deviceInfoJObj, "analytics.deviceInfo.v1", timestamp));
            if(userAttributes == null)
            {
                items.Add(CreatePayloadJObjectFromValuesJObject(new JObject(), "analytics.deliveryUserAttributes.v1", timestamp));
            }
            else
            {
                items.Add(CreatePayloadJObjectFromValuesJObject(JObject.FromObject(userAttributes), "analytics.deliveryUserAttributes.v1", timestamp));
            }
            if(appAttributes == null)
            {
                items.Add(CreatePayloadJObjectFromValuesJObject(new JObject(), "analytics.deliveryAppAttributes.v1", timestamp));
            }
            else
            {
                items.Add(CreatePayloadJObjectFromValuesJObject(JObject.FromObject(appAttributes), "analytics.deliveryAppAttributes.v1", timestamp));
            }

            foreach(var func in requestPayloadProviders)
            {
                items.Add(func.Invoke());
            }

            var sb = new StringBuilder();

            using (var textWriter = new StringWriter(sb))
            {
                ToNewlineDelimitedJson(textWriter, items);
            }

            return sb.ToString();
        }

        internal void DoRequest(string jsonText)
        {
            var request = new RCUnityWebRequest();
            request.unityWebRequest = new UnityWebRequest();
            request.url = "https://config.uca.cloud.unity3d.com";
            request.method = UnityWebRequest.kHttpVerbPOST;
            request.SetRequestHeader("Content-Type", "application/json");
            request.timeout = 10;
            foreach(var headerProvider in requestHeaderProviders)
            {
                var header = headerProvider.Invoke();
                request.SetRequestHeader(header.key, header.value);
            }
            request.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(jsonText));
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SendWebRequest().completed += (AsyncOperation op) => {
                var origin = ConfigOrigin.Remote;
                var response = ((UnityWebRequestAsyncOperation)op).webRequest;
                var configResponse = new ConfigResponse() { requestOrigin = origin, status = requestStatus };
                if (response.isHttpError || response.isNetworkError)
                {
                    configResponse.status = ConfigRequestStatus.Failed;
                    FetchCompleted?.Invoke(configResponse);
                }
                else
                {
                    RawResponseReturned?.Invoke(origin, request.GetResponseHeaders(), request.downloadHandler.text);
                }
            };
        }

        public void SaveCache(ConfigOrigin origin, Dictionary<string, string> headers, string result)
        {
            if(origin == ConfigOrigin.Remote)
            {
                try
                {
                    using (StreamWriter writer = File.CreateText(Path.Combine(Application.persistentDataPath, cacheFile)))
                    {
                        writer.Write(result);
                    }
                    using (StreamWriter writer = File.CreateText(Path.Combine(Application.persistentDataPath, cacheHeadersFile)))
                    {
                        writer.Write(JsonConvert.SerializeObject(headers));
                    }
                }
                catch(Exception e)
                {
                    Debug.LogError(e);
                }
            }
        }

        public void LoadFromCache()
        {
            try
            {
                byte[] bodyResult;
                using (FileStream reader = File.Open(Path.Combine(Application.persistentDataPath, cacheFile), FileMode.Open))
                {
                    bodyResult = new byte[reader.Length];
                    reader.Read(bodyResult, 0, (int)reader.Length);
                }

                byte[] headerResult;
                using (FileStream reader = File.Open(Path.Combine(Application.persistentDataPath, cacheHeadersFile), FileMode.Open))
                {
                    headerResult = new byte[reader.Length];
                    reader.Read(headerResult, 0, (int)reader.Length);
                }
                RawResponseReturned?.Invoke(ConfigOrigin.Cached, JsonConvert.DeserializeObject<Dictionary<string, string>>(System.Text.Encoding.ASCII.GetString(headerResult)), System.Text.Encoding.ASCII.GetString(bodyResult));
            }
            catch
            {
                RawResponseReturned?.Invoke(ConfigOrigin.Cached, null, null);
            }
        }

        internal JObject CreatePayloadJObjectFromValuesJObject(JObject jObject, string type, long ts)
        {
            jObject.Add("ts", ts);
            JObject returnObj = new JObject();
            returnObj.Add("type", type);
            returnObj.Add("msg", jObject);
            return returnObj;
        }

        internal void ToNewlineDelimitedJson<T>(Stream stream, IEnumerable<T> items)
        {
            // Let caller dispose the underlying stream	
            using (var textWriter = new StreamWriter(stream, new UTF8Encoding(false, true), 1024, true))
            {
                ToNewlineDelimitedJson(textWriter, items);
            }
        }

        internal void ToNewlineDelimitedJson<T>(TextWriter textWriter, IEnumerable<T> items)
        {
            var serializer = JsonSerializer.CreateDefault();

            foreach (var item in items)
            {
                // Formatting.None is the default; I set it here for clarity.
                using (var writer = new JsonTextWriter(textWriter) { Formatting = Formatting.None, CloseOutput = false })
                {
                    serializer.Serialize(writer, item);
                }
                // http://specs.okfnlabs.org/ndjson/
                // Each JSON text MUST conform to the [RFC7159] standard and MUST be written to the stream followed by the newline character \n (0x0A). 
                // The newline charater MAY be preceeded by a carriage return \r (0x0D). The JSON texts MUST NOT contain newlines or carriage returns.
                textWriter.Write("\n");
            }
        }

        internal void AddRequestPayloadProvider(Func<JObject> provider)
        {
            requestPayloadProviders.Add(provider);
        }

        internal void RemoveRequestPayloadProvider(Func<JObject> provider)
        {
            requestPayloadProviders.Remove(provider);
        }

        internal void AddRequestHeaderProvider(Func<RequestHeaderTuple> provider)
        {
            requestHeaderProviders.Add(provider);
        }

        internal void RemoveRequestHeaderProvider(Func<RequestHeaderTuple> provider)
        {
            requestHeaderProviders.Remove(provider);
        }

        internal void AddRawPayloadValidatorFunc(Func<Dictionary<string, string>, string, bool> validatorFunc)
        {
            rawResponseValidators.Add(validatorFunc);
        }

        internal void RemoveRawPayloadValidatorFunc(Func<Dictionary<string, string>, string, bool> validatorFunc)
        {
            rawResponseValidators.Remove(validatorFunc);
        }
    }

    /// <summary>
    /// An enum describing the origin point of your most recently loaded configuration settings.
    /// </summary>
    public enum ConfigOrigin
    {
        /// <summary>
        /// Indicates that no configuration settings loaded in the current session.
        /// </summary>
        Default,
        /// <summary>
        /// Indicates that the configuration settings loaded in the current session are cached from a previous session (in other words, no new configuration settings loaded).
        /// </summary>
        Cached,
        /// <summary>
        /// Indicates that new configuration settings loaded from the remote server in the current session.
        /// </summary>
        Remote
    }

    /// <summary>
    /// An enum representing the status of the current Remote Config request.
    /// </summary>
    public enum ConfigRequestStatus
    {
        /// <summary>
        /// Indicates that no Remote Config request has been made.
        /// </summary>
        None,
        /// <summary>
        /// Indicates that the Remote Config request failed.
        /// </summary>
        Failed,
        /// <summary>
        /// Indicates that the Remote Config request succeeded.
        /// </summary>
        Success,
        /// <summary>
        /// Indicates that the Remote Config request is still processing.
        /// </summary>
        Pending
    }

    /// <summary>
    /// A struct representing the response of a Remote Config fetch.
    /// </summary>
    public struct ConfigResponse
    {
        /// <summary>
        /// The origin point of the last retrieved configuration settings.
        /// </summary>
        /// <returns>
        /// An enum describing the origin point of your most recently loaded configuration settings.
        /// </returns>
        public ConfigOrigin requestOrigin;
        /// <summary>
        /// The status of the current Remote Config request.
        /// </summary>
        /// <returns>
        /// An enum representing the status of the current Remote Config request.
        /// </returns>
        public ConfigRequestStatus status;
    }

    [Serializable]
    internal struct Delivery
    {
        public string customUserId;
        public string environmentId;
        public string packageVersion;
        public string originService;
        public JObject attributionMetadata;
    }

    [Serializable]
    internal struct Common
    {
#if !UNITY_SWITCH && !UNITY_PS4
        public string appid;
        public string userid;
        public long sessionid;
        public long session_count;
#endif
        public string platform;
        public int platform_id;
        public string sdk_ver;
        public bool debug_device;
        public string device_id;
    }
 #pragma warning disable CS0649
    [Serializable]
    internal struct RequestHeaderTuple
    {
        public string key;
        public string value;
    }

    internal class DeviceInfo
    {
        public string os_ver;
        public string app_ver;
        public bool rooted_jailbroken;
        public bool debug_build;
        public string model;
        public string cpu;
        public int cpu_count;
        public int cpu_freq;
        public int ram;
        public int vram;
        public string screen;
        public int dpi;
        public string lang;
        public string app_name;
        public string app_install_mode;
        public string app_install_store;
        public int gfx_device_id;
        public int gfx_device_vendor_id;
        public string gfx_name;
        public string gfx_vendor;
        public string gfx_ver;
        public int gfx_shader;
        public int max_texture_size;

        public DeviceInfo()
        {
            os_ver = SystemInfo.operatingSystem;
            app_ver = Application.version;
            rooted_jailbroken = Application.sandboxType == ApplicationSandboxType.SandboxBroken;
            debug_build = Debug.isDebugBuild;
            model = GetDeviceModel();
            cpu = SystemInfo.processorType;
            cpu_count = SystemInfo.processorCount;
            cpu_freq = SystemInfo.processorFrequency;
            ram = SystemInfo.systemMemorySize;
            vram = SystemInfo.graphicsMemorySize;
            screen = Screen.currentResolution.ToString();
            dpi = (int)Screen.dpi;
            lang = GetISOCodeFromLangStruct(Application.systemLanguage);
            app_name = Application.identifier;
            app_install_mode = Application.installMode.ToString();
            app_install_store = Application.installerName;
            gfx_device_id = SystemInfo.graphicsDeviceID;
            gfx_device_vendor_id = SystemInfo.graphicsDeviceVendorID;
            gfx_name = SystemInfo.graphicsDeviceName;
            gfx_vendor = SystemInfo.graphicsDeviceVendor;
            gfx_ver = SystemInfo.graphicsDeviceVersion;
            gfx_shader = SystemInfo.graphicsShaderLevel;
            max_texture_size = SystemInfo.maxTextureSize;
        }

        private string GetDeviceModel()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
        // Get manufacturer, model, and device
        AndroidJavaClass jc = new AndroidJavaClass("android.os.Build");
        string manufacturer = jc.GetStatic<string>("MANUFACTURER");
        string model = jc.GetStatic<string>("MODEL");
        string device = jc.GetStatic<string>("DEVICE");
        return string.Format("{0}/{1}/{2}", manufacturer, model, device);
#else
            return SystemInfo.deviceModel;
#endif
        }

        private string GetISOCodeFromLangStruct(SystemLanguage systemLanguage)
        {
            switch (systemLanguage)
            {
                case SystemLanguage.Afrikaans:
                    return "af";
                case SystemLanguage.Arabic:
                    return "ar";
                case SystemLanguage.Basque:
                    return "eu";
                case SystemLanguage.Belarusian:
                    return "be";
                case SystemLanguage.Bulgarian:
                    return "bg";
                case SystemLanguage.Catalan:
                    return "ca";
                case SystemLanguage.Chinese:
                case SystemLanguage.ChineseTraditional:
                case SystemLanguage.ChineseSimplified:
                    return "zh";
                case SystemLanguage.Czech:
                    return "cs";
                case SystemLanguage.Danish:
                    return "da";
                case SystemLanguage.Dutch:
                    return "nl";
                case SystemLanguage.English:
                    return "en";
                case SystemLanguage.Estonian:
                    return "et";
                case SystemLanguage.Faroese:
                    return "fo";
                case SystemLanguage.Finnish:
                    return "fi";
                case SystemLanguage.French:
                    return "fr";
                case SystemLanguage.German:
                    return "de";
                case SystemLanguage.Greek:
                    return "el";
                case SystemLanguage.Hebrew:
                    return "he";
                case SystemLanguage.Hungarian:
                    return "hu";
                case SystemLanguage.Icelandic:
                    return "is";
                case SystemLanguage.Indonesian:
                    return "id";
                case SystemLanguage.Italian:
                    return "it";
                case SystemLanguage.Japanese:
                    return "ja";
                case SystemLanguage.Korean:
                    return "ko";
                case SystemLanguage.Latvian:
                    return "lv";
                case SystemLanguage.Lithuanian:
                    return "lt";
                case SystemLanguage.Norwegian:
                    return "no";
                case SystemLanguage.Polish:
                    return "pl";
                case SystemLanguage.Portuguese:
                    return "pt";
                case SystemLanguage.Romanian:
                    return "ro";
                case SystemLanguage.Russian:
                    return "ru";
                case SystemLanguage.SerboCroatian:
                    return "sr";
                case SystemLanguage.Slovak:
                    return "sk";
                case SystemLanguage.Slovenian:
                    return "sl";
                case SystemLanguage.Spanish:
                    return "es";
                case SystemLanguage.Swedish:
                    return "sv";
                case SystemLanguage.Thai:
                    return "th";
                case SystemLanguage.Turkish:
                    return "tk";
                case SystemLanguage.Ukrainian:
                    return "uk";
                case SystemLanguage.Unknown:
                    return "en";
                case SystemLanguage.Vietnamese:
                    return "vi";
                default:
                    return "en";
            }
        }
    }
}