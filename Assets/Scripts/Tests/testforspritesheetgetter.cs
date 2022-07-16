using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SimpleSpriteAnimator;
using raonyreis13.Utils;
using System.Linq;

public class testforspritesheetgetter : MonoBehaviour
{
    public SpriteAnimator animator;
    void Start()
    {
        Dictionary<string, Sprite> spritesd = UsefulFunctions.GetSpritesheetXml(Application.persistentDataPath, "A.xml", "A.png", new Vector2(0.5f, 0.5f), FilterMode.Bilinear, 100);
        List<string> keys = spritesd.Keys.ToList();
        List<Sprite> sprites = new List<Sprite>();
        foreach (string key in keys) {
            sprites.Add(spritesd[key]);
        }
        SpriteAnimation spAnimation = ScriptableObject.CreateInstance<SpriteAnimation>();
        spAnimation.Frames = new List<SpriteAnimationFrame>();
        spAnimation.FPS = 24;
        spAnimation.Name = "A";
        spAnimation.name = "A";
        spAnimation.SpriteAnimationType = SpriteAnimationType.Looping;
        foreach (Sprite sprite in sprites) {
            SpriteAnimationFrame frame = new SpriteAnimationFrame();
            frame.Sprite = sprite;
            spAnimation.Frames.Add(frame);
        }
        animator.spriteAnimations = new List<SpriteAnimation>();
        animator.spriteAnimations.Add(spAnimation);
        animator.Play("A");
    }
}
