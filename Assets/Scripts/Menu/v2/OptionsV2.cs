using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using Newtonsoft.Json;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class OptionsV2 : MonoBehaviour
{
    [Header("Game Options")] public GameObject[] allWindows;
    [Space]
    public Button[] allTabs;

    public Color selectedTabColor;
    public Color unselectedTabColor;

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
    public TMP_Text startSongKeybindText;
    private KeybindSet _currentKeybindSet;
    private bool _settingKeybind;

    [Header("Note Colors")] public ColorPicker[] colorPickers;

    public TMP_InputField[] colorFields;

    [Header("Volumes")] public TMP_InputField instVolField;
    public Slider instVolSlider;
    public static float instVolume;
    public TMP_InputField voicesVolField;
    public Slider voicesVolSlider;
    public static float voicesVolume;
    public TMP_InputField missVolField;
    public Slider missVolSlider;
    public static float missVolume;
    public TMP_InputField menuVolField;
    public Slider menuVolSlider;
    public static float menuVolume;
    public TMP_InputField miscVolField;
    public Slider miscVolSlider;
    public static float miscVolume;

    public static OptionsV2 instance;

    public static bool LiteMode { get; set; }
    [Header("Misc")]

    public Toggle liteModeToggle;
    public static bool Downscroll { get; set; }
    public Toggle downscrollToggle;
    public static bool Middlescroll { get; set; }
    public Toggle middleScrollToggle;
    public static bool DebugMode { get; set; }
    public Toggle debugModeToggle;

    public static bool DesperateMode
    {
        get => _desperateMode;
        set
        {
            if (value)
            {
                LiteMode = true;
                _desperateMode = true;
            }
            else
            {
                _desperateMode = false;
            }
        }
    }

    private static bool _desperateMode;

    public Toggle desperateModeToggle;
    public static bool SongDuration { get; set; }
    public Toggle songDurationToggle;

    // Start is called before the first frame update
    void Start()
    {
        LoadKeybinds();
        LoadNotePrefs();
        LoadMisc();
        LoadVolumeProperties();
        HookVolumeCallbacks();

        instance = this;
    }
    #region Options Windows
    public void ChangeOptionsWindow(GameObject window)
    {
        foreach (GameObject optionWindow in allWindows)
            optionWindow.SetActive(false);

        window.SetActive(true);
    }

    public void ChangeOptionsTab(Button tab)
    {
        ColorBlock colorBlock;
        foreach (Button tabButton in allTabs)
        {
            colorBlock = tabButton.colors;
            colorBlock.normalColor = unselectedTabColor;
            tabButton.colors = colorBlock;
        }
        colorBlock = tab.colors;
        colorBlock.normalColor = selectedTabColor;
        tab.colors = colorBlock;
        
    }
    #endregion
    #region Note Colors

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
    #endregion
    #region Keybinds
    private void LoadKeybinds()
    {
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
        Player.startSongKey = savedKeybinds.startSongKeyCode;

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
        startSongKeybindText.text = "START SONG\n" + savedKeybinds.startSongKeyCode;
    }


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
        Reset = 10,
        StartSong = 11
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
            case KeybindSet.StartSong:
                startSongKeybindText.text = "START SONG\nPress a Key";
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
    #endregion
    #region Volumes

    public void LoadVolumeProperties()
    {
        string volumeData = PlayerPrefs.GetString("Volume", JsonConvert.SerializeObject(new VolumeProperties()));

        VolumeProperties volumeProperties = JsonConvert.DeserializeObject<VolumeProperties>(volumeData);

        instVolSlider.value = volumeProperties.instVol;
        voicesVolSlider.value = volumeProperties.voicesVol;
        menuVolSlider.value = volumeProperties.menuVol;
        missVolSlider.value = volumeProperties.missVol;
        miscVolSlider.value = volumeProperties.miscVol;

        instVolField.SetTextWithoutNotify((volumeProperties.instVol).ToString(CultureInfo.InvariantCulture));
        voicesVolField.SetTextWithoutNotify((volumeProperties.voicesVol).ToString(CultureInfo.InvariantCulture));
        menuVolField.SetTextWithoutNotify((volumeProperties.menuVol).ToString(CultureInfo.InvariantCulture));
        missVolField.SetTextWithoutNotify((volumeProperties.missVol).ToString(CultureInfo.InvariantCulture));
        miscVolField.SetTextWithoutNotify((volumeProperties.miscVol).ToString(CultureInfo.InvariantCulture));

        instVolume = volumeProperties.instVol/100;
        voicesVolume = volumeProperties.voicesVol/100;
        menuVolume = volumeProperties.menuVol/100;
        missVolume = volumeProperties.missVol/100;
        miscVolume = volumeProperties.miscVol/100;

        MenuV2.Instance.musicSource.volume = menuVolume;
    }
    public void HookVolumeCallbacks()
    {
        instVolSlider.onValueChanged.AddListener(OnInstSliderChange);
        voicesVolSlider.onValueChanged.AddListener(OnVoicesSliderChange);
        menuVolSlider.onValueChanged.AddListener(OnMenuSliderChange);
        missVolSlider.onValueChanged.AddListener(OnMissSliderChange);
        miscVolSlider.onValueChanged.AddListener(OnMiscSliderChange);

        instVolField.onEndEdit.AddListener(OnInstFieldChange);
        voicesVolField.onEndEdit.AddListener(OnVoicesFieldChange);
        menuVolField.onEndEdit.AddListener(OnMenuFieldChange);
        missVolField.onEndEdit.AddListener(OnMissFieldChange);
        miscVolField.onEndEdit.AddListener(OnMiscFieldChange);
    }

    public void OnInstSliderChange(float value)
    {
        instVolField.SetTextWithoutNotify(value.ToString(CultureInfo.InvariantCulture));

        instVolume = value / 100;
        
    }

    public void OnInstFieldChange(string text)
    {
        var volume = float.Parse(text);
        instVolume = volume / 100;

        instVolSlider.value = volume;
    }
    public void OnVoicesSliderChange(float value)
    {
        voicesVolField.SetTextWithoutNotify(value.ToString(CultureInfo.InvariantCulture));

        voicesVolume = value / 100;;
    }

    public void OnVoicesFieldChange(string text)
    {
        var volume = float.Parse(text);
        voicesVolume = volume / 100;

        voicesVolSlider.value = volume;
    }
    public void OnMissSliderChange(float value)
    {
        missVolField.SetTextWithoutNotify(value.ToString(CultureInfo.InvariantCulture));

        missVolume = value / 100;;
    }

    public void OnMissFieldChange(string text)
    {
        var volume = float.Parse(text);
        missVolume = volume / 100;

        missVolSlider.value = volume;
    }
    public void OnMenuSliderChange(float value)
    {
        menuVolField.SetTextWithoutNotify(value.ToString(CultureInfo.InvariantCulture));

        menuVolume = value / 100;;
    }

    public void OnMenuFieldChange(string text)
    {
        var volume = float.Parse(text);
        menuVolume = volume / 100;

        menuVolSlider.value = volume;
    }
    public void OnMiscSliderChange(float value)
    {
        menuVolField.SetTextWithoutNotify(value.ToString(CultureInfo.InvariantCulture));

        menuVolume = value / 100;;
    }

    public void OnMiscFieldChange(string text)
    {
        var volume = float.Parse(text);
        menuVolume = volume / 100;

        menuVolSlider.value = volume;
    }

    public void SaveVolume()
    {
        VolumeProperties volumeProperties = new VolumeProperties
        {
            instVol = instVolume * 100,
            voicesVol = voicesVolume * 100,
            menuVol = menuVolume * 100,
            missVol = missVolume * 100,
            miscVol = miscVolume * 100
        };

        PlayerPrefs.SetString("Volume", JsonConvert.SerializeObject(volumeProperties));
        PlayerPrefs.Save();

        MenuV2.Instance.musicSource.volume = menuVolume;
    }
    
    
    #endregion
    #region Offset

    public void OpenOffset()
    {
        SceneManager.LoadScene("Calibration");
    }
    #endregion
    #region Misc

    public void DesperateModeToggle(bool value)
    {
        if (value)
        {
            liteModeToggle.isOn = true;
            LiteMode = true;
        }
    }
    
    public void SaveMisc()
    {
        MiscOptions options = new MiscOptions
        {
            enableDownscroll = Downscroll,
            enableMiddlescroll = Middlescroll,
            enableLiteMode = LiteMode,
            enableDesperateMode = DesperateMode,
            enableSongDuration = SongDuration,
            enableDebugMode =  DebugMode
        };

        PlayerPrefs.SetString("MiscOptions", JsonConvert.SerializeObject(options));
        PlayerPrefs.Save();
    }

    public void LoadMisc()
    {
        MiscOptions options = JsonConvert.DeserializeObject<MiscOptions>(PlayerPrefs.GetString("MiscOptions",JsonConvert.SerializeObject(new MiscOptions())));

        Downscroll = options!.enableDownscroll;
        Middlescroll = options.enableMiddlescroll;
        LiteMode = options.enableLiteMode;
        DesperateMode = options.enableDesperateMode;
        SongDuration = options.enableSongDuration;
        DebugMode = options.enableDebugMode;

        downscrollToggle.SetIsOnWithoutNotify(Downscroll);
        middleScrollToggle.SetIsOnWithoutNotify(Middlescroll);
        liteModeToggle.SetIsOnWithoutNotify(LiteMode);
        desperateModeToggle.SetIsOnWithoutNotify(DesperateMode);
        songDurationToggle.SetIsOnWithoutNotify(SongDuration);
        debugModeToggle.SetIsOnWithoutNotify(DebugMode);

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

[Serializable]
public class VolumeProperties
{
    public float instVol = 75f;
    public float voicesVol = 75f;
    public float menuVol = 75f;
    public float missVol = 30f;
    public float miscVol = 75f;
}

[Serializable]
public class MiscOptions
{
    public bool enableDownscroll = false;
    public bool enableMiddlescroll = false;
    public bool enableSongDuration = false;
    public bool enableLiteMode = false;
    public bool enableDesperateMode = false;
    public bool enableDebugMode = false;
}
