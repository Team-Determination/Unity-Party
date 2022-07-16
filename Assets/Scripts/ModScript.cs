using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SimpleSpriteAnimator;
using System.IO;

class ModScript : MonoBehaviour
{
    public List<SpriteAnimationFrame> rainSprites = new List<SpriteAnimationFrame>();

    public void Start()
    {
        GameObject gmRain = new GameObject();
        gmRain.name = "Rain";
        gmRain.AddComponent<SpriteAnimator>();
        gmRain.transform.localScale = new Vector2(0.6f, 2f);
        SpriteRenderer spr = gmRain.GetComponent<SpriteRenderer>();
        spr.drawMode = SpriteDrawMode.Tiled;
        spr.tileMode = SpriteTileMode.Continuous;
        SpriteAnimator spriteAnimator = gmRain.GetComponent<SpriteAnimator>();
        spriteAnimator.playAutomatically = false;
        string[] rainPaths = Directory.GetFiles(Path.Combine(Song.instance.metaSong.songPath, "CustomSprites", "Rain"));
        foreach (string rainSprite in rainPaths)
        {
            SpriteAnimationFrame frame = new SpriteAnimationFrame();
            frame.Sprite = Song.instance.GetSprite(rainSprite, new Vector2(0.5f, 0.0f));
            rainSprites.Add(frame);
        }

        SpriteAnimation animation = ScriptableObject.CreateInstance<SpriteAnimation>();
        animation.FPS = 24;
        animation.Name = "Rain";
        animation.SpriteAnimationType = SpriteAnimationType.Looping;
        foreach (SpriteAnimationFrame frame in rainSprites)
        {
            animation.Frames.Add(frame);
        }
        spriteAnimator.spriteAnimations = new List<SpriteAnimation>();
        spriteAnimator.spriteAnimations.Add(animation);
        spriteAnimator.Play();
    }

    public void OnBeat()
    {

    }
}