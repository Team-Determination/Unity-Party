using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class ThinkScene : MonoBehaviour
{
    private int _level = 0;
    public Camera errorCamera;
    public SpriteRenderer errorSprite;
    
    // Start is called before the first frame update
    void Start()
    {
        Song.instance.uiCamera.GetUniversalAdditionalCameraData().SetRenderer(1);
    }

    // Update is called once per frame
    void Update()
    {
        if (Song.instance.musicSources[0].time >= 123.717f)
        {
            errorCamera.enabled = true;
            Pause.instance.canPause = false;
            errorSprite.enabled = true;
        }
    }
}
