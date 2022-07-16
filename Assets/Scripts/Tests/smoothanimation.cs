using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SimpleSpriteAnimator;
using raonyreis13.Utils;
using System.IO;

public class smoothanimation : MonoBehaviour
{
    public SpriteAnimator animator;
    private void Start() {
        Texture2D tex = UsefulFunctions.GetTexture(Path.Combine(Application.persistentDataPath, "cardfull.png"), FilterMode.Trilinear);
        List<Sprite> sprites = UsefulFunctions.CreateSmoothAnimationWIthRect(tex, true, new Vector2(.5f, .1f));

        SpriteAnimation animation = ScriptableObject.CreateInstance<SpriteAnimation>();
        animation.Name = "Test";
        animation.name = "Test";
        animation.FPS = 60;
        animation.SpriteAnimationType = SpriteAnimationType.Looping;
        animation.Frames = new List<SpriteAnimationFrame>();
        foreach (Sprite sprite in sprites) {
            SpriteAnimationFrame frame = new SpriteAnimationFrame();
            frame.Sprite = sprite;
            animation.Frames.Add(frame);
        }

        animator.spriteAnimations = new List<SpriteAnimation>();
        animator.spriteAnimations.Add(animation);
        animator.Play("Test");
    }
}
