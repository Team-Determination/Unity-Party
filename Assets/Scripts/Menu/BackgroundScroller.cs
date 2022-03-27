using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BackgroundScroller : MonoBehaviour
{
    public Vector2 newPos;

    public float time;

    // Start is called before the first frame update
    public void MoveBackground()
    {
        
        LeanTween.move(gameObject, newPos, time).setLoopClamp();
    }
    
    // Update is called once per frame
    void Update()
    {
        
    }
}
