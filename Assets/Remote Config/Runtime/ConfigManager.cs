using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using Newtonsoft.Json.Linq;
using UnityEngine;

[assembly: InternalsVisibleTo("Unity.RemoteConfig.Tests")]

namespace Unity.RemoteConfig
{
    /// <summary>
    /// Use this class to fetch and apply your configuration settings at runtime.
    /// ConfigManager is wrapper class to mimic the functionality of underlying ConfigManagerImpl class.
    /// It uses an instance of ConfigManagerImpl class, making it a primitive class of ConfigManagerImpl.
    /// </summary>
    public static class ConfigManager
    {
        private static ConfigManagerImpl _configmanagerImpl = new ConfigManagerImpl("remote-config");

        /// <summary>
        /// Returns the status of the current configuration request from the service.
        /// </summary>
        /// <returns>
        /// An enum representing the status of the current Remote Config request.
        /// </returns>
        public static ConfigRequestStatus requestStatus
        {
            get { return _configmanagerImpl.requestStatus; }
            set { _configmanagerImpl.requestStatus = value; }
        }

        /// <summary>
        /// This event fires when the configuration manager successfully fetches settings from the service.
        /// </summary>
        /// <returns>
        /// A struct representing the response of a Remote Config fetch.
        /// </returns>
        public static event Action<ConfigResponse> FetchCompleted
        {
            add { _configmanagerImpl.FetchCompleted += value; }
            remove { _configmanagerImpl.FetchCompleted -= value; }
        }

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
        public static RuntimeConfig appConfig
        {
            get { return _configmanagerImpl.appConfig; }
            set { _configmanagerImpl.appConfig = value; }
        }

        /// <summary>
        /// Sets a custom user identifier for the Remote Config delivery request payload.
        /// </summary>
        /// <param name="customUserID">Custom user identifier.</param>
        public static void SetCustomUserID(string customUserID)
        {
            _configmanagerImpl.SetCustomUserID(customUserID);
        }

        /// <summary>
        /// Sets an environment identifier in the Remote Config delivery request payload.
        /// </summary>
        /// <param name="environmentID">Environment unique identifier.</param>
        public static void SetEnvironmentID(string environmentID)
        {
            _configmanagerImpl.SetEnvironmentID(environmentID);
        }

        /// <summary>
        /// Fetches an app configuration settings from the remote server.
        /// </summary>
        /// <param name="userAttributes">A struct containing custom user attributes. If none apply, use an empty struct.</param>
        /// <param name="appAttributes">A struct containing custom app attributes. If none apply, use an empty struct.</param>
        /// <typeparam name="T">The type of the <c>userAttributes</c> struct.</typeparam>
        /// <typeparam name="T2">The type of the <c>appAttributes</c> struct.</typeparam>
        public static void FetchConfigs<T, T2>(T userAttributes, T2 appAttributes) where T : struct where T2 : struct
        {
            _configmanagerImpl.FetchConfigs(userAttributes, appAttributes);
        }

        internal static void SaveCache(ConfigOrigin origin, Dictionary<string, string> headers, string result)
        {
            _configmanagerImpl.SaveCache(origin, headers, result);
        }

        internal static void LoadFromCache()
        {
            _configmanagerImpl.LoadFromCache();
        }

    }
}
