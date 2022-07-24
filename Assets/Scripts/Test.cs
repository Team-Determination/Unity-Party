using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Test : MonoBehaviour
{
    public SpriteRenderer renderer;

    public void Enter() {
        renderer.color = new Color(0.7f, 0.7f, 0.7f, 1);
    }

    public void Exit() {
        renderer.color = new Color(1f, 1f, 1f, 1);
    }

    public void Down() {
        
    }
}
