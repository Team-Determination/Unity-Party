using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Intro : MonoBehaviour
{
    public float bpm;
    public Animator introAnimator;

    private float _currentBeatTimer;
    private static readonly int Beat = Animator.StringToHash("Beat");

    // Start is called before the first frame update
    void Start()
    {
        _currentBeatTimer = bpm / 60 * 2; //Every second beat
    }

    // Update is called once per frame
    void Update()
    {
        _currentBeatTimer -= Time.deltaTime;
        if (_currentBeatTimer <= 0)
        {
            introAnimator.SetTrigger(Beat);
            _currentBeatTimer = bpm / 60 * 2;
        }
    }
}
