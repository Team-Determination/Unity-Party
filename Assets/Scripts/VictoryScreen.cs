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
        var overallStats = song.overallStats;
        float sickScore = overallStats.totalSicks * 4;
        float goodScore = overallStats.totalGoods * 3;
        float badScore = overallStats.totalBads * 2;
        float shitScore = overallStats.totalShits;

        float totalAccuracyScore = sickScore + goodScore + badScore + shitScore;
        var accuracy = totalAccuracyScore / (overallStats.totalNoteHits * 4);
        var accuracyPercent = (float) Math.Round(accuracy, 4);
        accuracyPercent *= 100;

        float finalScore = overallStats.currentScore * GetScoreMultiplier();
        finalScore = Mathf.Round(finalScore);
            
        songNameText.text = song.freeplay ? song.freeplaySong.songName : song.weekData.weekName;
        difficultyText.text = song.difficulty == 1 ? "Normal" : "Hard";
        scoreText.text = $"Overall Score: {finalScore}";
        accuracyText.text = $"Overall Accuracy: {accuracyPercent.ToString(CultureInfo.CurrentCulture)}%";
        missesText.text = $"Overall Misses: {overallStats.missedHits}";

        string scoreSaveName = (song.freeplay ? song.freeplaySong.songName : song.weekData.weekName) + "." + song.difficulty + ".HScore";

        print("Is " + PlayerPrefs.GetFloat(scoreSaveName,0) + ", from " + scoreSaveName + ", less than " + finalScore + "? Answer: "  + (PlayerPrefs.GetFloat(scoreSaveName,0) < finalScore));

        if(PlayerPrefs.GetFloat(scoreSaveName,0) < finalScore)
        {
            PlayerPrefs.SetFloat(scoreSaveName,finalScore);
            PlayerPrefs.Save();
        }
        
    }

    public float GetScoreMultiplier()
    {
        float multiplier = 1;

        if(Options.NoDeath) multiplier -= .25f;
        if(Options.LessHealthLoss) multiplier -= .065f;
        if(Options.MoreHealthGain) multiplier -= .075f;
        if(Options.NoHealthGain) multiplier += .08f;
        if(Options.LessHealthGain) multiplier += .065f;
        if(Options.MoreHealthLoss) multiplier += .075f;
        if(Options.RandomNotes) multiplier += .05f;
        if(Options.PerfectMode) multiplier += .25f;

        return multiplier;
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
