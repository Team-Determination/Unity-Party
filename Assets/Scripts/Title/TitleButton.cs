using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TitleButton : MonoBehaviour
{
    public Image image;

    public TMP_Text text;

    public int assignedIndex;

    private RectTransform _rect;

    private TitleScreen _title;

    private void Start()
    {
        _rect = GetComponent<RectTransform>();
        _title = FindObjectOfType<TitleScreen>();
    }
        

    private void Update()
    {
        float xPos = _rect.anchoredPosition.x;
        int realIndex = assignedIndex - _title.menuIndex;
        if (realIndex == 0)
        {
            xPos = Mathf.Lerp(xPos, 0, 2f);
            _rect.anchoredPosition = new Vector2(xPos, 145);
        } else if (realIndex == -1 || realIndex == 1)
        {
            xPos = Mathf.Lerp(xPos, 0, 2f);
            _rect.anchoredPosition = new Vector2(xPos, 145);
        }
    }
}
