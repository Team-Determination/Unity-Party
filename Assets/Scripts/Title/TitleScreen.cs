using System.Collections;
using System.Collections.Generic;
using DG.Tweening.Plugins.Options;
using UnityEngine;
using UnityEngine.UI;

public class TitleScreen : MonoBehaviour
{
    public int menuIndex;

    public TitleButtonData[] menuButtons;

    public GameObject menuButtonPrefab;

    public RectTransform menuButtonList;

    private List<GameObject> _placedMenuButtons;

    [Header("Title Offset")] public Vector2 centerSelection;

    public Vector2 nextSelection;

    public Vector2 otherSelection;
    // Start is called before the first frame update
    void Start()
    {
        int index = 0;
        foreach (var buttonData in menuButtons)
        {
            GameObject newButton = Instantiate(menuButtonPrefab, menuButtonList);
            _placedMenuButtons.Add(newButton);

            TitleButton button = newButton.GetComponent<TitleButton>();
            button.image.sprite = buttonData.buttonSprite;
            button.text.text = buttonData.buttonText;
            button.assignedIndex = index;

            index++;
        }
        
    }
    
    

    // Update is called once per frame
    void Update()
    {
        
    }
}
