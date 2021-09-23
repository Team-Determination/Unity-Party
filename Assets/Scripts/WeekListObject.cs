using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WeekListObject : MonoBehaviour
{
    [HideInInspector] public string bundleName;
    [HideInInspector] public string author;

    [Space] public TMP_Text bundleNameText;
    public TMP_Text authorText;

    public List<SongListObject> songs;

    public string BundleName
    {
        get => bundleName;
        set
        {
            bundleNameText.text = value;
            bundleName = value;
        }
    }

    public string Author
    {
        get => author;
        set
        {
            author = value;
            authorText.text = value;
        }
    }

    public void ToggleSongsVisibility()
    {
        foreach (SongListObject listObject in songs)
        {
            listObject.gameObject.SetActive(!listObject.gameObject.activeSelf);
        }

        LayoutRebuilder.ForceRebuildLayoutImmediate(Menu.instance.songListLayout);
    }
}
