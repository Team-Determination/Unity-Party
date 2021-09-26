using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using Newtonsoft.Json;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Networking;
using UnityEngine.UI;
using UnityEngine.Video;

public class Menu : MonoBehaviour
{
    public AudioSource audioSource;
    public AudioClip menuTheme;
    public AudioClip optionsMenu;

    [Header("User Interface")] public TMP_Text versionText;
    public Canvas menuCanvas;
    public GameObject mainMenu;
    
    [Space] public GameObject importBackground;
    public GameObject importPathField;
    public GameObject importGrabbingSongsMsg;
    public GameObject importConvertingSongsMsg;
    public GameObject importError;
    public GameObject importGrabError;
    public GameObject importSongObject;
    public RectTransform importSongList;
    public GameObject importFoundSongScreen;
    private List<DiscoveredSong> _grabbedSongs = new List<DiscoveredSong>();

    [Header("Bundle Downloader")] public GameObject downloadWindow;
    public GameObject bundleUrlPrompt;
    public GameObject downloadingBundlePrompt;
    public GameObject extractingBundlePrompt;
    public GameObject bundleDownloadFailPrompt;
    public GameObject bundleIncompatiblePrompt;
    public GameObject bundleExtractFailPrompt;
    
    [Header("Song List")] public Transform songListTransform;
    public GameObject songListObject;
    public Sprite defaultSongCover;
    [Space] public GameObject weekListObject;
    [Space] public GameObject songDetails;
    public GameObject chooseSongMsg;
    public GameObject loadingMsg;
    [Space] public Image previewSongCover;
    public TMP_Text previewSongName;
    public TMP_Text previewSongComposer;
    public TMP_Text previewSongCharter;
    public TMP_Text previewSongDifficulty;
    public TMP_Text previewSongDescription;
    public Button playSongButton;
    [Space] public RectTransform songDetailsLayout;
    public RectTransform songMiscDetailsLayout;
    public RectTransform songListLayout;
    [Space] public Button twoPlayerButton;
    public Button importButton;

    [Space] public GameObject introObject;
    public VideoPlayer introPlayer;
    public bool playingIntro;
    public PlayMode playMode;
    public bool canSkipIntro;
    
    
    public static Menu instance;

    // Start is called before the first frame update
    void Start()
    {
        instance = this;

        audioSource.clip = menuTheme;
        audioSource.loop = true;
        audioSource.Play();
        
        versionText.text = Application.version;

        introPlayer.Prepare();
    }
    
    public void AutoplaySingleplayer()
    {
        playMode = PlayMode.Auto;
        PlayIntro();
    }
    public void PlaySingleplayer(bool asEnemy)
    {
        playMode = asEnemy ? PlayMode.AsEnemy : PlayMode.AsBf;
        PlayIntro();
    }

    public void PlayWithTwoPlayers()
    {
        playMode = PlayMode.Two;
        PlayIntro();
    }

    public enum PlayMode
    {
        Auto = 0,
        AsBf = 1,
        AsEnemy = 2,
        Two = 3
    }

    public void PlayIntro()
    {
        playingIntro = true;
        audioSource.Stop();
        introObject.SetActive(true);
        introPlayer.Play();

        LeanTween.delayedCall(1f, () =>
        {
            canSkipIntro = true;
        });
    }
    
    
    #region Song List

    #region Song Importing

    public void DownloadBundle(TMP_InputField inputField)
    {
        downloadingBundlePrompt.SetActive(true);
        StartCoroutine(nameof(IEDownloadBundle),inputField.text);
    }

    IEnumerator IEDownloadBundle(string uri)
    {
        string bundlePath = Application.persistentDataPath + "/tmp/bundle.zip";
        UnityWebRequest www = new UnityWebRequest(uri,UnityWebRequest.kHttpVerbGET) {downloadHandler = new DownloadHandlerFile(bundlePath)};
        yield return www.SendWebRequest();
        if (www.isHttpError || www.isNetworkError)
        {
            bundleDownloadFailPrompt.SetActive(true);
            downloadingBundlePrompt.SetActive(false);
        }
        else
        {
            downloadingBundlePrompt.SetActive(false);
            extractingBundlePrompt.SetActive(true);
            string tempBundlePath = Application.persistentDataPath + "/tmp/bundle";
            if (Directory.Exists(tempBundlePath))
            {
                DirectoryInfo di = new DirectoryInfo(tempBundlePath);
                foreach (FileInfo file in di.EnumerateFiles())
                {
                    file.Delete(); 
                }
                foreach (DirectoryInfo dir in di.EnumerateDirectories())
                {
                    dir.Delete(true); 
                }
            }
            try
            {
                ZipFile.ExtractToDirectory(bundlePath, tempBundlePath);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                extractingBundlePrompt.SetActive(false);
                bundleIncompatiblePrompt.SetActive(true);
                throw;
            }
            if (!File.Exists(tempBundlePath + "/bundle-meta.json"))
            {
                bundleIncompatiblePrompt.SetActive(true);
                extractingBundlePrompt.SetActive(false);
            }
            else
            {
                string metaText = File.ReadAllText(tempBundlePath + "/bundle-meta.json");
                BundleMeta meta = JsonConvert.DeserializeObject<BundleMeta>(metaText);
                string newBundlePath = Song.instance.songsFolder + "/" + meta.bundleName;
                if (Directory.Exists(newBundlePath))
                {
                    DirectoryInfo di = new DirectoryInfo(newBundlePath);
                    foreach (FileInfo file in di.EnumerateFiles())
                    {
                        file.Delete(); 
                    }
                    foreach (DirectoryInfo dir in di.EnumerateDirectories())
                    {
                        dir.Delete(true); 
                    }
                    Directory.CreateDirectory(newBundlePath);
                }
                else
                {
                    Directory.CreateDirectory(newBundlePath);
                }
                foreach (string directory in Directory.GetDirectories(tempBundlePath))
                {
                    print("Moving " + directory + " to " + newBundlePath+directory.Replace(tempBundlePath,""));
                    Directory.Move(directory, newBundlePath+directory.Replace(tempBundlePath,""));
                }
                foreach (string file in Directory.EnumerateFiles(tempBundlePath))
                {
                    File.Move(file, newBundlePath + "/" + Path.GetFileName(file));
                }
                Directory.Delete(tempBundlePath);
                File.Delete(bundlePath);
                RefreshSongList();
                extractingBundlePrompt.SetActive(false);
                bundleUrlPrompt.SetActive(true);
                downloadWindow.SetActive(false);
            }
            
        }
    }

    public void ToggleAllFoundSongs()
    {
        foreach (DiscoveredSong discoveredSong in _grabbedSongs)
        {
            discoveredSong.toggle.isOn = !discoveredSong.toggle.isOn;
        }
    }
    

    public void ImportSelectedSongs()
    {
        importConvertingSongsMsg.SetActive(true);
        importFoundSongScreen.SetActive(false);

        foreach (Transform child in importSongList.transform)
        {
            Destroy(child.gameObject);
        }

        if (!Directory.Exists(Song.instance.songsFolder + "/Imported"))
        {
            Directory.CreateDirectory(Song.instance.songsFolder + "/Imported");
        }

        if (!File.Exists(Song.instance.songsFolder + "/Imported/bundle-meta.json"))
        {
            BundleMeta bundleMeta = new BundleMeta {bundleName = "Imported Songs", authorName = Environment.UserName};
            StreamWriter metaFile = File.CreateText(Song.instance.songsFolder + "/Imported/bundle-meta.json");
            metaFile.Write(JsonConvert.SerializeObject(bundleMeta));
            metaFile.Close();
        }

        foreach (DiscoveredSong discoveredSong in _grabbedSongs)
        {
            if (discoveredSong.doNotImport) continue;
            string difficulty = "";
            Color difficultyColor = new Color(1, 1, 0);
            
            switch (discoveredSong.info.difficulty)
            {
                case 0:
                    difficulty = "Easy";
                    difficultyColor = new Color(0, 1, 1);
                    break;
                case 1:
                    difficulty = "Normal";
                    difficultyColor = new Color(1, 1, 0);
                    break;
                case 2:
                    difficulty = "Hard";
                    difficultyColor = new Color(1, 0, 0);
                    break;
            }
            string newFolder = Song.instance.songsFolder + "/Imported/" + discoveredSong.info.songName + " ("+difficulty+")";
            if (Directory.Exists(newFolder)) continue;
            Directory.CreateDirectory(newFolder);
            File.Copy(discoveredSong.info.instPath, newFolder + "/Inst.ogg");
            File.Copy(discoveredSong.info.voicesPath, newFolder + "/Voices.ogg");
            File.Copy(discoveredSong.info.chartPath, newFolder + "/Chart.json");

            SongMeta newMeta = new SongMeta
            {
                authorName = "???",
                charterName = "???",
                songDescription = "Imported from an FNF game.",
                songName = discoveredSong.info.songName + " ("+difficulty+")",
                difficultyColor = difficultyColor,
                difficultyName = difficulty
            };
            
            
            StreamWriter metaFile = File.CreateText(newFolder + "/meta.json");
            metaFile.Write(JsonConvert.SerializeObject(newMeta));
            metaFile.Close();
        }

        importConvertingSongsMsg.SetActive(false);
        importPathField.SetActive(true);
        importBackground.SetActive(false);
        RefreshSongList();
    }
    
    public void GetSongsFromFnf(TMP_InputField field)
    {
        importGrabbingSongsMsg.SetActive(true);

        string dir = field.text;

        //dir = dir.Replace(@"\","/");
        
        string fnfSongs = dir + "/songs";
        string fnfData = dir + "/data";
        if (Directory.Exists(dir))
        {
            print(dir + " exists!");
            
            
            if (Directory.Exists(fnfSongs) & Directory.Exists(fnfData))
            {
                print($"{fnfSongs} exists!");
                print($"{fnfData} exists!");

                SearchOption option = SearchOption.TopDirectoryOnly;
                foreach (string directory in Directory.GetDirectories(fnfSongs, "*", option))
                {
                    string directoryName = directory.Replace(fnfSongs+@"\","");
                    print($"Checking if {fnfData}/{directoryName} exists.");
                    if (Directory.Exists(fnfData + "/" + directoryName))
                    {
                        print("It exists!");
                        if (File.Exists(fnfSongs + "/" + directoryName + "/Voices.ogg") &
                            File.Exists(fnfSongs + "/" + directoryName + "/Inst.ogg"))
                        {

                            print("Required music files exist.");
                            print("Checking if " + fnfData + "/" + directoryName + "/" + directoryName + "-easy.json exist.");
                            if (File.Exists(fnfData + "/" + directoryName + "/" + directoryName + "-easy.json"))
                            {
                                print("Easy chart detected.");
                                GameObject newSongGameObject = Instantiate(importSongObject,importSongList);
                                DiscoveredSong discoveredSong = newSongGameObject.GetComponent<DiscoveredSong>();
                                discoveredSong.info.songName = directoryName;
                                discoveredSong.info.chartPath =
                                    fnfData + "/" + directoryName + "/" + directoryName + "-easy.json";
                                discoveredSong.info.instPath = fnfSongs + "/" + directoryName + "/Inst.ogg";
                                discoveredSong.info.voicesPath = fnfSongs + "/" + directoryName + "/Voices.ogg";
                                discoveredSong.info.difficulty = 0;
                                
                                discoveredSong.songText.text = directoryName + " (Easy)";
                                _grabbedSongs.Add(discoveredSong);
                            }
                            print("Checking if " + fnfData + "/" + directoryName + "/" + directoryName + ".json exist.");
                            if (File.Exists(fnfData + "/" + directoryName + "/" + directoryName + ".json"))
                            {
                                print("Normal chart detected.");
                                GameObject newSongGameObject = Instantiate(importSongObject,importSongList);
                                DiscoveredSong discoveredSong = newSongGameObject.GetComponent<DiscoveredSong>();
                                discoveredSong.info.songName = directoryName;
                                discoveredSong.info.chartPath =
                                    fnfData + "/" + directoryName + "/" + directoryName + ".json";
                                discoveredSong.info.instPath = fnfSongs + "/" + directoryName + "/Inst.ogg";
                                discoveredSong.info.voicesPath = fnfSongs + "/" + directoryName + "/Voices.ogg";
                                discoveredSong.info.difficulty = 1;

                                discoveredSong.songText.text = directoryName + " (Norm)";
                                _grabbedSongs.Add(discoveredSong);
                            }
                            print("Checking if " + fnfData + "/" + directoryName + "/" + directoryName + "-hard.json exist.");
                            if (File.Exists(fnfData + "/" + directoryName + "/" + directoryName + "-hard.json"))
                            {
                                print("Hard chart detected.");
                                GameObject newSongGameObject = Instantiate(importSongObject,importSongList);
                                DiscoveredSong discoveredSong = newSongGameObject.GetComponent<DiscoveredSong>();
                                discoveredSong.info.songName = directoryName;
                                discoveredSong.info.chartPath =
                                    fnfData + "/" + directoryName + "/" + directoryName + "-hard.json";
                                discoveredSong.info.instPath = fnfSongs + "/" + directoryName + "/Inst.ogg";
                                discoveredSong.info.voicesPath = fnfSongs + "/" + directoryName + "/Voices.ogg";
                                discoveredSong.info.difficulty = 2;

                                discoveredSong.songText.text = directoryName + " (Hard)";
                                _grabbedSongs.Add(discoveredSong);
                            }

                            

                        }
                        
                        
                    }
                }
                print("Finished finding songs.");
                importGrabbingSongsMsg.SetActive(false);
                importFoundSongScreen.SetActive(true);
            }
        }
        else
        {
            importGrabError.SetActive(true);
            importGrabbingSongsMsg.SetActive(false);
        }
    }

    #endregion

    
    public void RefreshSongList()
    {
        if(songListTransform.childCount != 0)
            foreach (Transform child in songListTransform)
                Destroy(child.gameObject);
        
        SearchOption option = SearchOption.TopDirectoryOnly;
        foreach (string dir in Directory.GetDirectories(Song.instance.songsFolder, "*", option))
        {
            if (File.Exists(dir + "/bundle-meta.json"))
            {
                BundleMeta bundleMeta =
                    JsonConvert.DeserializeObject<BundleMeta>(File.ReadAllText(dir + "/bundle-meta.json"));

                if (bundleMeta == null)
                {
                    Debug.LogError("Error whilst trying to read JSON file! " + dir + "/bundle-meta.json");
                    break;
                }

                WeekListObject newWeek = Instantiate(weekListObject, songListTransform).GetComponent<WeekListObject>();

                newWeek.Author = bundleMeta.authorName;
                newWeek.BundleName = bundleMeta.bundleName;
                
                foreach (string songDir in Directory.GetDirectories(dir, "*", option))
                {
                    print("Searching in " + dir);
                    print("We got " + songDir);
                    if (File.Exists(songDir + "/meta.json") & File.Exists(songDir + "/Voices.ogg") & File.Exists(songDir + "/Inst.ogg") & File.Exists(songDir + "/Chart.json"))
                    {
                        SongMeta meta = JsonConvert.DeserializeObject<SongMeta>(File.ReadAllText(songDir + "/meta.json"));

                        if (meta == null)
                        {
                            Debug.LogError("Error whilst trying to read JSON file! " + songDir + "/meta.json");
                            break;
                        }
                
                        SongListObject newSong = Instantiate(songListObject,songListTransform).GetComponent<SongListObject>();

                        newSong.Author = meta.authorName;
                        newSong.Charter = meta.charterName;
                        newSong.Difficulty = meta.difficultyName;
                        newSong.DifficultyColor = meta.difficultyColor;
                        newSong.SongName = meta.songName;
                        newSong.Description = meta.songDescription;
                        newSong.InsturmentalPath = songDir + "/Inst.ogg";
                        newSong.directory = songDir;
                
                        string coverDir = songDir + "/Cover.png";
                
                        if (File.Exists(coverDir))
                        {
                            byte[] coverData = File.ReadAllBytes(coverDir);

                            Texture2D coverTexture2D = new Texture2D(512,512);
                            coverTexture2D.LoadImage(coverData);

                            newSong.Icon = Sprite.Create(coverTexture2D,
                                new Rect(0, 0, coverTexture2D.width, coverTexture2D.height), new Vector2(0, 0), 100);
                        }
                        else
                        {
                            newSong.Icon = defaultSongCover;
                        }

                        newWeek.songs.Add(newSong);

                        newSong.gameObject.SetActive(false);
                    }
                    else
                    {
                        Debug.LogError("Failed to find required files in " + songDir);
                    }
                }
            }
            
            
        }

        LayoutRebuilder.ForceRebuildLayoutImmediate(songListLayout);
    }

    #endregion

    // Update is called once per frame
    void Update()
    {
        if (playingIntro)
        {
            if (!introPlayer.isPlaying & introPlayer.isPrepared)
            {
                FinishIntro();
            } else if (Input.anyKeyDown & canSkipIntro)
            {
                FinishIntro();
            }

            if (!menuCanvas.enabled)
            {
                if (introPlayer.isPlaying)
                {
                    introPlayer.Stop();
                }
            }
        }
    }

    public void FinishIntro()
    {
        canSkipIntro = false;
        if (introPlayer.isPlaying)
            introPlayer.Stop();
        switch (playMode)
        {
            case PlayMode.Auto:
                Player.playAsEnemy = false;
                Player.twoPlayers = false;
                Song.instance.PlaySong(true);
                break;
            case PlayMode.AsBf:
                Player.playAsEnemy = false;
                Player.twoPlayers = false;
                Song.instance.PlaySong(false);
                break;
            case PlayMode.AsEnemy:
                Player.playAsEnemy = true;
                Player.twoPlayers = false;
                Song.instance.PlaySong(false);
                break;
            case PlayMode.Two:
                Player.twoPlayers = true;
                Player.playAsEnemy = false;
                Song.instance.PlaySong(false);
                break;
            default:
                Player.playAsEnemy = false;
                Player.twoPlayers = false;
                Song.instance.PlaySong(true);
                break;
        }

        playingIntro = false;
        introObject.SetActive(false);
    }
}
