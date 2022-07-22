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
        [SerializeField] public bool applyOffset = true;
        [SerializeField] public bool applySize = true;
        public SpriteAnimationFrame currentFrame;
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
        private RectTransform rect;

        public SpriteAnimationHelper spriteAnimationHelper;

        private SpriteAnimationState state = SpriteAnimationState.Playing;

        private void Awake()
        {
            if (GetComponent<SpriteRenderer>() != null) spriteRenderer = GetComponent<SpriteRenderer>();
            if (GetComponent<Image>() != null) imageRenderer = GetComponent<Image>();
            if (GetComponent<RectTransform>() != null) rect = GetComponent<RectTransform>();

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
            if (Playing && !dontPlay)
            {
                
                currentFrame = spriteAnimationHelper.UpdateAnimation(Time.deltaTime);

                if (currentFrame != null)
                {
                    spriteRenderer.sprite = currentFrame.Sprite;
                    if (applyOffset)
                        transform.localPosition = currentFrame.Offset;
                    if (GetComponent<Image>() != null) imageRenderer.sprite = currentFrame.Sprite;
                    if (GetComponent<RectTransform>() != null && applySize) rect.sizeDelta = currentFrame.SizeDelta;
                }
            }
        }

        public void SetFrame(string animName, int frame) {
            if (CurrentAnimation != GetAnimationByName(animName))
                spriteAnimationHelper.ChangeAnimation(GetAnimationByName(animName));
            SpriteAnimationFrame currentFrame;
            curFrame = frame;
            currentFrame = CurrentAnimation.Frames[frame];
            if (currentFrame != null) {
                spriteRenderer.sprite = currentFrame.Sprite;
                if (applyOffset)
                    transform.localPosition = currentFrame.Offset;
                if (GetComponent<Image>() != null)
                    imageRenderer.sprite = currentFrame.Sprite;
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

        public SpriteAnimation GetAnimationByName(string name)
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
