using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Video;

public class VideoPlayerScene : MonoBehaviour
{
    public static VideoClip videoToPlay;

    public static string nextScene = "Title";

    public VideoPlayer videoPlayer;

    public TMP_Text skipText;
    [Space] public VideoClip defaultVideo;
    // Start is called before the first frame update
    void Start()
    {
        videoPlayer.clip = videoToPlay == null ? defaultVideo : videoToPlay;
        videoPlayer.Play();
        StartCoroutine(nameof(EndVideo));
    }

    IEnumerator EndVideo()
    {
        yield return new WaitForSecondsRealtime(5);
        skipText.gameObject.SetActive(true);
        yield return new WaitUntil(() => !videoPlayer.isPlaying || Input.anyKeyDown);
        if (videoPlayer.isPlaying) videoPlayer.Pause();
        skipText.gameObject.SetActive(false);
        LoadingTransition.instance.Show(() => SceneManager.LoadScene(nextScene));
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
