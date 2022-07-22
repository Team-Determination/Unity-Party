using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class RatingController : MonoBehaviour
{
    public List<SpriteRenderer> numbers;
    public SpriteRenderer parent;
    public GameObject parentNumbers;
    public List<Sprite> spritesOfNumbers;
    public List<Sprite> ratingSprites;
    LTDescr lean;
    LTDescr lean2;

    public Vector3[] defaultPositionsOfNumbers = { 
        new Vector3(-1.53400004f, -1.29999995f, 0f), 
        new Vector3(-0.975000024f, -1.25800002f, 0f), 
        new Vector3(-0.430999994f, -1.20299995f, 0f), 
        new Vector3(0.109999999f, -1.19200003f, 0f) 
    };

    public string currentCombooo;

    public void Rate(int combo, int ratingValue, int player) {
        StartCoroutine(Release());
        currentCombooo = combo.ToString("0000");
        parent.color = Color.white;
        foreach (SpriteRenderer number in numbers) {
            number.color = Color.white;
            number.gameObject.transform.localPosition = defaultPositionsOfNumbers[numbers.IndexOf(number)];
        }
        char[] chars = currentCombooo.ToCharArray();
        foreach (char charles in chars)
            print(charles + "      aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa");
        if (combo >= 1000) {
            numbers[0].gameObject.SetActive(true);
        } else {
            numbers[0].gameObject.SetActive(false);
        }

        for (int i = 0; i < chars.Length; i++) {
            numbers[i].sprite = GetNumberSpriteByIndex(Convert.ToInt32(chars[i].ToString()));
        }

        parent.sprite = GetSpriteOfRating(ratingValue);

        lean = LeanTween.value(parent.gameObject.transform.position.y, parent.gameObject.transform.position.y + 0.3f, 0.5f).setOnUpdate((float f) => {
            parent.gameObject.transform.position = new Vector2(parent.gameObject.transform.position.x, f);
        }).setOnComplete(() => {
            LeanTween.cancel(lean.id);
            Time();
        });
    }

    void Time() {
        lean = LeanTween.value(parent.color.a, 0, 0.55f).setOnUpdate((float f) => {
            parent.color = new Color(1, 1, 1, f);
        }).setOnComplete(() => LeanTween.cancel(lean.id));

        foreach (SpriteRenderer number in numbers) {
            lean = LeanTween.value(number.color.a, 0, 0.65f).setOnUpdate((float f) => {
                number.color = new Color(1, 1, 1, f);
            }).setOnComplete(() => LeanTween.cancel(lean.id));

            lean2 = LeanTween.value(number.gameObject.transform.position.y, (number.gameObject.transform.position.y - UnityEngine.Random.Range(-0.3f, 0.3f)), 0.45f).setOnUpdate((float f) => {
                number.gameObject.transform.position = new Vector2(number.gameObject.transform.position.x, f);
            }).setOnComplete(() => {
                LeanTween.cancel(lean2.id);
                Song.instance.ratingObjectPool.Release(parent.gameObject);
            });
        }
    }

    IEnumerator Release() {
        yield return new WaitForSeconds(2.1f);
        Song.instance.ratingObjectPool.Release(parent.gameObject);
    }

    Sprite GetSpriteOfRating(int index) {
        return ratingSprites[index];
    }

    Sprite GetNumberSpriteByIndex(int index) {
        return spritesOfNumbers[index]; //Max 0 to 9
    }
}
