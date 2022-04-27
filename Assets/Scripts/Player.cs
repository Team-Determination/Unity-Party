using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

public class Player : MonoBehaviour
{
    public float safeFrames = 10;


    public static List<KeyCode> primaryKeyCodes = new List<KeyCode> {KeyCode.LeftArrow, KeyCode.DownArrow, KeyCode.UpArrow, KeyCode.RightArrow};

    public static List<KeyCode> secondaryKeyCodes = new List<KeyCode> {KeyCode.A, KeyCode.S, KeyCode.W, KeyCode.D};

    public static KeyCode pauseKey = KeyCode.Return;
    public static KeyCode resetKey = KeyCode.R;
    public static KeyCode startSongKey = KeyCode.Space;

    public static bool demoMode = false;
    public static bool twoPlayers = false;
    public static bool playAsEnemy = false;

    public static float maxHitRoom;
    public static float safeZoneOffset;
    public static Player instance;
    public static float inputOffset;
    public static float visualOffset;

    public static KeyMode currentKeyMode = KeyMode.FourKey;

    public static SavedKeybinds keybinds;
    
    public List<NoteObject> player1DummyNotes = new List<NoteObject>();
    public List<NoteObject> player2DummyNotes = new List<NoteObject>();


    private void Start()
    {
        instance = this;
        maxHitRoom = -135 * Time.timeScale;
        safeZoneOffset = safeFrames / 60 * 1000;

        inputOffset = PlayerPrefs.GetFloat("Input Offset", 0f);
        visualOffset = PlayerPrefs.GetFloat("Visual Offset", 0f);

        print(keybinds.primary4K.Count);
        foreach (KeyCode code in keybinds.primary4K)
        {
            print(code.ToString());
        }
        print(keybinds.secondary4K.Count);
        foreach (KeyCode code in keybinds.secondary4K)
        {
            print(code.ToString());
        }

        switch (currentKeyMode)
        {
            case KeyMode.FourKey:
                primaryKeyCodes = keybinds.primary4K;
                secondaryKeyCodes = keybinds.secondary4K;
                break;
            case KeyMode.FiveKey:
                break;
            case KeyMode.SixKey:
                break;
            case KeyMode.SevenKey:
                break;
            case KeyMode.EightKey:
                break;
            case KeyMode.NineKey:
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
        

        for (var index = 0; index < primaryKeyCodes.Count; index++)
        {
            var dummyNote = Instantiate(Song.instance.downArrow);
            var noteObject = dummyNote.GetComponent<NoteObject>();
            noteObject.mustHit = true;
            noteObject.type = index;
            player1DummyNotes.Add(noteObject);
        }
        for (var index = 0; index < secondaryKeyCodes.Count; index++)
        {
            var dummyNote = Instantiate(Song.instance.downArrow);
            var noteObject = dummyNote.GetComponent<NoteObject>();
            noteObject.mustHit = true;
            noteObject.type = index;
            player2DummyNotes.Add(noteObject);
            
        }
    }

    public enum KeyMode
    {
        FourKey,
        FiveKey,
        SixKey,
        SevenKey,
        EightKey,
        NineKey
    }

    

    // Update is called once per frame
    private void Update()
    {
        var song = Song.instance;
        if (!song.songSetupDone || !song.songStarted || demoMode || song.isDead || Pause.instance.pauseScreen.activeSelf)
            return;

        var playerOneNotes = song.player1NotesObjects;
        var playerTwoNotes = song.player2NotesObjects;

        #region Player 1 Inputs

        if (!playAsEnemy)
        {
            for (var index = 0; index < primaryKeyCodes.Count; index++)
            {
                print(index);
                KeyCode key = primaryKeyCodes[index];
                NoteObject note = player1DummyNotes[index];
                if(playerOneNotes[index].Count != 0)
                {
                    note = playerOneNotes[index][0];
                }

                if (Input.GetKey(key))
                {
                    if (note != null)
                    {
                        if (note.susNote && !note.dummyNote)
                        {
                            if (note.strumTime + visualOffset <= song.stopwatch.ElapsedMilliseconds)
                            {
                                song.NoteHit(note);
                            }
                        }
                    }
                }

                if (Input.GetKeyDown(key))
                {
                    if (CanHitNote(note))
                    {
                        song.NoteHit(note);
                    }
                    else
                    {
                        song.AnimateNote(1, index, "Pressed");
                        if (!OptionsV2.GhostTapping)
                        {
                            song.NoteMiss(note);
                        }
                    }
                }

                if (Input.GetKeyUp(key))
                {
                    song.AnimateNote(1, index, "Normal");
                }
            }
        }

        #endregion

        #region Player 2 Inputs & Player 1 Sub-Inputs

        if (twoPlayers || playAsEnemy)
        {
            for (var index = 0; index < secondaryKeyCodes.Count; index++)
            {
                KeyCode key = secondaryKeyCodes[index];
                NoteObject note = player2DummyNotes[index];
                if(playerTwoNotes[index].Count != 0)
                    note = playerTwoNotes[index][0];

                if (Input.GetKey(key))
                {
                    if (note != null)
                    {
                        if (note.susNote && !note.dummyNote)
                        {
                            if (note.strumTime + visualOffset <= song.stopwatch.ElapsedMilliseconds)
                            {
                                song.NoteHit(note);
                            }
                        }
                    }
                }

                if (Input.GetKeyDown(key))
                {
                    if (CanHitNote(note))
                    {
                        song.NoteHit(note);
                    }
                    else
                    {
                        song.AnimateNote(2, index, "Pressed");
                        if (!OptionsV2.GhostTapping)
                        {
                            song.NoteMiss(note);
                        }
                    }
                }

                if (Input.GetKeyUp(key))
                {
                    song.AnimateNote(2, index, "Normal");
                }
            }

            if (!twoPlayers)
            {
                for (var index = 0; index < primaryKeyCodes.Count; index++)
                {
                    KeyCode key = primaryKeyCodes[index];
                    NoteObject note = player2DummyNotes[index];
                    if(playerTwoNotes[index].Count != 0)
                        note = playerTwoNotes[index][0];

                    if (Input.GetKey(key))
                    {
                        if (note != null)
                        {
                            if (note.susNote && !note.dummyNote)
                            {
                                if (note.strumTime + visualOffset <= song.stopwatch.ElapsedMilliseconds)
                                {
                                    song.NoteHit(note);
                                }
                            }
                        }
                    }

                    if (Input.GetKeyDown(key))
                    {
                        if (CanHitNote(note))
                        {
                            song.NoteHit(note);
                        }
                        else
                        {
                            song.AnimateNote(2, index, "Pressed");
                            if (!OptionsV2.GhostTapping)
                            {
                                song.NoteMiss(note);
                            }
                        }
                    }

                    if (Input.GetKeyUp(key))
                    {
                        song.AnimateNote(2, index, "Normal");
                    }
                }
            }
           
        }
        else
        {
            for (var index = 0; index < secondaryKeyCodes.Count; index++)
            {
                KeyCode key = secondaryKeyCodes[index];
                NoteObject note = player1DummyNotes[index];
                if(playerOneNotes[index].Count != 0)
                    note = playerOneNotes[index][0];

                if (Input.GetKey(key))
                {
                    if (note != null)
                    {
                        if (note.susNote && !note.dummyNote)
                        {
                            if (note.strumTime + visualOffset <= song.stopwatch.ElapsedMilliseconds)
                            {
                                song.NoteHit(note);
                            }
                        }
                    }
                }

                if (Input.GetKeyDown(key))
                {
                    if (CanHitNote(note))
                    {
                        song.NoteHit(note);
                    }
                    else
                    {
                        song.AnimateNote(1, index, "Pressed");
                        if (!OptionsV2.GhostTapping)
                        {
                            song.NoteMiss(note);
                        }
                    }
                }

                if (Input.GetKeyUp(key))
                {
                    song.AnimateNote(1, index, "Normal");
                }
            }

        }
    }
    #endregion

    public bool CanHitNote(NoteObject noteObject)
    {
        if (noteObject == null) return false;
        /*
        var position = noteObject.transform.position;
        return position.y <= 4.55 + Song.instance.topSafeWindow & position.y >= 4.55 - Song.instance.bottomSafeWindow & !noteObject.dummyNote;
    */
        float noteDiff = noteObject.strumTime + visualOffset - Song.instance.stopwatch.ElapsedMilliseconds +
                         inputOffset;

        return noteDiff <= 135 * Time.timeScale & noteDiff >= -135 * Time.timeScale & !noteObject.dummyNote;
    }
}
