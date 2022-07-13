using System;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

[Serializable]
public class SongMetaV2
{
    [JsonProperty("Song Name")]
    public string songName;
    [JsonProperty("Song Credits")]
    public Dictionary<string, string> credits;
    [JsonProperty("Song Difficulties")]
    public Dictionary<string, Color> difficulties;
    [JsonProperty("Song Description")]
    public string songDescription;
    [JsonProperty("Has Cutscene")]
    public bool hasCutscene = false;
    [JsonProperty("Has Dialogue")]
    public bool hasDialogue = false;
    [JsonProperty("Has Custom Notes")]
    public bool hasCustomNotes = false;
    [JsonProperty("Custom Notes Name")]
    public List<string> customNotesName = new List<string>();
    
    //NOT SERIALIZED
    [JsonIgnore] public string songPath;
    [JsonIgnore] public Sprite songCover;
    [JsonIgnore] public BundleMeta bundleMeta;
    [JsonIgnore] public bool isFromModPlatform;
    [JsonIgnore] public string modURL;
}