using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;
using System.IO;

public class CustomNoteParser : MonoBehaviour
{
    public static CustomNoteParser instance;

    private void Awake() {
        instance = this;
    }

    public List<CustomNote> ParseCustomNotes(string path, List<string> notesPathName) {
        List<CustomNote> customNotes = new List<CustomNote>();
        foreach (string notePath in notesPathName) {
            customNotes.Add(JsonConvert.DeserializeObject<CustomNote>(File.ReadAllText(Path.Combine(path, notePath, "note.json"))));
        }
        return customNotes;
    }
}


public class CustomNote {
    [JsonProperty("Name")]
    public string Name {
        get; set;
    }

    [JsonProperty("Xml File Name")]
    public string XmlFileName {
        get; set;
    }

    [JsonProperty("Png File Name")]
    public string PngFileName {
        get; set;
    }

    [JsonProperty("Event File Name")]
    public string EventFileName {
        get; set;
    }

    [JsonProperty("Ingnore Miss")]
    public bool IngnoreMiss {
        get; set;
    }

    [JsonProperty("Ingnore Hit")]
    public bool IngnoreHit {
        get; set;
    }

    [JsonProperty("Frames Name")]
    public List<string> FramesName {
        get; set;
    }

    [JsonProperty("On Hit")]
    public string OnHit {
        get; set;
    }

    [JsonProperty("On Miss")]
    public string OnMiss {
        get; set;
    }

    [JsonProperty("On Bot Hit")]
    public string OnBotHit {
        get; set;
    }

    [JsonIgnore] public Dictionary<string, Sprite> noteSprites = new Dictionary<string, Sprite>();
}

public class CustomNoteRoot {
    [JsonProperty("Custom Note")]
    public CustomNote CustomNote {
        get; set;
    }
}

