using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using UnityEngine;
using UnityEditor;

public class UpdateInfoGenerator : EditorWindow
{
    public string updateName = "No Updates?";
    public string updateExternalUri;
    public string updateImageBase = "";
    public string updateBody = "cool swag.";
    
    [MenuItem("Window/Update Info Generator")]
    public static void Init()
    {
        UpdateInfoGenerator generator = (UpdateInfoGenerator)EditorWindow.GetWindow(typeof(UpdateInfoGenerator));
        generator.Show();
    }

    private void OnGUI()
    {
        
        
        GUILayout.Label("Update Information Generator", EditorStyles.boldLabel);

        GUILayout.Space(10);
        
        updateName = EditorGUILayout.TextField("Update Name", updateName);
        updateImageBase = EditorGUILayout.TextField("Update Image Base", updateImageBase);
        updateExternalUri = EditorGUILayout.TextField("Update External URL", updateExternalUri);

        GUILayout.Space(10);

        
        GUILayout.Label("Update Body");

        
        updateBody = GUILayout.TextArea(updateBody, 500,GUILayout.Height(100));
        

        if (GUILayout.Button("Generate Update JSON"))
            GenerateJsonData();

    }

    private void GenerateJsonData()
    {
        string jsonData = JsonConvert.SerializeObject(new UpdateInformation
        {
            updateBody = updateBody,
            updateName = updateName,
            updateImageBase = updateImageBase,
            updateUri = updateExternalUri
        });

        GUIUtility.systemCopyBuffer = jsonData;
    }
}
