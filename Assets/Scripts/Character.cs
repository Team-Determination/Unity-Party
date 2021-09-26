using System;
using Newtonsoft.Json;
using UnityEngine;


[CreateAssetMenu(fileName = "New Character", menuName = "Create New Character", order = 0),Serializable] 
public class Character : ScriptableObject
{
    public string characterName;
    [JsonIgnore] public AnimatorOverrideController animator;
    public bool idleOnly = false;
    [Header("Floating")] public bool doesFloat;
    public float floatToOffset;
    public float floatSpeed;
    [Header("Camera")] public Vector3 offset;
    [Header("Portrait"),JsonIgnore] public Sprite portrait;
    public Sprite portraitDead;
    public Vector2 portraitSize;
}
