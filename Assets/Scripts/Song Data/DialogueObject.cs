using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[Serializable]
public class DialogueObject
{
    [TextArea(3,6)]
    public string dialog;

    public Sprite portrait;
}
