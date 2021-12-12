using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[Serializable, CreateAssetMenu(menuName = "Create New Character Meta",fileName = "New Character Meta")]
public class CharacterMeta : ScriptableObject
{
    public Character Character;
    public Dictionary<string, List<Vector2>> Offsets = new Dictionary<string, List<Vector2>>();
}
