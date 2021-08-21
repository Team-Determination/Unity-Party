using System.Collections;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ReplayItem : MonoBehaviour
{
    public TMP_Text songNameText;
    public TMP_Text replayDateText;
    private string _replayDir;
    public string replayDirectory
    {
        get => _replayDir;
        set
        {
            _replayDir = value;
            GetComponent<Button>().onClick.AddListener(() =>
            {
                Replay.instance.LoadReplay(replayDirectory);
                Replay.instance.replayScreen.SetActive(false);
            });
        
            ReplayData data = JsonConvert.DeserializeObject<ReplayData>(File.ReadAllText(replayDirectory));
            songNameText.text = data.songName;
            replayDateText.text = data.dateTime.ToShortDateString() + "-" + data.dateTime.ToShortTimeString();
        }
    }

    
}