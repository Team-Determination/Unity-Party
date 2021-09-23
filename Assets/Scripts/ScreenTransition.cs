using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ScreenTransition : MonoBehaviour
{
    public GameObject fromScreen;
    public GameObject toScreen;
    public GameObject nextScreen;
    public static GameObject nextButton;

    [Space] public Image transitionImage;

    public static ScreenTransition instance;
    // Start is called before the first frame update
    void Start()
    {
        instance = this;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    
    public void SetFromScreen(GameObject screen) => fromScreen = screen;
    

    public void SetToScreen(GameObject screen) => toScreen = screen;
    public void SetNextButton(GameObject btn) => nextButton = btn;
    public void StartTransition() => StartTransition(toScreen, fromScreen);

    public void StartTransition(GameObject screenTwo, GameObject screenOne = null)
    {
        transitionImage.raycastTarget = true;
        LeanTween.alpha(transitionImage.GetComponent<RectTransform>(), 1, .45f).setOnComplete(() =>
        {
            if(screenOne != null)
                screenOne.SetActive(false);
            screenTwo.SetActive(true);
            LeanTween.alpha(transitionImage.GetComponent<RectTransform>(), 0, .45f).setOnComplete(() =>
            {
                transitionImage.raycastTarget = false;
                if(Application.isConsolePlatform)
                    EventSystem.current.SetSelectedGameObject(nextButton);
            });
        });
    }
}
