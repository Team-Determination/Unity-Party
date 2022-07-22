using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NoteHelper : MonoBehaviour
{
    public GameObject noteObject;
    public void OnMouseDown() {
        noteObject.SetActive(!noteObject.activeSelf);
    }
}
