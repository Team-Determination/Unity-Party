using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class SavedKeybinds
{
    public List<KeyCode> primary4K = new();
    public List<KeyCode> secondary4K = new();

    public KeyCode pauseKeyCode = KeyCode.Return;
    public KeyCode resetKeyCode = KeyCode.R;
    public KeyCode startSongKeyCode = KeyCode.Space;
}