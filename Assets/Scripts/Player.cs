using System;
using System.Collections;
using System.Diagnostics.CodeAnalysis;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[SuppressMessage("ReSharper", "InconsistentNaming")]
public class Player : MonoBehaviour
{
    public LayerMask listeningLayer;

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

    private bool _leftDown = false;
    private TouchControl _leftControl;
    private bool _downDown = false;
    private TouchControl _downControl;
    private bool _upDown = false;
    private TouchControl _upControl;
    private bool _rightDown = false;
    private TouchControl _rightControl;

    private Camera _camera;

    public static bool demoMode = false;

    private EventSystem _eventSystem;

    public static Player Instance;

    public bool[] simulateHolding = {false,false,false,false};

    private void Start()
    {
        Instance = this;
        _camera = Camera.main;
        _eventSystem = EventSystem.current;
        UpdateKeySet();

        _leftControl = Song.instance.leftTouch.GetComponent<TouchControl>();
        _downControl = Song.instance.downTouch.GetComponent<TouchControl>();
        _upControl = Song.instance.upTouch.GetComponent<TouchControl>();
        _rightControl = Song.instance.rightTouch.GetComponent<TouchControl>();
    }

    public static void UpdateKeySet()
    {
        switch (PlayerPrefs.GetInt("Key Set", 0))
        {
            case 0:
                secLeftArrowKey = KeyCode.A;
                secDownArrowKey = KeyCode.S;
                secUpArrowKey = KeyCode.W;
                secRightArrowKey = KeyCode.D;
                break;
            case 1:
                secLeftArrowKey = KeyCode.D;
                secDownArrowKey = KeyCode.F;
                secUpArrowKey = KeyCode.J;
                secRightArrowKey = KeyCode.K;
                break;

        }
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
        
        #region Android Input System
        #if UNITY_ANDROID

        /*if (Input.touchCount != 0)
        {
            print("YOOOOO WE GOT A TOUCH");
            foreach (Touch touch in Input.touches)
            {
                print(" checking a TOUCYH REWEEEEEE");
                if (Physics.Raycast(_camera.ScreenPointToRay(touch.position), out var hit,9999f))
                {
                    print("TOUCHING THE THINGY MAJECH TIRHGIORIDJIDFIO");
                    
                }
            }
            
        }*/

        int currIndex = 1;
        
        foreach (bool simHold in simulateHolding)
        {
            if(simHold)
                OnAndroidInput(currIndex, InputType.Hold);
            currIndex++;
            print("Simulating holding for " + currIndex);
        }
        #endif
        #endregion
        
        #region PC Input System
#if UNITY_STANDALONE

        if (Input.GetKey(leftArrowKey) || Input.GetKey(secLeftArrowKey))
        {
            if (leftNote.susNote && !leftNote.dummyNote)
            {
                print("Next left note is sus");
                if(leftNote.transform.position.y >= 4.35)
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
                if(downNote.transform.position.y >= 4.35)
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
                if(upNote.transform.position.y >= 4.35)
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
                if(rightNote.transform.position.y >= 4.35)
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
        

        #endif
#endregion
    }
    
    #region Android Input Callbacks
    #if UNITY_ANDROID
    public enum InputType
    {
        Press = 1,
        Hold = 2,
        Release = 3
    }

    public void OnAndroidInput(int type, InputType inputType)
    {
        switch (type)
        {
            case 1:
                switch (inputType)
                {
                    case InputType.Press:
                        if (CanHitNote(leftNote))
                        {
                            Song.instance.NoteHit(0);
                        }
                        else
                        {
                            Song.instance.AnimateNote(1, 0, "Pressed");
                            Song.instance.NoteMiss(0);
                        }

                        _leftControl.selected = true;
                        break;
                    
                    case InputType.Hold:
                        if (leftNote.susNote && !leftNote.dummyNote)
                        {
                            print("Next left note is sus");
                            if(leftNote.transform.position.y >= 4.35)
                            {
                                print("Attempting to hit left note.");
                                Song.instance.NoteHit(0);
                            }
                        }
                        break;
                    case InputType.Release:
                        print("Left up");
                        Song.instance.AnimateNote(1, 0, "Normal");
                        _leftControl.selected = false;
                        break;
                }
                

                break;
            case 2:
                switch (inputType)
                {
                    case InputType.Press:
                        if (CanHitNote(leftNote))
                        {
                            Song.instance.NoteHit(1);
                        }
                        else
                        {
                            Song.instance.AnimateNote(1, 1, "Pressed");
                            Song.instance.NoteMiss(1);
                        }

                        _downControl.selected = true;
                        break;
                    case InputType.Hold:
                        if (downNote.susNote && !downNote.dummyNote)
                        {
                            print("Next down note is sus");
                            if(downNote.transform.position.y >= 4.35)
                            {
                                print("Attempting to hit down note.");
                                Song.instance.NoteHit(1);
                            }
                        }
                        break;
                    case InputType.Release:
                        print("Down up");
                        Song.instance.AnimateNote(1, 1, "Normal");
                        _downControl.selected = false;
                        break;
                }

                break;
            case 3:
                switch (inputType)
                {
                    case InputType.Press:
                        if (CanHitNote(upNote))
                        {
                            Song.instance.NoteHit(2);
                        }
                        else
                        {
                            Song.instance.AnimateNote(1, 2, "Pressed");
                            Song.instance.NoteMiss(2);
                        }

                        _upControl.selected = true;
                        break;
                    case InputType.Hold:
                        if (upNote.susNote && !upNote.dummyNote)
                        {
                            print("Next up note is sus");
                            if(upNote.transform.position.y >= 4.35)
                            {
                                print("Attempting to hit up note.");
                                Song.instance.NoteHit(2);
                            }
                        }
                        break;
                    case InputType.Release:
                        print("Up up");
                        Song.instance.AnimateNote(1, 2, "Normal");
                        _upControl.selected = false;
                        break;
                }
                

                break;
            case 4:
                switch (inputType)
                {
                    case InputType.Press:
                        if (CanHitNote(rightNote))
                        {
                            Song.instance.NoteHit(3);
                        }
                        else
                        {
                            Song.instance.AnimateNote(1, 3, "Pressed");
                            Song.instance.NoteMiss(3);
                        }

                        _rightControl.selected = true;
                        break;
                    case InputType.Hold:
                        if (rightNote.susNote && !rightNote.dummyNote)
                        {
                            print("Next right note is sus");
                            if(rightNote.transform.position.y >= 4.35)
                            {
                                print("Attempting to hit right note.");
                                Song.instance.NoteHit(3);
                            }
                        }
                        break;
                    case InputType.Release:
                        print("Right up");
                        Song.instance.AnimateNote(1, 3, "Normal");
                        _rightControl.selected = false;
                        break;
                }
                

                break;
        }
    }
    #endif
    #endregion
    
    
    
    


    private bool CanHitNote(NoteObject noteObject)
    {
        if (noteObject.transform.position.y <= 4.45 + Song.instance.topSafeWindow & noteObject.transform.position.y >= 4.45 - Song.instance.bottomSafeWindow & !noteObject.dummyNote)
        {
            return true;
        }
        else
        {
            return false;
        }
    }
}


