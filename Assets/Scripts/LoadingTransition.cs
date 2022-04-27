using System;
using System.Collections;
using System.Collections.Generic;
using Coffee.UIExtensions;
using TMPro;
using UnityEngine;

public class LoadingTransition : MonoBehaviour
{
    public static LoadingTransition instance;

    public UITransitionEffect transitionEffect;

    public TMP_Text loadingText;

    public bool toggled;
    // Start is called before the first frame update
    void Start()
    {
        if (instance != null)
        {
            Destroy(gameObject);
        } 
        else
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }

    public void Show(Action action)
    {
        if (!toggled) toggled = true;
        else return;
        transitionEffect.Show();
        LeanTween.value(gameObject, Color.clear, Color.white, .5f).setDelay(transitionEffect.duration).setOnUpdate(val =>
        {
            loadingText.color = val;
        }).setOnComplete(() =>
        {
            LeanTween.delayedCall(.5f, action);
        });
    }

    public void Hide()
    {
        if (toggled) toggled = false;
        else return;
        LeanTween.value(gameObject, Color.white, Color.clear, .5f).setDelay(transitionEffect.duration).setOnUpdate(val =>
        {
            loadingText.color = val;
        }).setOnComplete(() =>
        {
            transitionEffect.Hide();
        });
        
    }
}
