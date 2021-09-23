using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Replay : MonoBehaviour
{
    public static Replay instance;
    public static bool replaying;

    public static bool recording;


    public static bool[] keysHeld = {false, false, false, false,false,false,false,false};


    [Header("Replay Viewer")] public GameObject replayScreen; 
    public RectTransform replayList;
    public GameObject replayItem;

    public ReplayData data;

    public string dataDirectory;

    private List<KeyEvent> _boyfriendEvents = new List<KeyEvent>();
    
    private List<float> _boyfriendMilliseconds = new List<float>();

    private List<KeyEvent> _opponentEvents = new List<KeyEvent>();

    private List<float> _opponentMilliseconds = new List<float>();
    // Start is called before the first frame update
    void Start()
    {
        instance = this;
        
    }

    public void RefreshReplayList()
    {
        string replayDir = Application.persistentDataPath + "/Replays";
        
        if(replayList.childCount != 0)
            foreach (Transform child in replayList)
                Destroy(child.gameObject);

        foreach (string filePath in Directory.GetFiles(replayDir))
        {
            GameObject newObject = Instantiate(replayItem, replayList);
            ReplayItem item = newObject.GetComponent<ReplayItem>();
            item.replayDirectory = filePath;
        }
    }

    private void Update()
    {
        if (!replaying || !Song.instance.songStarted) return;

        if (_boyfriendEvents.Count != 0)
        {
            List<int> indexesToRemove = new List<int>();

            foreach (var millisecond in _boyfriendMilliseconds)
            {
                if (millisecond <= Song.instance.stopwatch.ElapsedMilliseconds)
                {
                    int index = _boyfriendMilliseconds.IndexOf(millisecond);
                    indexesToRemove.Add(index);
                    SimulateKeyAction(_boyfriendEvents[index], 1);
                }
                else
                {
                    break;
                }
            }

            foreach (int index in indexesToRemove)
            {
                _boyfriendEvents.RemoveAt(index);
                _boyfriendMilliseconds.RemoveAt(index);
            }
        }
    }

    private void SimulateKeyAction(KeyEvent @event, int player)
    {
        Player playerScript = Player.instance;
        switch (@event)
        {
            case KeyEvent.LKeyPress:
                if(player == 1)
                {
                    if (playerScript.CanHitNote(playerScript.leftNote))
                    {
                        Song.instance.NoteHit(0);
                    }
                    else
                    {
                        Song.instance.NoteMiss(0);
                        Song.instance.AnimateNote(player, 0, "Pressed");
                    }
                    keysHeld[0] = true;
                }
                else
                {
                    if (playerScript.CanHitNote(playerScript.secLeftNote))
                    {
                        Song.instance.NoteHit(0,2);
                    }
                    else
                    {
                        Song.instance.NoteMiss(0,2);
                        Song.instance.AnimateNote(player, 0, "Pressed");
                    }
                    keysHeld[4] = true;
                    
                }
                break;
            case KeyEvent.LKeyRelease:
                if(player == 1)
                {
                    keysHeld[0] = false;
                }
                else
                {
                    keysHeld[4] = false;
                }
                Song.instance.AnimateNote(player, 0, "Normal");
                break;
            case KeyEvent.DKeyPress:
                if(player == 1)
                {
                    if (playerScript.CanHitNote(playerScript.downNote))
                    {
                        Song.instance.NoteHit(1);
                    }
                    else
                    {
                        Song.instance.NoteMiss(1);
                        Song.instance.AnimateNote(player, 1, "Pressed");
                    }
                    keysHeld[1] = true;
                }
                else
                {
                    if (playerScript.CanHitNote(playerScript.secDownNote))
                    {
                        Song.instance.NoteHit(1,2);
                    }
                    else
                    {
                        Song.instance.NoteMiss(1,2);
                        Song.instance.AnimateNote(player, 1, "Pressed");
                    }
                    keysHeld[5] = true;
                }

                break;
            case KeyEvent.DKeyRelease:
                if(player == 1)
                {
                    keysHeld[1] = false;
                }
                else
                {
                    keysHeld[5] = false;
                }
                Song.instance.AnimateNote(player, 1, "Normal");
                break;
            case KeyEvent.UKeyPress:
                if(player == 1)
                {
                    keysHeld[2] = true; 
                    if (playerScript.CanHitNote(playerScript.upNote))
                    {
                        Song.instance.NoteHit(2);
                    }
                    else
                    {
                        Song.instance.NoteMiss(2);
                        Song.instance.AnimateNote(player, 2, "Pressed");
                    }
                }
                else
                {
                    if (playerScript.CanHitNote(playerScript.secUpNote))
                    {
                        Song.instance.NoteHit(2,2);
                    }
                    else
                    {
                        Song.instance.NoteMiss(2,2);
                        Song.instance.AnimateNote(player, 2, "Pressed");
                    }
                    keysHeld[6] = true;
                }
                break;
            case KeyEvent.UKeyRelease:
                if(player == 1)
                {
                    keysHeld[2] = false;
                }
                else
                {
                    keysHeld[6] = false;  
                }
                Song.instance.AnimateNote(player, 2, "Normal");
                break;
            case KeyEvent.RKeyPress:
                if(player == 1)
                {
                    keysHeld[3] = true;
                    if (playerScript.CanHitNote(playerScript.rightNote))
                    {
                        Song.instance.NoteHit(3);
                    }
                    else
                    {
                        Song.instance.NoteMiss(3);
                        Song.instance.AnimateNote(player, 3, "Pressed");
                    }
                }
                else
                {
                    if (playerScript.CanHitNote(playerScript.secRightNote))
                    {
                        Song.instance.NoteHit(3,2);
                    }
                    else
                    {
                        Song.instance.NoteMiss(3,2);
                        Song.instance.AnimateNote(player, 3, "Pressed");
                    }
                    keysHeld[7] = true;
                }
                break;
            case KeyEvent.RKeyRelease:
                if(player == 1)
                {
                    keysHeld[3] = false;
                }
                else
                {
                    keysHeld[7] = false;
                }
                Song.instance.AnimateNote(player, 3, "Normal");

                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    public void RegisterKeyEvent(KeyEvent @event, int player)
    {
        Song song = Song.instance;
        if (!song.songStarted || song.isDead || song.respawning || Pause.instance.pauseScreen.activeSelf || !recording) return;
        if(player == 1)
        {
            data.boyfriendKeyEvents.Add((int)@event);
            data.boyfriendMilliseconds.Add(Song.instance.stopwatch.ElapsedMilliseconds);
        }
        else
        {
            data.opponentKeyEvents.Add((int)@event);
            data.opponentMilliseconds.Add(Song.instance.stopwatch.ElapsedMilliseconds);
        }
    }

    public void LoadReplay(string path)
    {
        dataDirectory = path;
        data = JsonConvert.DeserializeObject<ReplayData>(File.ReadAllText(dataDirectory));

        data.boyfriendMilliseconds = data.boyfriendMilliseconds.OrderBy(f => f).ToList();
        data.opponentMilliseconds = data.opponentMilliseconds.OrderBy(f => f).ToList();

        for (var index = 0; index < data.boyfriendKeyEvents.Count; index++)
        {
            int keyEvent = data.boyfriendKeyEvents[index];
            float millisecond = data.boyfriendMilliseconds[index];
            _boyfriendEvents.Add((KeyEvent) keyEvent);
            _boyfriendMilliseconds.Add(millisecond);
        }
        for (var index = 0; index < data.opponentKeyEvents.Count; index++)
        {
            int keyEvent = data.opponentKeyEvents[index];
            float millisecond = data.opponentMilliseconds[index];
            _opponentEvents.Add((KeyEvent) keyEvent);
            _opponentMilliseconds.Add(millisecond);
        }

        if (data == null)
        {
            return;
        }

        replaying = true;
        recording = false;
        
        Song.instance.PlaySong(false,data.songDirectory);
    }

    public void RefreshReplay()
    {   
        data = JsonConvert.DeserializeObject<ReplayData>(File.ReadAllText(dataDirectory));
    }

    public void SaveReplay()
    {
        string replayDir = Application.persistentDataPath + "/Replays";
        DateTime now = DateTime.Now;
        string fileName = $"{now.Month}-{now.Day}-{now.Year}-{now.Hour}-{now.Minute}-{now.Second}-{now.Millisecond}";
        string replayFile = replayDir + "/" + fileName + ".json";
        data.songDirectory = Song.instance.selectedSongDir;
        data.dateTime = now;
        data.songName = Song.instance.selectedSong.SongName;

        if (Player.twoPlayers)
        {
            data.replayType = ReplayType.AsBoth;
        } else if (Player.playAsEnemy)
        {
            data.replayType = ReplayType.AsEnemy;
        }
        else
        {
            data.replayType = ReplayType.AsBoyfriend;
        }
        
        if (!Directory.Exists(replayDir))
        {
            Directory.CreateDirectory(replayDir);
        }

        File.WriteAllText(replayFile, JsonConvert.SerializeObject(data));
    }
    
    public void InitializeRecorder()
    {
        data = new ReplayData();
    }

    public enum KeyEvent
    {
        LKeyPress = 1,
        LKeyRelease = 2,
        DKeyPress = 3,
        DKeyRelease = 4,
        UKeyPress = 5,
        UKeyRelease = 6,
        RKeyPress = 7,
        RKeyRelease = 8
    }

    public enum ReplayType
    {
        AsBoyfriend = 1,
        AsEnemy = 2,
        AsBoth = 3
    }
}
[Serializable]
public class ReplayData
{
    public string songDirectory;
    public Replay.ReplayType replayType;
    public List<float> boyfriendMilliseconds = new List<float>();
    public List<float> opponentMilliseconds = new List<float>();
    public List<int> boyfriendKeyEvents = new List<int>();
    public List<int> opponentKeyEvents = new List<int>();
    public string songName = "A Song";
    public DateTime dateTime = DateTime.MinValue;
}

