using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DiscoveredSong : MonoBehaviour
{
    public TMP_Text songText;
    public Toggle toggle;

    public bool doNotImport = true;
    
    public DiscoveredSongInfo info = new DiscoveredSongInfo();

    private void Start()
    {
        toggle.onValueChanged.AddListener(val =>
        {
            doNotImport = !val;
        });
    }
}
[Serializable]
public class DiscoveredSongInfo
{
    public string songName;
    public string chartPath;
    public string instPath;
    public string voicesPath;
    public int difficulty;
}
