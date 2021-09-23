using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SongListObject : MonoBehaviour
{
    [SerializeField] private string songName;
    [SerializeField] private string songAuthorName;
    [SerializeField] private string songCharterName;
    [SerializeField] private string songDifficulty;
    [SerializeField] private string songDescription;
    [SerializeField] private Color songDifficultyColor;
    [SerializeField] private Sprite songIconSprite;
    [Space]
    [SerializeField] private TMP_Text songNameText;
    [SerializeField] private TMP_Text songComposerText;
    [SerializeField] private TMP_Text songDifficultyText;
    [SerializeField] private Image songIcon;
    [Space] [SerializeField] private string instDir;

    public string directory;
    
    public static bool noChanging = false;
    
    public string SongName
    {
        get => songName;
        set
        {
            songName = value;
            songNameText.text = value;
        }
    }

    public string Author
    {
        get => songAuthorName;
        set
        {
            songAuthorName = value;
            songComposerText.text = value;
        }
    }

    public string Charter
    {
        get => songCharterName;
        set => songCharterName = value;
    }
    
    public string Difficulty
    {
        get => songDifficulty;
        set
        {
            songDifficulty = value;
            songDifficultyText.text = value;
        }
    }

    public string Description
    {
        get => songDescription;
        set => songDescription = value;
    }
    
    public string InsturmentalPath
    {
        get => instDir;
        set => instDir = value;
    }
    
    public Color DifficultyColor
    {
        get => songDifficultyColor;
        set
        {
            songDifficultyText.color = value;
            songDifficultyColor = value;
        }
    }

    public Sprite Icon
    {
        get => songIconSprite;
        set
        {
            songIconSprite = value;
            songIcon.sprite = value;
        }
    }

    public void PreviewSong()
    {
        if (noChanging)
            return;
        Menu.instance.playSongButton.interactable = false;
        noChanging = true;
        
        Menu menu = Menu.instance;
        AudioSource source = Song.instance.musicSources[0];

        menu.songDetails.SetActive(false);
        menu.chooseSongMsg.SetActive(false);
        menu.loadingMsg.SetActive(true);
        
        LeanTween.value(source.gameObject, source.volume, 0, 3f).setOnComplete(o =>
        {
            source.Stop();
            StartCoroutine(nameof(LoadSongAudio), source);
        });

        menu.previewSongCover.sprite = Icon;
        menu.previewSongCharter.text = "<color=yellow>Charted by </color>" + Charter;
        menu.previewSongComposer.text = Author;
        menu.previewSongDescription.text = Description;
        menu.previewSongDifficulty.text = Difficulty;
        menu.previewSongDifficulty.color = DifficultyColor;
        menu.previewSongName.text = SongName;
        menu.playSongButton.interactable = true;
        Song.instance.selectedSong = this;

        LayoutRebuilder.ForceRebuildLayoutImmediate(menu.songDetailsLayout);
        LayoutRebuilder.ForceRebuildLayoutImmediate(menu.songMiscDetailsLayout);

    }

    IEnumerator LoadSongAudio(AudioSource source)
    {
        

        WWW www = new WWW(instDir);
        if (www.error != null)
        {
            Debug.LogError(www.error);
        }
        else
        {
            source.clip = www.GetAudioClip();
            while (source.clip.loadState != AudioDataLoadState.Loaded)
                yield return new WaitForSeconds(0.1f);
            source.Play();
            LeanTween.value(source.gameObject, source.volume, .75f, 3f);
            Menu.instance.songDetails.SetActive(true);
            Menu.instance.loadingMsg.SetActive(false);
            noChanging = false;
            Menu.instance.playSongButton.interactable = true;

        }
    }
}
