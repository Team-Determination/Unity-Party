using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SimpleSpriteAnimator;
using raonyreis13.Utils;
using System.Linq;
using System.IO;
using B83.TextureTools;

public class testforspritesheetgetter : MonoBehaviour {
    public SpriteAnimator animator;
    public SpriteRenderer renderer;

    public int defaultW, defaultH;
    public TextureFormat formalDef;
    void Start() {
        Dictionary<string, Dictionary<Vector2, Sprite>> testSprites;
        testSprites = UsefulFunctions.GetSpritesheetXml(
            Path.Combine(Application.persistentDataPath),
            "test.xml",
            new Vector2(0.5f, 0.5f),
            FilterMode.Trilinear,
            100
        );

        List<string> keys = testSprites.Keys.ToList();

        List<Dictionary<Vector2, Sprite>> parsedSprites = new List<Dictionary<Vector2, Sprite>>();


        foreach (string key in keys) {
            if (key.Contains("Normal")) {
                parsedSprites.Add(testSprites[key]);
            }
        }

        SpriteAnimation animation = UsefulFunctions.CreateAnimationWithOffsets(parsedSprites, "All", 24, SpriteAnimationType.Looping);
        animator.spriteAnimations = new List<SpriteAnimation>();
        animator.spriteAnimations.Add(animation);
        animator.Play("All");
    }
}
