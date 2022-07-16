using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;

namespace SimpleSpriteAnimator
{
    [RequireComponent(typeof(SpriteRenderer))]
    public class SpriteAnimator : MonoBehaviour
    {
        [SerializeField] public List<SpriteAnimation> spriteAnimations;

        [SerializeField] public bool playAutomatically = true;

        [SerializeField] public bool dontPlay = false;

        public int curFrame;

        public SpriteAnimation DefaultAnimation
        {
            get { return spriteAnimations.Count > 0 ? spriteAnimations[0] : null; }
        }

        public SpriteAnimation CurrentAnimation
        {
            get { return spriteAnimationHelper.CurrentAnimation; }
        }

        public bool Playing
        {
            get { return state == SpriteAnimationState.Playing; }
        }

        public bool Paused
        {
            get { return state == SpriteAnimationState.Paused; }
        }

        private SpriteRenderer spriteRenderer;
        private Image imageRenderer;

        public SpriteAnimationHelper spriteAnimationHelper;

        private SpriteAnimationState state = SpriteAnimationState.Playing;

        private void Awake()
        {
            if (GetComponent<SpriteRenderer>() != null) spriteRenderer = GetComponent<SpriteRenderer>();
            if (GetComponent<Image>() != null) imageRenderer = GetComponent<Image>();

            spriteAnimationHelper = new SpriteAnimationHelper();
        }

        private void Start()
        {
            if (playAutomatically)
            {
                Play(DefaultAnimation);
            }
        }

        private void LateUpdate()
        {
            if (Playing)
            {
                SpriteAnimationFrame currentFrame;
                if (!dontPlay) currentFrame = spriteAnimationHelper.UpdateAnimation(Time.deltaTime);
                else currentFrame = CurrentAnimation.Frames[curFrame];

                if (currentFrame != null)
                {
                    spriteRenderer.sprite = currentFrame.Sprite;
                    if (GetComponent<Image>() == null) transform.localPosition = currentFrame.Offset;
                    if (GetComponent<Image>() != null) imageRenderer.sprite = currentFrame.Sprite;
                }
            }
        }

        public void Play()
        {
            if (CurrentAnimation == null)
            {
                spriteAnimationHelper.ChangeAnimation(DefaultAnimation);
            }

            Play(CurrentAnimation);
        }

        public void Play(string name)
        {
            Play(GetAnimationByName(name));
        }

        public void Play(SpriteAnimation animation)
        {
            state = SpriteAnimationState.Playing;
            spriteAnimationHelper.ChangeAnimation(animation);
        }
 
        private SpriteAnimation GetAnimationByName(string name)
        {
            foreach (var t in spriteAnimations)
            {
                if (t.Name == name)
                {
                    return t;
                }
            }

            
            return null;
        }
    }
}
