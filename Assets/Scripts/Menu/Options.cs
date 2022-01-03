using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Options : MonoBehaviour
{
    [Header("Volume Testing")] public GameObject volumeScreen; 
    public AudioClip testVoices;
    public AudioClip testInst;
    public string testData;
    public Slider voiceVolumeSlider;
    public Slider instVolumeSlider;
    public Slider oopsVolumeSlider;
    public Slider completedVolumeSlider;
    public Slider menuVolumeSlider;
    public GameObject saveTooltip;
    public bool isTesting;
    
    [Header("Keybinding")] public TMP_Text primaryLeftKeybindText;
    public TMP_Text primaryDownKeybindText;
    public TMP_Text primaryUpKeybindText;
    public TMP_Text primaryRightKeybindText;
    public TMP_Text secondaryLeftKeybindText;
    public TMP_Text secondaryDownKeybindText;
    public TMP_Text secondaryUpKeybindText;
    public TMP_Text secondaryRightKeybindText;
    public TMP_Text pauseKeybindText;
    public TMP_Text resetKeybindText;
    private KeybindSet _currentKeybindSet;
    private bool _settingKeybind;

    [Header("Note Customizer")] public ColorPicker[] colorPickers;
    public TMP_InputField[] colorFields;
    public GameObject customizeNotesScreen;

    [Header("Misc Options")] public Toggle enableCharacters;
    public Toggle enableScenes;
    public Toggle enableDownscroll;
    public Toggle enableLite;
    public Toggle enableMiddle;
    public Toggle enableGhostTapping;
    public Toggle enableNoHealthGain;

    [Header("Modifier Options")] public Toggle enablePerfect;
    public Toggle enableRandom;
    public Toggle enableLessHPGain;
    public Toggle enableMoreHPGain;
    public Toggle enableLessHPLoss;
    public Toggle enableMoreHPLoss;
    public Toggle enableNoDeath;

    public static Options instance;

    
    public static float menuVolume = .75f;

    public static float instVolume = .75f;

    public static float voiceVolume = .75f;

    public static float completedVolume = .75f;

    public static float oopsVolume = .75f;


    public static bool DualCameras = true;
    public static bool Downscroll;
    public static bool LoadCharacters;
    public static bool LoadScenes;
    public static bool LiteMode;
    public static bool Middlescroll;
    public static bool GhostTapping;

    //SONG MODIFIERS
    public static bool PerfectMode;
    public static bool RandomNotes;
    public static bool LessHealthGain;
    public static bool MoreHealthGain;
    public static bool LessHealthLoss;
    public static bool MoreHealthLoss;
    public static bool NoDeath;
    public static bool NoHealthGain;


    [Space] public GameObject mainOptionsScreen;
    // Start is called before the first frame update
    void Start()
    {
        instance = this;
    
        instVolume = PlayerPrefs.GetFloat("Music Volume", .75f);
        menuVolume = PlayerPrefs.GetFloat("Menu Volume", .75f);
        voiceVolume = PlayerPrefs.GetFloat("Voice Volume", .75f);
        completedVolume = PlayerPrefs.GetFloat("Completed Volume", .75f);
        oopsVolume = PlayerPrefs.GetFloat("Oops Volume", .40f);

        Song.instance.musicSources[0].volume = menuVolume;
        Song.instance.musicSources[1].volume = voiceVolume;
        Song.instance.oopsSource.volume = oopsVolume;
        Song.instance.vocalSource.volume = voiceVolume;
        
        voiceVolumeSlider.value = voiceVolume;
        instVolumeSlider.value = instVolume;
        oopsVolumeSlider.value = oopsVolume;
        completedVolumeSlider.value = completedVolume;
        menuVolumeSlider.value = menuVolume;


        customizeNotesScreen.SetActive(true);
        
        foreach (var colorPicker in colorPickers)
        {
            colorPicker.onColorChanged += OnColorUpdated;
        }
        
        /*
        Load the player's note preferences too.
        */

        LoadNotePrefs();
        LoadMiscPrefs();

        customizeNotesScreen.SetActive(false);

        /*
         * This is something I am not proud of and I am pretty sure
         * there's a possible way to make it better, but this is what
         * I came up with and it works so fuck it.
         *
         * It grabs a saved JSON string and tries to convert it from JSON to a SavedKeybinds class.
         * If there's no JSON string, it'll be empty so the game will auto-generate and save
         * a default SavedKeybinds class JSON value.
         */
        string keys = PlayerPrefs.GetString("Saved Keybinds", String.Empty);

        SavedKeybinds savedKeybinds = new SavedKeybinds();

        if (!string.IsNullOrWhiteSpace(keys))
        {
            savedKeybinds = JsonConvert.DeserializeObject<SavedKeybinds>(keys);
        }
        else
        {
            PlayerPrefs.SetString("Saved Keybinds", JsonConvert.SerializeObject(savedKeybinds));
            PlayerPrefs.Save();
        }
        
        
        /*
         * It will then take each keybind in the referenced SavedKeybinds class
         * and assign them to the Player class variables respectively.
         *
         * We will also update the text in the Game Options for the KeyBinds.
         */
        Player.leftArrowKey = savedKeybinds.primaryLeftKeyCode;
        Player.downArrowKey = savedKeybinds.primaryDownKeyCode;
        Player.upArrowKey = savedKeybinds.primaryUpKeyCode;
        Player.rightArrowKey = savedKeybinds.primaryRightKeyCode;
        Player.secLeftArrowKey = savedKeybinds.secondaryLeftKeyCode;
        Player.secDownArrowKey = savedKeybinds.secondaryDownKeyCode;
        Player.secUpArrowKey = savedKeybinds.secondaryUpKeyCode;
        Player.secRightArrowKey = savedKeybinds.secondaryRightKeyCode;
        Player.pauseKey = savedKeybinds.pauseKeyCode;
        Player.resetKey = savedKeybinds.resetKeyCode;

        primaryLeftKeybindText.text = "LEFT\n" + savedKeybinds.primaryLeftKeyCode;
        primaryDownKeybindText.text = "DOWN\n" + savedKeybinds.primaryDownKeyCode;
        primaryUpKeybindText.text = "UP\n" + savedKeybinds.primaryUpKeyCode;
        primaryRightKeybindText.text = "RIGHT\n" + savedKeybinds.primaryRightKeyCode;
        secondaryLeftKeybindText.text = "LEFT\n" + savedKeybinds.secondaryLeftKeyCode;
        secondaryDownKeybindText.text = "DOWN\n" + savedKeybinds.secondaryDownKeyCode;
        secondaryUpKeybindText.text = "UP\n" + savedKeybinds.secondaryUpKeyCode;
        secondaryRightKeybindText.text = "RIGHT\n" + savedKeybinds.secondaryRightKeyCode;
        pauseKeybindText.text = "PAUSE\n" + savedKeybinds.pauseKeyCode;
        resetKeybindText.text = "RESET\n" + savedKeybinds.resetKeyCode;
    }

    public void JoinDiscordServer()
    {
        Application.OpenURL("https://discord.gg/KSGTkaCFRr");
    }

    public void CalibrateNotes()
    {
        SceneManager.LoadScene("Calibration");
    }
    
    public void OldTestVolume()
    {
        Player.autoPlay = true;
        Player.twoPlayers = false;
        Player.playAsEnemy = false;


        Directory.CreateDirectory(Application.persistentDataPath + "/tmp");

        StreamWriter testFile = File.CreateText(Application.persistentDataPath + "/tmp/ok.json");
        testFile.Write(testData);
        testFile.Close();

        Song.instance.vocalClip = testVoices;
        Song.instance.musicClip = testInst;

        voiceVolumeSlider.transform.parent.gameObject.SetActive(true);
        instVolumeSlider.transform.parent.gameObject.SetActive(true);
        saveTooltip.SetActive(true);

        Song.instance.jsonDir = Application.persistentDataPath + "/tmp/ok.json";
        Song.instance.GenerateSong();

        isTesting = true;
    }
    #region Volume
    public void ChangeInstVolume(float value)
    {
        instVolume = value;
        
        Song.instance.musicSources[0].volume = instVolume;
    }

    public void ChangeVocalVolume(float value)
    {
        voiceVolume = value;

        Song.instance.vocalSource.volume = voiceVolume;
        Song.instance.musicSources[1].volume = voiceVolume;
    }

    public void ChangeOopsVolume(float value)
    {
        oopsVolume = value;

        Song.instance.oopsSource.volume = oopsVolume;
    }

    public void ChangeMenuVolume(float value)
    {
        menuVolume = value;
    }

    public void ChangeCompletedVolume(float value)
    {
        completedVolume = value;
    }

    public void TestVolume()
    {
        Song.instance.musicSources[0].Stop();
        
        Song.instance.musicSources[0].clip = testInst;
        Song.instance.musicSources[0].volume = instVolume;
        Song.instance.vocalSource.clip = testVoices;

        Song.instance.musicSources[0].loop = true;
        Song.instance.vocalSource.loop = true;
        
        Song.instance.musicSources[0].Play();
        Song.instance.vocalSource.Play();
    }

    public void SaveVolume()
    {
        instVolume = instVolumeSlider.value;
        menuVolume = menuVolumeSlider.value;
        voiceVolume = voiceVolumeSlider.value;
        oopsVolume = oopsVolumeSlider.value;
        completedVolume = completedVolumeSlider.value;

        PlayerPrefs.SetFloat("Music Volume",instVolume);
        PlayerPrefs.SetFloat("Menu Volume",menuVolume);
        PlayerPrefs.SetFloat("Voice Volume",voiceVolume);
        PlayerPrefs.SetFloat("Oops Volume", oopsVolume);
        PlayerPrefs.SetFloat("Completed Volume", completedVolume);

        PlayerPrefs.Save();
        if (!Song.instance.songStarted)
        {
            Song.instance.musicSources[0].Stop();
            Song.instance.vocalSource.Stop();

            Song.instance.musicSources[0].clip = Menu.instance.menuTheme;
            Song.instance.vocalSource.clip = null;

            Song.instance.musicSources[0].loop = true;
            Song.instance.vocalSource.loop = false;

            Song.instance.musicSources[0].volume = menuVolume;
            Song.instance.musicSources[0].Play();
        }
        else
        {
            Pause.instance.SaveVolume();
        }
    }
    #endregion
    #region Misc Options
    public void OnMiddlescrollChanged(bool val)
    {
        Middlescroll = val;

        PlayerPrefs.SetInt("Middlescroll", val ? 1 : 0);
        PlayerPrefs.Save();
    }

    public void OnDownscrollChanged(bool val)
    {
        Downscroll = val;

        PlayerPrefs.SetInt("Downscroll", val ? 1 : 0);
        PlayerPrefs.Save();
    }

    public void OnLiteModeChanged(bool val)
    {
        LiteMode = val;

        Song.instance.girlfriendObject.SetActive(!val);

        PlayerPrefs.SetInt("Lite Mode", val ? 1 : 0);
        PlayerPrefs.Save();
    }

    public void OnGhostTappingChanged(bool val)
    {
        GhostTapping = val;

        PlayerPrefs.SetInt("Ghost Tapping", val ? 1 : 0);
        PlayerPrefs.Save();
    }
    
    public void LoadMiscPrefs()
    {
        if (PlayerPrefs.GetInt("Lite Mode",0) == 1)
        {
            LiteMode = true;

            Song.instance.girlfriendObject.SetActive(false);

            enableLite.isOn = true;

            LiteMode = true;
        }
        else
        {
            enableLite.isOn = false;
            LiteMode = false;
        }
        
        if (PlayerPrefs.GetInt("Middlescroll", 0) == 1)
        {
            Middlescroll = true;
            enableMiddle.isOn = true;
        }
        else
        {
            Middlescroll = false;
            enableMiddle.isOn = false;
        }

        if (PlayerPrefs.GetInt("Downscroll", 0) == 1)
        {
            Downscroll = true;
            enableDownscroll.isOn = true;
        }
        else
        {
            Downscroll = false;
            enableDownscroll.isOn = false;
        }

        if (PlayerPrefs.GetInt("Ghost Tapping", 0) == 1)
        {
            GhostTapping = true;
            enableGhostTapping.isOn = true;
        }
        else
        {
            GhostTapping = false;
            enableGhostTapping.isOn = false;
        }
    }
    #endregion

    #region Song Modifiers

    public void OnPerfectModeChanged(bool val)
    {
        PerfectMode = val;

        if(val)
        {
            NoDeath = false;
            enableNoDeath.SetIsOnWithoutNotify(false);
        }
    }

    public void OnRandomModeChanged(bool val)
    {
        RandomNotes = val;
    }

    public void OnLessHPGainModeChanged(bool val)
    {
        LessHealthGain = val;
        if(val)
        {
            MoreHealthGain = false;
            enableMoreHPGain.SetIsOnWithoutNotify(false);

            NoHealthGain = false;
            enableNoHealthGain.SetIsOnWithoutNotify(false);
        }
    }

    public void OnMoreHPGainModeChanged(bool val)
    {
        MoreHealthGain = val;
        if(val)
        {
            LessHealthGain = false;
            enableLessHPGain.SetIsOnWithoutNotify(false);

            NoHealthGain = false;
            enableNoHealthGain.SetIsOnWithoutNotify(false);
        }
    }

    public void OnLessHPLossModeChanged(bool val)
    {
        LessHealthLoss = val;
        if(val)
        {
            MoreHealthLoss = false;
            enableMoreHPLoss.SetIsOnWithoutNotify(false);
        }
    }

    public void OnMoreHPLossModeChanged(bool val)
    {
        MoreHealthLoss = val;
        if(val)
        {
            LessHealthLoss = false;
            enableLessHPLoss.SetIsOnWithoutNotify(false);
        }
    }

    public void OnNoDeathModeChanged(bool val)
    {
        NoDeath = val;
        if(val)
        {
            PerfectMode = false;
            enablePerfect.SetIsOnWithoutNotify(false);
        }
    }

    public void OnNoHealthGainChanged(bool val)
    {
        NoHealthGain = val;
        if(val)
        {
            PerfectMode = false;
            enablePerfect.SetIsOnWithoutNotify(false);

            MoreHealthGain = false;
            enableMoreHPGain.SetIsOnWithoutNotify(false);

            LessHealthGain = false;
            enableLessHPGain.SetIsOnWithoutNotify(false);
        }
    }

    public void ClearModifiers()
    {
        enablePerfect.isOn = false;
        enableRandom.isOn = false;
        enableLessHPGain.isOn = false;
        enableMoreHPGain.isOn = false;
        enableLessHPLoss.isOn = false;
        enableMoreHPLoss.isOn = false;
        enableNoHealthGain.isOn = false;
        enableNoDeath.isOn = false;
    }

    #endregion

    #region Note Color
    public void OnColorUpdated(Color color)
    {
        for (int i = 0; i < colorPickers.Length; i++)
        {
            colorFields[i].text = ColorUtility.ToHtmlStringRGB(colorPickers[i].color);
        }
    }

    public void OnLeftArrowFieldUpdated(string colorString)
    {
        colorString = "#" + colorString.Replace("#", "");

        if (colorString.Length != 7) return;
        if (ColorUtility.TryParseHtmlString(colorString, out var color))
        {
            colorPickers[0].color = color;
        }
    }
    public void OnDownArrowFieldUpdated(string colorString)
    {
        colorString = "#" + colorString.Replace("#", "");

        if (colorString.Length != 7) return;
        if (ColorUtility.TryParseHtmlString(colorString, out var color))
        {
            colorPickers[1].color = color;
        }
    }
    public void OnUpArrowFieldUpdated(string colorString)
    {
        colorString = "#" + colorString.Replace("#", "");

        if (colorString.Length != 7) return;
        if (ColorUtility.TryParseHtmlString(colorString, out var color))
        {
            colorPickers[2].color = color;
        }
    }
    public void OnRightArrowFieldUpdated(string colorString)
    {
        colorString = "#" + colorString.Replace("#", "");

        if (colorString.Length != 7) return;
        if (ColorUtility.TryParseHtmlString(colorString, out var color))
        {
            colorPickers[3].color = color;
        }
    }

    


    public void SaveNotePrefs()
    {
        List<Color> colors = new List<Color>();
        foreach (ColorPicker picker in colorPickers)
        {
            colors.Add(picker.color);
        }

        NoteCustomization noteCustomization = new NoteCustomization
        {
            savedColors = colors.ToArray()
        };

        PlayerPrefs.SetString("Note Customization", JsonConvert.SerializeObject(noteCustomization));
    }
    public void LoadNotePrefs()
    {
        string savedCustomization = PlayerPrefs.GetString("Note Customization");

        if (!string.IsNullOrWhiteSpace(savedCustomization))
        {
            NoteCustomization noteCustomization = JsonConvert.DeserializeObject<NoteCustomization>(savedCustomization);

            for (int i = 0; i < colorPickers.Length; i++)
            {
                // ReSharper disable once PossibleNullReferenceException
                colorPickers[i].color = noteCustomization.savedColors[i];
                colorFields[i].text = ColorUtility.ToHtmlStringRGB(noteCustomization.savedColors[i]);
            }
        }
        else
        {
            for (int i = 0; i < colorPickers.Length; i++)
            {
                Color selectedColor = NoteCustomization.defaultFnfColors[i];
                
                colorPickers[i].color = selectedColor;

                colorFields[i].text = ColorUtility.ToHtmlStringRGB(selectedColor);
            }

            PlayerPrefs.SetString("Note Customization",
                JsonConvert.SerializeObject(new NoteCustomization {savedColors = NoteCustomization.defaultFnfColors}));
            PlayerPrefs.Save();
        }
    }
    #endregion
    #region Keybinds
    public enum KeybindSet
    {
        PrimaryLeft = 1,
        PrimaryDown = 2,
        PrimaryUp = 3,
        PrimaryRight = 4,
        SecondaryLeft = 5,
        SecondaryDown = 6,
        SecondaryUp = 7,
        SecondaryRight = 8,
        Pause = 9,
        Reset = 10
    }

    public void ChangeKeybind(int key)
    {
        KeybindSet keybind = (KeybindSet) Enum.ToObject(typeof(KeybindSet), key);

        _currentKeybindSet = keybind;
        _settingKeybind = true;

        switch (keybind)
        {
            case KeybindSet.PrimaryLeft:
                primaryLeftKeybindText.text = "LEFT\nPress a Key";
                break;
            case KeybindSet.PrimaryDown:
                primaryDownKeybindText.text = "DOWN\nPress a Key";
                break;
            case KeybindSet.PrimaryUp:
                primaryUpKeybindText.text = "UP\nPress a Key";
                break;
            case KeybindSet.PrimaryRight:
                primaryRightKeybindText.text = "RIGHT\nPress a Key";
                break;
            case KeybindSet.SecondaryLeft:
                secondaryLeftKeybindText.text = "LEFT\nPress a Key";
                break;
            case KeybindSet.SecondaryDown:
                secondaryDownKeybindText.text = "DOWN\nPress a Key";
                break;
            case KeybindSet.SecondaryUp:
                secondaryUpKeybindText.text = "UP\nPress a Key";
                break;
            case KeybindSet.SecondaryRight:
                secondaryRightKeybindText.text = "RIGHT\nPress a Key";
                break;
            case KeybindSet.Pause:
                pauseKeybindText.text = "PAUSE\nPress a Key";
                break;
            case KeybindSet.Reset:
                resetKeybindText.text = "RESET\nPress a Key";
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
    #endregion
    // Update is called once per frame
    void Update()
    {
        if (_settingKeybind)
        {
            if (!Input.anyKeyDown) return;

            KeyCode newKey = KeyCode.A;
            
            foreach(KeyCode kcode in Enum.GetValues(typeof(KeyCode)))
            {
                if (Input.GetKeyDown(kcode))
                {
                    newKey = kcode;
                    break;
                }
            }
            
            switch (_currentKeybindSet)
            {
                case KeybindSet.PrimaryLeft:
                    primaryLeftKeybindText.text = "LEFT\n" + newKey;
                    Player.leftArrowKey = newKey;
                    break;
                case KeybindSet.PrimaryDown:
                    primaryDownKeybindText.text = "DOWN\n" + newKey;
                    Player.downArrowKey = newKey;
                    break;
                case KeybindSet.PrimaryUp:
                    primaryUpKeybindText.text = "UP\n" + newKey;
                    Player.upArrowKey = newKey;
                    break;
                case KeybindSet.PrimaryRight:
                    primaryRightKeybindText.text = "RIGHT\n" + newKey;
                    Player.rightArrowKey = newKey;
                    break;
                case KeybindSet.SecondaryLeft:
                    secondaryLeftKeybindText.text = "LEFT\n" + newKey;
                    Player.secLeftArrowKey = newKey;
                    break;
                case KeybindSet.SecondaryDown:
                    secondaryDownKeybindText.text = "DOWN\n" + newKey;
                    Player.secDownArrowKey = newKey;
                    break;
                case KeybindSet.SecondaryUp:
                    secondaryUpKeybindText.text = "UP\n" + newKey;
                    Player.secUpArrowKey = newKey;
                    break;
                case KeybindSet.SecondaryRight:
                    secondaryRightKeybindText.text = "RIGHT\n" + newKey;
                    Player.secRightArrowKey = newKey;
                    break;
                case KeybindSet.Pause:
                    pauseKeybindText.text = "PAUSE\n" + newKey;
                    Player.pauseKey = newKey;
                    break;
                case KeybindSet.Reset:
                    resetKeybindText.text = "RESET\n" + newKey;
                    Player.resetKey = newKey;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            Player.SaveKeySet();
            _settingKeybind = false;
        }
    }
}
