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
    [JsonIgnore] public List<SpriteAnimation> animations;
    public bool idleOnly = false;
    [Header("Floating")] public bool doesFloat;
    public float floatToOffset;
    public float floatSpeed;

    [FormerlySerializedAs("offset")] [Header("Camera")]
    public Vector3 cameraOffset = new Vector3(2, 6, -10);
    [Header("Portrait"),JsonIgnore] public Sprite portrait;
    
    [JsonIgnore]
    public Sprite portraitDead;
    public Vector2 portraitSize;
    public Color healthColor = Color.red;
}
