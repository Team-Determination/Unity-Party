using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[Serializable]
public class SceneData
{
    public string sceneName;
    public string sceneAuthor;
    public List<SceneObject> objects;

    public Vector2 protagonistPos;
    public Vector2 opponentPos;
    public Vector2 metronomePos;
}
