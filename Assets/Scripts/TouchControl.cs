using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class TouchControl : MonoBehaviour, IPointerUpHandler,IPointerDownHandler
{
    public float lerpSpeed = .1f;
    public bool selected;
    public UIGradient topGradient;
    public UIGradient bottomGradient;

    public int type;

    private RectTransform _rectTransform;
    private BoxCollider2D _collider;
    private EventTrigger _trigger;
    
    private void Start()
    {
        _rectTransform = GetComponent < RectTransform>();
        _collider = GetComponent<BoxCollider2D>();

        _trigger = FindObjectOfType<EventTrigger>();
        
    }

    private void Update()
    {
        if (selected)
        {
            topGradient.m_color1 = Color.Lerp(topGradient.m_color1, new Color(255, 255, 255, 255), lerpSpeed);
            bottomGradient.m_color2 = Color.Lerp(bottomGradient.m_color2, new Color(255, 255, 255, 255), lerpSpeed);
        }
        else
        {
            topGradient.m_color1 = Color.Lerp(topGradient.m_color1, new Color(255, 255, 255, 0), lerpSpeed);
            bottomGradient.m_color2 = Color.Lerp(bottomGradient.m_color2, new Color(255, 255, 255, 0), lerpSpeed);
        }

        var sizeDelta = _rectTransform.sizeDelta;
        _collider.size = new Vector2(sizeDelta.x, sizeDelta.y);
    }

    private void SimDown()
    {
        selected = true;


        Player.Instance.simulateHolding[type - 1] = true;
        Player.Instance.OnAndroidInput(type, Player.InputType.Press);
    }

    private void SimHold()
    {
        
        if (!selected) return;

        Player.Instance.OnAndroidInput(type, Player.InputType.Hold);
    }

    private void SimUp()
    {
        selected = false;
        Player.Instance.simulateHolding[type - 1] = false;

        Player.Instance.OnAndroidInput(type, Player.InputType.Release);
    }
    

    public void OnPointerUp(PointerEventData eventData)
    {
        SimUp();
        print("Release!");
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        SimDown();
        print("Hold!");
    }
}



