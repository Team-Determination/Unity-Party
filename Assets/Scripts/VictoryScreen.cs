using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using TMPro;
using UnityEngine;

public class VictoryScreen : MonoBehaviour
{
    public TMP_Text songNameText;

    public TMP_Text difficultyText;

    public TMP_Text scoreText;

    public TMP_Text accuracyText;

    public TMP_Text missesText;
    public GameObject victoryScreen;

    public static VictoryScreen Instance;

    public void Start()
    {
        Instance = this;
    }

    public void ResetScreen()
    {
        Song song = Song.instance;
        var accuracy = (float)song.overallStats.hitNotes / song.overallStats.totalNoteHits;
        var accuracyPercent = (float) Math.Round(accuracy, 4);
        accuracyPercent *= 100;
            
        songNameText.text = song.weekData.weekName;
        difficultyText.text = song.difficulty == 1 ? "Normal" : "Hard";
        scoreText.text = $"Overall Score: {song.overallStats.currentScore}";
        accuracyText.text = $"Overall Accuracy: {accuracyPercent.ToString(CultureInfo.CurrentCulture)}%";
        missesText.text = $"Overall Misses: {song.overallStats.missedHits}";
    }

    public void ExitScreen()
    {
        Song song = Song.instance;
        ScreenTransition.instance.StartTransition(song.menuScreen,victoryScreen);

        song.musicSources[0].clip = song.menuClip;
        song.musicSources[0].loop = true;
        song.musicSources[0].volume = Options.menuVolume;
        song.musicSources[0].Play();
    }
}
