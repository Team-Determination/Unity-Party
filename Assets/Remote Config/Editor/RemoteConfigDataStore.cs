using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Newtonsoft.Json.Linq;
using System.Runtime.CompilerServices;
using UnityEditor;

[assembly: InternalsVisibleTo("Unity.RemoteConfig.Editor.Tests")]

namespace Unity.RemoteConfig.Editor
{
    public class RemoteConfigDataStore : ScriptableObject, ISerializationCallbackReceiver
    {
        public event Action EnvironmentChanged;
        public event Action RulesDataStoreChanged;
        public event Action RemoteSettingDataStoreChanged;

        const string k_CurrentEnvironment = "UnityRemoteConfigEditorEnvironment";

        public readonly int defaultRulePriority = 1000;
        private const int maxRulePriority = 1000;
        private const int minRulePriority = 0;
        public static readonly List<string> rsTypes = new List<string> { "string", "bool", "float", "int", "long", "json" };

        public enum m_DataStoreStatus {
            Init = 0,
            UnSynchronized = 1,
            Synchronized = 2,
            Pending = 3,
            Error = 4
        };
        public m_DataStoreStatus dataStoreStatus { get; set; }

        private void OnEnable()
        {
            RestoreLastSelectedEnvironment(currentEnvironmentName);
            dataStoreStatus = m_DataStoreStatus.Init;
        }

        // Data stores for Remote Settings
        public JArray rsKeyList { get; set; } = new JArray();

        public JArray rsLastCachedKeyList { get; set; } = new JArray();
        public string configId { get; set; } = "";

        public string currentEnvironmentName { get; set; } = "";
        public string currentEnvironmentId { get; set; } = "";
        public bool currentEnvironmentIsDefault { get; set; }

        public JArray environments { get; set; } = new JArray();
        private JArray m_rulesList;

        public JArray rulesList
        {
            get { return m_rulesList;}
            set {
                m_rulesList = new JArray();
                var defaultSettings = rsKeyList;
                foreach(var rule in value)
                {
                    if(rule["type"].Value<string>() == "segmentation")
                    {
                        var settingsInRule = new JArray();
                        foreach (var setting in rule["value"])
                        {
                            var newSetting = new JObject();

                            // if rule is already formatted, with ["rs"] key present in the setting, leave setting as is:
                            if (setting["rs"] != null)
                            {
                                newSetting = (JObject)setting;
                            }
                            else
                            {
                                string entityId = null;
                                var defaultSettingIndex = -1;
                                for (int i = 0; i < defaultSettings.Count; i++)
                                {
                                    if (defaultSettings[i]["rs"]["key"].Value<string>() == setting["key"].Value<string>() && defaultSettings[i]["rs"]["type"].Value<string>() == setting["type"].Value<string>())
                                    {
                                        defaultSettingIndex = i;
                                    }
                                }
                                if (defaultSettingIndex == -1)
                                {
                                    entityId = Guid.NewGuid().ToString();
                                }
                                else
                                {
                                    entityId = defaultSettings[defaultSettingIndex]["metadata"]["entityId"].Value<string>();
                                }

                                newSetting["metadata"] = new JObject();
                                newSetting["metadata"]["entityId"] = entityId;
                                newSetting["rs"] = setting;
                            }

                            settingsInRule.Add(newSetting);
                        }
                        var newRule = rule;
                        newRule["value"] = settingsInRule;
                        m_rulesList.Add(newRule);
                    }
                    else if(rule["type"].Value<string>() == "variant")
                    {
                        foreach (var variant in rule["value"])
                        {
                            var variantsJArray = (JArray)variant["values"];
                            var settingsInVariant = new JArray();
                            for(int i = 0; i < variantsJArray.Count; i++)
                            {
                                var newSetting = new JObject();

                                // if rule is already formatted, with ["rs"] key present in the setting, leave setting as is:
                                if (variantsJArray[i]["rs"] != null)
                                {
                                    newSetting = (JObject)variantsJArray[i];
                                }
                                else
                                {
                                    string entityId = null;
                                    var defaultSettingIndex = -1;
                                    for (int j = 0; j < defaultSettings.Count; j++)
                                    {
                                        if (defaultSettings[j]["rs"]["key"].Value<string>() == variantsJArray[i]["key"].Value<string>() && defaultSettings[j]["rs"]["type"].Value<string>() == variantsJArray[i]["type"].Value<string>())
                                        {
                                            defaultSettingIndex = j;
                                        }
                                    }
                                    if (defaultSettingIndex == -1)
                                    {
                                        entityId = Guid.NewGuid().ToString();
                                    }
                                    else
                                    {
                                        entityId = defaultSettings[defaultSettingIndex]["metadata"]["entityId"].Value<string>();
                                    }

                                    newSetting["metadata"] = new JObject();
                                    newSetting["metadata"]["entityId"] = entityId;
                                    newSetting["rs"] = variantsJArray[i];
                                }

                                settingsInVariant.Add(newSetting);
                            }
                            variant["values"] = settingsInVariant;
                        }
                        m_rulesList.Add(rule);
                    }
                }
                RulesDataStoreChanged?.Invoke();
                RemoteSettingDataStoreChanged?.Invoke();
                }
        }

        public JArray lastCachedRulesList { get; set; } = new JArray();

        public List<string> addedRulesIDs { get; set; } = new List<string>();
        public List<string> updatedRulesIDs { get; set; } = new List<string>();
        public List<string> deletedRulesIDs { get; set; } = new List<string>();

        private JObject m_config;
        public JObject config
        {
            get { return m_config;}
            set {
                rsKeyList = new JArray();
                if (value.HasValues)
                {
                    foreach(var val in value["value"])
                    {
                        var newSetting = new JObject();
                        newSetting["metadata"] = new JObject();
                        newSetting["metadata"]["entityId"] = Guid.NewGuid().ToString();
                        newSetting["rs"] = val;
                        rsKeyList.Add(newSetting);
                    }
                    configId = value["id"].Value<string>();
                }
                else
                {
                    configId = null;
                }

                m_config = value;
            }
        }

        // Values for Serialization
        public string _rsKeyList;
        public string _rsLastCachedKeyList;
        public string _environments;
        public string _rulesList;
        public string _lastCachedRulesList;

        public void OnBeforeSerialize()
        {
            _rsKeyList = rsKeyList == null ? "" : rsKeyList.ToString();
            _rsLastCachedKeyList = rsLastCachedKeyList == null ? "" : rsLastCachedKeyList.ToString();
            _environments = environments == null ? "" : environments.ToString();
            _rulesList = rulesList == null ? "" : rulesList.ToString();
            _lastCachedRulesList = lastCachedRulesList == null ? "" : lastCachedRulesList.ToString();
        }

        public void OnAfterDeserialize()
        {
            rsKeyList = string.IsNullOrEmpty(_rsKeyList) ? new JArray() : JArray.Parse(_rsKeyList);
            rsLastCachedKeyList = string.IsNullOrEmpty(_rsLastCachedKeyList) ? new JArray() : JArray.Parse(_rsLastCachedKeyList);
            environments = string.IsNullOrEmpty(_environments) ? new JArray() : JArray.Parse(_environments);
            rulesList = string.IsNullOrEmpty(_rulesList) ? new JArray() : JArray.Parse(_rulesList);
            lastCachedRulesList = string.IsNullOrEmpty(_lastCachedRulesList) ? new JArray() : JArray.Parse(_lastCachedRulesList);
        }

        public int settingsCount
        {
            get
            {
                return rsKeyList == null ? 0 : rsKeyList.Count;
            }
        }

        /// <summary>
        /// Gets the RuleWithSettingsMetadata at the given index in the rulesList.
        /// </summary>
        /// <param name="selectedRuleIndex">The index of the RuleWithSettingsMetadata we are getting from the rulesList</param>
        /// <returns>The RuleWithSettingsMetadata from the rulesList at the given index</returns>
        public JObject GetRuleAtIndex(int selectedRuleIndex)
        {
            return (JObject) rulesList[selectedRuleIndex];
        }

        /// <summary>
        /// Gets the RuleWithSettingsMetadata for the given Rule Id.
        /// </summary>
        /// <param name="ruleId">The Id of the RuleWithSettingsMetadata that we want to get</param>
        /// <returns>The RuleWithSettingsMetadata from the rulesList for the given index</returns>
        public JObject GetRuleByID(string ruleId)
        {
            for (int i = 0; i < rulesList.Count; i++)
            {
                if (rulesList[i]["id"].Value<string>() == ruleId)
                {
                    return (JObject) rulesList[i];
                }
            }

            return new JObject();
        }

        /// <summary>
        /// Sets the the current environment ID name.
        /// </summary>
        /// <param name="currentEnvironment">Current Environment object containing the current environment name and ID</param>
        public void SetCurrentEnvironment(JObject currentEnvironment)
        {
            currentEnvironmentName = currentEnvironment["name"].Value<string>();
            currentEnvironmentId = currentEnvironment["id"].Value<string>();
            currentEnvironmentIsDefault = false;
            if (currentEnvironment["isDefault"] != null)
            {
                currentEnvironmentIsDefault = currentEnvironment["isDefault"].Value<bool>();
            }

            EnvironmentChanged?.Invoke();
        }

        /// <summary>
        /// Sets the default environment.
        /// </summary>
        /// <param name="defaultEnvironmentId">default Environment ID</param>
        public void SetDefaultEnvironment(string defaultEnvironmentId)
        {
            for (var i=0; i < environments.Count; i++)
            {
                ((JObject)environments[i])["isDefault"] = environments[i]["id"].Value<string>() == defaultEnvironmentId;
            }

            // if current environment became default, update the isDefault flag
            if (currentEnvironmentId == defaultEnvironmentId)
            {
                currentEnvironmentIsDefault = true;
            }

            CheckEnvironmentsValid();
        }

        /// <summary>
        /// Checks if set of environments is valid. There must be exactly one default environment.
        /// </summary>
        public void CheckEnvironmentsValid()
        {
            if (environments.Count < 1)
            {
                Debug.Log("Please create at least one environment");
            }

            var defaultEnvironmentsCount = environments.Count((e) => {
                if (((JObject)e)["isDefault"] != null)
                {
                    return e["isDefault"].Value<bool>();
                }
                return false;
            });
            if (defaultEnvironmentsCount < 1)
            {
                Debug.Log("Please set environment as default");
            }
            if (defaultEnvironmentsCount > 1)
            {
                Debug.LogWarning("Something went wrong. More than one default environment");
            }

            var environmentsWithNoName = environments.Where(e => e["name"].Value<string>() == "");
            for (var i = 0; i < environmentsWithNoName.Count(); i++)
            {
                Debug.LogWarning($"Environment with id: {environments.ElementAt(i)["id"].Value<string>()} has an empty name");
            }
        }

        /// <summary>
        /// Returns the name of the last selected environment that is stored in EditorPrefs.
        /// </summary>
        /// <param name="defaultEnvironment"> The default environment name to be returned if last selected environment is not found</param>
        /// <returns> Name of last selected environment or defaultEnvironment if last selected is not found</returns>
        public string RestoreLastSelectedEnvironment(string defaultEnvironment)
        {
            return EditorPrefs.GetString(k_CurrentEnvironment + Application.cloudProjectId, defaultEnvironment);
        }

        /// <summary>
        /// Sets the name of the last selected environment and stores it in EditorPrefs.
        /// </summary>
        /// <param name="environmentName"> Name of environment to be stored</param>
        public void SetLastSelectedEnvironment (string environmentName)
        {
            EditorPrefs.SetString(k_CurrentEnvironment + Application.cloudProjectId, environmentName);
        }

        /// <summary>
        /// Checks to see if any rules exist
        /// </summary>
        /// <returns>true if there is at leave one rule and false if there are no rules</returns>
        public bool HasRules()
        {
            return rulesList == null ? false : rulesList.Count > 0;
        }

        /// <summary>
        /// Checks if the given setting is being used by the given rule
        /// </summary>
        /// <param name="ruleId">ID of the rule that needs to be checked</param>
        /// <param name="rsEntityId">EntityId of the setting that needs to be checked</param>
        /// <returns>true if the given setting is being used by the given rule</returns>
        public bool IsSettingInRule(string ruleId, string rsEntityId)
        {
            for(int i = 0; i < rulesList.Count; i++)
            {
                if(rulesList[i]["id"].Value<string>() == ruleId)
                {
                    var settings = (JArray)rulesList[i]["value"];
                    for(int j = 0; j < settings.Count; j++)
                    {
                        if(settings[j]["metadata"]["entityId"].Value<string>() == rsEntityId)
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Returns list of settings for particular rule
        /// </summary>
        /// <param name="ruleId">ID of the rule </param>
        /// <returns>list of settings used by the given rule</returns>
        public JArray GetSettingsListForRule(string ruleId)
        {
            var settingsInRule = new JArray();
            for (int i = 0; i < rulesList.Count; i++)
            {
                if (rulesList[i]["id"].Value<string>() == ruleId)
                {
                    settingsInRule = (JArray)rulesList[i]["value"];
                }
            }
            return settingsInRule;
        }

        bool IsSegmentationRule(JObject rule)
        {
            return rule["type"].Value<string>() == "segmentation";
        }

        bool IsVariantRule(JObject rule)
        {
            return rule["type"].Value<string>() == "variant";
        }
        public void UpdateRule(JObject oldRule, JObject newRule)
        {
            if (IsVariantRule(oldRule))
            {
                for(int i = 0; i < rulesList.Count; i++)
                {
                    if(rulesList[i]["id"].Value<string>() == oldRule["id"].Value<string>())
                    {
                        rulesList[i] = newRule;
                    }
                }
            }
            AddRuleToUpdatedRuleIDs(newRule["id"].Value<string>());
            RulesDataStoreChanged?.Invoke();
        }

        private void AddRuleToUpdatedRuleIDs(string updatedRule)
        {
            //this is a new rule, do nothing - the changes will get picked up the add rule request
            if (!addedRulesIDs.Contains(updatedRule) && !updatedRulesIDs.Contains(updatedRule))
            {
                updatedRulesIDs.Add(updatedRule);
            }
        }

        /// <summary>
        /// Removes the given rule ID from the list of added rules ID's.
        /// </summary>
        /// <param name="ruleId">ID of the rule to be removed from the list of added rule ID's</param>
        public void RemoveRuleFromAddedRuleIDs(string ruleId)
        {
            addedRulesIDs.Remove(ruleId);
        }

        /// <summary>
        /// Removes the given rule ID from the list of updated rule ID's.
        /// </summary>
        /// <param name="ruleId">ID of the rule to be removed from the list of updated rule ID's</param>
        public void RemoveRuleFromUpdatedRuleIDs(string ruleId)
        {
            updatedRulesIDs.Remove(ruleId);
        }

        /// <summary>
        /// Removes the given rule ID from the list of deleted rule ID's.
        /// </summary>
        /// <param name="ruleId">ID of the rule to be remove from the list of deleted rule ID's</param>
        public void RemoveRuleFromDeletedRuleIDs(string ruleId)
        {
            deletedRulesIDs.Remove(ruleId);
        }

        /// <summary>
        /// Clears the list of added rule ID's, list of updated rule ID's, and the list of deleted rule ID's.
        /// </summary>
        public void ClearUpdatedRulesLists()
        {
            if (addedRulesIDs != null) { addedRulesIDs.Clear(); }
            if (updatedRulesIDs != null) { updatedRulesIDs.Clear(); }
            if (deletedRulesIDs != null) { deletedRulesIDs.Clear(); }
        }

        /// <summary>
        /// Adds a rule to the Rules data store. This will add it to the rulesList.
        /// </summary>
        /// <param name="newRule">The RuleWithSettingsMetadata to be added</param>
        public void AddRule(JObject newRule)
        {
            rulesList.Add(newRule);
            RulesDataStoreChanged?.Invoke();
            RemoteSettingDataStoreChanged?.Invoke();
            AddRuleToAddedRuleIDs(newRule);
        }

        private void AddRuleToAddedRuleIDs(JObject newRule)
        {
            addedRulesIDs.Add(newRule["id"].Value<string>());
        }

        /// <summary>
        /// Deletes a rule from the Rules data store. This will delete it from the rulesList.
        /// </summary>
        /// <param name="deletedRuleID">ID of the Rule to be deleted</param>
        public void DeleteRule(string deletedRuleID)
        {
            AddRuleToDeletedRuleIDs(deletedRuleID);
            for(int i = 0; i < rulesList.Count; i++)
            {
                if(rulesList[i]["id"].Value<string>() == deletedRuleID)
                {
                    rulesList.RemoveAt(i);
                    break;
                }
            }
            RulesDataStoreChanged?.Invoke();
            RemoteSettingDataStoreChanged?.Invoke();
        }

        private void AddRuleToDeletedRuleIDs(string deletedRuleId)
        {
            bool ruleAdded = false;
            if (addedRulesIDs.Contains(deletedRuleId))
            {
                addedRulesIDs.Remove(deletedRuleId);
                ruleAdded = true;
            }

            if (updatedRulesIDs.Contains(deletedRuleId))
            {
                updatedRulesIDs.Remove(deletedRuleId);
            }

            if (!ruleAdded)
            {
                deletedRulesIDs.Add(deletedRuleId);
            }
        }

        /// <summary>
        /// Checks to see if the given Rule's attributes are within the accepted range.
        /// </summary>
        /// <param name="rule">RuleWithSettingsMetadata object to be validated</param>
        /// <returns>true if the rule is valid and false if the rule is not valid</returns>
        public bool ValidateRule(JObject rule)
        {
            if (ValidateRulePriority(rule) && ValidateRuleName(rule))
            {
                dataStoreStatus = m_DataStoreStatus.UnSynchronized;
                return true;
            }
            dataStoreStatus = m_DataStoreStatus.Error;
            return false;
        }

        /// <summary>
        /// Checks to see if the given rule's name is valid.
        /// </summary>
        /// <param name="rule">RuleWithSettingsMetadata object to be validated</param>
        /// <returns>true if the rule's name is valid, false if it is not valid</returns>
        public bool ValidateRuleName(JObject rule)
        {
            for(int i = 0; i < rulesList.Count; i++)
            {
                if (rulesList[i]["id"].Value<string>() != rule["id"].Value<string>() && rulesList[i]["name"].Value<string>() == rule["name"].Value<string>())
                {
                    Debug.LogWarning(rulesList[i]["name"].Value<string>() + " already exists. Rule names must be unique.");
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Checks to see if the given rule's priority is valid.
        /// </summary>
        /// <param name="rule">RuleWithSettingsMetadata object to be validated</param>
        /// <returns>true if the rule's priority is valid, false if it is not valid</returns>
        public bool ValidateRulePriority(JObject rule)
        {
            if (rule["priority"].Value<int>() < 0 || rule["priority"].Value<int>() > 1000)
            {
                Debug.LogWarning("Rule: " + rule["name"].Value<string>() + " has an invalid priority. The set priority is " + rule["priority"].Value<int>() + ". The values for priority must be between " + minRulePriority + " and " + maxRulePriority);
                return false;
            }
            return true;
        }

        /// <summary>
        /// Updates the attributes for a given rule. This will update the rule in the rulesList.
        /// </summary>
        /// <param name="ruleId">ID of the rule being updated</param>
        /// <param name="newRule">RuleWithSettingsMetadata object containing the new attributes</param>
        public void UpdateRuleAttributes(string ruleId, JObject newRule)
        {
            if (ValidateRule(newRule))
            {
                for(int i = 0; i < rulesList.Count; i++)
                {
                    if(rulesList[i]["id"].Value<string>() == ruleId)
                    {
                        rulesList[i] = newRule;
                        break;
                    }
                }
                RulesDataStoreChanged?.Invoke();
                AddRuleToUpdatedRuleIDs(ruleId);
            }
        }

        /// <summary>
        /// Updates the type of a given rule.
        /// </summary>
        /// <param name="ruleId">ID of the rule being updated</param>
        /// <param name="newType">New type for the rule</param>
        public void UpdateRuleType(string ruleId, string newType)
        {
            // see which type is being changed to.
            // if going from segmentation to variant, need to change datastructure
            // if going from variant to segmentation, need to blow away variant structure and move to old structure

            // find the rule first

            for(int i = 0; i < rulesList.Count; i++)
            {
                if(rulesList[i]["id"].Value<string>() == ruleId)
                {
                    var ruleObj = rulesList[i];
                    var oldType = ruleObj["type"].Value<string>();

                    if(oldType == "segmentation" && newType == "variant")
                    {
                        var oldValue = (JArray)ruleObj["value"];
                        oldValue.Parent.Remove();
                        var newVariant = new JObject();
                        newVariant["id"] = "variant-1";
                        newVariant["type"] = newType;
                        newVariant["weight"] = null;
                        newVariant["values"] = oldValue;
                        var ruleValueJArray = new JArray();
                        ruleValueJArray.Add(newVariant);
                        ruleObj["value"] = ruleValueJArray;
                        ruleObj["type"] = newType;
                    }
                    else if(oldType == "variant" && newType == "segmentation")
                    {
                        if (!EditorUtility.DisplayDialog("Switching rule type from 'variant' to 'segmentation'",
                        "This action will remove your current variants. \nDo you want to proceed?", "Yes", "No"))
                        {
                            return;
                        }
                        var oldValue = (JArray)ruleObj["value"];
                        oldValue.Parent.Remove();
                        var newValues = (JArray)oldValue[0]["values"];
                        ruleObj["type"] = newType;
                        ruleObj["value"] = newValues;
                    }
                    RulesDataStoreChanged?.Invoke();
                    AddRuleToUpdatedRuleIDs(ruleId);
                }
            }
        }

        /// <summary>
        /// Enables or disables the given rule.
        /// </summary>
        /// <param name="ruleId">ID of Rule to be enabled or disabled</param>
        /// <param name="enabled">true = enabled, false = disabled</param>
        public void EnableOrDisableRule(string ruleId, bool enabled)
        {
            for(int i = 0; i < rulesList.Count; i++)
            {
                if(rulesList[i]["id"].Value<string>() == ruleId)
                {
                    rulesList[i]["enabled"] = enabled;
                    break;
                }
            }
            AddRuleToUpdatedRuleIDs(ruleId);
            RulesDataStoreChanged?.Invoke();
        }

        /// <summary>
        /// Adds the given setting to the given rule.
        /// </summary>
        /// <param name="selectedRuleId">ID of the rule that the setting should be added to</param>
        /// <param name="entityId">EntityId of the setting to be added to the given rule</param>
        public void AddSettingToRule(string selectedRuleId, string entityId)
        {
            if(IsSettingInRule(selectedRuleId, entityId))
            {
                return;
            }

            for (int i = 0; i < rulesList.Count; i++)
            {
                if (rulesList[i]["id"].Value<string>() == selectedRuleId)
                {
                    var currentRuleSettings = (JArray)rulesList[i]["value"];
                    for (int j = 0; j < rsKeyList.Count; j++)
                    {
                        if (rsKeyList[j]["metadata"]["entityId"].Value<string>() == entityId)
                        {
                            currentRuleSettings.Add(rsKeyList[j]);
                        }
                    }
                }
            }
            RemoteSettingDataStoreChanged?.Invoke();
            AddRuleToUpdatedRuleIDs(selectedRuleId);
        }

        /// <summary>
        /// Deletes the given setting to the given Rule.
        /// </summary>
        /// <param name="ruleId">ID of the rule that the setting should be deleted from</param>
        /// <param name="entityId">EntityId of the setting to be deleted from the given rule</param>
        public void DeleteSettingFromRule(string ruleId, string entityId)
        {
            if(!IsSettingInRule(ruleId, entityId))
            {
                return;
            }

            for (int i = 0; i < rulesList.Count; i++)
            {
                if (rulesList[i]["id"].Value<string>() == ruleId)
                {
                    var currentRuleSettings = (JArray)rulesList[i]["value"];
                    for (int j = 0; j < currentRuleSettings.Count; j++)
                    {
                        if (currentRuleSettings[j]["metadata"]["entityId"].Value<string>() == entityId)
                        {
                            currentRuleSettings.Remove(currentRuleSettings[j]);
                        }
                    }
                }
            }
            RemoteSettingDataStoreChanged?.Invoke();
            AddRuleToUpdatedRuleIDs(ruleId);
        }

        /// <summary>
        /// Updates the value of the given setting for the given rule.
        /// </summary>
        /// <param name="ruleId">ID of the rule that the updated setting belong to</param>
        /// <param name="updatedSetting">A RsKvtData containing the updated value</param>
        public void UpdateSettingForRule(string ruleId, JObject updatedSetting)
        {
            for(int i = 0; i < rulesList.Count; i++)
            {
                if(rulesList[i]["id"].Value<string>() == ruleId)
                {
                    var tempArr = (JArray)rulesList[i]["value"];
                    for(int j = 0; j < tempArr.Count; j++)
                    {
                        if(tempArr[j]["metadata"]["entityId"].Value<string>() == updatedSetting["metadata"]["entityId"].Value<string>())
                        {
                            tempArr[j] = updatedSetting;
                        }
                    }
                }
            }
            RemoteSettingDataStoreChanged?.Invoke();
            AddRuleToUpdatedRuleIDs(ruleId);
        }

        /// <summary>
        /// Adds a setting to the Remote Settings data store. This will add the setting to the rsKeyList.
        /// </summary>
        /// <param name="newSetting">The setting to be added</param>
        public void AddSetting(JObject newSetting)
        {
            rsKeyList.Add(newSetting);
            RemoteSettingDataStoreChanged?.Invoke();
        }

        /// <summary>
        /// Deletes a setting from the Remote Settings data store. This will delete the setting from the rsKeyList.
        /// </summary>
        /// <param name="entityId">The EntityId of the setting to be deleted</param>
        public void DeleteSetting(string entityId)
        {
            for(int i = 0; i < rsKeyList.Count; i++)
            {
                if (rsKeyList[i]["metadata"]["entityId"].Value<string>() == entityId)
                {
                    rsKeyList.RemoveAt(i);
                    break;
                }
            }
            //m_DataStore.rsKeyList.Remove(m_DataStore.rsKeyList.Find(s => s.metadata.entityId == entityId));
            RemoteSettingDataStoreChanged?.Invoke();
        }

        /// <summary>
        /// Updates a setting in the Remote Settings data store. This will update the setting in the rsKeyList.
        /// </summary>
        /// <param name="oldSetting">The RsKvtData of the setting to be updated</param>
        /// <param name="newSetting">The new setting with the updated fields</param>
        public void UpdateSetting(JObject oldSetting, JObject newSetting)
        {
            var isError = false;
            if (newSetting["rs"]["key"].Value<string>().Length >= 255)
            {
                Debug.LogWarning(newSetting["rs"]["key"].Value<string>() + " is at the maximum length of 255 characters.");
                isError = true;
            }
            if (!isError)
            {
                for (int i = 0; i < rsKeyList.Count; i++)
                {
                    if (rsKeyList[i]["metadata"]["entityId"].Value<string>() == oldSetting["metadata"]["entityId"].Value<string>())
                    {
                        rsKeyList[i] = newSetting;
                    }
                }
                RemoteSettingDataStoreChanged?.Invoke();
            }
        }

        public void UpdateRuleId(string oldRuleId, string newRuleId)
        {
            var rule = GetRuleByID(oldRuleId);
            rule["id"] = newRuleId;
            RulesDataStoreChanged?.Invoke();
        }

    }
}