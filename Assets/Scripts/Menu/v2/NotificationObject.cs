using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class NotificationObject : MonoBehaviour
{
    [SerializeField]
    private Image backgroundImage;

    public TMP_Text notificationText;

    [SerializeField] public RectTransform[] refreshableRects;


    public Color BackgroundColor
    {
        set
        {
            backgroundImage.color = value;
            backgroundImage.SetVerticesDirty();

        }
        get => backgroundImage.color;
    }
    
    // Start is called before the first frame update
    void Start()
    {
        foreach (RectTransform rect in refreshableRects)
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(rect);
        }
        backgroundImage.SetVerticesDirty();

        LayoutRebuilder.ForceRebuildLayoutImmediate(MenuV2.Instance.notificationLists);

        LeanTween.delayedCall(8f, () =>
        {
            Destroy(gameObject);
            LayoutRebuilder.ForceRebuildLayoutImmediate(MenuV2.Instance.notificationLists);        

        });
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
