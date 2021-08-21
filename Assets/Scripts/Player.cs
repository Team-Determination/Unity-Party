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
    
    public static KeyCode secLeftArrowKey;
    public static KeyCode secDownArrowKey;
    public static KeyCode secUpArrowKey;
    public static KeyCode secRightArrowKey;

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

    private void Start()
    {
        instance = this;
        maxHitRoom = -135 * Time.timeScale;
        safeZoneOffset = safeFrames / 60 * 1000;
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
            secondaryRightKeyCode = secRightArrowKey
        };
        
        PlayerPrefs.SetString("Saved Keybinds", JsonConvert.SerializeObject(savedKeybinds)); 
        PlayerPrefs.Save();
    }

    // Update is called once per frame
    private void Update()
    {
        if (!Song.instance.hasStarted || demoMode)
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
            if (Input.GetKey(leftArrowKey) && !Replay.replaying || Replay.keysHeld[0])
            {
                if (leftNote.susNote && !leftNote.dummyNote)
                {
                    if(leftNote.strumTime <= Song.instance.stopwatch.ElapsedMilliseconds)
                    {
                        Song.instance.NoteHit(0);
                    }
                }
            }
            if (Input.GetKey(downArrowKey) && !Replay.replaying || Replay.keysHeld[1])
            {
                if (downNote.susNote && !downNote.dummyNote)
                {
                    if(downNote.strumTime <= Song.instance.stopwatch.ElapsedMilliseconds)
                    {
                        Song.instance.NoteHit(1);
                    }
                }
            }
            if (Input.GetKey(upArrowKey) && !Replay.replaying || Replay.keysHeld[2])
            {
                if (upNote.susNote && !upNote.dummyNote)
                {
                    if(upNote.strumTime <= Song.instance.stopwatch.ElapsedMilliseconds)
                    {
                        Song.instance.NoteHit(2);
                    }
                }
            }
            if (Input.GetKey(rightArrowKey) && !Replay.replaying || Replay.keysHeld[3])
            {
                if (rightNote.susNote && !rightNote.dummyNote)
                {
                    if(rightNote.strumTime <= Song.instance.stopwatch.ElapsedMilliseconds)
                    {
                        Song.instance.NoteHit(3);
                    }
                }
            }
        
            if (Input.GetKeyDown(leftArrowKey))
            {
                Replay.instance.RegisterKeyEvent(Replay.KeyEvent.LKeyPress, 1);
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
                Replay.instance.RegisterKeyEvent(Replay.KeyEvent.DKeyPress, 1);
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
                Replay.instance.RegisterKeyEvent(Replay.KeyEvent.UKeyPress, 1);
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
                Replay.instance.RegisterKeyEvent(Replay.KeyEvent.RKeyPress, 1);
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
                Replay.instance.RegisterKeyEvent(Replay.KeyEvent.LKeyRelease, 1);
                Song.instance.AnimateNote(1, 0, "Normal");
            }
            if (Input.GetKeyUp(downArrowKey))
            {
                Replay.instance.RegisterKeyEvent(Replay.KeyEvent.DKeyRelease, 1);
                Song.instance.AnimateNote(1, 1, "Normal");
            }
            if (Input.GetKeyUp(upArrowKey))
            {
                Replay.instance.RegisterKeyEvent(Replay.KeyEvent.UKeyRelease, 1);
                Song.instance.AnimateNote(1, 2, "Normal");
            }
            if (Input.GetKeyUp(rightArrowKey))
            {
                Replay.instance.RegisterKeyEvent(Replay.KeyEvent.RKeyRelease, 1);
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
                Replay.instance.RegisterKeyEvent(Replay.KeyEvent.LKeyPress, 2);
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
                Replay.instance.RegisterKeyEvent(Replay.KeyEvent.DKeyPress, 2);
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
                Replay.instance.RegisterKeyEvent(Replay.KeyEvent.UKeyPress, 2);
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
                Replay.instance.RegisterKeyEvent(Replay.KeyEvent.RKeyPress, 2);
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
                Replay.instance.RegisterKeyEvent(Replay.KeyEvent.LKeyRelease, 2);
                Song.instance.AnimateNote(2, 0, "Normal");
            }

            if (Input.GetKeyUp(secDownArrowKey))
            {
                Replay.instance.RegisterKeyEvent(Replay.KeyEvent.DKeyRelease, 2);
                Song.instance.AnimateNote(2, 1, "Normal");
            }

            if (Input.GetKeyUp(secUpArrowKey))
            {
                Replay.instance.RegisterKeyEvent(Replay.KeyEvent.UKeyRelease, 2);
                Song.instance.AnimateNote(2, 2, "Normal");
            }

            if (Input.GetKeyUp(secRightArrowKey))
            {
                Replay.instance.RegisterKeyEvent(Replay.KeyEvent.RKeyRelease, 2);
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
                Replay.instance.RegisterKeyEvent(Replay.KeyEvent.LKeyPress, 1);
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
                Replay.instance.RegisterKeyEvent(Replay.KeyEvent.DKeyPress, 1);
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
                Replay.instance.RegisterKeyEvent(Replay.KeyEvent.UKeyPress, 1);
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
                Replay.instance.RegisterKeyEvent(Replay.KeyEvent.RKeyPress, 1);
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
                Replay.instance.RegisterKeyEvent(Replay.KeyEvent.LKeyRelease, 1);
                Song.instance.AnimateNote(1, 0, "Normal");
            }
            if (Input.GetKeyUp(secDownArrowKey))
            {
                Replay.instance.RegisterKeyEvent(Replay.KeyEvent.DKeyRelease, 1);
                Song.instance.AnimateNote(1, 1, "Normal");
            }
            if (Input.GetKeyUp(secUpArrowKey))
            {
                Replay.instance.RegisterKeyEvent(Replay.KeyEvent.UKeyRelease, 1);
                Song.instance.AnimateNote(1, 2, "Normal");
            }
            if (Input.GetKeyUp(secRightArrowKey))
            {
                Replay.instance.RegisterKeyEvent(Replay.KeyEvent.RKeyRelease, 1);
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
        float noteDiff = noteObject.strumTime - Song.instance.stopwatch.ElapsedMilliseconds;

        return noteDiff <= 135 * Time.timeScale & noteDiff >= -135 * Time.timeScale & !noteObject.dummyNote;
    }
}
