using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SongButtonV2 : MonoBehaviour
{    
    [SerializeField]
    private TMP_Text songNameText;
    [SerializeField]
    private TMP_Text composerNameText;
    [SerializeField]
    private Image coverArtImage;

    [SerializeField] private Button button;

    public Sprite CoverArtSprite
    {
        get => coverArtImage.sprite;
        set => coverArtImage.sprite = value;
    }
    private SongMetaV2 _meta;
    
    public SongMetaV2 Meta
    {
        get => _meta;
        set
        {
            _meta = value;

            songNameText.text = value.songName;
        }
    }



}
