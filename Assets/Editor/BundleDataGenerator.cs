using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using UnityEngine;
using UnityEditor;

public class BundleDataGenerator : EditorWindow
{
    public string bundleName = "Week 1";
    public string authorName = "Kawai Sprite";
    
    [MenuItem("Window/Bundle Meta Data Generator")]
    public static void Init()
    {
        BundleDataGenerator generator = (BundleDataGenerator)EditorWindow.GetWindow(typeof(BundleDataGenerator));
        generator.Show();
    }

    private void OnGUI()
    {
        
        
        GUILayout.Label("Bundle Meta Data Generator", EditorStyles.boldLabel);

        GUILayout.Space(10);
        
        bundleName = EditorGUILayout.TextField("Bundle Name", bundleName);
        authorName = EditorGUILayout.TextField("Creator Name", authorName);
        

        GUILayout.Space(10);
 

        if (GUILayout.Button("Generate Bundle Meta Data"))
            GenerateMetaData();

    }

    private void GenerateMetaData()
    {
        string metaJson = JsonConvert.SerializeObject(new BundleMeta()
        {
            bundleName = bundleName,
            authorName = authorName
        });

        string filePath = EditorUtility.SaveFilePanel("Save Bundle Meta Data", Application.persistentDataPath, "bundle-meta",
            "json");

        if (filePath.Length == 0) return;
        File.WriteAllText(filePath, metaJson);
        AssetDatabase.Refresh();
    }
}
