using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class SavedKeybinds
{
    public List<KeyCode> primary4K = new List<KeyCode> { KeyCode.LeftArrow,KeyCode.DownArrow,KeyCode.UpArrow,KeyCode.RightArrow};
    public List<KeyCode> secondary4K = new List<KeyCode> { KeyCode.A, KeyCode.S, KeyCode.W, KeyCode.D};

    public KeyCode pauseKeyCode;
    public KeyCode resetKeyCode;
    public KeyCode startSongKeyCode;
}