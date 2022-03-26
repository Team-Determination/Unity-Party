using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class BundleMigration : MonoBehaviour
{
    public TMP_Text progressText;
    public Image progressImage;

    [Space] public GameObject completeScreen;
    public GameObject processScreen;
    public GameObject confirmScreen;



    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void ConvertBundles()
    {
        confirmScreen.SetActive(false);
        processScreen.SetActive(true);
        
        string oldDir = Application.persistentDataPath + "/Songs";
        string newDir = Application.persistentDataPath + "/NewBundles";
        string finalDir = Application.persistentDataPath + "/Bundles";
        string[] bundleDirectories = Directory.GetDirectories(oldDir,"*",SearchOption.TopDirectoryOnly);

        int bundlesMoved = 0;
        int totalSongsConverted = 0;
        progressText.text = "Moving old bundles to temporary folder...";
        progressImage.fillAmount = (float)bundlesMoved / bundleDirectories.Length;

        if (!Directory.Exists(newDir))
        {
            Directory.CreateDirectory(newDir);
        }

        if (!Directory.Exists(finalDir))
        {
            Directory.CreateDirectory(finalDir);
        }
            
        foreach (string directory in bundleDirectories)
        {
            DirectoryInfo dirInfo = new DirectoryInfo(directory);
            Directory.Move(directory, newDir+"/"+dirInfo.Name);
            bundlesMoved++;
            
        }
        progressText.text = "Converting old song meta files...";
        progressImage.fillAmount = 0;
        bundleDirectories = Directory.GetDirectories(newDir, "*", SearchOption.TopDirectoryOnly);
        List<string> songDirectories = new List<string>();
        foreach (string directory in bundleDirectories)
        {
            songDirectories.AddRange(Directory.GetDirectories(directory));
        }

        foreach (string directory in songDirectories)
        {
            string metaJson = directory + "/meta.json";
            string chart = directory + "/Chart.json";
            if (File.Exists(metaJson) & File.Exists(chart))
            {
                SongMeta meta = JsonConvert.DeserializeObject<SongMeta>(File.ReadAllText(metaJson));

                string metaDifficultyName = StripIllegalCharacters(meta.difficultyName);
                SongMetaV2 newMeta = new SongMetaV2
                {
                    songName = meta.songName,
                    credits = new Dictionary<string, string>
                    {
                        {"Composer",meta.authorName},
                        {"Charter",meta.charterName}
                    },
                    difficulties = new Dictionary<string, Color>
                    {
                        {metaDifficultyName,meta.difficultyColor}
                    },
                    songDescription = meta.songDescription
                };
                File.WriteAllText(metaJson, JsonConvert.SerializeObject(newMeta));

                File.Move(chart, directory + "/Chart-" + metaDifficultyName.ToLower()+".json");
            }

            totalSongsConverted++;
            progressImage.fillAmount = totalSongsConverted / (float) songDirectories.Count;
        }

        bundlesMoved = 0;
        bundleDirectories = Directory.GetDirectories(newDir,"*",SearchOption.TopDirectoryOnly);

        progressText.text = "Moving the converted bundles to the final destination...";
        
        foreach (string bundle in bundleDirectories)
        {
            DirectoryInfo dirInfo = new DirectoryInfo(bundle);
            Directory.Move(bundle, finalDir+"/"+dirInfo.Name);
            bundlesMoved++;
            progressImage.fillAmount = bundlesMoved / (float) bundleDirectories.Length;

        }
        

        ConvertDone();
    }

    public void ReturnToMenu()
    {
        SceneManager.LoadScene("Title");
    }

    public string StripIllegalCharacters(string input)
    {
        string[] illegalChars =
        {
            "?",
            "<",
            ">",
            ":",
            "\"",
            "/",
            "\\",
            "|",
            "*"
        };
        
        
        foreach(string character in illegalChars)
        {
           input = input.Replace(character, "");
        }

        return input;
    }
    
    public void ConvertDone()
    {
        string songsFolder = Application.persistentDataPath + "/Songs";
        if (Directory.Exists(songsFolder))
        {
            Directory.Delete(songsFolder,true);
        }
        
        completeScreen.SetActive(true);
        processScreen.SetActive(false);
    }
}


