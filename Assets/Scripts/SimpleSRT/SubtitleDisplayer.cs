using System;
using System.Collections;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
// ReSharper disable InconsistentNaming

public class SubtitleDisplayer : MonoBehaviour
{
  public TextAsset Subtitle;
  public TMP_Text Text;
  public TMP_Text Text2;
  public bool paused;
  public Coroutine process;
  
  [Range(0, 1.00f)]
  public float FadeTime;

  public void StartSubtitles()
  {
    if (process != null)
    {
      StopCoroutine(process);
    }

    Text.text = string.Empty;
    Text2.text = string.Empty;
    
    process = StartCoroutine(nameof(Begin));
  }

  public void StopSubtitles()
  {
    if (process != null)
    {
      StopCoroutine(process);
    }

    Text.text = string.Empty;
    Text2.text = string.Empty;
  }
  
  public IEnumerator Begin()
  {
    var currentlyDisplayingText = Text;
    var fadedOutText = Text2;

    currentlyDisplayingText.text = string.Empty;
    fadedOutText.text = string.Empty;

    currentlyDisplayingText.gameObject.SetActive(true);
    fadedOutText.gameObject.SetActive(true);

    yield return FadeTextOut(currentlyDisplayingText);
    yield return FadeTextOut(fadedOutText);

    var parser = new SRTParser(Subtitle);

    var startTime = Time.time;
    SubtitleBlock currentSubtitle = null;
    while (true)
    {
      if(paused) yield return null;
      var elapsed = Time.time - startTime;
      var subtitle = parser.GetForTime(elapsed);
      if (subtitle != null)
      {
        if (!subtitle.Equals(currentSubtitle))
        {
          currentSubtitle = subtitle;

          // Swap references around
          var temp = currentlyDisplayingText;
          currentlyDisplayingText = fadedOutText;
          fadedOutText = temp;

          // Switch subtitle text
          currentlyDisplayingText.text = currentSubtitle.Text;

          // And fade out the old one. Yield on this one to wait for the fade to finish before doing anything else.
          StartCoroutine(FadeTextOut(fadedOutText));

          // Yield a bit for the fade out to get part-way
          yield return new WaitForSeconds(FadeTime / 3);

          // Fade in the new current
          yield return FadeTextIn(currentlyDisplayingText);
        }
        yield return null;
      }
      else
      {
        Debug.Log("Subtitles ended");
        StartCoroutine(FadeTextOut(currentlyDisplayingText));
        yield return FadeTextOut(fadedOutText);
        currentlyDisplayingText.gameObject.SetActive(false);
        fadedOutText.gameObject.SetActive(false);
        yield break;
      }
    }
  }

  void OnValidate()
  {
    FadeTime = ((int)(FadeTime * 10)) / 10f;
  }

  IEnumerator FadeTextOut(TMP_Text text)
  {
    var toColor = text.color;
    toColor.a = 0;
    yield return Fade(text, toColor, Ease.OutSine);
  }

  IEnumerator FadeTextIn(TMP_Text text)
  {
    var toColor = text.color;
    toColor.a = 1;
    yield return Fade(text, toColor, Ease.InSine);
  }

  IEnumerator Fade(TMP_Text text, Color toColor, Ease ease)
  {
    yield return DOTween.To(() => text.color, color => text.color = color, toColor, FadeTime).SetEase(ease).OnComplete(
      () =>
      {
        if (ease == Ease.OutSine)
        {
          text.text = string.Empty;
        }
      }).WaitForCompletion();
  }
}
