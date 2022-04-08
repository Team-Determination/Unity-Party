using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class KeybindOptions : MonoBehaviour
{
    [Header("Keybinding")] public TMP_Text[] fourKeyPrimaryText;
    public TMP_Text[] fourKeySecondaryText;
    [Space] public TMP_Text resetKeybindText;
    public TMP_Text startSongKeybindText;
    public TMP_Text pauseKeybindText;
    [Space] public GameObject settingKeybindMessage;
    private KeybindSet _currentKeybindSet;
    private Player.KeyMode _currentKeyMode;
    private bool _settingKeybind;
    [FormerlySerializedAs("keybindTabs")] [Header("Keybind Tabs")] public GameObject[] keybindWindows;
    public Button[] keybindTabs;
    public Color selectedTabColor;
    public Color unselectedTabColor;

    public void LoadKeybinds()
    {
        string keysJson = PlayerPrefs.GetString("Saved Keybinds", String.Empty);

        if (!string.IsNullOrWhiteSpace(keysJson))
        {
            Player.keybinds = JsonConvert.DeserializeObject<SavedKeybinds>(keysJson);
        }
        else
        {
            PlayerPrefs.SetString("Saved Keybinds", JsonConvert.SerializeObject(new SavedKeybinds()));
            PlayerPrefs.Save();

            Player.keybinds = new SavedKeybinds();
        }

        SavedKeybinds keys = Player.keybinds;

        Player.pauseKey = keys.pauseKeyCode;
        Player.resetKey = keys.resetKeyCode;
        Player.startSongKey = keys.startSongKeyCode;

        for (var index = 0; index < keys.primary4K.Count; index++)
        {
            KeyCode key = keys.primary4K[index];
            fourKeyPrimaryText[index].text = key.ToString();
        }

        for (var index = 0; index < keys.secondary4K.Count; index++)
        {
            KeyCode key = keys.secondary4K[index];
            fourKeySecondaryText[index].text = key.ToString();
        }
    }


    public void ChangeKeybindWindow(GameObject windowOpening)
    {
        foreach (GameObject window in keybindWindows) window.SetActive(false);
        windowOpening.SetActive(true);
    }

    public void ChangeTab(Button tab)
    {
        ColorBlock colorBlock;
        foreach (Button tabButton in keybindTabs)
        {
            colorBlock = tabButton.colors;
            colorBlock.normalColor = unselectedTabColor;
            tabButton.colors = colorBlock;
        }
        colorBlock = tab.colors;
        colorBlock.normalColor = selectedTabColor;
        tab.colors = colorBlock;
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

    
    public void SaveKeySet()
    {

        PlayerPrefs.SetString("Saved Keybinds", JsonConvert.SerializeObject(_currentKeybindSet));
        PlayerPrefs.Save();
    }

    public void ChangeKeybind(int key)
    {
        KeybindSet keybind = (KeybindSet) Enum.ToObject(typeof(KeybindSet), key);

        _currentKeybindSet = keybind;
        _settingKeybind = true;

        
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (_settingKeybind)
        {
            if (Input.anyKeyDown)
            {
                var allKeys = System.Enum.GetValues(typeof(KeyCode)).Cast<KeyCode>();
                foreach (var key in allKeys) {
                    if (Input.GetKeyDown(key))
                    {
                        if (key == KeyCode.Mouse0 || key == KeyCode.Mouse1 || key == KeyCode.Mouse2)
                            return;
                        switch (_currentKeybindSet)
                        {
                            case KeybindSet.Reset:
                                Player.keybinds.resetKeyCode = key;
                                resetKeybindText.text = key.ToString();
                                break;
                            case KeybindSet.StartSong:
                                Player.keybinds.startSongKeyCode = key;
                                startSongKeybindText.text = key.ToString();
                                break;
                            case KeybindSet.Pause:
                                Player.keybinds.pauseKeyCode = key;
                                pauseKeybindText.text = key.ToString();
                                break;
                            default:
                                switch (_currentKeyMode)
                                {
                                    case Player.KeyMode.FourKey:
                                        switch (_currentKeybindSet)
                                        {
                                            case KeybindSet.PrimaryLeft:
                                                Player.keybinds.primary4K[0] = key;
                                                fourKeyPrimaryText[0].text = key.ToString();
                                                break;
                                            case KeybindSet.PrimaryDown:
                                                Player.keybinds.primary4K[1] = key;
                                                fourKeyPrimaryText[1].text = key.ToString();
                                                break;
                                            case KeybindSet.PrimaryUp:
                                                Player.keybinds.primary4K[2] = key;
                                                fourKeyPrimaryText[2].text = key.ToString();
                                                break;
                                            case KeybindSet.PrimaryRight:
                                                Player.keybinds.primary4K[3] = key;
                                                fourKeyPrimaryText[3].text = key.ToString();
                                                break;
                                            case KeybindSet.SecondaryLeft:
                                                Player.keybinds.secondary4K[0] = key;
                                                fourKeySecondaryText[0].text = key.ToString();
                                                break;
                                            case KeybindSet.SecondaryDown:
                                                Player.keybinds.secondary4K[1] = key;
                                                fourKeySecondaryText[1].text = key.ToString();
                                                break;
                                            case KeybindSet.SecondaryUp:
                                                Player.keybinds.secondary4K[2] = key;
                                                fourKeySecondaryText[2].text = key.ToString();
                                                break;
                                            case KeybindSet.SecondaryRight:
                                                Player.keybinds.secondary4K[3] = key;
                                                fourKeySecondaryText[3].text = key.ToString();
                                                break;
                                        }
                                        break;
                                    case Player.KeyMode.FiveKey:
                                        break;
                                    case Player.KeyMode.SixKey:
                                        break;
                                    case Player.KeyMode.SevenKey:
                                        break;
                                    case Player.KeyMode.EightKey:
                                        break;
                                    case Player.KeyMode.NineKey:
                                        break;
                                }
                                break;
                        }
                    }
                }

                settingKeybindMessage.SetActive(false);
            }
        }
    }
}
