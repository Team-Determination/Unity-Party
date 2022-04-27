using System;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace Unity.RemoteConfig.Editor
{
    internal class RemoteConfigEnvironmentWindowController
    {
        private RemoteConfigDataStore m_DataStore;
        public event Action environmentUpdated;
        public event Action environmentDeleted;
        public event Action <string> environmentCreated;
        public event Action <JObject> environmentCRUDError;
        public event Action <JArray> fetchEnvironmentsFinished;
        public event Action<JObject> fetchConfigsFinished;
        public event Action<JArray> fetchRulesFinished;

        internal enum EnvironmentWindowState {
            None = 0,
            Creating = 1,
            Updating = 2,
            Deleting = 3
        }

        public EnvironmentWindowState currentLoadingState = EnvironmentWindowState.None;

        public RemoteConfigEnvironmentWindowController()
        {
            m_DataStore = RemoteConfigUtilities.CheckAndCreateDataStore();

            RemoteConfigWebApiClient.environmentUpdated += OnEnvironmentUpdated;
            RemoteConfigWebApiClient.environmentDeleted += OnEnvironmentDeleted;
            RemoteConfigWebApiClient.environmentCreated += OnEnvironmentCreated;
            RemoteConfigWebApiClient.environmentCRUDError += OnEnvironmentCRUDError;
            RemoteConfigWebApiClient.fetchEnvironmentsFinished += OnFetchEnvironmentsFinished;
            RemoteConfigWebApiClient.fetchConfigsFinished += OnFetchConfigsFinished;
            RemoteConfigWebApiClient.fetchRulesFinished += OnFetchRulesFinished;
        }

        private void OnEnvironmentUpdated()
        {
            environmentUpdated?.Invoke();
        }

        private void OnEnvironmentDeleted()
        {
            environmentDeleted?.Invoke();
        }

        private void OnEnvironmentCreated(string environmentId)
        {
            environmentCreated?.Invoke(environmentId);
        }

        private void OnEnvironmentCRUDError(JObject error)
        {
            environmentCRUDError?.Invoke(error);
        }

        private void OnFetchEnvironmentsFinished(JArray environments)
        {
            fetchEnvironmentsFinished?.Invoke(environments);
        }

        private void OnFetchConfigsFinished(JObject config)
        {
            fetchConfigsFinished?.Invoke(config);
        }

        private void OnFetchRulesFinished(JArray rules)
        {
            fetchRulesFinished?.Invoke(rules);
        }

        public bool IsCurrentEnvironmentDefault()
        {
            return m_DataStore.currentEnvironmentIsDefault;
        }

        public string GetCurrentEnvironmentId()
        {
            return m_DataStore.currentEnvironmentId;
        }

        public bool SetEnvironmentData(JArray environments, string currentEnvironmentName = "")
        {
            if (environments != null && environments.Count > 0)
            {
                m_DataStore.environments = environments;
                if (String.IsNullOrEmpty(currentEnvironmentName))
                {
                    currentEnvironmentName = m_DataStore.currentEnvironmentName;
                }
                var currentEnvironment = LoadEnvironments(environments, currentEnvironmentName);
                m_DataStore.SetCurrentEnvironment(currentEnvironment);
                m_DataStore.SetLastSelectedEnvironment(currentEnvironment["name"].Value<string>());

                return true;
            }
            return false;
        }

        public JObject LoadEnvironments(JArray environments, string currentEnvName)
        {
            if (environments.Count > 0)
            {
                var currentEnvironment = environments[0];
                foreach (var environment in environments)
                {
                    if (environment["name"].Value<string>() == currentEnvName)
                    {
                        currentEnvironment = environment;
                        break;
                    }
                }
                return (JObject)currentEnvironment;
            }

            Debug.LogWarning("No environments loaded. Please restart the Remote Config editor window");
            return new JObject();
        }

        public void RefreshDataStore(JObject config)
        {
            m_DataStore.config = config;
            m_DataStore.rsLastCachedKeyList = new JArray(m_DataStore.rsKeyList);
        }

        public void RefreshRulesDataStore(JArray rules)
        {
            m_DataStore.ClearUpdatedRulesLists();
            m_DataStore.rulesList = rules;
            m_DataStore.lastCachedRulesList = new JArray(m_DataStore.rulesList);
        }

        public bool DeleteEnvironment(string envId)
        {
            if (!IsCurrentEnvironmentDefault())
            {
                try
                {
                   RemoteConfigWebApiClient.DeleteEnvironment(Application.cloudProjectId, envId);
                }
                catch
                {
                    Debug.LogWarning("Error deleting environment: " + envId);
                }
            }
            return true;
        }

        public bool UpdateEnvironment(string envId, string envName)
        {
            currentLoadingState = EnvironmentWindowState.Updating;
            try
            {
                RemoteConfigWebApiClient.UpdateEnvironment(Application.cloudProjectId, envId, envName);
            }
            catch
            {
                Debug.LogWarning("Error updating environment: " + envId);
            }
            return true;
        }

        public bool SetDefaultEnvironment(string envId)
        {
            currentLoadingState = EnvironmentWindowState.Updating;
            try
            {
                RemoteConfigWebApiClient.SetDefaultEnvironment(Application.cloudProjectId, envId);
            }
            catch
            {
                Debug.LogWarning("Error setting default environment: " + envId);
            }
            return true;
        }

        public bool CreateEnvironment(string envName)
        {
            currentLoadingState = EnvironmentWindowState.Creating;
            try
            {
                RemoteConfigWebApiClient.CreateEnvironment(Application.cloudProjectId, envName);
            }
            catch
            {
                Debug.LogWarning("Error creating environment: " + envName);
            }
            return true;
        }
    }
}