using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ScenePropertiesEditor : MonoBehaviour
{
    public TMP_InputField zoomField;

    private CurrentEditingCharacter _currentEditingCharacter = CurrentEditingCharacter.None;

    public enum CurrentEditingCharacter
    {
        Protagonist = 1,
        Opponent = 2,
        Metronome = 3,
        None = 0
    }
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
