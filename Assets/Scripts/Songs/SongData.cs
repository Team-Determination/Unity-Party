using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(menuName = "Create Song",fileName = "New Song")]
public class SongData : ScriptableObject
{
    public string songName;
    
    [Space,TextArea(5,10)]
    public string normalData;
    [TextArea(5,10)]
    public string hardData;

    [Space] public AudioClip instrumentals;
    public AudioClip vocals;
    public AudioClip nikoVocals;
}