using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class SongNotes
{
    public int lengthInSteps = 16;
    public float bpm = 180;
    public bool changeBPM;
    public List<List<double>> sectionNotes;
    public int typeOfSection;
}
