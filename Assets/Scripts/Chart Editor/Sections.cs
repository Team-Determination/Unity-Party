using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FridayNightFunkin;

public class Sections : MonoBehaviour
{
    public List<Section> sections = new List<Section>();
    public int currentSection = 0;
    public int sectionDiv = 1000;
    private void Start() {
        FNFSong song = new FNFSong("C:/Users/Raony Reis/AppData/LocalLow/Rei/FridayNight/Bundles/vs_qt.1/01 Censory-Overload/" + "Chart-hard.json");

        foreach (FNFSong.FNFNote note in song.Sections[4].Notes) {
            List<decimal> noteParsed = note.ConvertToNote();
            currentSection += 1;//(int)(noteParsed[0] / sectionDiv);
            sections[0].minisections[currentSection].controllers[(int)noteParsed[1]].SetStateInternally(true);
        }    
    }
}
