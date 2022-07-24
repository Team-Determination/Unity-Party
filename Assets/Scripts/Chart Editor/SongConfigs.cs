using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using TMPro;
using FridayNightFunkin;
using System.Globalization;
using System;
using UnityEngine.SceneManagement;

public class SongConfigs : MonoBehaviour
{
    [Header("Song Screen")]
    public TMP_InputField songName, bpm, speed;
    public Button saveTemp, import, bpmMore, bpmLess, speedMore, speedLess;
    public Toggle hasVoiceTracks;
    public InputFieldWithButtons bpmInputConfigs, speedInputConfigs;
    public TMP_Dropdown playerSkin, enemySkin;
    public static FNFSong song;
    public static FNFSong editedSong;
    

    private void Start() {
        LoadingTransition.instance.Hide();
        Song.inEditorMode = true;
        DiscordController.instance.SetEditorState(song.SongName);
        bpmInputConfigs = new InputFieldWithButtons(bpm, new Button[] { bpmMore, bpmLess }, 1, 1, song.Bpm);
        speedInputConfigs = new InputFieldWithButtons(speed, new Button[] { speedMore, speedLess }, 0.1f, 0.1f, song.Speed);
        TryParse();
    }

    void TryParse() {
        try {
            songName.text = song.SongName;
            bpm.text = song.Bpm.ToString();
            speed.text = song.Speed.ToString();
            hasVoiceTracks.SetIsOnWithoutNotify(song.NeedVoices);
        } catch {
            print("fuck");
        }
    }

    public void TrySaveTemp() {
        editedSong = song;
        editedSong.Bpm = Convert.ToInt32(bpm.text);
        editedSong.Speed = (long)float.Parse(speed.text, CultureInfo.InvariantCulture);
        editedSong.SongName = songName.text;
        editedSong.NeedVoices = hasVoiceTracks;
        Song.editedSong = editedSong;
        LoadingTransition.instance.Show(() => SceneManager.LoadScene("Game_Backup3"));
    }
}


public class InputFieldWithButtons {
    public TMP_InputField input;
    public Button[] buttons;
    public float value;
    public float addValue;
    public float subValue;

    public InputFieldWithButtons(TMP_InputField input, Button[] buttons, float addValue, float subValue, float value) {
        this.addValue = addValue;
        this.subValue = subValue;
        this.input = input;
        this.buttons = buttons;
        this.value = value;
        buttons[0].onClick.AddListener(delegate {
            AddValue();
        });

        buttons[1].onClick.AddListener(delegate {
            SubValue();
        });
    }

    public void AddValue() {
        value += addValue;
        input.text = value.ToString();
    }

    public void SubValue() {
        value -= subValue;
        input.text = value.ToString();
    }
}
