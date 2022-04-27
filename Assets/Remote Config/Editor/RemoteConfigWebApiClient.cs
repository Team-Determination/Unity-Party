using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using UnityEditor;
using Unity.RemoteConfig.Core;
using Newtonsoft.Json.Linq;

namespace Unity.RemoteConfig.Editor
{
    /// <summary>
    /// This is a utility class for Remote Config to handle all Remote Config CRUD operations.
    /// </summary>
    public static class RemoteConfigWebApiClient
    {
        public static event Action<JArray> fetchEnvironmentsFinished;
        public static event Action<string> fetchDefaultEnvironmentFinished;
        public static event Action<JObject> fetchConfigsFinished;
        public static event Action<JArray> fetchRulesFinished;
        public static event Action<JObject, string> postAddRuleFinished;
        public static event Action<string, string> ruleRequestSuccess;
        public static event Action settingsRequestFinished;
        public static event Action<string> postConfigRequestFinished;
        public static event Action<long, string> rcRequestFailed;
        public static event Action environmentDeleted;
        public static event Action environmentUpdated;
        public static event Action <string> environmentCreated;
        public static event Action <JObject> environmentCRUDError;

        static List<IEnumerator<AsyncOperation>> m_WebRequestEnumerators = new List<IEnumerator<AsyncOperation>>();

        static bool m_UpdateListenerAlreadyAdded;

        private static string m_NoEnvErrorMsg = "There is no currently selected environment. Aborting operation.";
        private static string m_EmptyEnvNameErrorMsg = "Environment name can not be empty. Aborting operation.";
        private static string m_NoConfigId = "There is no config ID for this config. Aborting operation.";
        private static string m_NoCloudProjectIdErrorMsg = "This app does not have a cloud project ID, please go to Window > Services, and follow the prompts to associate this project with a Unity Organization";

        /// <summary>
        /// Checks if there are any unfinished web requests. Returns true if all web requests are done.
        /// </summary>
        public static bool webRequestsAreDone
        {
            get { return m_WebRequestEnumerators.Count == 0; }
        }

        /// <summary>
        /// Fetches all environments for the current project.
        /// </summary>
        /// <param name="cloudProjectId">Cloud Project ID for this Unity application.</param>
        /// <param name="responseParseErrorCallback">Optional callback for parsing errors.</param>
        public static void FetchEnvironments(string cloudProjectId, Action<Exception> responseParseErrorCallback = null)
        {
            if (!IsStringNullOrEmpty(cloudProjectId, m_NoCloudProjectIdErrorMsg))
            {
                m_WebRequestEnumerators.Add(_FetchEnvironments(cloudProjectId, responseParseErrorCallback));
                AddUpdateListenerIfNeeded();
            }
        }

        /// <summary>
        /// Fetches default environment for the current project.
        /// </summary>
        /// <param name="cloudProjectId">Cloud Project ID for this Unity application.</param>
        /// <param name="responseParseErrorCallback">Optional callback for parsing errors.</param>
        public static void FetchDefaultEnvironment(string cloudProjectId, Action<Exception> responseParseErrorCallback = null)
        {
            if (!IsStringNullOrEmpty(cloudProjectId, m_NoCloudProjectIdErrorMsg))
            {
                m_WebRequestEnumerators.Add(_FetchDefaultEnvironment(cloudProjectId, responseParseErrorCallback));
                AddUpdateListenerIfNeeded();
            }
        }

        /// <summary>
        /// Fetches all configs for the given environment ID.
        /// </summary>
        /// <param name="cloudProjectId">Cloud Project ID for this Unity application.</param>
        /// <param name="environmentId">ID of the environment that we want to fetch configs for</param>
        /// <param name="responseParseErrorCallback">Optional callback for parsing errors.</param>
        public static void FetchConfigs(string cloudProjectId, string environmentId, Action<Exception> responseParseErrorCallback = null)
        {
            if (!IsStringNullOrEmpty(cloudProjectId, m_NoCloudProjectIdErrorMsg) && !IsStringNullOrEmpty(environmentId, m_NoEnvErrorMsg))
            {
                m_WebRequestEnumerators.Add(_FetchConfigs(cloudProjectId, environmentId, responseParseErrorCallback));
                AddUpdateListenerIfNeeded();
            }
        }

        /// <summary>
        /// Fetches all rules for the given config ID.
        /// </summary>
        /// <param name="cloudProjectId">Cloud Project ID for this Unity application.</param>
        /// <param name="configId">ID of the config that we want to fetch rules for</param>
        /// <param name="responseParseErrorCallback">Optional callback for parsing errors.</param>
        public static void FetchRules(string cloudProjectId, string configId, Action<Exception> responseParseErrorCallback = null)
        {
            if (!IsStringNullOrEmpty(cloudProjectId, m_NoCloudProjectIdErrorMsg) && !IsStringNullOrEmpty(configId, m_NoConfigId))
            {
                m_WebRequestEnumerators.Add(_FetchRules(cloudProjectId, configId, responseParseErrorCallback));
                AddUpdateListenerIfNeeded();
            }
        }

        /// <summary>
        /// Pushes updates to the given existing config to the server for the given environment.
        /// </summary>
        /// <param name="cloudProjectId">Cloud Project ID for this Unity application.</param>
        /// <param name="environmentId">ID of the environment containing the config to be pushed.</param>
        /// <param name="configId">ID of the config to be pushed.</param>
        /// <param name="configValue">List of settings to be pushed.</param>
        public static void PutConfig(string cloudProjectId, string environmentId, string configId, JArray configValue)
        {
            if (!IsStringNullOrEmpty(cloudProjectId, m_NoCloudProjectIdErrorMsg) && !IsStringNullOrEmpty(environmentId, m_NoEnvErrorMsg) && !IsStringNullOrEmpty(configId, m_NoConfigId))
            {
                var payload = SerializeConfigStruct(environmentId, configValue);
                m_WebRequestEnumerators.Add(_PutConfig(cloudProjectId, configId, payload));
                AddUpdateListenerIfNeeded();
            }
        }

        /// <summary>
        /// Pushes a new config to the server in the given environment.
        /// </summary>
        /// <param name="cloudProjectId">Cloud Project ID for this Unity application.</param>
        /// <param name="environmentId">ID of the environment containing the config to be pushed.</param>
        /// <param name="configValue">List of settings to be pushed.</param>
        /// <param name="responseParseErrorCallback">Optional callback for parsing errors.</param>
        public static void PostConfig(string cloudProjectId, string environmentId, JArray configValue, Action<Exception> responseParseErrorCallback = null)
        {
            if (!IsStringNullOrEmpty(cloudProjectId, m_NoCloudProjectIdErrorMsg) && !IsStringNullOrEmpty(environmentId, m_NoEnvErrorMsg))
            {
                var payload = SerializeConfigStruct(environmentId, configValue);
                m_WebRequestEnumerators.Add(_PostConfig(cloudProjectId, payload, responseParseErrorCallback));
                AddUpdateListenerIfNeeded();
            }
        }

        /// <summary>
        /// Sends a POST request to add a new rule to the given config in the given environment.
        /// </summary>
        /// <param name="cloudProjectId">Cloud Project ID for this Unity application.</param>
        /// <param name="environmentId">ID of the environment where the new rule was added</param>
        /// <param name="configId">ID of the config containing this rule</param>
        /// <param name="rule">The rule that was added</param>
        /// <param name="responseParseErrorCallback">Optional callback for parsing errors.</param>
        public static void PostAddRule(string cloudProjectId, string environmentId, string configId, JObject rule, Action<Exception> responseParseErrorCallback = null)
        {
            if (!IsStringNullOrEmpty(cloudProjectId, m_NoCloudProjectIdErrorMsg) && !IsStringNullOrEmpty(configId, m_NoEnvErrorMsg))
            {
                rule["configId"] = configId;
                rule["environmentId"] = environmentId;
                var oldRuleId = rule["id"].Value<string>();
                m_WebRequestEnumerators.Add(_PostAddRule(cloudProjectId, rule.ToString(), oldRuleId, responseParseErrorCallback));
                AddUpdateListenerIfNeeded();
            }
        }

        /// <summary>
        /// Sends a PUT request to update a rule in the given environment and config.
        /// </summary>
        /// <param name="cloudProjectId">Cloud Project ID for this Unity application.</param>
        /// <param name="environmentId">ID of the environment where the updated rule exists</param>
        /// <param name="configId">ID of the config where the updated rule exists</param>
        /// <param name="rule">The updated rule with the new attributes</param>
        public static void PutEditRule(string cloudProjectId, string environmentId, string configId, JObject rule)
        {
            if (!IsStringNullOrEmpty(cloudProjectId, m_NoCloudProjectIdErrorMsg) && !IsStringNullOrEmpty(environmentId, m_NoEnvErrorMsg))
            {
                rule["configId"] = configId;
                rule["environmentId"] = environmentId;
                m_WebRequestEnumerators.Add(_PutEditRule(cloudProjectId, rule["id"].Value<string>(), rule.ToString()));
                AddUpdateListenerIfNeeded();
            }
        }

        /// <summary>
        /// Delete a rule from the given environment.
        /// </summary>
        /// <param name="cloudProjectId">Cloud Project ID for this Unity application.</param>
        /// <param name="environmentId">ID of the environment where the rule was deleted from</param>
        /// <param name="ruleId">ID of the deleted rule</param>
        public static void DeleteRule(string cloudProjectId, string environmentId, string ruleId)
        {
            if (!IsStringNullOrEmpty(cloudProjectId, m_NoCloudProjectIdErrorMsg) && !IsStringNullOrEmpty(environmentId, m_NoEnvErrorMsg))
            {
                m_WebRequestEnumerators.Add(_DeleteRule(cloudProjectId, ruleId));
                AddUpdateListenerIfNeeded();
            }

        }

        /// <summary>
        /// Sends a PUT request to update an environment.
        /// </summary>
        /// <param name="cloudProjectId">Cloud Project ID for this Unity application.</param>
        /// <param name="environmentId">ID of the environment to be updated</param>
        /// <param name="envName">Name of the environment to be updated</param>
        public static void UpdateEnvironment(string cloudProjectId, string environmentId, string envName)
        {
            if (!IsStringNullOrEmpty(cloudProjectId, m_NoCloudProjectIdErrorMsg) && !IsStringNullOrEmpty(environmentId, m_NoEnvErrorMsg) && !IsStringNullOrEmpty(envName, m_EmptyEnvNameErrorMsg))
            {
                var payloadJObj = new JObject();
                payloadJObj["name"] = envName;
                var payload = payloadJObj.ToString();
                m_WebRequestEnumerators.Add(_UpdateEnvironment(cloudProjectId, environmentId, payload));
                AddUpdateListenerIfNeeded();
            }
        }

        /// <summary>
        /// Sets default environment.
        /// </summary>
        /// <param name="cloudProjectId">Cloud Project ID for this Unity application.</param>
        /// <param name="environmentId">ID of the environment to be updated</param>
        public static void SetDefaultEnvironment(string cloudProjectId, string environmentId)
        {
            if (!IsStringNullOrEmpty(cloudProjectId, m_NoCloudProjectIdErrorMsg) && !IsStringNullOrEmpty(environmentId, m_NoEnvErrorMsg))
            {
                var payloadJObj = new JObject();
                payloadJObj["id"] = environmentId;
                var payload = payloadJObj.ToString();
                m_WebRequestEnumerators.Add(_SetDefaultEnvironment(cloudProjectId, payload));
                AddUpdateListenerIfNeeded();
            }
        }

        /// <summary>
        /// Sends a DELETE request to delete an environment.
        /// </summary>
        /// <param name="environmentId">ID of the environment to be deleted</param>
        public static void DeleteEnvironment(string cloudProjectId, string environmentId)
        {
            if (!IsStringNullOrEmpty(cloudProjectId, m_NoCloudProjectIdErrorMsg) && !IsStringNullOrEmpty(environmentId, m_NoEnvErrorMsg))
            {
                m_WebRequestEnumerators.Add(_DeleteEnvironment(cloudProjectId, environmentId));
                AddUpdateListenerIfNeeded();
            }
        }

        /// <summary>
        /// Sends a DELETE request to delete aconfig.
        /// </summary>
        /// <param name="configId">ID of the config to be deleted</param>
        public static void DeleteConfig(string cloudProjectId, string configId)
        {
            if (!IsStringNullOrEmpty(cloudProjectId, m_NoCloudProjectIdErrorMsg) && !IsStringNullOrEmpty(configId, m_NoConfigId))
            {
                m_WebRequestEnumerators.Add(_DeleteConfig(cloudProjectId, configId));
                AddUpdateListenerIfNeeded();
            }
        }

        /// <summary>
        /// Creates new environment.
        /// </summary>
        /// <param name="cloudProjectId">Cloud Project ID for this Unity application.</param>
        /// <param name="environmentName">Name of the environment to be created.</param>
        /// <param name="responseParseErrorCallback">Optional callback for parsing errors.</param>
        public static void CreateEnvironment(string cloudProjectId, string environmentName, Action<Exception> responseParseErrorCallback = null)
        {
            if (!IsStringNullOrEmpty(cloudProjectId, m_NoCloudProjectIdErrorMsg) && !IsStringNullOrEmpty(environmentName, m_NoEnvErrorMsg))
            {
                var payloadJObj = new JObject();
                payloadJObj["name"] = environmentName;
                var payload = payloadJObj.ToString();
                m_WebRequestEnumerators.Add(_CreateEnvironment(cloudProjectId, payload));
                AddUpdateListenerIfNeeded();
            }
        }

        private static IEnumerator<AsyncOperation> _FetchEnvironments(string cloudProjectId, Action<Exception> responseParseErrorCallback = null)
        {
            string url = string.Format(RemoteConfigEnvConf.environmentPath, cloudProjectId);
            var request = Authorize(UnityWebRequest.Get(url));
            yield return request.SendWebRequest();

            CleanupCurrentRequest();

            if (request.isNetworkError || request.isHttpError)
            {
                var error = ParseResponseToJError(request);
                Debug.LogWarning("Failed to fetch remote configurations: " + request.error + "\n" + error["message"].Value<string>());
                rcRequestFailed?.Invoke(request.responseCode, request.error);
                yield break;
            }
            ParseEnvironments(request.downloadHandler.text, responseParseErrorCallback);
        }

        private static IEnumerator<AsyncOperation> _FetchDefaultEnvironment(string cloudProjectId, Action<Exception> responseParseErrorCallback = null)
        {
            string url = string.Format(RemoteConfigEnvConf.getDefaultEnvironmentPath, cloudProjectId);
            var request = Authorize(UnityWebRequest.Get(url));
            yield return request.SendWebRequest();

            CleanupCurrentRequest();

            if (request.isNetworkError || request.isHttpError)
            {
                var error = ParseResponseToJError(request);
                Debug.LogWarning("Failed to fetch default environment: " + request.error + "\n" + error["message"].Value<string>());
                rcRequestFailed?.Invoke(request.responseCode, request.error);
                yield break;
            }
            ParseDefaultEnvironment(request.downloadHandler.text, responseParseErrorCallback);
        }

        private static IEnumerator<AsyncOperation> _FetchConfigs(string cloudProjectId, string environmentId, Action<Exception> responseParseErrorCallback = null)
        {
            string remoteSettingsUrl = string.Format(RemoteConfigEnvConf.getConfigPath, cloudProjectId, environmentId);
            var request = Authorize(UnityWebRequest.Get(remoteSettingsUrl));
            yield return request.SendWebRequest();

            CleanupCurrentRequest();

            if (request.isNetworkError || request.isHttpError)
            {
                var error = ParseResponseToJError(request);
                Debug.LogWarning("Failed to fetch remote config: " + request.error + "\n" + error["message"].Value<string>());
                rcRequestFailed?.Invoke(request.responseCode, request.error);
                yield break;
            }
            string remoteSettingsJson = request.downloadHandler.text;
            ParseConfigs(remoteSettingsJson, responseParseErrorCallback);
        }

        private static IEnumerator<AsyncOperation> _PutConfig(string cloudProjectId, string configId, string payload)
        {
            string url = string.Format(RemoteConfigEnvConf.putConfigPath, cloudProjectId, configId);

            var request = Authorize(UnityWebRequest.Put(url, payload));
            yield return request.SendWebRequest();

            CleanupCurrentRequest();

            if (request.isNetworkError || request.isHttpError)
            {
                var error = ParseResponseToJError(request);
                Debug.LogWarning("Failed to push remote config: " + request.error + "\n" + error["message"].Value<string>());
                rcRequestFailed?.Invoke(request.responseCode, request.error);
            }
            settingsRequestFinished?.Invoke();
        }

        private static IEnumerator<AsyncOperation> _PostConfig(string cloudProjectId, string payload, Action<Exception> responseParseErrorCallback = null)
        {
            string url = string.Format(RemoteConfigEnvConf.postConfigPath, cloudProjectId);

            var request = new UnityWebRequest(url, UnityWebRequest.kHttpVerbPOST);
            request.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(payload));
            request = Authorize(request);
            request.downloadHandler = new DownloadHandlerBuffer();
            yield return request.SendWebRequest();

            CleanupCurrentRequest();

            string configId = null;

            if (request.isNetworkError || request.isHttpError)
            {
                var error = ParseResponseToJError(request);
                Debug.LogWarning("Failed to push remote config: " + request.error + "\n" + error["message"].Value<string>());
                rcRequestFailed?.Invoke(request.responseCode, request.error);
            }
            else
            {
                string postConfigResponseJson = request.downloadHandler.text;
                configId = ParsePostConfigResponse(postConfigResponseJson, responseParseErrorCallback)["id"].Value<string>();
            }
            postConfigRequestFinished?.Invoke(configId);
        }

        private static IEnumerator<AsyncOperation> _FetchRules(string cloudProjectId, string configId, Action<Exception> responseParseErrorCallback = null)
        {
            string url = string.Format(RemoteConfigEnvConf.multiRulesPath, cloudProjectId, configId);
            var request = Authorize(UnityWebRequest.Get(url));
            yield return request.SendWebRequest();

            CleanupCurrentRequest();

            if (request.isNetworkError || request.isHttpError)
            {
                var error = ParseResponseToJError(request);
                Debug.LogWarning("Failed to GET all rules: " + request.error + "\n" + error["message"].Value<string>());
                rcRequestFailed?.Invoke(request.responseCode, request.error);
            }
            else
            {
                ParseRules(request.downloadHandler.text, responseParseErrorCallback);
            }
        }

        private static IEnumerator<AsyncOperation> _PostAddRule(string cloudProjectId, string payload, string oldRuleId, Action<Exception> responseParseErrorCallback = null)
        {
            string url = string.Format(RemoteConfigEnvConf.postRulePath, cloudProjectId);

            //Make sure the POST request doesn't send up the data as a form
            var request = new UnityWebRequest(url, UnityWebRequest.kHttpVerbPOST);
            request.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(payload));
            request = Authorize(request);
            request.downloadHandler = new DownloadHandlerBuffer();
            yield return request.SendWebRequest();

            CleanupCurrentRequest();

            if (request.isNetworkError || request.isHttpError)
            {
                var error = ParseResponseToJError(request);
                Debug.LogWarning("Create Rule Error: " + request.error + "\n" + error["message"].Value<string>());
                rcRequestFailed?.Invoke(request.responseCode, request.error);
            }
            else
            {
                string addRuleResponseJson = request.downloadHandler.text;
                ParseAddRuleResponse(addRuleResponseJson, oldRuleId, responseParseErrorCallback);
                ruleRequestSuccess?.Invoke(UnityWebRequest.kHttpVerbPOST, oldRuleId);
            }

        }

        private static IEnumerator<AsyncOperation> _PutEditRule(string cloudProjectId, string ruleId, string payload)
        {
            string url = string.Format(RemoteConfigEnvConf.singleRulePath, cloudProjectId, ruleId);

            var request = Authorize(UnityWebRequest.Put(url, payload));
            yield return request.SendWebRequest();
            CleanupCurrentRequest();

            if (request.isNetworkError || request.isHttpError)
            {
                var error = ParseResponseToJError(request);
                rcRequestFailed?.Invoke(request.responseCode, request.error);
                Debug.LogWarning("Update Rule Error: " + request.error + "\n" + error["message"].Value<string>());
            }
            else
            {
                ruleRequestSuccess?.Invoke(UnityWebRequest.kHttpVerbPUT, ruleId);
            }

        }

        private static IEnumerator<AsyncOperation> _DeleteRule(string cloudProjectId, string ruleId)
        {
            string url = string.Format(RemoteConfigEnvConf.singleRulePath, cloudProjectId, ruleId);

            var request = Authorize(UnityWebRequest.Delete(url));
            yield return request.SendWebRequest();

            CleanupCurrentRequest();

            if (request.isNetworkError || request.isHttpError)
            {
                var error = ParseResponseToJError(request);
                rcRequestFailed?.Invoke(request.responseCode, request.error);
                Debug.LogWarning("Delete Rule Error: " + request.error + "\n" + error["message"].Value<string>());
            }
            else
            {
                ruleRequestSuccess?.Invoke(UnityWebRequest.kHttpVerbDELETE, ruleId);
            }

        }

        private static IEnumerator<AsyncOperation> _UpdateEnvironment(string cloudProjectId, string environmentId, string payload)
        {
            string url = string.Format(RemoteConfigEnvConf.getEnvironmentPath, cloudProjectId, environmentId);

            var request = Authorize(UnityWebRequest.Put(url, payload));
            yield return request.SendWebRequest();

            CleanupCurrentRequest();

            if (request.isNetworkError || request.isHttpError)
            {
                var error = ParseResponseToJError(request);
                Debug.LogWarning("Update Environment Error: " + request.error + "\n" + error["message"].Value<string>());
                environmentCRUDError?.Invoke(error);
            }
            else
            {
                environmentUpdated?.Invoke();
            }
        }

        private static IEnumerator<AsyncOperation> _SetDefaultEnvironment(string cloudProjectId, string payload)
        {
            string url = string.Format(RemoteConfigEnvConf.getDefaultEnvironmentPath, cloudProjectId);

            var request = Authorize(UnityWebRequest.Put(url, payload));
            yield return request.SendWebRequest();

            CleanupCurrentRequest();

            if (request.isNetworkError || request.isHttpError)
            {
                var error = ParseResponseToJError(request);
                Debug.LogWarning("Update Environment Error: " + request.error + "\n" + error["message"].Value<string>());
                environmentCRUDError?.Invoke(error);
            }
            else
            {
                environmentUpdated?.Invoke();
            }
        }

        private static IEnumerator<AsyncOperation> _DeleteEnvironment(string cloudProjectId, string environmentId)
        {
            string url = string.Format(RemoteConfigEnvConf.getEnvironmentPath, cloudProjectId, environmentId);

            var request = Authorize(UnityWebRequest.Delete(url+"&recursive=true"));
            yield return request.SendWebRequest();

            CleanupCurrentRequest();

            if (request.isNetworkError || request.isHttpError)
            {
                var error = ParseResponseToJError(request);
                Debug.LogWarning("Delete Environment Error: " + request.error + "\n" + error["message"].Value<string>());
                environmentCRUDError?.Invoke(error);
            }
            else
            {
                environmentDeleted?.Invoke();
            }
        }

        private static IEnumerator<AsyncOperation> _DeleteConfig(string cloudProjectId, string configId)
        {
            string url = string.Format(RemoteConfigEnvConf.putConfigPath, cloudProjectId, configId);
            var request = Authorize(UnityWebRequest.Delete(url));
            yield return request.SendWebRequest();

            CleanupCurrentRequest();

            if (request.isNetworkError || request.isHttpError)
            {
                var error = ParseResponseToJError(request);
                Debug.LogWarning("Delete Config Error: " + request.error + "\n" + error["message"].Value<string>());
            }

        }

        private static IEnumerator<AsyncOperation> _CreateEnvironment(string cloudProjectId, string payload)
        {
            string url = string.Format(RemoteConfigEnvConf.environmentPath, cloudProjectId);
            var request =  new UnityWebRequest(url, "POST");
            byte[] bodyRaw = Encoding.UTF8.GetBytes(payload);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            Authorize(request);

            yield return request.SendWebRequest();

            CleanupCurrentRequest();

            if (request.isNetworkError || request.isHttpError)
            {
                var error = ParseResponseToJError(request);
                Debug.LogWarning("Create Environment Error: " + request.error + "\n" + error["message"].Value<string>());
                environmentCRUDError?.Invoke(error);
            }
            else
            {
                string createEnvironmentResponseJson = request.downloadHandler.text;
                var response = JObject.Parse(createEnvironmentResponseJson);
                environmentCreated?.Invoke(response["id"].Value<string>());
            }
        }

        private static void ParseRules(string json, Action<Exception> responseParseErrorCallback)
        {
            var rules = new JArray();
            try
            {
                var tempRules = (JArray)(JObject.Parse(json)["rules"]);
                for(int i = 0; i < tempRules.Count; i++)
                {
                    var ruleJObject = (JObject)tempRules[i];
                    if (ruleJObject["type"].Value<string>() == "segmentation")
                    {
                        var newRule = ruleJObject.DeepClone();
                        var responseValue = (JArray)newRule["value"];
                        for (int j = 0; j < responseValue.Count; j++)
                        {
                            var settingResponseValue = responseValue[j];
                            var settingVal = settingResponseValue["values"][0];
                            settingResponseValue["values"].Parent.Remove();
                            settingResponseValue["value"] = settingVal;
                        }
                        rules.Add(newRule);
                    }
                    else if(ruleJObject["type"].Value<string>() == "variant")
                    {
                        rules.Add(ruleJObject.DeepClone());
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning("Rules response was not valid JSON:\n" + json + "\n" + e);
                responseParseErrorCallback?.Invoke(e);
                rules = new JArray();
            }

            fetchRulesFinished?.Invoke(rules);
        }

        private static void ParseEnvironments(string json, Action<Exception> responseParseErrorCallback)
        {
            var parsedEnvironments = new JArray();
            try
            {
                parsedEnvironments =(JArray)(JObject.Parse(json)["environments"]);
            }
            catch (Exception e)
            {
                Debug.LogWarning("Remote Config response was not valid JSON:\n" + json + "\n" + e);
                responseParseErrorCallback?.Invoke(e);
                parsedEnvironments = null;
            }
            fetchEnvironmentsFinished?.Invoke(parsedEnvironments);
        }

        private static void ParseDefaultEnvironment(string json, Action<Exception> responseParseErrorCallback)
        {
            var defaultEnvironmentId = "";
            try
            {
                defaultEnvironmentId = JObject.Parse(json)["id"].Value<string>();
            }
            catch (Exception e)
            {
                Debug.LogWarning("Remote Config response for defaultEnvironment was not valid JSON:\n" + json + "\n" + e);
                responseParseErrorCallback?.Invoke(e);
            }
            fetchDefaultEnvironmentFinished?.Invoke(defaultEnvironmentId);
        }

        private static void ParseConfigs(string json, Action<Exception> responseParseErrorCallback)
        {
            JObject config = new JObject();
            try
            {
                var configData = (JArray)(JObject.Parse(json)["configs"]);
                for (int i = 0; i < configData.Count; i++)
                {
                    if (configData[i]["type"].Value<string>() == "settings")
                    {
                        config = (JObject)configData[i];
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning("Remote Config response was not valid JSON:\n" + json + "\n" + e);
                responseParseErrorCallback?.Invoke(e);
            }

            fetchConfigsFinished?.Invoke(config);
        }

        private static JObject ParsePostConfigResponse(string json, Action<Exception> responseParseErrorCallback)
        {
            JObject response = new JObject();
            try
            {
                response = JObject.Parse(json);
            }
            catch (Exception e)
            {
                Debug.LogWarning("POST config reponse was not valid JSON:\n" + json + "\n" + e);
                responseParseErrorCallback?.Invoke(e);
            }
            return response;
        }

        private static void ParseAddRuleResponse(string json, string oldRuleId, Action<Exception> responseParseErrorCallback)
        {
            JObject response;
            try
            {
                response = JObject.Parse(json);
            }
            catch (Exception e)
            {
                response = new JObject();
                Debug.LogWarning("POST Add Rule reponse was not valid JSON:\n" + json + "\n" + e);
                responseParseErrorCallback?.Invoke(e);
            }
            postAddRuleFinished?.Invoke(response, oldRuleId);
        }

        private static JObject ParseResponseToJError(UnityWebRequest request)
        {
            JObject errorStruct;
            try
            {
                errorStruct = JObject.Parse(request.downloadHandler.text);
            }
            catch (Exception e)
            {
                errorStruct = new JObject();
                errorStruct["code"] = request.responseCode;
                errorStruct["message"] = request.error;
                Debug.LogWarning("request download handler for error is not valid JSON:\n" + request.downloadHandler.text + "\n" + e);
            }
            return errorStruct;
        }

        private static string SerializeConfigStruct(string environmentId, JArray configValue)
        {
            var payload = new JObject();
            payload["environmentId"] = environmentId;
            payload["type"] = "settings";
            payload["value"] = configValue;
            return payload.ToString();
        }

        private static void AddUpdateListenerIfNeeded()
        {
            if (!m_UpdateListenerAlreadyAdded)
            {
                EditorApplication.update += Update;
                m_UpdateListenerAlreadyAdded = true;
            }
        }

        private static UnityWebRequest Authorize(UnityWebRequest request)
        {
            request.SetRequestHeader("Content-Type", "application/json");
            request.SetRequestHeader("User-Agent", "Unity Editor " + Application.unityVersion + " RC " + RemoteConfigEnvConf.pluginVersion);
            request.SetRequestHeader("Authorization", string.Format("Bearer {0}", CloudProjectSettings.accessToken));
            CloudProjectSettings.RefreshAccessToken((accessTokenRefreshed) => { });
            return request;
        }

        private static bool IsStringNullOrEmpty(string stringToCheck, string errorMsg)
        {
            if (string.IsNullOrEmpty(stringToCheck))
            {
                if (!string.IsNullOrEmpty(errorMsg))
                {
                    Debug.LogWarning(errorMsg);
                }
                throw new ArgumentException(errorMsg);
            }
            return false;
        }

        private static void Update()
        {
            UpdateCoroutine();
        }

        private static void UpdateCoroutine()
        {
            if (m_WebRequestEnumerators.Count > 0)
            {
                var m_webRequestEnumerator = m_WebRequestEnumerators[0];
                if (m_webRequestEnumerator != null)
                {
                    if (m_webRequestEnumerator.Current == null)
                    {
                        m_webRequestEnumerator.MoveNext();
                    }
                    else if (m_webRequestEnumerator.Current.isDone)
                    {
                        m_webRequestEnumerator.MoveNext();
                    }
                }
            }
        }

        private static void CleanupCurrentRequest()
        {
            m_WebRequestEnumerators.RemoveAt(0);
            if (webRequestsAreDone && m_UpdateListenerAlreadyAdded)
            {
                EditorApplication.update -= Update;
                m_UpdateListenerAlreadyAdded = false;
            }
        }

    }
}