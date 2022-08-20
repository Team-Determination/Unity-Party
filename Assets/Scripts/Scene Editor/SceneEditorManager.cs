using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using Runtime2DTransformInteractor;
using SimpleFileBrowser;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneEditorManager : MonoBehaviour
{
    private string _lastPath;
    private SceneObjectEditor _sceneObjectEditor;
    
    [Header("Saving")] public TMP_InputField saveNameField;
    public TMP_InputField saveAuthorField;
    [Header("Loading")] public TMP_InputField loadNameField;

    public string LastPath
    {
        set => _lastPath = value;
        get => _lastPath;
    }
    
    // Start is called before the first frame update
    void Start()
    {
        _lastPath = Application.persistentDataPath;
        
        FileBrowser.SetFilters(false, new FileBrowser.Filter("Images", ".png"));
        string userPath = @"C:\Users\" + Environment.UserName;
        FileBrowser.AddQuickLink("Downloads", userPath + @"\Downloads");
        FileBrowser.AddQuickLink("Documents", userPath + @"\Documents");
        FileBrowser.AddQuickLink("Pictures", userPath + @"\Pictures");
        FileBrowser.AddQuickLink("Drive C:", @"C:\");

        LoadingTransition.instance.Hide();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SaveScene()
    {
        List<SceneObject> sceneObjects = new List<SceneObject>();
        foreach (GameObject gObject in _sceneObjectEditor.objects.Keys)
        {
            SceneObject newSceneObject = new SceneObject
            {
                position = gObject.transform.position,
                rotation = gObject.transform.rotation,
                size = gObject.transform.localScale,
                fileName = Path.GetFileName(_sceneObjectEditor.objects[gObject]),
                layer = gObject.GetComponent<SpriteRenderer>().sortingOrder
            };
            sceneObjects.Add(newSceneObject);
        }

        SceneData data = new SceneData {sceneAuthor = saveAuthorField.text, sceneName = saveNameField.text, objects = sceneObjects};

        string saveDirectory = Application.persistentDataPath + "/Scenes/" + data.sceneName;
        if (Directory.Exists(saveDirectory))
        {
            Directory.Delete(saveDirectory,true);
        }

        Directory.CreateDirectory(saveDirectory);

        StreamWriter dataWriter = new StreamWriter(saveDirectory + "/scene.json", true);
        dataWriter.Write(JsonConvert.SerializeObject(data));
        dataWriter.Close();

        string imagesDirectory = saveDirectory + "/images";

        Directory.CreateDirectory(imagesDirectory);

        foreach (GameObject gObject in _sceneObjectEditor.objects.Keys)
        {
            string fileName = Path.GetFileName(_sceneObjectEditor.objects[gObject]);
            File.Copy(_sceneObjectEditor.objects[gObject], imagesDirectory + "/" + fileName);
        }

        saveAuthorField.text = string.Empty;
        saveNameField.text = string.Empty;
    }

    public void LoadScene()
    {
        string sceneName = loadNameField.text;
        string saveDirectory = Application.persistentDataPath + "/Scenes/" + sceneName;
        if (Directory.Exists(saveDirectory))
        {
            if (File.Exists(saveDirectory + "/scene.json"))
            {
                SceneData data =
                    JsonConvert.DeserializeObject<SceneData>(File.ReadAllText(saveDirectory + "/scene.json"));

                if (data != null)
                {
                    string tempScene = Application.persistentDataPath + "/tmpscene";

                    if (!Directory.Exists(tempScene))
                    {
                        Directory.CreateDirectory(tempScene);
                    }
                    
                    string imagesDirectory = saveDirectory + "/images";
                    if(Directory.Exists(imagesDirectory))
                    {
                        foreach (SceneObject sceneObject in data.objects)
                        {
                            if (File.Exists(imagesDirectory + "/" + sceneObject.fileName))
                            {
                                string path = imagesDirectory + "/" + sceneObject.fileName;
                                string tempImagesDir = tempScene+"/images";
                                if (!Directory.Exists(tempImagesDir))
                                {
                                    Directory.CreateDirectory(tempImagesDir);
                                }
                                string tempFilePath = tempImagesDir + "/" + sceneObject.fileName;
                                File.Copy(path, tempFilePath);
                                if (File.Exists(tempFilePath))
                                {
                                    byte[] imageData = File.ReadAllBytes(tempFilePath);


                                    Texture2D imageTexture = new Texture2D(2, 2);
                                    imageTexture.LoadImage(imageData);

                                    GameObject newImage = new GameObject();
                                    SpriteRenderer renderer = newImage.AddComponent<SpriteRenderer>();
                                    renderer.sprite = Sprite.Create(imageTexture,
                                        new Rect(0, 0, imageTexture.width, imageTexture.height), Vector2.zero, 100);
                                    renderer.sortingOrder = sceneObject.layer;
                                    newImage.AddComponent<BoxCollider2D>();
                                    newImage.AddComponent<TransformInteractor>();
                                    newImage.name = Path.GetFileName(tempFilePath);

                                    newImage.transform.position = sceneObject.position;
                                    newImage.transform.localScale = sceneObject.size;
                                    newImage.transform.rotation = sceneObject.rotation;

                                    _sceneObjectEditor.objects.Add(newImage, tempFilePath);
                                    
                                    GameObject newObjectButton = Instantiate(_sceneObjectEditor.objectButtonPrefab, _sceneObjectEditor.objectsListRect);

                                    newObjectButton.transform.Find("Delete").GetComponent<Button>().onClick.AddListener(() =>
                                    {
                                        ObjectProperties.instance.DeleteObject(newImage);
                                        Destroy(newObjectButton);
                                        LayoutRebuilder.ForceRebuildLayoutImmediate(_sceneObjectEditor.objectsListRect);
                                    });
                                    newObjectButton.transform.Find("Edit").GetComponent<Button>().onClick.AddListener(() =>
                                    {
                                        TransformInteractor transformInteractor = newImage.GetComponent<TransformInteractor>();
                                        transformInteractor.Select();
                
                                        transformInteractor.interactor = Instantiate(transformInteractor.spriteBoundsPrefab).GetComponent<Interactor>();
                
                                        transformInteractor.interactor.Setup(transformInteractor.gameObject);

                                    });

                                    newObjectButton.transform.Find("Object Name").Find("Text").GetComponent<TMP_Text>().text = newImage.name;

                                    
                                    print("Adding " + newImage + " to the dictionary with value " + tempFilePath);
                                }
                            }
                        }
                    }

                    Directory.Delete(tempScene, true);
                }
            }
        }
    }

    public void ExitToMenu()
    {
        LoadingTransition.instance.Show(() => SceneManager.LoadScene("Title"));
    }
}
