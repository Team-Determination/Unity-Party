using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

public class VideoCallScene : MonoBehaviour
{
    private Song _song;

    private int _level = 0;

    public PlayableDirector director;

    private GameObject _hecker;

    private GameObject _boyfriend;
    // Start is called before the first frame update
    void Start()
    {
        _song = Song.instance;

        _hecker = _song.opponentObject.transform.parent.gameObject;
        _boyfriend = _song.boyfriendObject.transform.parent.gameObject;
    }

    // Update is called once per frame
    void Update()
    {
        if (!_song.musicSources[0].isPlaying) return;
        switch (_level)
        {
            case 0:
                if (_song.musicSources[0].time >= 232.710f)
                {
                    director.Play();
                    _hecker.SetActive(false);
                    _level++;
                }

                break;
            case 1:
                if (director.time >= 1.383333333f)
                {
                    _boyfriend.SetActive(false);
                    _level++;
                }

                break;
            case 2:
                break;
        }
    }
}
