using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class SavedKeybinds
{
    public List<KeyCode> primary4K = new List<KeyCode>
    {
        KeyCode.A,KeyCode.S,KeyCode.W,KeyCode.D
    };
    public List<KeyCode> secondary4K = new List<KeyCode>
    {
        KeyCode.LeftArrow,KeyCode.DownArrow,KeyCode.UpArrow,KeyCode.RightArrow
    };

    public KeyCode pauseKeyCode = KeyCode.Return;
    public KeyCode resetKeyCode = KeyCode.R;
    public KeyCode startSongKeyCode = KeyCode.Space;
}