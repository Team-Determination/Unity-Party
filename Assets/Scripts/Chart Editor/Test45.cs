using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

public class Test45 : MonoBehaviour
{
    public Image img;
    private void OnBecameInvisible() {
        img.enabled = false;
    }

    private void OnBecameVisible() {
        img.enabled = true;
    }
}
