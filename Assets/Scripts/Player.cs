using System;
using Newtonsoft.Json;
using UnityEngine;

public class Player : MonoBehaviour
{
    public static KeyCode leftArrowKey = KeyCode.LeftArrow;
    public static KeyCode downArrowKey = KeyCode.DownArrow;
    public static KeyCode upArrowKey = KeyCode.UpArrow;
    public static KeyCode rightArrowKey = KeyCode.RightArrow;
    
    public static KeyCode secLeftArrowKey;
    public static KeyCode secDownArrowKey;
    public static KeyCode secUpArrowKey;
    public static KeyCode secRightArrowKey;
    
    NoteObject leftNote;
    NoteObject downNote;
    NoteObject upNote;
    NoteObject rightNote;

    public static bool demoMode = false;

    private void Start()
    {
        
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

        
        if (Song.instance.player1NotesObjects[0].Count != 0)
            leftNote = Song.instance.player1NotesObjects[0][0];
        else
            if(!leftNote.dummyNote || leftNote == null)
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
        


        if (Input.GetKey(leftArrowKey) || Input.GetKey(secLeftArrowKey))
        {
            if (leftNote.susNote && !leftNote.dummyNote)
            {
                print("Next left note is sus");
                if(leftNote.transform.position.y >= 4.55f)
                {
                    print("Attempting to hit left note.");
                    Song.instance.NoteHit(0);
                }
            }
        }
        if (Input.GetKey(downArrowKey) || Input.GetKey(secDownArrowKey))
        {
            if (downNote.susNote && !downNote.dummyNote)
            {
                print("Next down note is sus");
                if(downNote.transform.position.y >= 4.55f)
                {
                    print("Attempting to hit down note.");
                    Song.instance.NoteHit(1);
                }
            }
        }
        if (Input.GetKey(upArrowKey) || Input.GetKey(secUpArrowKey))
        {
            if (upNote.susNote && !upNote.dummyNote)
            {
                print("Next up note is sus");
                if(upNote.transform.position.y >= 4.55f)
                {
                    print("Attempting to hit up note.");
                    Song.instance.NoteHit(2);
                }
            }
        }
        if (Input.GetKey(rightArrowKey) || Input.GetKey(secRightArrowKey))
        {
            if (rightNote.susNote && !rightNote.dummyNote)
            {
                print("Next right note is sus");
                if(rightNote.transform.position.y >= 4.55f)
                {
                    print("Attempting to hit right note.");
                    Song.instance.NoteHit(3);
                }
            }
        }
        
        if (Input.GetKeyDown(leftArrowKey) || Input.GetKeyDown(secLeftArrowKey))
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
        if (Input.GetKeyDown(downArrowKey) || Input.GetKeyDown(secDownArrowKey))
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
        if (Input.GetKeyDown(upArrowKey) || Input.GetKeyDown(secUpArrowKey))
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
        if (Input.GetKeyDown(rightArrowKey) || Input.GetKeyDown(secRightArrowKey))
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

        
        
        
        if (Input.GetKeyUp(leftArrowKey) || Input.GetKeyUp(secLeftArrowKey))
        {
            print("Left up");
            Song.instance.AnimateNote(1, 0, "Normal");
        }
        if (Input.GetKeyUp(downArrowKey) || Input.GetKeyUp(secDownArrowKey))
        {
            print("Down up");
            Song.instance.AnimateNote(1, 1, "Normal");
        }
        if (Input.GetKeyUp(upArrowKey) || Input.GetKeyUp(secUpArrowKey))
        {
            print("Up up");
            Song.instance.AnimateNote(1, 2, "Normal");
        }
        if (Input.GetKeyUp(rightArrowKey) || Input.GetKeyUp(secRightArrowKey))
        {
            print("Right up");
            Song.instance.AnimateNote(1, 3, "Normal");
        }
        

        
    }


    private bool CanHitNote(NoteObject noteObject)
    {
        if (noteObject.transform.position.y <= 4.55 + Song.instance.topSafeWindow & noteObject.transform.position.y >= 4.55 - Song.instance.bottomSafeWindow & !noteObject.dummyNote)
        {
            return true;
        }
        else
        {
            return false;
        }
    }
}
