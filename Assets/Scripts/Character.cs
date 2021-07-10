using UnityEngine;


[CreateAssetMenu(fileName = "New Character", menuName = "Create New Character", order = 0)] 
public class Character : ScriptableObject
{
    public string characterName;
    public AnimatorOverrideController animator;
    [Header("Floating")] public bool doesFloat;
    public float floatToOffset;
    public float floatSpeed;
    [Header("Camera")] public Vector3 offset;
    [Header("Portrait")] public Sprite portrait;
    public Vector2 portraitSize;
}
