using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Create New Protagonist",fileName = "Protagonist")]
public class Protagonist : Character
{
    [Header("Protagonist")] public AnimatorOverrideController deathAnimator;
    public AudioClip deathStartSound;
    public AudioClip deathLoopMusic;
    public AudioClip deathConfirmSound;
    public bool hasMissAnimations;

    public bool doNotFlip;
}
