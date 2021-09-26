using System;
using UnityEngine;

public class NoteObject : MonoBehaviour
{
    private float _scrollSpeed;
    private SpriteRenderer _sprite;
    
    public float strumTime;
    private Song _song;
    public bool mustHit;
    public bool susNote;
    public int type;
    public bool dummyNote = true;
    public bool lastSusNote = false;
    public int layer;

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
        _sprite = GetComponentInChildren<SpriteRenderer>();
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
        oldScale.y *= -((float) (Song.instance.stepCrochet / 100 * 1.8 * ScrollSpeed) / 1.76f)*2;
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
        oldPos.y = (float) (4.45f - (_song.stopwatch.ElapsedMilliseconds - (strumTime + Player.visualOffset)) * (0.45f * (_scrollSpeed)));

        if (Options.downscroll)
        {
            oldPos.y -= 4.45f * 2;
            oldPos.y = -oldPos.y;
        }
        
        if (lastSusNote)
            oldPos.y += ((float) (Song.instance.stepCrochet / 100 * 1.8 * ScrollSpeed) / 1.76f) * (_scrollSpeed);
        
        if (Options.downscroll)
        {
            if (lastSusNote)
            {
                Vector3 currentScale = transform.localScale;
                currentScale.y = -currentScale.y;
                transform.localScale = currentScale;
            }
        }
        /*print(_song.player1Left.position.x);
        switch (type)
        {
            case 0:
                oldPos.x = Mathf.Lerp(oldPos.x, mustHit ? _song.player1Left.position.x : _song.player2Left.position.x, (0.45f * (_scrollSpeed)));
                break;
            case 1:
                oldPos.x = Mathf.Lerp(oldPos.x, mustHit ? _song.player1Down.position.x : _song.player2Down.position.x, (0.45f * (_scrollSpeed)));
                break;
            case 2:
                oldPos.x = Mathf.Lerp(oldPos.x, mustHit ? _song.player1Up.position.x : _song.player2Up.position.x, (0.45f * (_scrollSpeed)));
                break;
            case 3:
                oldPos.x = Mathf.Lerp(oldPos.x, mustHit ? _song.player1Right.position.x : _song.player2Right.position.x, (0.45f * (_scrollSpeed)));
                break;
                
        }*/
        
        transform.position = oldPos;
        Color color = _song.player1NoteSprites[type].color;
        if (susNote)
            color.a = .75f;
        _sprite.color = color;
        if (!mustHit)
        {
            //return;
            if (Player.twoPlayers || Player.playAsEnemy)
            {
                if (!((strumTime + Player.visualOffset) - _song.stopwatch.ElapsedMilliseconds < Player.maxHitRoom)) return;
                Song.instance.NoteMiss(type,2);
                _song.player2NotesObjects[type].Remove(this);
                Destroy(gameObject);
            }
            else
            {
                if (!((strumTime + Player.visualOffset) <= _song.stopwatch.ElapsedMilliseconds)) return;

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
        }
        else
        {
            //return;
            if(!Player.demoMode & !Player.playAsEnemy)
            {
                if (!((strumTime + Player.visualOffset) - _song.stopwatch.ElapsedMilliseconds < Player.maxHitRoom)) return;
                Song.instance.NoteMiss(type);
                _song.player1NotesObjects[type].Remove(this);
                Destroy(gameObject);
            }
            else
            {
                if (!((strumTime + Player.visualOffset) <= _song.stopwatch.ElapsedMilliseconds)) return;
                Song.instance.NoteHit(type);
            }
        }
    }
}
