using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SongEventsHandler : MonoBehaviour
{
    public List<SongEvent> songEvents;

    public static SongEventsHandler instance;

    private void Start()
    {
        instance = this;
    }

    // Update is called once per frame
    void Update()
    {
        Song song = Song.instance;
        if (song.isDead || !song.songStarted || !song.songSetupDone || !song.musicSources[0].isPlaying) return;
        if (song.modInstance == null) return;
        if(songEvents == null || songEvents.Count == 0) return;
        foreach (SongEvent @event in songEvents)
        {
            if (@event.time >= song.stopwatch.ElapsedMilliseconds)
            {
                song.modInstance.Invoke(@event.functionToCall,@event.functionArguments);
            }
            else
            {
                break;
            }
        }
    }
}
[Serializable]
public class SongEvent
{
    public float time;
    public string functionToCall;
    public List<object> functionArguments = new List<object>();
}

[Serializable]
public class SongEvents
{
    public List<SongEvent> events;
}


