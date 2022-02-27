using System;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

[Serializable]
public class SongMeta
{
    public string songName;
    public string authorName;
    public string charterName;
    [JsonProperty("difficultyName")]
    public string difficultyName;
    [JsonProperty("difficultyColor")]
    public Color difficultyColor;
    
    
    public string songDescription;

    
    public int formatVersion = 2;
}
