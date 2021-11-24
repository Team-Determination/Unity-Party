using UnityEngine;
using System.Collections;
using System;

namespace SimpleSpriteAnimator
{
    [Serializable]
    public class SpriteAnimationFrame
    {
        [SerializeField]
        private Sprite sprite;

        public Sprite Sprite
        {
            get => sprite;
            set => sprite = value;
        }
        
        [SerializeField]
        private Vector2 offset;

        public Vector2 Offset
        {
            get => offset;
            set => offset = value;
        }
    }
        
}