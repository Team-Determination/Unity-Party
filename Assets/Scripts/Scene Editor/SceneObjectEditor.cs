using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using Runtime2DTransformInteractor;
using SimpleFileBrowser;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneObjectEditor : MonoBehaviour
{
    [Header("Item Selection")] public static bool allowSelection = true;
    public TMP_Text selectionStatusText;
    
    //INTERNAL SCENE DATA
    public Dictionary<GameObject,string> objects;

    [Header("Objects Manager")] public GameObject objectButtonPrefab;

    public RectTransform objectsListRect;

    private SceneEditorManager _editorManager;
    // Start is called before the first frame update
    void Start()
    {
        objects = new Dictionary<GameObject,string>();

        _editorManager = FindObjectOfType<SceneEditorManager>();
    }

    public void PlaceImage()
    {
;       FileBrowser.ShowLoadDialog(paths =>
        {
            string path = paths[0];
            _editorManager.LastPath = new FileInfo(path).Directory?.ToString();
            byte[] imageData = File.ReadAllBytes(path);


            Texture2D imageTexture = new Texture2D(2, 2);
            imageTexture.LoadImage(imageData);

            GameObject newImage = new GameObject();
            SpriteRenderer renderer = newImage.AddComponent<SpriteRenderer>();
            renderer.sprite = Sprite.Create(imageTexture,
                new Rect(0, 0, imageTexture.width, imageTexture.height), Vector2.zero, 100);
            renderer.sortingOrder = 3;
            newImage.AddComponent<PolygonCollider2D>();
            newImage.AddComponent<TransformInteractor>();
            newImage.name = Path.GetFileName(path);
            objects.Add(newImage, path);

            GameObject newObjectButton = Instantiate(objectButtonPrefab, objectsListRect);

            newObjectButton.transform.Find("Delete").GetComponent<Button>().onClick.AddListener(() =>
            {
                TransformInteractor transformInteractor = newImage.GetComponent<TransformInteractor>();
                if (transformInteractor.selected)
                {
                    ObjectProperties.instance.layerField.interactable = false;
                    ObjectProperties.instance.posXField.interactable = false;
                    ObjectProperties.instance.posYField.interactable = false;
                    ObjectProperties.instance.rotZField.interactable = false;
                    ObjectProperties.instance.scaleXField.interactable = false;
                    ObjectProperties.instance.scaleYField.interactable = false;
                    ObjectProperties.instance.deleteButton.interactable = false;
                    ObjectProperties.instance.rotResetButton.interactable = false;
                    ObjectProperties.instance.scaleResetButton.interactable = false;
                    ObjectProperties.instance.posResetButton.interactable = false;
                    transformInteractor.UnSelect();
                }

                ObjectProperties.instance.DeleteObject(newImage);
                Destroy(newObjectButton);
                LayoutRebuilder.ForceRebuildLayoutImmediate(objectsListRect);
            });
            newObjectButton.transform.Find("Edit").GetComponent<Button>().onClick.AddListener(() =>
            {
                TransformInteractor transformInteractor = newImage.GetComponent<TransformInteractor>();
                transformInteractor.Select();
                
                transformInteractor.interactor = Instantiate(transformInteractor.spriteBoundsPrefab).GetComponent<Interactor>();
                
                transformInteractor.interactor.Setup(transformInteractor.gameObject);
            });

            newObjectButton.transform.Find("Object Name").Find("Text").GetComponent<TMP_Text>().text = newImage.name;
            
            print("Adding " + newImage + " to the dictionary with value " + path);
        },null,FileBrowser.PickMode.Files,false,_editorManager.LastPath,null,"Load Image");
        
    }

    // Update is called once per frame
    void Update()
    {
        if (!FileBrowser.IsOpen)
        {
            if (Input.GetKeyDown(KeyCode.Tab))
            {
                allowSelection = !allowSelection;

                TransformInteractor.ShouldBeActive = allowSelection;
                
                selectionStatusText.text = allowSelection ? "Item selection ENABLED. Toggle with TAB." : "Item selection DISABLED. Toggle with TAB.";
            }
        }
    }
}
