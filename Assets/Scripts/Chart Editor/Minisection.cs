using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

[System.Serializable]
public class Minisection : MonoBehaviour
{
    public List<GameObject> notesInSection;
    public Section currentSection;
    public int indexInSection;

    private void Start() {
        currentSection = transform.parent.gameObject.GetComponent<Section>();
    }

    public bool SetColorOnIndex(int index, Color color) {
        try {
            notesInSection[index].GetComponent<Image>().color = color;
            return true;
        } catch {
            return false;
        }
    }

    public bool SetActive(int index, bool trueOrFalse) {
        try {
            notesInSection[index].SetActive(trueOrFalse);
            currentSection.Callback("Edited Note", "Color", index, currentSection, notesInSection[index]);
            return true;
        } catch {
            return false;
        }
    }
}
