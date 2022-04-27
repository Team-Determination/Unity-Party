using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class ImageDownloader : MonoBehaviour
{
    public static ImageDownloader instance;
    private void Start()
    {
        instance = this;
    }

    public void DownloadImgFromUrl(string url, Image image){    
        StartCoroutine(DownloadImage(url,image));
    }

    private IEnumerator DownloadImage(string url,Image image)
    {
        using UnityWebRequest request = UnityWebRequestTexture.GetTexture(url);
        yield return request.SendWebRequest();

        if (request.responseCode >= 400 || request.isHttpError || request.isNetworkError)
        {
            Debug.LogWarning("There was an error downloading your image from " + request.url + " \nError: " + request.error);
        }
        else
        {
            Texture2D loadedTexture = DownloadHandlerTexture.GetContent(request);
            image.sprite = Sprite.Create(loadedTexture, new Rect(0f, 0f, loadedTexture.width, loadedTexture.height), Vector2.zero);
            image.SetNativeSize();
                
        }
        request.Dispose();
    }
}
