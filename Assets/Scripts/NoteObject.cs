using System;
using UnityEngine;

public class NoteObject : MonoBehaviour
{
    private float _scrollSpeed;

    public float strumTime;
    private Song _song;
    public bool mustHit;
    public bool susNote;
    public int type;
    public bool dummyNote = true;
    public bool lastSusNote = false;

    public float susLength;
    public float ScrollSpeed
    {
        get => _scrollSpeed * 100;
        set => _scrollSpeed = value / 100;
    }


    
    // Start is called before the first frame update
    void Start()
    { 
        _song = Song.instance;
    }

    public void GenerateHold(NoteObject prevNote)
    {
        var noteTransform = transform;

        if (lastSusNote)
        {
            Vector3 oldPos = noteTransform.position;
            oldPos.y += -((float) (Song.instance.stepCrochet / 100 * 1.8 * ScrollSpeed) / 1.76f) + 1.3f;
            return;
        }
        Vector3 oldScale = noteTransform.localScale;
        oldScale.y *= -((float) (Song.instance.stepCrochet / 100 * 1.8 * ScrollSpeed) / 1.76f);
        noteTransform.localScale = oldScale;
        
        
        
        /*if (!prevNote.susNote)
        {
            return;
        }
            
        var prevNoteTransform = prevNote.transform;
        Vector3 oldScale = prevNoteTransform.localScale;
        oldScale.y *= -((float) (Song.instance.stepCrochet / 100 * 1.8 * ScrollSpeed) / 1.76f);
        prevNoteTransform.localScale = oldScale;*/
        
    }

    // Update is called once per frame
    void Update()
    {
        if (!_song.hasStarted || !_song.musicSources[0].isPlaying || dummyNote)
            return;
        Vector3 oldPos = transform.position;
        oldPos.y = (float) (4.45f - (_song.stopwatch.ElapsedMilliseconds - strumTime) * (0.45f * (_scrollSpeed)));
        if (lastSusNote)
            oldPos.y += ((float) (Song.instance.stepCrochet / 100 * 1.8 * ScrollSpeed) / 1.76f) * (_scrollSpeed);
        transform.position = oldPos;
        if (!mustHit)
        {
            //return;
            if (!(transform.position.y >= 4.45f)) return;
            switch (type)
            {
                case 0: //Left
                    Song.instance.EnemyPlayAnimation("Sing Left");
                    break;
                case 1: //Down
                    Song.instance.EnemyPlayAnimation("Sing Down");
                    break;
                case 2: //Up
                    Song.instance.EnemyPlayAnimation("Sing Up");
                    break;
                case 3: //Right
                    Song.instance.EnemyPlayAnimation("Sing Right");
                    break;
            }
            Song.instance.AnimateNote(2, type, "Activated");
                
                

            _song.vocalSource.mute = false;
            _song.player2NotesObjects[type].Remove(this);
            Destroy(gameObject);
        }
        else
        {
            //return;
            if(!Player.demoMode)
            {
                if (!(transform.position.y >= 4.45f + Song.instance.topSafeWindow)) return;
                Song.instance.NoteMiss(type);

                _song.player1NotesObjects[type].Remove(this);
                Destroy(gameObject);
            }
            else
            {
                if (!(transform.position.y >= 4.45f)) return;
                Song.instance.NoteHit(type);
                _song.player1NotesObjects[type].Remove(this);
                Destroy(gameObject);
            }
        }
    }
}
