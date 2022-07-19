using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SimpleSpriteAnimator;
using raonyreis13.Utils;
using System.Linq;
using System.IO;

public class testforspritesheetgetter : MonoBehaviour
{
    public SpriteAnimator animator;
    void Start()
    {
        Dictionary<string, Sprite> testSprites;
        testSprites = UsefulFunctions.GetSpritesheetXmlWithoutOffset(
            Path.Combine(Application.persistentDataPath),
            "test.xml",
            new Vector2(.5f, .0f),
            FilterMode.Trilinear,
            64
        );

        List<string> keys = testSprites.Keys.ToList();

        List<Sprite> parsedSprites = new List<Sprite>();
        foreach (string key in keys) {
            if (key.Contains("")) {
                parsedSprites.Add(testSprites[key]);
            }
        }

        SpriteAnimation animation = UsefulFunctions.CreateAnimation(parsedSprites, "All", 24, SpriteAnimationType.Looping);
        animator.spriteAnimations = new List<SpriteAnimation>();
        animator.spriteAnimations.Add(animation);
        animator.Play("All");
    }
}
