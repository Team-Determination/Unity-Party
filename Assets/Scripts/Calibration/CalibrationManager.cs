using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using FridayNightFunkin;
using TMPro;
using UnityEditor;
using UnityEngine;

public class CalibrationManager : MonoBehaviour
{
    public static CalibrationManager instance;

    public bool runCalibration;
    public CalibrationMode mode;

    public AudioSource audioSource;
    public AudioClip visualCalibrationClip;

    public TextAsset inputChart;

    public List<List<CalibrationNote>> notes;
    public CalibrationNote dummyNote;
    public Sprite[] noteSprites;
    public Transform[] staticNotes;
    public Animator[] staticNoteAnimators;
    public GameObject calibrationNote;

    [Header("Input Offset")] public Canvas inputCanvas;
    public AudioClip inputCalibrationClip;
    public TextAsset visualChart;
    public TMP_Text suggestedOffsetText;
    public TMP_InputField inputOffsetField;

    [Header("Visual Offset")] public Canvas visualCanvas;
    public TMP_Text currentVisualOffsetText;
    public TMP_InputField visualOffsetField;

    [Space]
    public float currentVisualOffset = 0f;
    public float suggestedOffset = 0f;
    public float currentInputOffset = 0f;
    
    public Stopwatch stopwatch;
    public FNFSong song;
    private void Start()
    {
        instance = this;

        notes = new List<List<CalibrationNote>>
        {
            new List<CalibrationNote>(),
            new List<CalibrationNote>(),
            new List<CalibrationNote>(),
            new List<CalibrationNote>()
        };

        inputOffsetField.text = PlayerPrefs.GetFloat("Input Offset", 0f).ToString(CultureInfo.InvariantCulture);
        visualOffsetField.text = PlayerPrefs.GetFloat("Visual Offset", 0f).ToString(CultureInfo.InvariantCulture);

        SetInputOffset();
        SetVisualOffset();
        
        CalibrateVisuals();
    }

    public void ReturnToMenu()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("Game_Backup3");
    }

    public void CalibrateVisuals()
    {
        stopwatch?.Stop();
        foreach (List<CalibrationNote> noteList in notes)
        {
            for (var index = noteList.Count - 1; index >= 0; index--)
            {
                CalibrationNote note = noteList[index];
                noteList.Remove(note);
                Destroy(note.gameObject);
            }
        }
        
        visualCanvas.enabled = true;
        inputCanvas.enabled = false;

        if (!Directory.Exists(Application.persistentDataPath + "/tmp"))
        {
            Directory.CreateDirectory(Application.persistentDataPath + "/tmp");
        }
            
    
        File.Create(Application.persistentDataPath + "/tmp/tmp.json").Dispose();
        File.WriteAllText(Application.persistentDataPath + "/tmp/tmp.json", visualChart.text);
        
        song = new FNFSong(Application.persistentDataPath + "/tmp/tmp.json");

        foreach (FNFSong.FNFSection section in song.Sections)
        {
            foreach (FNFSong.FNFNote note in section.Notes)
            {
                var noteType = ((int) note.Type);
                if (noteType > 3) return;
                GameObject newNote = Instantiate(calibrationNote);
                newNote.transform.position = new Vector3(staticNotes[noteType].position.x, 0, 0);
                newNote.GetComponentInChildren<SpriteRenderer>().sprite = noteSprites[noteType];
                CalibrationNote noteScript = newNote.GetComponent<CalibrationNote>();
                noteScript.strumTime = (float) note.Time;
                noteScript.Speed = song.Speed;
                noteScript.type = noteType;
                notes[noteType].Add(noteScript);
            }
        }

        File.Delete(Application.persistentDataPath + "/tmp/tmp.json");

        audioSource.clip = visualCalibrationClip;
        audioSource.Play();
        stopwatch = new Stopwatch();
        stopwatch.Start();
        mode = CalibrationMode.Visual;
        runCalibration = true;

        currentVisualOffsetText.text = $"Current Offset: {currentVisualOffset}ms";
    }

    public void CalibrateInputs()
    {
        stopwatch?.Stop();
        foreach (List<CalibrationNote> noteList in notes)
        {
            for (var index = noteList.Count - 1; index >= 0; index--)
            {
                CalibrationNote note = noteList[index];
                noteList.Remove(note);
                print("Removing note");
                Destroy(note.gameObject);
            }
        }
        
        
        visualCanvas.enabled = false;
        inputCanvas.enabled = true;
        File.Create(Application.persistentDataPath + "/tmp/tmp.json").Dispose();
        File.WriteAllText(Application.persistentDataPath + "/tmp/tmp.json", inputChart.text);
        
        song = new FNFSong(Application.persistentDataPath + "/tmp/tmp.json");

        foreach (FNFSong.FNFSection section in song.Sections)
        {
            foreach (FNFSong.FNFNote note in section.Notes)
            {
                var noteType = ((int) note.Type);
                if (noteType > 3) return;
                GameObject newNote = Instantiate(calibrationNote);
                newNote.transform.position = new Vector3(staticNotes[noteType].position.x, 0, 0);
                newNote.GetComponentInChildren<SpriteRenderer>().sprite = noteSprites[noteType];
                CalibrationNote noteScript = newNote.GetComponent<CalibrationNote>();
                noteScript.strumTime = (float) note.Time;
                noteScript.Speed = song.Speed;
                noteScript.type = noteType;
                noteScript.mustHit = section.MustHitSection;
                notes[noteType].Add(noteScript);
            }
        }

        File.Delete(Application.persistentDataPath + "/tmp/tmp.json");

        audioSource.clip = inputCalibrationClip;
        audioSource.Play();
        stopwatch = new Stopwatch();
        stopwatch.Start();
        mode = CalibrationMode.Input;
        runCalibration = true;
    }

    public void SetInputOffset()
    {
        currentInputOffset = float.Parse(inputOffsetField.text);
        PlayerPrefs.SetFloat("Input Offset", currentInputOffset);
        PlayerPrefs.Save();
    }


    public void ResetInputOffset()
    {
        inputOffsetField.text = "0";
        SetInputOffset();
    }

    public void SetVisualOffset()
    {
        currentVisualOffset = float.Parse(visualOffsetField.text);
        currentVisualOffsetText.text = $"Current Offset: {currentVisualOffset}ms";
        PlayerPrefs.SetFloat("Visual Offset", currentVisualOffset);
        PlayerPrefs.Save();
    }

    public void ResetVisualOffset()
    {
        visualOffsetField.text = "0";
        SetVisualOffset();
    }
    
    public enum CalibrationMode
    {
        Visual = 1,
        Input = 2
    }

    private void Update()
    {
        if (mode == CalibrationMode.Input)
        {
            var leftNote = notes[0].Count != 0 ? notes[0][0] : dummyNote;

            var downNote = notes[1].Count != 0 ? notes[1][0] : dummyNote;

            var upNote = notes[2].Count != 0 ? notes[2][0] : dummyNote;

            var rightNote = notes[3].Count != 0 ? notes[3][0] : dummyNote;
        
            if (Input.GetKeyDown(Player.leftArrowKey))
            {
                if (CanHitNote(leftNote))
                {
                    NoteHit(0);
                }
                else
                {
                    AnimateNote(1, 0, "Pressed");
                }
            }
            if (Input.GetKeyDown(Player.downArrowKey))
            {
                if (CanHitNote(downNote))
                {
                    NoteHit(1);
                }
                else
                {
                    AnimateNote(1, 1, "Pressed");
                }
            }
            if (Input.GetKeyDown(Player.upArrowKey))
            {
                if (CanHitNote(upNote))
                {
                    NoteHit(2);
                }
                else
                {
                    AnimateNote(1, 2, "Pressed");
                }
            }
            if (Input.GetKeyDown(Player.rightArrowKey))
            {
                if (CanHitNote(rightNote))
                {
                    NoteHit(3);
                }
                else
                {
                    AnimateNote(1, 3, "Pressed");
                }
            }

            if (Input.GetKeyUp(Player.leftArrowKey))
            {
                AnimateNote(1, 0, "Normal");
            }
            if (Input.GetKeyUp(Player.downArrowKey))
            {
                AnimateNote(1, 1, "Normal");
            }
            if (Input.GetKeyUp(Player.upArrowKey))
            {
                AnimateNote(1, 2, "Normal");
            }
            if (Input.GetKeyUp(Player.rightArrowKey))
            {
                AnimateNote(1, 3, "Normal");
            }
        }
        else
        {
            if (Input.GetKeyDown(KeyCode.Minus))
            {
                EditOffset(!Input.GetKey(KeyCode.LeftShift) ? -1 : -10);
            }

            if (Input.GetKeyDown(KeyCode.Equals))
            {
                EditOffset(!Input.GetKey(KeyCode.LeftShift) ? 1 : 10);
            }

            if (!audioSource.isPlaying)
            {
                CalibrateVisuals();
            }

            
        }
    }

    public void AnimateNote(int player, int type, string animationName)
    {
        
    }

    public bool CanHitNote(CalibrationNote note)
    {
        float noteDiff = (note.strumTime + currentVisualOffset - stopwatch.ElapsedMilliseconds)+currentInputOffset;

        return noteDiff <= 135 * Time.timeScale & noteDiff >= -135 * Time.timeScale;
    }

    public void NoteHit(int type)
    {
        CalibrationNote note = notes[type][0];
        notes[type].Remove(note);
        if(note.mustHit)
        {
            suggestedOffset = stopwatch.ElapsedMilliseconds - (note.strumTime + currentVisualOffset)-currentInputOffset;
            suggestedOffset = -suggestedOffset;
            
            
            suggestedOffsetText.text = "Suggested offset: " + suggestedOffset + "ms";
        }
        
        Destroy(note.gameObject);

    }
    public void NoteMiss(int type)
    {
        CalibrationNote note = notes[type][0];
        notes[type].Remove(note);
        Destroy(note.gameObject);

    }

    public void EditOffset(float value)
    {
        currentVisualOffset += value;
        
        currentVisualOffsetText.text = $"Current Offset: {currentVisualOffset}ms";

        visualOffsetField.text = currentVisualOffset.ToString(CultureInfo.InvariantCulture);
        
        
        PlayerPrefs.SetFloat("Visual Offset", currentVisualOffset);
        PlayerPrefs.Save();

    }
}

