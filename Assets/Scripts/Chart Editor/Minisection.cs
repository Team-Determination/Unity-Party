using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Minisection : MonoBehaviour
{
    public List<NoteController> controllers;
    public List<NoteObjectEditor> notes = new List<NoteObjectEditor>();
    public Section parentSection;

    private void Start() {
        parentSection = GetComponentInParent<Section>();
        foreach (NoteController controller in controllers) {
            notes.Add(controller.note);
        }
    }

    public void Callback(NoteObjectEditor note) {
        parentSection.Callback(note, this, notes.IndexOf(note));
    }
}
