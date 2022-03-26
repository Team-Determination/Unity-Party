using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using TMPro;
using UnityEngine;

public class BundleButtonV2 : MonoBehaviour
{
    [SerializeField] private TMP_Text bundleNameText;
    [SerializeField] private TMP_Text bundleCreatorText;
    [SerializeField] private TMP_Text songsAmountText;
    
    private List<SongButtonV2> songButtons;

    public string directory;
    public bool isMod;

    public void ToggleSongsVisibility()
    {
        foreach (SongButtonV2 button in songButtons)
        {
            button.gameObject.SetActive(!button.gameObject.activeSelf);
        }
    }

    public void EditBundle()
    {
        Process.Start("explorer.exe", directory);
    }

    public void DeleteBundle()
    {
        if (isMod)
        {
            MenuV2.Instance.DisplayNotification(
                Color.red,
                "To delete this bundle, you must unsubscribe from it via the Mod.io portal in the main menu.");
        }
    }

    public string Creator
    {
        set => bundleCreatorText.text = value;
        get => bundleCreatorText.text;
    }

    public string Name
    {
        set => bundleNameText.text = value;
        get => bundleNameText.text;
    }

    public List<SongButtonV2> SongButtons
    {
        set => songButtons = value;
        get => songButtons;
    }

    public void UpdateCount()
    {
        songsAmountText.text = $"{SongButtons.Count} Songs";
    }
}
