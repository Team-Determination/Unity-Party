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

[CreateAssetMenu(fileName = "New Song",menuName = "Create New Song")]
public class WeekSong : ScriptableObject
{
    public string songName;
    public string sceneName;
    
    [Space]
    public TextAsset chart;
    public AudioClip instrumentals;
    public AudioClip vocals;
    
}