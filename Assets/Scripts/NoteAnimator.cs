using System.Collections;
using System.Collections.Generic;
using SimpleSpriteAnimator;
using UnityEngine;

public class NoteAnimator : MonoBehaviour
{
    public SpriteAnimator[] animators;

    public float autoPlayPressTime;
    public bool forPlayerOne;

    private float _autoPlayCurrentPressTime;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    public void Play(string animationName, int type = 0)
    {
        foreach (SpriteAnimator animator in animators)
        {
            animator.Play(animationName);

            if (animationName != "Normal")
            {
                animator.spriteRenderer.color = forPlayerOne
                    ? Song.instance.player1NoteColors[type]
                    : Song.instance.player2NoteColors[type];
                
                if (Player.demoMode || !forPlayerOne & !Player.playAsEnemy || forPlayerOne & Player.playAsEnemy & !Player.twoPlayers)
                {
                    _autoPlayCurrentPressTime = autoPlayPressTime;
                }
            }
            else
            {
                animator.spriteRenderer.color = Color.white;
            }
        }

        
    }

    // Update is called once per frame
    void Update()
    {
        if (Player.demoMode || !forPlayerOne & !Player.playAsEnemy || forPlayerOne & Player.playAsEnemy)
        {
            _autoPlayCurrentPressTime -= Time.deltaTime;

            if (_autoPlayCurrentPressTime <= 0 & animators[0].CurrentAnimation.Name != "Normal")
            {
                Play("Normal");
            }
        }
    }
}
