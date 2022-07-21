using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(fileName = "New Week",menuName = "Create New Week")]
public class Week : ScriptableObject
{
    public string weekName;
    public WeekSong[] songs;
}

