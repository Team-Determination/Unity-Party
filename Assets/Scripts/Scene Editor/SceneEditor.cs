using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using Runtime2DTransformInteractor;
using SFB;
using TMPro;
using UnityEngine;

public class SceneEditor : MonoBehaviour
{
    [Header("Saving")] public TMP_InputField saveNameField;
    public TMP_InputField saveAuthorField;

    //INTERNAL SCENE DATA
    private Dictionary<GameObject,string> _objects;
    // Start is called before the first frame update
    void Start()
    {
        _objects = new Dictionary<GameObject,string>();
    }

    public void PlaceImage()
    {
        StandaloneFileBrowser.OpenFilePanelAsync("Open Image", Environment.CurrentDirectory, "png", false, paths =>
        {
            string path = paths[0];
            byte[] imageData = File.ReadAllBytes(path);
            

            Texture2D imageTexture = new Texture2D(2, 2);
            imageTexture.LoadImage(imageData);
            
            GameObject newImage = new GameObject();
            SpriteRenderer renderer = newImage.AddComponent<SpriteRenderer>();
            renderer.sprite = Sprite.Create(imageTexture,
                new Rect(0, 0, imageTexture.width, imageTexture.height), Vector2.zero, 100);
            renderer.sortingOrder = 3;
            newImage.AddComponent<BoxCollider2D>();
            newImage.AddComponent<TransformInteractor>();
            newImage.name = Path.GetFileName(path);
            _objects.Add(newImage, path);
            print("Adding " + newImage + " to the dictionary with value " + path);
        });
    }
    public void SaveScene()
    {
        List<SceneObject> sceneObjects = new List<SceneObject>();
        foreach (GameObject gObject in _objects.Keys)
        {
            SceneObject newSceneObject = new SceneObject
            {
                position = gObject.transform.position,
                rotation = gObject.transform.rotation,
                size = gObject.transform.localScale,
                fileName = Path.GetFileName(_objects[gObject]),
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

        foreach (GameObject gObject in _objects.Keys)
        {
            string fileName = Path.GetFileName(_objects[gObject]);
            File.Copy(_objects[gObject], imagesDirectory + "/" + fileName);
        }

        saveAuthorField.text = string.Empty;
        saveNameField.text = string.Empty;
    }

    public void LoadScene(string sceneName)
    {
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

                                    _objects.Add(newImage, tempFilePath);
                                    print("Adding " + newImage + " to the dictionary with value " + tempFilePath);
                                }
                            }
                        }
                    }
                }
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
