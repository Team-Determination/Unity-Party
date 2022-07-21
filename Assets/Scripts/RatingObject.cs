using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RatingObject : MonoBehaviour
{
    public SpriteRenderer sprite;
    public float lerpSpeed;
    public float timer;
    
    // Start is called before the first frame update
    public void ShowRating()
    {
        sprite.enabled = true;

        timer = 2.5f;

        transform.localScale = !OptionsV2.LiteMode ? new Vector3(.62f, .62f, 1f) : new Vector3(.5f, .5f, 1f);
    }


    private void Update()
    {
        if (timer > 0)
            timer -= Time.deltaTime;
        else
        {
            sprite.enabled = false;
        }
        
        if (OptionsV2.LiteMode) return;
        transform.localScale = Vector3.Lerp(transform.localScale, new Vector3(.5f, .5f, 1f),lerpSpeed);
    }
}