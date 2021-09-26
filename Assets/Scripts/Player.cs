using System;
using Newtonsoft.Json;
using UnityEngine;

public class Player : MonoBehaviour
{
    public float safeFrames = 10;
    
    public static KeyCode leftArrowKey = KeyCode.LeftArrow;
    public static KeyCode downArrowKey = KeyCode.DownArrow;
    public static KeyCode upArrowKey = KeyCode.UpArrow;
    public static KeyCode rightArrowKey = KeyCode.RightArrow;
    
    public static KeyCode secLeftArrowKey = KeyCode.A;
    public static KeyCode secDownArrowKey = KeyCode.S;
    public static KeyCode secUpArrowKey = KeyCode.W;
    public static KeyCode secRightArrowKey = KeyCode.D;

    public static KeyCode pauseKey = KeyCode.Return;
    public static KeyCode resetKey = KeyCode.R;

    public NoteObject leftNote;
    public NoteObject downNote;
    public NoteObject upNote;
    public NoteObject rightNote;

    public NoteObject secLeftNote;
    public NoteObject secDownNote;
    public NoteObject secUpNote;
    public NoteObject secRightNote;

    public static bool demoMode = false;
    public static bool twoPlayers = false;
    public static bool playAsEnemy = false;

    public static float maxHitRoom;
    public static float safeZoneOffset;
    public static Player instance;
    public static float inputOffset;
    public static float visualOffset;

    private void Start()
    {
        instance = this;
        maxHitRoom = -135 * Time.timeScale;
        safeZoneOffset = safeFrames / 60 * 1000;

        inputOffset = PlayerPrefs.GetFloat("Input Offset", 0f);
        visualOffset = PlayerPrefs.GetFloat("Visual Offset", 0f);
    }

    public static void SaveKeySet()
    {
        SavedKeybinds savedKeybinds = new SavedKeybinds
        {
            primaryLeftKeyCode = leftArrowKey,
            primaryDownKeyCode = downArrowKey,
            primaryUpKeyCode = upArrowKey,
            primaryRightKeyCode = rightArrowKey,
            secondaryLeftKeyCode = secLeftArrowKey,
            secondaryDownKeyCode = secDownArrowKey,
            secondaryUpKeyCode = secUpArrowKey,
            secondaryRightKeyCode = secRightArrowKey,
            pauseKeyCode = pauseKey,
            resetKeyCode = resetKey
        };
        
        PlayerPrefs.SetString("Saved Keybinds", JsonConvert.SerializeObject(savedKeybinds)); 
        PlayerPrefs.Save();
    }

    // Update is called once per frame
    private void Update()
    {
        if (!Song.instance.hasStarted || demoMode || Song.instance.isDead || Pause.instance.pauseScreen.activeSelf)
            return;


        if (!playAsEnemy)
        {
            if (Song.instance.player1NotesObjects[0].Count != 0)
                leftNote = Song.instance.player1NotesObjects[0][0];
            else if (!leftNote.dummyNote || leftNote == null)
                leftNote = new GameObject().AddComponent<NoteObject>();
                    
            if (Song.instance.player1NotesObjects[1].Count != 0)
                downNote = Song.instance.player1NotesObjects[1][0];
            else if(!downNote.dummyNote || downNote == null)
                downNote = new GameObject().AddComponent<NoteObject>();
        
            if (Song.instance.player1NotesObjects[2].Count != 0)
                upNote = Song.instance.player1NotesObjects[2][0];
            else if(!upNote.dummyNote || upNote == null)
                upNote = new GameObject().AddComponent<NoteObject>();
        
            if (Song.instance.player1NotesObjects[3].Count != 0)
                rightNote = Song.instance.player1NotesObjects[3][0];
            else if(!rightNote.dummyNote || rightNote == null)
                rightNote = new GameObject().AddComponent<NoteObject>();
        }
        
        if(twoPlayers || playAsEnemy)
        {
            if (Song.instance.player2NotesObjects[0].Count != 0)
                secLeftNote = Song.instance.player2NotesObjects[0][0];
            else if (!secLeftNote.dummyNote || secLeftNote == null)
                secLeftNote = new GameObject().AddComponent<NoteObject>();

            if (Song.instance.player2NotesObjects[1].Count != 0)
                secDownNote = Song.instance.player2NotesObjects[1][0];
            else if (!secDownNote.dummyNote || secDownNote == null)
                secDownNote = new GameObject().AddComponent<NoteObject>();

            if (Song.instance.player2NotesObjects[2].Count != 0)
                secUpNote = Song.instance.player2NotesObjects[2][0];
            else if (!secUpNote.dummyNote || secUpNote == null)
                secUpNote = new GameObject().AddComponent<NoteObject>();

            if (Song.instance.player2NotesObjects[3].Count != 0)
                secRightNote = Song.instance.player2NotesObjects[3][0];
            else if (!secRightNote.dummyNote || secRightNote == null)
                secRightNote = new GameObject().AddComponent<NoteObject>();

        }

        #region Player 1 Inputs

        if (!playAsEnemy)
        {
            if (Input.GetKey(leftArrowKey))
            {
                if (leftNote.susNote && !leftNote.dummyNote)
                {
                    if(leftNote.strumTime + visualOffset <= Song.instance.stopwatch.ElapsedMilliseconds)
                    {
                        Song.instance.NoteHit(0);
                    }
                }
            }
            if (Input.GetKey(downArrowKey))
            {
                if (downNote.susNote && !downNote.dummyNote)
                {
                    if(downNote.strumTime + visualOffset <= Song.instance.stopwatch.ElapsedMilliseconds)
                    {
                        Song.instance.NoteHit(1);
                    }
                }
            }
            if (Input.GetKey(upArrowKey))
            {
                if (upNote.susNote && !upNote.dummyNote)
                {
                    if(upNote.strumTime + visualOffset <= Song.instance.stopwatch.ElapsedMilliseconds)
                    {
                        Song.instance.NoteHit(2);
                    }
                }
            }
            if (Input.GetKey(rightArrowKey))
            {
                if (rightNote.susNote && !rightNote.dummyNote)
                {
                    if(rightNote.strumTime + visualOffset <= Song.instance.stopwatch.ElapsedMilliseconds)
                    {
                        Song.instance.NoteHit(3);
                    }
                }
            }
        
            if (Input.GetKeyDown(leftArrowKey))
            {
                if (CanHitNote(leftNote))
                {
                    Song.instance.NoteHit(0);
                }
                else
                {
                    Song.instance.AnimateNote(1, 0, "Pressed");
                    Song.instance.NoteMiss(0);
                }
            }
            if (Input.GetKeyDown(downArrowKey))
            {
                if (CanHitNote(downNote))
                {
                    Song.instance.NoteHit(1);
                }
                else
                {
                    Song.instance.AnimateNote(1, 1, "Pressed");
                    Song.instance.NoteMiss(1);
                }
            }
            if (Input.GetKeyDown(upArrowKey))
            {
                if (CanHitNote(upNote))
                {
                    Song.instance.NoteHit(2);
                }
                else
                {
                    Song.instance.AnimateNote(1, 2, "Pressed");
                    Song.instance.NoteMiss(2);
                }
            }
            if (Input.GetKeyDown(rightArrowKey))
            {
                if (CanHitNote(rightNote))
                {
                    Song.instance.NoteHit(3);
                }
                else
                {
                    Song.instance.AnimateNote(1, 3, "Pressed");
                    Song.instance.NoteMiss(3);
                }
            }

            if (Input.GetKeyUp(leftArrowKey))
            {
                Song.instance.AnimateNote(1, 0, "Normal");
            }
            if (Input.GetKeyUp(downArrowKey))
            {
                Song.instance.AnimateNote(1, 1, "Normal");
            }
            if (Input.GetKeyUp(upArrowKey))
            {
                Song.instance.AnimateNote(1, 2, "Normal");
            }
            if (Input.GetKeyUp(rightArrowKey))
            {
                Song.instance.AnimateNote(1, 3, "Normal");
            }
        }
        #endregion
        
        #region Player 2 Inputs & Player 1 Sub-Inputs

        if (twoPlayers || playAsEnemy)
        {
            if (Input.GetKey(secLeftArrowKey))
            {
                if (secLeftNote.susNote && !secLeftNote.dummyNote)
                {
                    if (secLeftNote.strumTime <= Song.instance.stopwatch.ElapsedMilliseconds)
                    {
                        Song.instance.NoteHit(0, 2);
                    }
                }
            }

            if (Input.GetKey(secDownArrowKey))
            {
                if (secDownNote.susNote && !secDownNote.dummyNote)
                {
                    if (secDownNote.strumTime <= Song.instance.stopwatch.ElapsedMilliseconds)
                    {
                        Song.instance.NoteHit(1, 2);
                    }
                }
            }

            if (Input.GetKey(secUpArrowKey))
            {
                if (secUpNote.susNote && !secUpNote.dummyNote)
                {
                    if (secUpNote.strumTime <= Song.instance.stopwatch.ElapsedMilliseconds)
                    {
                        Song.instance.NoteHit(2, 2);
                    }
                }
            }

            if (Input.GetKey(secRightArrowKey))
            {
                if (secRightNote.susNote && !secRightNote.dummyNote)
                {
                    if (secRightNote.strumTime <= Song.instance.stopwatch.ElapsedMilliseconds)
                    {
                        Song.instance.NoteHit(3, 2);
                    }
                }
            }

            if (Input.GetKeyDown(secLeftArrowKey))
            {
                if (CanHitNote(secLeftNote))
                {
                    Song.instance.NoteHit(0, 2);
                }
                else
                {
                    Song.instance.AnimateNote(2, 0, "Pressed");
                    Song.instance.NoteMiss(0, 2);
                }
            }

            if (Input.GetKeyDown(secDownArrowKey))
            {
                if (CanHitNote(secDownNote))
                {
                    Song.instance.NoteHit(1, 2);
                }
                else
                {
                    Song.instance.AnimateNote(2, 1, "Pressed");
                    Song.instance.NoteMiss(1, 2);
                }
            }

            if (Input.GetKeyDown(secUpArrowKey))
            {
                if (CanHitNote(secUpNote))
                {
                    Song.instance.NoteHit(2, 2);
                }
                else
                {
                    Song.instance.AnimateNote(2, 2, "Pressed");
                    Song.instance.NoteMiss(2, 2);
                }
            }

            if (Input.GetKeyDown(secRightArrowKey))
            {
                if (CanHitNote(secRightNote))
                {
                    Song.instance.NoteHit(3, 2);
                }
                else
                {
                    Song.instance.AnimateNote(2, 3, "Pressed");
                    Song.instance.NoteMiss(3,2);
                }
            }

            if (Input.GetKeyUp(secLeftArrowKey))
            {
                Song.instance.AnimateNote(2, 0, "Normal");
            }

            if (Input.GetKeyUp(secDownArrowKey))
            {
                Song.instance.AnimateNote(2, 1, "Normal");
            }

            if (Input.GetKeyUp(secUpArrowKey))
            {
                Song.instance.AnimateNote(2, 2, "Normal");
            }

            if (Input.GetKeyUp(secRightArrowKey))
            {
                Song.instance.AnimateNote(2, 3, "Normal");
            }
        }
        else
        {
            if (Input.GetKey(secLeftArrowKey))
            {
                if (leftNote.susNote && !leftNote.dummyNote)
                {
                    if(leftNote.transform.position.y >= 4.55f)
                    {
                        Song.instance.NoteHit(0);
                    }
                }
            }
            if (Input.GetKey(secDownArrowKey))
            {
                if (downNote.susNote && !downNote.dummyNote)
                {
                    if(downNote.transform.position.y >= 4.55f)
                    {
                        Song.instance.NoteHit(1);
                    }
                }
            }
            if (Input.GetKey(secUpArrowKey))
            {
                if (upNote.susNote && !upNote.dummyNote)
                {
                    if(upNote.transform.position.y >= 4.55f)
                    {
                        Song.instance.NoteHit(2);
                    }
                }
            }
            if (Input.GetKey(secRightArrowKey))
            {
                if (rightNote.susNote && !rightNote.dummyNote)
                {
                    if(rightNote.transform.position.y >= 4.55f)
                    {
                        Song.instance.NoteHit(3);
                    }
                }
            }
        
            if (Input.GetKeyDown(secLeftArrowKey))
            {
                if (CanHitNote(leftNote))
                {
                    Song.instance.NoteHit(0);
                }
                else
                {
                    Song.instance.AnimateNote(1, 0, "Pressed");
                    Song.instance.NoteMiss(0);
                }
            }
            if (Input.GetKeyDown(secDownArrowKey))
            {
                if (CanHitNote(downNote))
                {
                    Song.instance.NoteHit(1);
                }
                else
                {
                    Song.instance.AnimateNote(1, 1, "Pressed");
                    Song.instance.NoteMiss(1);
                }
            }
            if (Input.GetKeyDown(secUpArrowKey))
            {
                if (CanHitNote(upNote))
                {
                    Song.instance.NoteHit(2);
                }
                else
                {
                    Song.instance.AnimateNote(1, 2, "Pressed");
                    Song.instance.NoteMiss(2);
                }
            }
            if (Input.GetKeyDown(secRightArrowKey))
            {
                if (CanHitNote(rightNote))
                {
                    Song.instance.NoteHit(3);
                }
                else
                {
                    Song.instance.AnimateNote(1, 3, "Pressed");
                    Song.instance.NoteMiss(3);
                }
            }

            if (Input.GetKeyUp(secLeftArrowKey))
            {
                Song.instance.AnimateNote(1, 0, "Normal");
            }
            if (Input.GetKeyUp(secDownArrowKey))
            {
                Song.instance.AnimateNote(1, 1, "Normal");
            }
            if (Input.GetKeyUp(secUpArrowKey))
            {
                Song.instance.AnimateNote(1, 2, "Normal");
            }
            if (Input.GetKeyUp(secRightArrowKey))
            {
                Song.instance.AnimateNote(1, 3, "Normal");
            }
        }
        #endregion
        
    }


    public bool CanHitNote(NoteObject noteObject)
    {
        /*
        var position = noteObject.transform.position;
        return position.y <= 4.55 + Song.instance.topSafeWindow & position.y >= 4.55 - Song.instance.bottomSafeWindow & !noteObject.dummyNote;
    */
        float noteDiff = noteObject.strumTime + visualOffset - Song.instance.stopwatch.ElapsedMilliseconds + inputOffset;

        return noteDiff <= 135 * Time.timeScale & noteDiff >= -135 * Time.timeScale & !noteObject.dummyNote;
    }
}
