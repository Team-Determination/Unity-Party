using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using UnityEditor;
using System.IO;
using UnityEngine;

namespace Unity.RemoteConfig.Editor
{
    internal static class RemoteConfigUtilities
    {
        const string k_DataStoreAssetFileName = "{0}.asset";
        const string k_DataStoreName = "RemoteConfigDataStoreAsset";
        const string k_PathToDataStore = "Assets/Editor/RemoteConfig/Data";

        /// <summary>
        /// compares two lists of jObjects
        /// </summary>
        /// <param name="objectListNew">first list to compare </param>
        /// <param name="objectListOld">second list to compare </param>
        /// <returns>true if lists are equal</returns>
        public static bool CompareJArraysEquality(JArray objectListNew, JArray objectListOld)
        {
            if (objectListOld.Count != objectListNew.Count)
            {
                return false;
            }
            for (var i = 0; i < objectListNew.Count; i++)
            {
                if (!JToken.DeepEquals(objectListNew[i], objectListOld[i]))
                {
                    return false;
                }
            }
            return true;
        }

        private static RemoteConfigDataStore m_DataStore;

        public static RemoteConfigDataStore DataStore
        {
            get
            {
                if (m_DataStore == null)
                {
                    m_DataStore = CheckAndCreateDataStore();
                }
                return m_DataStore;
            }
            set { m_DataStore = value; }
        }

        /// <summary>
        /// Checks for the existence of the Remote Config data store. Creates a new data store if one doesn't already exist
        /// and saves it to the AssetDatabase.
        /// </summary>
        /// <returns>Remote Config data store object</returns>
        public static RemoteConfigDataStore CheckAndCreateDataStore()
        {
            string formattedPath = Path.Combine(k_PathToDataStore, string.Format(k_DataStoreAssetFileName, k_DataStoreName));
            if (AssetDatabase.FindAssets(k_DataStoreName).Length > 0)
            {
                if (AssetDatabase.LoadAssetAtPath(formattedPath, typeof(RemoteConfigDataStore)) == null)
                {
                    AssetDatabase.DeleteAsset(formattedPath);
                }
            }
            if (AssetDatabase.FindAssets(k_DataStoreName).Length == 0)
            {
                RemoteConfigDataStore asset = InitDataStore();
                CheckAndCreateAssetFolder(k_PathToDataStore);
                AssetDatabase.CreateAsset(asset, formattedPath);
                AssetDatabase.SaveAssets();
            }
            return AssetDatabase.LoadAssetAtPath(formattedPath, typeof(RemoteConfigDataStore)) as RemoteConfigDataStore;
        }

        private static RemoteConfigDataStore InitDataStore()
        {
            var asset = ScriptableObject.CreateInstance<RemoteConfigDataStore>();
            asset.rsKeyList = new JArray();
            asset.currentEnvironmentName = "Please create an environment.";
            asset.currentEnvironmentId = null;
            asset.environments = new JArray();
            asset.rulesList = new JArray();
            asset.lastCachedRulesList = new JArray();
            asset.rsLastCachedKeyList = new JArray();
            asset.addedRulesIDs = new List<string>();
            asset.updatedRulesIDs = new List<string>();
            asset.deletedRulesIDs = new List<string>();

            return asset;
        }

        private static void CheckAndCreateAssetFolder(string dataStorePath)
        {
            string[] folders = dataStorePath.Split('/');
            string assetPath = null;
            foreach (string folder in folders)
            {
                if (assetPath == null)
                {
                    assetPath = folder;
                }
                else
                {
                    string folderPath = Path.Combine(assetPath, folder);
                    if (!Directory.Exists(folderPath))
                    {
                        AssetDatabase.CreateFolder(assetPath, folder);
                    }
                    assetPath = folderPath;
                }
            }
        }


    }
}