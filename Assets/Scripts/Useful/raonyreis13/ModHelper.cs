using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using SimpleSpriteAnimator;

public static class ModHelper
{
    public static GameObject canvasMod, gameplayCamera, blankPrefab;
    public static Song song;

    public static Dictionary<string, object> DrawInModCanvas(string objectName, Sprite sprite, Vector2 localPosition, Vector2 localScale, Quaternion localRotation, List<Type> components, List<Sprite> animations = null, bool oneAnimForEachSprite = false, SpriteAnimationType spriteAnimationType = SpriteAnimationType.PlayOnce, string animationName = "Animation", int fps = 24) {
        GameObject a = new GameObject(objectName, components.ToArray());
        a.transform.SetParent(canvasMod.transform);
        Dictionary<string, object> objects = new Dictionary<string, object>();
        objects.Add("GameObject", a);
        objects.Add("ParentGameObject", canvasMod);
        foreach (Type component in components) {
            objects.Add(component.FullName.Split('.')[component.FullName.Split('.').Length], a.GetComponent(component));
        }
        if (objects["Image"] is Image) (objects["Image"] as Image).sprite = sprite;
        if (objects["GameObject"] is GameObject) (objects["GameObject"] as GameObject).transform.localPosition = localPosition;
        if (objects["GameObject"] is GameObject) (objects["GameObject"] as GameObject).transform.localScale = localScale;
        if (objects["GameObject"] is GameObject) (objects["GameObject"] as GameObject).transform.localRotation = localRotation;
        if (animations != null) {
            SpriteAnimator animator;
            List<SpriteAnimationFrame> frames = new List<SpriteAnimationFrame>();
            List<SpriteAnimation> newAnimations = new List<SpriteAnimation>();
            if (a.TryGetComponent<SpriteAnimator>(out animator)) {
                foreach (Sprite animation in animations) {
                    SpriteAnimationFrame frame = new SpriteAnimationFrame();
                    frame.Sprite = animation;
                }
                if (oneAnimForEachSprite) {
                    foreach (SpriteAnimationFrame frame in frames) {
                        SpriteAnimation anim = ScriptableObject.CreateInstance<SpriteAnimation>();
                        anim.FPS = 60;
                        anim.Name = frames.IndexOf(frame).ToString();
                        anim.name = frames.IndexOf(frame).ToString();
                        anim.SpriteAnimationType = spriteAnimationType;
                        anim.Frames = new List<SpriteAnimationFrame>();
                        anim.Frames.Add(frame);
                        newAnimations.Add(anim);
                    }
                } else {
                    SpriteAnimation anim = ScriptableObject.CreateInstance<SpriteAnimation>();
                    anim.FPS = 60;
                    anim.Name = animationName;
                    anim.name = animationName;
                    anim.SpriteAnimationType = spriteAnimationType;
                    anim.Frames = new List<SpriteAnimationFrame>();
                    foreach (SpriteAnimationFrame frame in frames) {
                        anim.Frames.Add(frame);
                    }
                    newAnimations.Add(anim);
                }
                animator.spriteAnimations = newAnimations;
                animator.playAutomatically = false;
            }
        }
        return objects;
    }
}
