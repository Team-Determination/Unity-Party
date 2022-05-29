using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using SimpleSpriteAnimator;
using UnityEngine;

public class MultiplayerMenuMusic : MonoBehaviour
{
    public AudioClip menuMusic;
    public float bpm;

    public AudioSource musicSource;

    [Space] public SpriteAnimator[] spriteAnimators;

    [Header("Camera")] public Camera mainCamera;
    public float cameraZoomLerpSpeed;
    public float cameraBeatZoom;
    private float _defaultZoom;

    private Stopwatch _beatStopwatch;

    private int _beatCount;
    // Start is called before the first frame update
    void Start()
    {
        musicSource.clip = menuMusic;
        musicSource.loop = false;
        musicSource.Play();

        _beatStopwatch = new Stopwatch();
        _beatStopwatch.Start();

        _defaultZoom = mainCamera.orthographicSize;
    }

    // Update is called once per frame
    void Update()
    {
        mainCamera.orthographicSize = Mathf.Lerp(mainCamera.orthographicSize, _defaultZoom, cameraZoomLerpSpeed);
        
        if (_beatStopwatch.IsRunning)
        {
            if (_beatStopwatch.ElapsedMilliseconds >= 60 / bpm * 1000)
            {
                _beatStopwatch.Restart();
                _beatCount++;
                
                if(_beatCount % 2 == 0)
                {
                    foreach (SpriteAnimator animator in spriteAnimators)
                    {
                        animator.Play();
                    }
                }

                if (_beatCount % 4 == 0)
                {
                    mainCamera.orthographicSize = cameraBeatZoom;
                }
            }

            if (!musicSource.isPlaying)
            {
                musicSource.Play();
                _beatCount = 0;
                _beatStopwatch.Restart();
            }
        }
    }
}
