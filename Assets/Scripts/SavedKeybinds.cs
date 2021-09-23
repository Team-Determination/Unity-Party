using System;
using UnityEngine;

[Serializable]
public class SavedKeybinds
{
    public KeyCode primaryLeftKeyCode = KeyCode.A;
    public KeyCode primaryDownKeyCode = KeyCode.S;
    public KeyCode primaryUpKeyCode = KeyCode.W;
    public KeyCode primaryRightKeyCode = KeyCode.D;
    public KeyCode secondaryLeftKeyCode = KeyCode.LeftArrow;
    public KeyCode secondaryDownKeyCode = KeyCode.DownArrow;
    public KeyCode secondaryUpKeyCode = KeyCode.UpArrow;
    public KeyCode secondaryRightKeyCode = KeyCode.RightArrow;
    public KeyCode pauseKeyCode = KeyCode.Return;
    public KeyCode resetKeyCode = KeyCode.R;
}