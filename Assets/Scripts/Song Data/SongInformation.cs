using System;
using System.Collections.Generic;

[Serializable]

public class Note
{
    public int lengthInSteps = 16;
    public float bpm = 180; 
    public bool changeBpm;
    public bool mustHitSection;
    public List<List<double>> sectionNotes;
    public int typeOfSection;

    public Note(List<List<double>> sectionNotes)
    {
        this.sectionNotes = sectionNotes;
    }
}
[Serializable]

public class SongCore
{
    public string song;
    public List<Note> notes;
    public float bpm;
    public int sections;
    public bool needsVoices;
    public string player1;
    public string player2;
    public List<object> sectionLengths;
    public float speed;
    public bool validScore;
}
[Serializable]

public class SongInformation
{
    public string song;
    public float bpm;
    public int sections;
    public List<Note> notes;
}
