using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;
using System.IO;
using System.Linq;
using raonyreis13.Utils;
using Slowsharp;

public class CustomNoteParser : MonoBehaviour
{
    public static CustomNoteParser instance;

    private void Awake() {
        instance = this;
    }

    public Dictionary<int, CustomNote> ParseCustomNotes(string path, List<string> notesPathName) {
        Dictionary<int, CustomNote> customNotes = new Dictionary<int, CustomNote>();
        foreach (string notePath in notesPathName) {
            CustomNoteRoot cnn;
            cnn = JsonConvert.DeserializeObject<CustomNoteRoot>(File.ReadAllText(Path.Combine(path, "Custom Notes", notePath, "note.json")));
            customNotes.Add(cnn.CustomNote.IndexOf, cnn.CustomNote);
        }
        
        foreach (CustomNote customNote in customNotes.Values.ToList()) {
            customNote.noteSprites = new Dictionary<string, Sprite>();
            customNote.noteSprites.Add("Note Normal", UsefulFunctions.GetSprite(Path.Combine(path, "Custom Notes", notesPathName[customNotes.Values.ToList().IndexOf(customNote)], "sprites/Note Normal.png"), new Vector2(.5f, .5f), FilterMode.Trilinear, 100));
            customNote.noteSprites.Add("Hold Middle", UsefulFunctions.GetSprite(Path.Combine(path, "Custom Notes", notesPathName[customNotes.Values.ToList().IndexOf(customNote)], "sprites/Hold Middle.png"), new Vector2(.5f, .5f), FilterMode.Trilinear, 100));
            customNote.noteSprites.Add("Hold End", UsefulFunctions.GetSprite(Path.Combine(path, "Custom Notes", notesPathName[customNotes.Values.ToList().IndexOf(customNote)], "sprites/Hold End.png"), new Vector2(.5f, .5f), FilterMode.Trilinear, 100));
            customNote.noteScript = CScript.CreateRunner(File.ReadAllText(Path.Combine(path, "Custom Notes", notesPathName[customNotes.Values.ToList().IndexOf(customNote)], customNote.EventFileName))).Instantiate("NoteScript");
        }
        return customNotes;
    }
}

[System.Serializable]
public class CustomNote {
    [JsonProperty("Name")]
    public string Name {
        get; set;
    }

    [JsonProperty("IndexOf")]
    public int IndexOf {
        get; set;
    }

    [JsonProperty("Xml File Name")]
    public string XmlFileName {
        get; set;
    } = "note.xml";

    [JsonProperty("Png File Name")]
    public string PngFileName {
        get; set;
    } = "note.png";

    [JsonProperty("Event File Name")]
    public string EventFileName {
        get; set;
    } = "note.csx";

    [JsonProperty("Ignore Miss")]
    public bool IgnoreMiss {
        get; set;
    }

    [JsonProperty("Ignore Hit")]
    public bool IgnoreHit {
        get; set;
    }

    [JsonProperty("Ignore Player Colors")]
    public bool IgnorePlayerColors {
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
    [JsonIgnore] public HybInstance noteScript = null;
}

[System.Serializable]
public class CustomNoteRoot {
    [JsonProperty("Custom Note")]
    public CustomNote CustomNote {
        get; set;
    }
}

