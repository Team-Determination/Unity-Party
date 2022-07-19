using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using SimpleSpriteAnimator;
using UnityEngine;
using UnityEngine.Serialization;


[CreateAssetMenu(fileName = "New Character", menuName = "Create New Character", order = 0),Serializable] 
public class Character : ScriptableObject
{
    public string characterName = string.Empty;
    public string xmlFileName = "character.xml";
    public List<string> animationsName = new List<string>();
    public int pixelsPerUnity = 100;
    public int framesPerSecond = 24;
    public float[] allPivot = new float[] { 
        0.5f, 0.0f
    };
    public bool useXmlOffset = true;
    public float offsetDiv = 64.1f;
    [JsonIgnore] public List<SpriteAnimation> animations;
    public bool idleOnly = false;
    [Header("Floating")] public bool doesFloat;
    public float floatToOffset;
    public float floatSpeed;

    [Header("Size")] public float scale = 1;

    [FormerlySerializedAs("offset")] [Header("Camera")]
    public Vector3 cameraOffset = new Vector3(2, 6, -10);

    [Header("Portrait Data"),JsonIgnore] public PortraitData portraitData;
    public Vector2 portraitSize;
    public Color healthColor = Color.red;
}
