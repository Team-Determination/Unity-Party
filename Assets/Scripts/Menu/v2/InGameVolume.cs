using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using Newtonsoft.Json;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class InGameVolume : MonoBehaviour
{
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
    // Start is called before the first frame update

    #region Volumes

    public void LoadVolumeProperties()
    {
        string volumeData = PlayerPrefs.GetString("Volume", JsonConvert.SerializeObject(new VolumeProperties()));

        VolumeProperties volumeProperties = JsonConvert.DeserializeObject<VolumeProperties>(volumeData);

        instVolSlider.value = volumeProperties.instVol * 100;
        voicesVolSlider.value = volumeProperties.voicesVol * 100;
        menuVolSlider.value = volumeProperties.menuVol * 100;
        missVolSlider.value = volumeProperties.missVol * 100;
        miscVolSlider.value = volumeProperties.miscVol * 100;

        instVolume = volumeProperties.instVol;
        voicesVolume = volumeProperties.voicesVol;
        menuVolume = volumeProperties.menuVol;
        missVolume = volumeProperties.missVol;
        miscVolume = volumeProperties.miscVol;

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
            instVol = instVolume,
            voicesVol = voicesVolume,
            menuVol = menuVolume,
            missVol = missVolume,
            miscVol = miscVolume
        };

        PlayerPrefs.SetString("Volume", JsonConvert.SerializeObject(volumeProperties));
        PlayerPrefs.Save();
    }
    
    
    #endregion
   

}