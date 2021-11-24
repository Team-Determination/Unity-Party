using UnityEngine;
using System.Collections;

namespace SimpleSpriteAnimator
{
    public class SpriteAnimationHelper
    {
        public float animationTime = 0.0f;

        public SpriteAnimation CurrentAnimation { get; set; }

        public SpriteAnimationHelper()
        {
        }

        public SpriteAnimationHelper(SpriteAnimation spriteAnimation)
        {
            CurrentAnimation = spriteAnimation;
        }

        public SpriteAnimationFrame UpdateAnimation(float deltaTime)
        {
            if (CurrentAnimation)
            {
                animationTime += deltaTime * CurrentAnimation.FPS;

                return GetAnimationFrame();
            }

            return null;
        }

        public void ChangeAnimation(SpriteAnimation spriteAnimation)
        {
            animationTime = 0f;
            CurrentAnimation = spriteAnimation;
        }

        private SpriteAnimationFrame GetAnimationFrame()
        {
            int currentFrame = 0;

            switch (CurrentAnimation.SpriteAnimationType)
            {
                case SpriteAnimationType.Looping:
                    currentFrame = GetLoopingFrame();
                    break;
                case SpriteAnimationType.PlayOnce:
                    currentFrame = GetPlayOnceFrame();
                    break;
            }

            return CurrentAnimation.Frames[currentFrame];
        }

        private int GetLoopingFrame()
        {
            return (int)animationTime % CurrentAnimation.Frames.Count;
        }

        private int GetPlayOnceFrame()
        {
            return Mathf.Min((int)animationTime, CurrentAnimation.Frames.Count - 1);
        }
    }
}
