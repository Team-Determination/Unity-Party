﻿using System;
using UnityEngine;
using Random = UnityEngine.Random;

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
    public bool isAlt;
    public float currentStrumTime;
    public float currentStopwatch;

    public float susLength;
    public float ScrollSpeed
    {
        get => _scrollSpeed * 100;
        set => _scrollSpeed = value / 100;
    }


    
    // Start is called before the first frame update
    void Start()
    { 
        _sprite = GetComponentInChildren<SpriteRenderer>();
        
        
    }

    public void GenerateHold(NoteObject prevNote)
    {
        _sprite = GetComponentInChildren<SpriteRenderer>();
        _song = Song.instance;
        
        var noteTransform = transform;
        _sprite.flipY = OptionsV2.Downscroll;


        if (lastSusNote)
        {
            _sprite.drawMode = SpriteDrawMode.Sliced;
            _sprite.size = new Vector2(.5f,
                .44f * -(float) (Song.instance.stepCrochet / 100 * 1.84 * (ScrollSpeed + _song.speedDifference * 100)));
            return;
        }
        Vector3 oldScale = noteTransform.localScale;
        oldScale.y *= -(float) (Song.instance.stepCrochet / 100 * 1.84 *   (ScrollSpeed + _song.speedDifference * 100));

       
        
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
        _song = Song.instance;
        if (dummyNote)
            return;

        if(OptionsV2.Middlescroll)
        {
            if (Player.playAsEnemy)
            {
                if (mustHit) _sprite.enabled = false;
            }
            else
            {
                if (!mustHit) _sprite.enabled = false;
            }
        }

        var oldPos = transform.position;
        var yPos = mustHit ? Song.instance.player1NoteSprites[type].transform.position.y : Song.instance.player2NoteSprites[type].transform.position.y;

        if (_song.songSetupDone & !_song.songStarted)
        {
            oldPos.y = (yPos - (_song.stopwatch.ElapsedMilliseconds - (strumTime + Player.visualOffset)) *
                (0.45f * (_scrollSpeed + Song.instance.speedDifference)));
            if (lastSusNote)
                oldPos.y += ((float) (Song.instance.stepCrochet / 100 * 1.8 *  (ScrollSpeed + _song.speedDifference * 100)) / 1.76f) * (_scrollSpeed + Song.instance.speedDifference);
            if (OptionsV2.Downscroll)
            {
                oldPos.y -= 4.45f * 2;
                oldPos.y = -oldPos.y;
            }
            transform.position = oldPos;
        }

       
        
        Color color;

        if (mustHit) color = _song.player1NoteSprites[type].color;
        else color = _song.player2NoteSprites[type].color;
        
        if (susNote)
            color.a = .75f;
        _sprite.color = color;
        
        
        oldPos.y = (float) (yPos - (_song.stopwatch.ElapsedMilliseconds - (strumTime + Player.visualOffset)) * (0.45f * (_scrollSpeed + Song.instance.speedDifference)));
        /*
        if (lastSusNote)
            oldPos.y += ((float) (Song.instance.stepCrochet / 100 * 1.85 *  (ScrollSpeed + _song.speedDifference * 100)) / 1.76f) * (_scrollSpeed + Song.instance.speedDifference);
        */
        if (OptionsV2.Downscroll)
        {
            oldPos.y -= 4.45f * 2;
            oldPos.y = -oldPos.y;
        }
        transform.position = oldPos;

        if(!_song.musicSources[0].isPlaying) return;



        
        
        if (!mustHit)
        {
            //return;
            if (Player.twoPlayers || Player.playAsEnemy)
            {
                if (!(strumTime + Player.visualOffset - _song.stopwatch.ElapsedMilliseconds < Player.maxHitRoom)) return;
                Song.instance.NoteMiss(this);
                CameraMovement.instance.focusOnPlayerOne = layer == 1;
                _song.player2NotesObjects[type].Remove(this);
                if (susNote)
                {
                    _song.holdNotesPool.Release(gameObject);
                } else
                {

                    switch (type)
                    {
                        case 0:
                            _song.leftNotesPool.Release(gameObject);
                            break;
                        case 1:
                            _song.downNotesPool.Release(gameObject);
                            break;
                        case 2:
                            _song.upNotesPool.Release(gameObject);
                            break;
                        case 3:
                            _song.rightNotesPool.Release(gameObject);
                            break;
                    }
                }
            }
            else
            {
                /*if (!(strumTime - _song.stopwatch.ElapsedMilliseconds + Player.visualOffset <= _song.stopwatch.ElapsedMilliseconds)) return;
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
                
                CameraMovement.instance.focusOnPlayerOne = layer == 1;

                _song.vocalSource.mute = false;
                _song.player2NotesObjects[type].Remove(this);
                Destroy(gameObject);*/
                
                if (strumTime + Player.visualOffset >= _song.stopwatch.ElapsedMilliseconds) return;
                Song.instance.NoteHit(this);
                CameraMovement.instance.focusOnPlayerOne = layer == 1;

            }
        }
        else
        {
            //return;
            if(!Player.demoMode & !Player.playAsEnemy)
            {
                if (!(strumTime + Player.visualOffset - _song.stopwatch.ElapsedMilliseconds < Player.maxHitRoom)) return;
                Song.instance.NoteMiss(this);
                CameraMovement.instance.focusOnPlayerOne = layer == 1;
                _song.player1NotesObjects[type].Remove(this);
                if (susNote)
                {
                    _song.holdNotesPool.Release(gameObject);
                } else
                {

                    switch (type)
                    {
                        case 0:
                            _song.leftNotesPool.Release(gameObject);
                            break;
                        case 1:
                            _song.downNotesPool.Release(gameObject);
                            break;
                        case 2:
                            _song.upNotesPool.Release(gameObject);
                            break;
                        case 3:
                            _song.rightNotesPool.Release(gameObject);
                            break;
                    }
                }
            }
            else
            {
                if (strumTime + Player.visualOffset >= _song.stopwatch.ElapsedMilliseconds) return;
                Song.instance.NoteHit(this);
                CameraMovement.instance.focusOnPlayerOne = layer == 1;
            }
        }
    }
}
