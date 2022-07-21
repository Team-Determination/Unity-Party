using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cache
{
    public static Dictionary<string, Character> cachedOpponents = new Dictionary<string, Character>();
    public static Dictionary<string, Protagonist> cachedProtagonists = new Dictionary<string, Protagonist>();
    public static Dictionary<string, Dictionary<SceneObject,Sprite>> cachedScenes = new Dictionary<string, Dictionary<SceneObject,Sprite>>();

}
