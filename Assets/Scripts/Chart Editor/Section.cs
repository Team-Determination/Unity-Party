using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Section : MonoBehaviour
{
    public List<Minisection> minisections = new List<Minisection>();
    public List<bool> boolers = new List<bool>() { 
        true,
        true,
        true,
        true,

        true,
        true,
        true,
        true,

        true,
        true,
        true,
        true,

        true,
        true,
        true,
        true,

        true,
        true,
        true,
        true,

        true,
        true,
        true,
        true,

        true,
        true,
        true,
        true,

        true,
        true,
        true,
        true,

        true,
        true,
        true,
        true,

        true,
        true,
        true,
        true,

        true,
        true,
        true,
        true,

        true,
        true,
        true,
        true,

        true,
        true,
        true,
        true,

        true,
        true,
        true,
        true,

        true,
        true,
        true,
        true,

        true,
        true,
        true,
        true,
    };


    public void Callback(NoteObjectEditor note, Minisection minisection, int indexOfNote) {
        boolers[indexOfNote + minisections.IndexOf(minisection) * 4] = note.gameObject.activeSelf;
    }

    private void Update() {
        
    }
}
