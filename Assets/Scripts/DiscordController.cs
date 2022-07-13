using System;
using System.Collections;
using System.Collections.Generic;
using DiscordRPC;
using DiscordRPC.Logging;
using UnityEngine;

public class DiscordController : MonoBehaviour
{
    public static DiscordController instance;
    private bool _enableGameStateLoop;
    private DateTime _startDateTime;
    
    public bool EnableGameStateLoop
    {
        get => _enableGameStateLoop;
        set
        {
            _enableGameStateLoop = value;

            if(value)
            {
                InvokeRepeating("SetGameState", 0, 4);
            }
            else
            {
                
                CancelInvoke("SetGameState");
            }
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        if (instance != null)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;

        DontDestroyOnLoad(gameObject);

    }

    public void SetMenuState(string state)
    {
        DiscordPresence presence = new DiscordPresence
        {
            details = "In the Main Menu",
            state = state,
            largeAsset = new DiscordAsset
            {
                image = "logo",
                tooltip = "Icon by Uni"
            },
            buttons = new[]
            {
                new DiscordButton
                {
                    label = "Play Unity Party",
                    url = "https://gamejolt.com/games/unityparty/632556"
                }
            }
        };
        DiscordManager.current.SetPresence(presence);
    }

    public void RefreshStartGameTime()
    {
        _startDateTime = DateTime.Now;
    }

    public void SetGameState()
    {
        SongMetaV2 songMeta = Song.currentSongMeta;
        DiscordPresence presence = new DiscordPresence
        {
            details = songMeta.songName + " " + PlayingCharacter(),
            state = GetPlayState(),
            largeAsset = new DiscordAsset
            {
                image = "logo",
                tooltip = "Icon by Uni"
            }
        };
        if(!songMeta.isFromModPlatform)
        {
            presence.buttons = new[]
            {
                new DiscordButton
                {
                    label = "Play Unity Party",
                    url = "https://gamejolt.com/games/unityparty/632556"
                }
            };
        }
        else
        {
            presence.buttons = new[]
            {
                new DiscordButton
                {
                    label = "Play Unity Party",
                    url = "https://gamejolt.com/games/unityparty/632556"
                },
                new DiscordButton
                {
                    label = "View Song Content",
                    url = songMeta.modURL
                }
            };
        }
        
        presence.startTime = Song.instance.musicSources[0].isPlaying ? DiscordTimestamp.ToUnixMilliseconds(_startDateTime) : 0;
        presence.endTime = Song.instance.musicSources[0].isPlaying ? DiscordTimestamp.ToUnixMilliseconds(GetEndTime()) : 0;
        DiscordManager.current.SetPresence(presence);
    }

    DateTime GetEndTime()
    {

        var musicSource = Song.instance.musicSources[0];
        float spentTime = musicSource.time;
        float timeLeft = musicSource.clip.length - spentTime;

        DateTime time = DateTime.UtcNow;
        time = time.AddSeconds(timeLeft);
        return time;

    }
    
    string PlayingCharacter()
    {
        switch (Song.modeOfPlay)
        {
            //Boyfriend
            case 1:
                return "as Boyfriend";
            //Opponent
            case 2:
                return "as " + Song.instance.enemy.characterName;
            //Local Multiplayer
            case 3:
                return "in Two Players Mode";
            //Auto
            case 4:
                return "in AUTOPLAY MODE";
            default:
                return "ERROR";
        }
    }

    string GetPlayState()
    {
        int score;
        string accuracy = GetCurrentPercentage().ToString("0.00");
        switch (Song.modeOfPlay)
        {
            case 1:
                score = Song.instance.playerOneStats.currentScore;
                break;
            case 2:
                score = Song.instance.playerTwoStats.currentScore;
                break;
            case 3:
                score = Song.instance.playerOneStats.currentScore + Song.instance.playerTwoStats.currentScore;
                break;
            default:
                score = 0;
                break;
        }

        return $"SCORE: {score} | ACCURACY: {accuracy}%";
    }

    float GetCurrentPercentage()
    {
        float sickScore;
        float goodScore;
        float badScore;
        float shitScore;
        
        float accuracyPercent;
        float totalAccuracyScore;
        float accuracy;
        PlayerStat playerOneStats = Song.instance.playerOneStats;
        PlayerStat playerTwoStats = Song.instance.playerTwoStats;
        switch (Song.modeOfPlay)
        {
            case 1:
                sickScore = playerOneStats.totalSicks * 4;
                goodScore = playerOneStats.totalGoods * 3;
                badScore = playerOneStats.totalBads * 2;
                shitScore = playerOneStats.totalShits;
                totalAccuracyScore = sickScore + goodScore + badScore + shitScore;

                accuracy = totalAccuracyScore / (playerOneStats.totalNoteHits * 4);
                
                accuracyPercent = (float) Math.Round(accuracy, 4);
                accuracyPercent *= 100;
                return accuracyPercent;
            case 2:
                sickScore = playerTwoStats.totalSicks * 4;
                goodScore = playerTwoStats.totalGoods * 3;
                badScore = playerTwoStats.totalBads * 2;
                shitScore = playerTwoStats.totalShits;

                totalAccuracyScore = sickScore + goodScore + badScore + shitScore;

                accuracy = totalAccuracyScore / (playerTwoStats.totalNoteHits * 4);
                
                accuracyPercent = (float) Math.Round(accuracy, 4);
                accuracyPercent *= 100;
                return accuracyPercent;
            case 3:
                sickScore = playerOneStats.totalSicks * 4 + playerTwoStats.totalSicks * 4;
                goodScore = playerOneStats.totalGoods * 3 + playerTwoStats.totalGoods * 3;
                badScore = playerOneStats.totalBads * 2 + playerTwoStats.totalBads * 2;
                shitScore = playerTwoStats.totalShits + playerTwoStats.totalShits;

                totalAccuracyScore = sickScore + goodScore + badScore + shitScore;

                accuracy = totalAccuracyScore / (playerOneStats.totalNoteHits * 4 + playerTwoStats.totalNoteHits * 4);
                
                accuracyPercent = (float) Math.Round(accuracy, 4);
                accuracyPercent *= 100;
                return accuracyPercent;
            default:
                return 0;
        }
    }

    private void OnDestroy()
    {
        if (instance != this) return;
        instance = null;
    }

}