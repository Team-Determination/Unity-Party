using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Newtonsoft.Json;
using SimpleSpriteAnimator;
using System.IO;
using raonyreis13.Utils;

public class SceneParser : MonoBehaviour
{
    SceneData currentScene;
    string songDir;
    public static SceneParser instance;

    private void Awake() {
        instance = this;
    }

    public void ParseScene(string songDir) {
        this.songDir = songDir;
        SceneRoot sceneRoot = JsonConvert.DeserializeObject<SceneRoot>(Path.Combine(songDir, "Scene/scene.json"));
        SceneData scene = sceneRoot.scene;

        Song.instance.girlfriendObject.SetActive(scene.gfEnabled);
        Song.instance.mainCamera.fieldOfView = scene.fovCamera;

        Song.instance.girlfriendObject.transform.localPosition = scene.gfTransform.position;
        Song.instance.girlfriendObject.transform.localRotation = scene.gfTransform.rotation;
        Song.instance.girlfriendObject.transform.localScale = scene.gfTransform.scale;

        Song.instance.boyfriendObject.transform.localPosition = scene.plTransform.position;
        Song.instance.boyfriendObject.transform.localRotation = scene.plTransform.rotation;
        Song.instance.boyfriendObject.transform.localScale = scene.plTransform.scale;

        Song.instance.opponentObject.transform.localPosition = scene.enTransform.position;
        Song.instance.opponentObject.transform.localRotation = scene.enTransform.rotation;
        Song.instance.opponentObject.transform.localScale = scene.enTransform.scale;

        foreach (string sceneObject in scene.sceneObjects) {
            ObjectData obj = JsonConvert.DeserializeObject<ObjectRoot>(Path.Combine(songDir, "Scene/Objects", sceneObject)).Object;
            scene.SceneObjects.Add(obj);
        }
        currentScene = scene;
        WorkOnObjects();
    }

    void WorkOnObjects() {
        foreach (ObjectData obj in currentScene.SceneObjects) {
            GameObject objGameObj = new GameObject(obj.ObjectName);
            if (obj.IsAnimated) {
                SpriteAnimator animator = objGameObj.AddComponent<SpriteAnimator>();
                animator.playAutomatically = obj.Animator.AutoPlay;
                animator.spriteAnimations = new List<SpriteAnimation>();
                foreach (SceneAnimationData animationData in obj.Animator.Animations) {
                    SpriteAnimation animation = ScriptableObject.CreateInstance<SpriteAnimation>();
                    animation.animationName = animationData.AnimationName;
                    animation.Name = animationData.AnimationName;
                    animation.name = animationData.AnimationName;
                    animation.SpriteAnimationType = animationData.AnimationType == "Looping" ? SpriteAnimationType.Looping : SpriteAnimationType.PlayOnce;
                    animation.FPS = animationData.FramesPerSecond;
                    animation.Frames = new List<SpriteAnimationFrame>();
                    if (Directory.Exists(Path.Combine(songDir, "Scene/Sprites", animationData.FramesFolder))) {
                        string[] files = Directory.GetFiles(Path.Combine(songDir, "Scene/Sprites", animationData.FramesFolder), "*.png");
                        foreach (string file in files) {
                            Sprite sprite = UsefulFunctions.GetSprite(file, new Vector2(.5f, .5f), FilterMode.Trilinear, 100);
                            SpriteAnimationFrame frame = new SpriteAnimationFrame();
                            frame.Sprite = sprite;
                            animation.Frames.Add(frame);
                        }
                    }
                    animator.spriteAnimations.Add(animation);
                }
                if (obj.Animator.AutoPlay)
                    animator.Play(obj.Animator.AutoPlayAnimation);
            } else {
                string fileSingle = Path.Combine(songDir, "Scene/Sprites", obj.SpriteFileName);
                objGameObj.AddComponent<SpriteRenderer>().sprite = UsefulFunctions.GetSprite(fileSingle, new Vector2(.5f, .5f), FilterMode.Trilinear, 100);
            }

            objGameObj.transform.localPosition = obj.Transform.position;
            objGameObj.transform.localRotation = obj.Transform.rotation;
            objGameObj.transform.localScale = obj.Transform.scale;
        }
    }
}

[Serializable]
public class SceneRoot {
    [JsonProperty("Metadata Version")] public string version = "1.0.0";
    [JsonProperty("Scene Configurations")] public SceneData scene = new SceneData();
}

[Serializable]
public class SceneData {
    [JsonProperty("Scene Name")] public string sceneName = "";
    [JsonProperty("Scene Objects")] public List<string> sceneObjects = new List<string>();
    [JsonProperty("Girlfriend Enabled")] public bool gfEnabled = true;
    [JsonProperty("Camera Zoom")] public float fovCamera = 5;
    [JsonProperty("Girlfriend Transform")] public SceneTransform gfTransform = new SceneTransform();
    [JsonProperty("Player Transform")] public SceneTransform plTransform = new SceneTransform();
    [JsonProperty("Enemy Transform")] public SceneTransform enTransform = new SceneTransform();

    [JsonIgnore] public List<ObjectData> SceneObjects = new List<ObjectData>();
}

[Serializable]
public class SceneTransform {
    public Vector2 position = new Vector2(0, 0);
    public Quaternion rotation = new Quaternion(0, 0, 0, 1);
    public Vector2 scale = new Vector2(1, 1);
    public Vector3 cameraOffset = new Vector3(0, 0, 0);

    public Transform ConvertToTransform() {
        Transform t = null;
        t.localPosition = position;
        t.localRotation = rotation;
        t.localScale = scale;
        return t;
    }
}

[Serializable]
public class ObjectRoot {
    public ObjectData Object = new ObjectData();
}

[Serializable]
public class ObjectData {
    public string ObjectName = "";
    public string SpriteFileName = "";
    public SceneTransform Transform = new SceneTransform();
    public bool IsAnimated = false;
    public SceneAnimatorData Animator = new SceneAnimatorData();
}

[Serializable]
public class SceneAnimatorData {
    public bool AutoPlay = false;
    public string AutoPlayAnimation = "";
    public List<SceneAnimationData> Animations = new List<SceneAnimationData>();
}

[Serializable]
public class SceneAnimationData {
    public string AnimationName = "";
    public int FramesPerSecond = 24;
    public string FramesFolder = "";
    public string AnimationType = "Looping";
}
