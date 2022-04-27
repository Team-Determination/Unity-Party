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
            if (Player.keybinds == null)
            {
                SavedKeybinds defaultBinds = new SavedKeybinds
                {
                    primary4K = {KeyCode.A,KeyCode.S,KeyCode.W,KeyCode.D},
                    secondary4K = {KeyCode.LeftArrow,KeyCode.DownArrow,KeyCode.UpArrow,KeyCode.RightArrow}
                };
                PlayerPrefs.SetString("Saved Keybinds", JsonConvert.SerializeObject(defaultBinds));
                PlayerPrefs.Save();

                Player.keybinds = defaultBinds;
            }
        }
        else
        {
            SavedKeybinds defaultBinds = new SavedKeybinds
            {
                primary4K = {KeyCode.A,KeyCode.S,KeyCode.W,KeyCode.D},
                secondary4K = {KeyCode.LeftArrow,KeyCode.DownArrow,KeyCode.UpArrow,KeyCode.RightArrow}
            };
            PlayerPrefs.SetString("Saved Keybinds", JsonConvert.SerializeObject(defaultBinds));
            PlayerPrefs.Save();

            Player.keybinds = defaultBinds;
        }

        SavedKeybinds keys = Player.keybinds;

        Player.pauseKey = keys.pauseKeyCode;
        Player.resetKey = keys.resetKeyCode;
        Player.startSongKey = keys.startSongKeyCode;

        SetKeybindText(keys);

        var primary4K = Player.keybinds.primary4K;
        print(primary4K.Count);
        foreach (KeyCode code in primary4K)
        {
            print(code.ToString());
        }

        var secondary4K = Player.keybinds.secondary4K;
        print(secondary4K.Count);
        foreach (KeyCode code in secondary4K)
        {
            print(code.ToString());
        }
        
    }

    private void SetKeybindText(SavedKeybinds keys)
    {
        for (var index = 0; index < fourKeyPrimaryText.Length; index++)
        {
            TMP_Text keyText = fourKeyPrimaryText[index];
            keyText.text = keys.primary4K[index].ToString();
        }
        for (var index = 0; index < fourKeySecondaryText.Length; index++)
        {
            TMP_Text keyText = fourKeySecondaryText[index];
            keyText.text = keys.secondary4K[index].ToString();
        }
        
        resetKeybindText.text = keys.resetKeyCode.ToString();
        pauseKeybindText.text = keys.pauseKeyCode.ToString();
        startSongKeybindText.text = keys.startSongKeyCode.ToString();
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

        PlayerPrefs.SetString("Saved Keybinds", JsonConvert.SerializeObject(Player.keybinds));
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
        LoadKeybinds();
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
                                                Player.keybinds.primary4K.RemoveAt(0);
                                                Player.keybinds.primary4K.Insert(0, key);
                                                fourKeyPrimaryText[0].text = key.ToString();
                                                break;
                                            case KeybindSet.PrimaryDown:
                                                Player.keybinds.primary4K.RemoveAt(1);
                                                Player.keybinds.primary4K.Insert(1, key);
                                                fourKeyPrimaryText[1].text = key.ToString();
                                                break;
                                            case KeybindSet.PrimaryUp:
                                                Player.keybinds.primary4K.RemoveAt(2);
                                                Player.keybinds.primary4K.Insert(2, key);
                                                fourKeyPrimaryText[2].text = key.ToString();
                                                break;
                                            case KeybindSet.PrimaryRight:
                                                Player.keybinds.primary4K.RemoveAt(3);
                                                Player.keybinds.primary4K.Insert(3, key);
                                                fourKeyPrimaryText[3].text = key.ToString();
                                                break;
                                            case KeybindSet.SecondaryLeft:
                                                Player.keybinds.secondary4K.RemoveAt(0);
                                                Player.keybinds.secondary4K.Insert(0, key);
                                                fourKeySecondaryText[0].text = key.ToString();
                                                break;
                                            case KeybindSet.SecondaryDown:
                                                Player.keybinds.secondary4K.RemoveAt(1);
                                                Player.keybinds.secondary4K.Insert(1, key);
                                                fourKeySecondaryText[1].text = key.ToString();
                                                break;
                                            case KeybindSet.SecondaryUp:
                                                Player.keybinds.secondary4K.RemoveAt(2);
                                                Player.keybinds.secondary4K.Insert(2, key);
                                                fourKeySecondaryText[2].text = key.ToString();
                                                break;
                                            case KeybindSet.SecondaryRight:
                                                Player.keybinds.secondary4K.RemoveAt(3);
                                                Player.keybinds.secondary4K.Insert(3, key);
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
                SaveKeySet();
            }
        }
    }
}
