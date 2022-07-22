using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RatingInstancer : MonoBehaviour
{
    public static RatingInstancer instance;

    private void Awake() {
        instance = this;
    }

    public void Rate(int player, int rate) {
        GameObject @object = Song.instance.ratingObjectPool.GetObject();
        @object.GetComponent<SpriteRenderer>().color = Color.white;
        if (player == 1) {
            @object.transform.SetParent(Song.instance.ratingP1Parent);
            @object.transform.position = new Vector2(2, 0);
        } else {
            @object.transform.SetParent(Song.instance.ratingP2Parent);
            @object.transform.position = new Vector2(-2, 0);
        }

        @object.GetComponent<RatingController>().Rate(player == 1 ? Song.instance.playerOneStats.currentCombo : Song.instance.playerTwoStats.currentCombo, rate, player);
    }
}
