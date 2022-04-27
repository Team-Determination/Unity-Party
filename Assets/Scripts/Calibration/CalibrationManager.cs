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
    public TMP_Text offsetsListText;
    public List<float> inputOffsets;
    

    [Header("Visual Offset")] public Canvas visualCanvas;
    public TMP_Text currentVisualOffsetText;
    public TMP_InputField visualOffsetField;

    [Space]
    public float currentVisualOffset = 0f;
    public float offsetHit = 0f;
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
        
        LoadingTransition.instance.Hide();
    }

    public void ReturnToMenu()
    {
        MenuV2.startPhase = MenuV2.StartPhase.Offset;
    
        UnityEngine.SceneManagement.SceneManager.LoadScene("Title");
        
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
        song = new FNFSong(visualChart.text,FNFSong.DataReadType.AsRawJson);

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
        song = new FNFSong(inputChart.text,FNFSong.DataReadType.AsRawJson);

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


        audioSource.clip = inputCalibrationClip;
        audioSource.Play();
        stopwatch = new Stopwatch();
        stopwatch.Start();
        mode = CalibrationMode.Input;

        offsetsListText.text = "All Offsets:";
        inputOffsets = new List<float>();

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
            
            for (var index = 0; index < Player.primaryKeyCodes.Count; index++)
            {
                KeyCode key = Player.primaryKeyCodes[index];
                CalibrationNote note = dummyNote;
                if(notes[index].Count != 0)
                {
                    note = notes[index][0];
                }

                if (Input.GetKeyDown(key))
                {
                    if (CanHitNote(note))
                    {
                        NoteHit(index);
                    }
                    else
                    {
                        AnimateNote(1, index, "Pressed");
                    }
                }

                if (Input.GetKeyUp(key))
                {
                    AnimateNote(1, index, "Normal");
                }
            }
            for (var index = 0; index < Player.secondaryKeyCodes.Count; index++)
            {
                KeyCode key = Player.secondaryKeyCodes[index];
                CalibrationNote note = dummyNote;
                if(notes[index].Count != 0)
                {
                    note = notes[index][0];
                }

                if (Input.GetKeyDown(key))
                {
                    if (CanHitNote(note))
                    {
                        NoteHit(index);
                    }
                    else
                    {
                        AnimateNote(1, index, "Pressed");
                    }
                }

                if (Input.GetKeyUp(key))
                {
                    AnimateNote(1, index, "Normal");
                }
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
            offsetHit = stopwatch.ElapsedMilliseconds - (note.strumTime + currentVisualOffset)-currentInputOffset;
            offsetHit = -offsetHit;

            inputOffsets.Add(offsetHit);

            offsetsListText.text += $"\n{offsetHit}ms";

            float averageOffset = 0;
            
            foreach (float offset in inputOffsets)
            {
                averageOffset += offset;
            }

            averageOffset /= inputOffsets.Count;
            
            suggestedOffsetText.text = "Suggested / Average Offset: " + averageOffset + "ms";
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

