using System;
using System.Collections;
using System.Collections.Generic;
using SFB;
using UnityEngine;

public class SceneEditor : MonoBehaviour
{
    [Header("Placing Mode")] public GameObject placingImgScreen;

    public GameObject placedImgTemplate;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    public void PlaceImage()
    {
        placingImgScreen.SetActive(true);
        StandaloneFileBrowser.OpenFilePanelAsync("Open Image", Environment.SystemDirectory, "png", false, paths =>
        {
            
        });
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
