﻿using UnityEngine;
[CreateAssetMenu(menuName = "Create New Song", fileName = "Song")]
public class SongData : ScriptableObject
{
    public string songName;
    [TextArea(4,6)]
    public string normalData;
    [TextArea(4,6)]
    public string hardData;

    [Space] public DialogueData DialogueData;
    public AudioClip dialogueMusic;

    [Space] public AudioClip instrumentals;
    public AudioClip vocals;
    public AudioClip nikoVocals;
    public bool noNikoVocals;
[Space]
    public string sceneName = "Room";

    public float cameraZoom = 5;
}
