using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class ThinkScene : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        Song.instance.uiCamera.GetUniversalAdditionalCameraData().SetRenderer(1);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
