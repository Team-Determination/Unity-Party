using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Intro : MonoBehaviour
{
    public float bpm;
    public int beatsToTransition;
    
    
    [Space]
    public Animator introAnimator;
    public Image logoImage;
    public float scaleBeatFactor = .1f;
    public float logoBeatTimer = .05f;
    public RectTransform logoTransform;

    [Space] public Menu menu;
    public GameObject selections;

    [Space] public bool introDone = false;
    
    private Vector3 _originalScale;
    private float _currentBeatTimer;
    private int _beatCounter;
    private static readonly int Beat = Animator.StringToHash("Beat");
    public static bool skipIntro;

    // Start is called before the first frame update
    void Start()
    {
        _currentBeatTimer = 60 / bpm; //Every second beat
        _originalScale = logoTransform.localScale;
    }

    // Update is called once per frame
    void Update()
    {
        
        
        _currentBeatTimer -= Time.deltaTime;
        
        if ((Input.anyKeyDown || skipIntro) & !introDone)
        {
            introAnimator.Play("PartEight");

            introDone = true;
            
            selections.SetActive(true);
            LeanTween.moveY(menu.selectionsRectTransform, -50f, 0);
            LeanTween.moveY(menu.selectionsRectTransform, 50f, .25f);
            _beatCounter = beatsToTransition * 8;
            return;
        }
        
        if (!(_currentBeatTimer <= 0)) return;
        
        _beatCounter++;
        
        
        var localScale = _originalScale;
        // ReSharper disable once Unity.InefficientPropertyAccess
        localScale = localScale + (localScale * scaleBeatFactor);
        logoTransform.localScale = localScale;
        LeanTween.scale(logoTransform, _originalScale, logoBeatTimer);
        
        _currentBeatTimer = 60 / bpm;

        if (_beatCounter % beatsToTransition != 0) return;
        
        

        if (_beatCounter == beatsToTransition * 8)
        {
            selections.SetActive(true);
            LeanTween.moveY(menu.selectionsRectTransform, -50f, 0);
            LeanTween.moveY(menu.selectionsRectTransform, 50f, .25f);
            introDone = true;

            return;
        }
        
        introAnimator.SetTrigger(Beat);

    }
}
