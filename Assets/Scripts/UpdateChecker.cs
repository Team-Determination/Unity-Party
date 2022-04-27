using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json;
using TMPro;
using Unity.RemoteConfig;
using UnityEngine;
using UnityEngine.UI;

public class UpdateChecker : MonoBehaviour
{
    public int build;
    
    public static bool notified;

    [Space] public GameObject updateNotificationObject;
    public TMP_Text updateNameText;
    public string updateUri;
    public Image updateImage;
    public TMP_Text updateBodyText;


    // Start is called before the first frame update
    void Start()
    {
        ConfigManager.FetchCompleted += ReceivedConfig;
        #if CANARY
        ConfigManager.SetEnvironmentID("8ce63c5b-812e-4e53-a1df-064a32d6dec4");
        #else
        ConfigManager.SetEnvironmentID("48f54a9a-9c8b-43e8-9171-80a120aad8c2");
        #endif
        if (notified) return;

        ConfigManager.FetchConfigs(Empty,Empty );
    }

    public Empty Empty { get; set; }


    private void ReceivedConfig(ConfigResponse response)
    {
        if (response.status == ConfigRequestStatus.Success)
        {
            int newVer = ConfigManager.appConfig.GetInt("NewBuild");
            if (build < newVer)
            {
                string json = ConfigManager.appConfig.GetJson("NewBuildInfo");
                UpdateInformation info = JsonConvert.DeserializeObject<UpdateInformation>(json);

                updateNameText.text = info.updateName;
                updateBodyText.text = info.updateBody;
                
                byte[]  imageBytes = Convert.FromBase64String(info.updateImageBase);
                Texture2D tex = new Texture2D(2, 2);
                tex.LoadImage( imageBytes );
                Sprite sprite = Sprite.Create(tex, new Rect(0.0f, 0.0f, tex.width, tex.height), new Vector2(0.5f, 0.5f), 100.0f);

                updateImage.sprite = sprite;
                
                updateUri = info.updateUri;
                updateNotificationObject.SetActive(true);

            }
        }
    }

    public void RemindNextTime()
    {
        updateNotificationObject.SetActive(false);
    }

    public void OpenUpdate()
    {
        Application.OpenURL(updateUri);
    }


    // Update is called once per frame
    void Update()
    {
        
    }
}

public struct Empty
{
}

[Serializable]
public class UpdateInformation
{
    [TextArea(2, 4)] public string updateName;
    [TextArea(2, 4)] public string updateUri;
    [TextArea(2, 4)] public string updateImageBase;
    [TextArea(4, 40)] public string updateBody;
}
