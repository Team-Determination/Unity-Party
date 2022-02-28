using UnityEngine;
using System.Collections.Generic;

namespace SimpleSpriteAnimator
{
    [RequireComponent(typeof(SpriteMask))]
    public class SpriteMaskAnimator : MonoBehaviour
    {
        [SerializeField] public List<SpriteAnimation> spriteAnimations;

        [SerializeField] public bool playAutomatically = true;

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

        private SpriteMask spriteRenderer;

        public SpriteAnimationHelper spriteAnimationHelper;

        private SpriteAnimationState state = SpriteAnimationState.Playing;

        private void Awake()
        {
            spriteRenderer = GetComponent<SpriteMask>();

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
                SpriteAnimationFrame currentFrame = spriteAnimationHelper.UpdateAnimation(Time.deltaTime);

                if (currentFrame != null)
                {
                    spriteRenderer.sprite = currentFrame.Sprite;
                    transform.localPosition = currentFrame.Offset;
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
