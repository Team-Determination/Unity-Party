using System;
using UnityEngine;


public class CalibrationNote : MonoBehaviour
{
    public float strumTime;
    private float _scrollSpeed;
    public float Speed
    {
        set => _scrollSpeed = value / 100;
        get => _scrollSpeed * 100;
    }
    public int type;

    public bool mustHit;
    public bool dummy;
    private SpriteRenderer _renderer;

    private void Start()
    {
        _renderer = GetComponentInChildren<SpriteRenderer>();
    }

    private void Update()
    {
        if (!CalibrationManager.instance.runCalibration || dummy) return;

        var noteTransform = transform;
        Vector3 oldPos = noteTransform.position;
        oldPos.y = (float)(4.45 + (CalibrationManager.instance.stopwatch.ElapsedMilliseconds - (strumTime + CalibrationManager.instance.currentVisualOffset)) *
            (0.45f * (_scrollSpeed)));
        noteTransform.position = oldPos;

        if (CalibrationManager.instance.mode == CalibrationManager.CalibrationMode.Visual)
        {
            if (CalibrationManager.instance.stopwatch.ElapsedMilliseconds >= (strumTime + CalibrationManager.instance.currentVisualOffset))
            {
                CalibrationManager.instance.NoteHit(type);
            }
        }
        else
        {
            if(mustHit)
            {
                if (CalibrationManager.instance.stopwatch.ElapsedMilliseconds -
                    (strumTime + CalibrationManager.instance.currentVisualOffset) >= 135)
                {
                    CalibrationManager.instance.NoteMiss(type);
                }
            }
            else
            {
                Color newColor = Color.white;
                newColor.a = .4f;
                _renderer.color = newColor;
                if (CalibrationManager.instance.stopwatch.ElapsedMilliseconds >= (strumTime + CalibrationManager.instance.currentVisualOffset))
                {
                    CalibrationManager.instance.NoteHit(type);
                }
            }
        }
    }
}

