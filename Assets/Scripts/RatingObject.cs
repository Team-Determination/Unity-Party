using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RatingObject : MonoBehaviour
{
    public SpriteRenderer sprite;
    public float liteTimer;
    
    // Start is called before the first frame update
    void Start()
    {
        if(!Song.instance.liteMode)
        {
            LeanTween.moveY(gameObject, transform.position.y - 0.2f, .75f).setOnComplete(() =>
            {
                LeanTween.alpha(gameObject, 0, .45f).setDelay(1f).setOnComplete(() => { Destroy(gameObject); });
            }).setEase(LeanTweenType.easeOutBounce);
        }
    }


    private void Update()
    {
        sprite.enabled = !(liteTimer <= 0);
        liteTimer -= Time.deltaTime;
    }
}
