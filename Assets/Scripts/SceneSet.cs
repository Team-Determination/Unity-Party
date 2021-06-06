using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneSet : MonoBehaviour
{
    private bool _initialized;
    public bool hideGirlfriend;

    public float gameZoom;

    private void Update()
    {
        if (_initialized)
        {
            enabled = false;
            return;
        }
        if (Song.instance == null) return;
        Song.instance.girlfriend.SetActive(!hideGirlfriend);
        Song.instance.gameCamera.orthographicSize = gameZoom;
        Song.instance.defaultGameZoom = gameZoom;
        _initialized = true;
        
    }
}
