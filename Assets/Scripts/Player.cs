using System;
using Newtonsoft.Json;
using UnityEngine;

public class Player : MonoBehaviour
{
    public float safeFrames = 10;
    
    public static KeyCode LeftArrowKey = KeyCode.LeftArrow;
    public static KeyCode DownArrowKey = KeyCode.DownArrow;
    public static KeyCode UpArrowKey = KeyCode.UpArrow;
    public static KeyCode RightArrowKey = KeyCode.RightArrow;
    
    public static KeyCode SecLeftArrowKey;
    public static KeyCode SecDownArrowKey;
    public static KeyCode SecUpArrowKey;
    public static KeyCode SecRightArrowKey;

    private NoteObject _leftNote;
    private NoteObject _downNote;
    private NoteObject _upNote;
    private NoteObject _rightNote;

    private NoteObject _secLeftNote;
    private NoteObject _secDownNote;
    private NoteObject _secUpNote;
    private NoteObject _secRightNote;

    public static bool demoMode = false;
    public static bool twoPlayers = false;
    public static bool playAsEnemy = false;

    public static float maxHitRoom;
    public static float safeZoneOffset;

    private void Start()
    {
        maxHitRoom = -135 * Time.timeScale;
        safeZoneOffset = safeFrames / 60 * 1000;
    }

    public static void SaveKeySet()
    {
        SavedKeybinds savedKeybinds = new SavedKeybinds
        {
            primaryLeftKeyCode = LeftArrowKey,
            primaryDownKeyCode = DownArrowKey,
            primaryUpKeyCode = UpArrowKey,
            primaryRightKeyCode = RightArrowKey,
            secondaryLeftKeyCode = SecLeftArrowKey,
            secondaryDownKeyCode = SecDownArrowKey,
            secondaryUpKeyCode = SecUpArrowKey,
            secondaryRightKeyCode = SecRightArrowKey
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
                _leftNote = Song.instance.player1NotesObjects[0][0];
            else if (!_leftNote.dummyNote || _leftNote == null)
                _leftNote = new GameObject().AddComponent<NoteObject>();
                    
            if (Song.instance.player1NotesObjects[1].Count != 0)
                _downNote = Song.instance.player1NotesObjects[1][0];
            else if(!_downNote.dummyNote || _downNote == null)
                _downNote = new GameObject().AddComponent<NoteObject>();
        
            if (Song.instance.player1NotesObjects[2].Count != 0)
                _upNote = Song.instance.player1NotesObjects[2][0];
            else if(!_upNote.dummyNote || _upNote == null)
                _upNote = new GameObject().AddComponent<NoteObject>();
        
            if (Song.instance.player1NotesObjects[3].Count != 0)
                _rightNote = Song.instance.player1NotesObjects[3][0];
            else if(!_rightNote.dummyNote || _rightNote == null)
                _rightNote = new GameObject().AddComponent<NoteObject>();
        }
        
        if(twoPlayers || playAsEnemy)
        {
            if (Song.instance.player2NotesObjects[0].Count != 0)
                _secLeftNote = Song.instance.player2NotesObjects[0][0];
            else if (!_secLeftNote.dummyNote || _secLeftNote == null)
                _secLeftNote = new GameObject().AddComponent<NoteObject>();

            if (Song.instance.player2NotesObjects[1].Count != 0)
                _secDownNote = Song.instance.player2NotesObjects[1][0];
            else if (!_secDownNote.dummyNote || _secDownNote == null)
                _secDownNote = new GameObject().AddComponent<NoteObject>();

            if (Song.instance.player2NotesObjects[2].Count != 0)
                _secUpNote = Song.instance.player2NotesObjects[2][0];
            else if (!_secUpNote.dummyNote || _secUpNote == null)
                _secUpNote = new GameObject().AddComponent<NoteObject>();

            if (Song.instance.player2NotesObjects[3].Count != 0)
                _secRightNote = Song.instance.player2NotesObjects[3][0];
            else if (!_secRightNote.dummyNote || _secRightNote == null)
                _secRightNote = new GameObject().AddComponent<NoteObject>();

        }

        #region Player 1 Inputs

        if (!playAsEnemy)
        {
            if (Input.GetKey(LeftArrowKey))
            {
                if (_leftNote.susNote && !_leftNote.dummyNote)
                {
                    if(_leftNote.strumTime <= Song.instance.stopwatch.ElapsedMilliseconds)
                    {
                        Song.instance.NoteHit(0);
                    }
                }
            }
            if (Input.GetKey(DownArrowKey))
            {
                if (_downNote.susNote && !_downNote.dummyNote)
                {
                    if(_downNote.strumTime <= Song.instance.stopwatch.ElapsedMilliseconds)
                    {
                        Song.instance.NoteHit(1);
                    }
                }
            }
            if (Input.GetKey(UpArrowKey))
            {
                if (_upNote.susNote && !_upNote.dummyNote)
                {
                    if(_upNote.strumTime <= Song.instance.stopwatch.ElapsedMilliseconds)
                    {
                        Song.instance.NoteHit(2);
                    }
                }
            }
            if (Input.GetKey(RightArrowKey))
            {
                if (_rightNote.susNote && !_rightNote.dummyNote)
                {
                    if(_rightNote.strumTime <= Song.instance.stopwatch.ElapsedMilliseconds)
                    {
                        Song.instance.NoteHit(3);
                    }
                }
            }
        
            if (Input.GetKeyDown(LeftArrowKey))
            {
                if (CanHitNote(_leftNote))
                {
                    Song.instance.NoteHit(0);
                }
                else
                {
                    Song.instance.AnimateNote(1, 0, "Pressed");
                    Song.instance.NoteMiss(0);
                }
            }
            if (Input.GetKeyDown(DownArrowKey))
            {
                if (CanHitNote(_downNote))
                {
                    Song.instance.NoteHit(1);
                }
                else
                {
                    Song.instance.AnimateNote(1, 1, "Pressed");
                    Song.instance.NoteMiss(1);
                }
            }
            if (Input.GetKeyDown(UpArrowKey))
            {
                if (CanHitNote(_upNote))
                {
                    Song.instance.NoteHit(2);
                }
                else
                {
                    Song.instance.AnimateNote(1, 2, "Pressed");
                    Song.instance.NoteMiss(2);
                }
            }
            if (Input.GetKeyDown(RightArrowKey))
            {
                if (CanHitNote(_rightNote))
                {
                    Song.instance.NoteHit(3);
                }
                else
                {
                    Song.instance.AnimateNote(1, 3, "Pressed");
                    Song.instance.NoteMiss(3);
                }
            }

            if (Input.GetKeyUp(LeftArrowKey))
            {
                Song.instance.AnimateNote(1, 0, "Normal");
            }
            if (Input.GetKeyUp(DownArrowKey))
            {
                Song.instance.AnimateNote(1, 1, "Normal");
            }
            if (Input.GetKeyUp(UpArrowKey))
            {
                Song.instance.AnimateNote(1, 2, "Normal");
            }
            if (Input.GetKeyUp(RightArrowKey))
            {
                Song.instance.AnimateNote(1, 3, "Normal");
            }
        }
        #endregion
        
        #region Player 2 Inputs & Player 1 Sub-Inputs

        if (twoPlayers || playAsEnemy)
        {
            if (Input.GetKey(SecLeftArrowKey))
            {
                if (_secLeftNote.susNote && !_secLeftNote.dummyNote)
                {
                    if (_secLeftNote.strumTime <= Song.instance.stopwatch.ElapsedMilliseconds)
                    {
                        Song.instance.NoteHit(0, 2);
                    }
                }
            }

            if (Input.GetKey(SecDownArrowKey))
            {
                if (_secDownNote.susNote && !_secDownNote.dummyNote)
                {
                    if (_secDownNote.strumTime <= Song.instance.stopwatch.ElapsedMilliseconds)
                    {
                        Song.instance.NoteHit(1, 2);
                    }
                }
            }

            if (Input.GetKey(SecUpArrowKey))
            {
                if (_secUpNote.susNote && !_secUpNote.dummyNote)
                {
                    if (_secUpNote.strumTime <= Song.instance.stopwatch.ElapsedMilliseconds)
                    {
                        Song.instance.NoteHit(2, 2);
                    }
                }
            }

            if (Input.GetKey(SecRightArrowKey))
            {
                if (_secRightNote.susNote && !_secRightNote.dummyNote)
                {
                    if (_secRightNote.strumTime <= Song.instance.stopwatch.ElapsedMilliseconds)
                    {
                        Song.instance.NoteHit(3, 2);
                    }
                }
            }

            if (Input.GetKeyDown(SecLeftArrowKey))
            {
                if (CanHitNote(_secLeftNote))
                {
                    Song.instance.NoteHit(0, 2);
                }
                else
                {
                    Song.instance.AnimateNote(2, 0, "Pressed");
                    Song.instance.NoteMiss(0, 2);
                }
            }

            if (Input.GetKeyDown(SecDownArrowKey))
            {
                if (CanHitNote(_secDownNote))
                {
                    Song.instance.NoteHit(1, 2);
                }
                else
                {
                    Song.instance.AnimateNote(2, 1, "Pressed");
                    Song.instance.NoteMiss(1, 2);
                }
            }

            if (Input.GetKeyDown(SecUpArrowKey))
            {
                if (CanHitNote(_secUpNote))
                {
                    Song.instance.NoteHit(2, 2);
                }
                else
                {
                    Song.instance.AnimateNote(2, 2, "Pressed");
                    Song.instance.NoteMiss(2, 2);
                }
            }

            if (Input.GetKeyDown(SecRightArrowKey))
            {
                if (CanHitNote(_secRightNote))
                {
                    Song.instance.NoteHit(3, 2);
                }
                else
                {
                    Song.instance.AnimateNote(2, 3, "Pressed");
                    Song.instance.NoteMiss(3,2);
                }
            }

            if (Input.GetKeyUp(SecLeftArrowKey))
            {
                Song.instance.AnimateNote(2, 0, "Normal");
            }

            if (Input.GetKeyUp(SecDownArrowKey))
            {
                Song.instance.AnimateNote(2, 1, "Normal");
            }

            if (Input.GetKeyUp(SecUpArrowKey))
            {
                Song.instance.AnimateNote(2, 2, "Normal");
            }

            if (Input.GetKeyUp(SecRightArrowKey))
            {
                Song.instance.AnimateNote(2, 3, "Normal");
            }
        }
        else
        {
            if (Input.GetKey(SecLeftArrowKey))
            {
                if (_leftNote.susNote && !_leftNote.dummyNote)
                {
                    if(_leftNote.transform.position.y >= 4.55f)
                    {
                        Song.instance.NoteHit(0);
                    }
                }
            }
            if (Input.GetKey(SecDownArrowKey))
            {
                if (_downNote.susNote && !_downNote.dummyNote)
                {
                    if(_downNote.transform.position.y >= 4.55f)
                    {
                        Song.instance.NoteHit(1);
                    }
                }
            }
            if (Input.GetKey(SecUpArrowKey))
            {
                if (_upNote.susNote && !_upNote.dummyNote)
                {
                    if(_upNote.transform.position.y >= 4.55f)
                    {
                        Song.instance.NoteHit(2);
                    }
                }
            }
            if (Input.GetKey(SecRightArrowKey))
            {
                if (_rightNote.susNote && !_rightNote.dummyNote)
                {
                    if(_rightNote.transform.position.y >= 4.55f)
                    {
                        Song.instance.NoteHit(3);
                    }
                }
            }
        
            if (Input.GetKeyDown(SecLeftArrowKey))
            {
                if (CanHitNote(_leftNote))
                {
                    Song.instance.NoteHit(0);
                }
                else
                {
                    Song.instance.AnimateNote(1, 0, "Pressed");
                    Song.instance.NoteMiss(0);
                }
            }
            if (Input.GetKeyDown(SecDownArrowKey))
            {
                if (CanHitNote(_downNote))
                {
                    Song.instance.NoteHit(1);
                }
                else
                {
                    Song.instance.AnimateNote(1, 1, "Pressed");
                    Song.instance.NoteMiss(1);
                }
            }
            if (Input.GetKeyDown(SecUpArrowKey))
            {
                if (CanHitNote(_upNote))
                {
                    Song.instance.NoteHit(2);
                }
                else
                {
                    Song.instance.AnimateNote(1, 2, "Pressed");
                    Song.instance.NoteMiss(2);
                }
            }
            if (Input.GetKeyDown(SecRightArrowKey))
            {
                if (CanHitNote(_rightNote))
                {
                    Song.instance.NoteHit(3);
                }
                else
                {
                    Song.instance.AnimateNote(1, 3, "Pressed");
                    Song.instance.NoteMiss(3);
                }
            }

            if (Input.GetKeyUp(SecLeftArrowKey))
            {
                Song.instance.AnimateNote(1, 0, "Normal");
            }
            if (Input.GetKeyUp(SecDownArrowKey))
            {
                Song.instance.AnimateNote(1, 1, "Normal");
            }
            if (Input.GetKeyUp(SecUpArrowKey))
            {
                Song.instance.AnimateNote(1, 2, "Normal");
            }
            if (Input.GetKeyUp(SecRightArrowKey))
            {
                Song.instance.AnimateNote(1, 3, "Normal");
            }
        }
        #endregion
        
    }


    private bool CanHitNote(NoteObject noteObject)
    {
        /*
        var position = noteObject.transform.position;
        return position.y <= 4.55 + Song.instance.topSafeWindow & position.y >= 4.55 - Song.instance.bottomSafeWindow & !noteObject.dummyNote;
    */
        float noteDiff = noteObject.strumTime - Song.instance.stopwatch.ElapsedMilliseconds;

        return noteDiff <= 135 * Time.timeScale & noteDiff >= -135 * Time.timeScale & !noteObject.dummyNote;
    }
}
