using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

[System.Serializable]
public class NoteController : MonoBehaviour
{
    public NoteObjectEditor note;
    public Minisection parentMinisection;

    private void Start() {
        parentMinisection = GetComponentInParent<Minisection>();
    }

    public NoteObjectEditor GetNote() {
        return note;
    }

    public void SetState() {
        note.gameObject.SetActive(!note.gameObject.activeSelf);
        parentMinisection.Callback(note);
    }

    public void SetStateInternally(bool state) {
        note.gameObject.SetActive(state);
        parentMinisection.Callback(note);
    }

    public NoteObjectEditor ChangeColor(Color color) {
        note.gameObject.GetComponent<Image>().color = color;
        parentMinisection.Callback(note);
        return note;
    }

    public NoteObjectEditor ChangeSprite(Sprite sprite) {
        note.gameObject.GetComponent<Image>().sprite = sprite;
        parentMinisection.Callback(note);
        return note;
    }

    public NoteObjectEditor ChangeOffset(Vector2 newOffset) {
        note.gameObject.transform.localPosition = newOffset;
        parentMinisection.Callback(note);
        return note;
    }

    public NoteObjectEditor SetNoteData(NoteObjectEditor newData) {
        note = newData;
        parentMinisection.Callback(note);
        return note;
    } 
}
