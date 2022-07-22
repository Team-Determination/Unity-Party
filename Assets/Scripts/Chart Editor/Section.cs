using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Section : MonoBehaviour
{
    public List<Minisection> minisections;
    public GameObject chartParent;

    private void Start() {
        chartParent = gameObject.transform.parent.gameObject;
    }

    public Minisection ModifyMinisection(int index) {
        try {
            return minisections[index];
        } catch {
            return null;
        }
    }

    public GameObject Callback(string callBackType, string callback, int index, Section section, GameObject gameObj) {
        return gameObj;
    }
}
