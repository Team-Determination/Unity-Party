using System;
using UnityEngine;

namespace Unity.RemoteConfig.Editor.UIComponents
{
    internal class MVRuleJsonModal : SettingsJsonModal
    {
        public static MVRuleJsonModal _instance;
        public int currentVariantIndex;
        public static new MVRuleJsonModal CreateInstance(string jsonString = "", string keyName = "", string jsonKeyEntityId = "", Type parentType = null)
        {
            if (_instance == null)
            {
                _instance = ScriptableObject.CreateInstance<MVRuleJsonModal>();
                _instance.Init(jsonString , keyName, jsonKeyEntityId, parentType);
            }
            return _instance;
        }
    }
}