using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(fileName = "New Protagonist",menuName = "Create new Protagonist")]
public class Protagonist : Character
{
    [Header("Protagonist Properties")] public bool noMissAnimations;
    public AnimatorOverrideController deathAnimator;
}
