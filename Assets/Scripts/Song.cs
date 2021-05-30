using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using FridayNightFunkin;
using Newtonsoft.Json;
using TMPro;
using TMPro.SpriteAssetUtilities;
using UnityEngine;
using UnityEngine.UI;
using Debug = UnityEngine.Debug;
using Random = UnityEngine.Random;

public class Song : MonoBehaviour
{

    #region Variables

    public AudioSource soundSource;
    public AudioClip startSound;
    [Space]
    public AudioSource[] musicSources;
    public AudioSource vocalSource;
    public AudioSource oopsSource;
    public AudioClip musicClip;
    public AudioClip vocalClip;
    public AudioClip menuClip;
    public AudioClip[] noteMissClip;

    [Space] public Image transitionImage;
    public GameObject fromScreen;
    public GameObject toScreen;
    
    [Space] public Notes notesObject;
    [Space] public bool hasStarted;
    public float bottomSafeWindow = .45f;
    public float topSafeWindow = 1f;

    [Space] public GameObject ratingObject; 
    public Sprite sickSprite;
    public Sprite goodSprite;
    public Sprite badSprite;
    public Sprite shitSprite;
    public TMP_Text currentScoringText;
    public float ratingLayerTimer;
    private float _ratingLayerDefaultTime = 1.2f;
    private int _currentRatingLayer = 0;
    private int _highestSickCombo = 0;
    private int _currentSickCombo = 0;
    private int _hitNotes = 0;
    private int _totalNoteHits = 0;
    private int _currentScore = 0;
    private int _missedHits = 0;

    public Stopwatch stopwatch;
    public Stopwatch beatStopwatch;

    [Space, TextArea(2,12)] public string jsonDir;
    public float notesOffset;
    public float notesSpeedBoost;
    public float noteDelay;

    [Header("Multiplayer")] public GameObject multiplayerScreen;
    [Space] public string myPlayerName;
    
    [Space] public TMP_Text boyfriendPlayerName;
    public TMP_Text opponentPlayerName;
    [Space] public TMP_Text lobbyChatText;

    [Space] public Canvas battleCanvas;
    public Canvas menuCanvas;
    public GameObject generatingSongMsg;
    public GameObject songListScreen;
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

    [Header("Volume Testing")]
    public AudioClip testVoices;
    public AudioClip testInst;
    public string testData;
    public Slider voiceVolumeSlider;
    public Slider instVolumeSlider;
    
    public GameObject menuScreen;
    public TMP_Dropdown keysetDropdown;
    
    [Header("Song List")] public Transform songListTransform;
    public GameObject songListObject;
    public Sprite defaultSongCover;
    [Space] public GameObject songDetails;
    public GameObject chooseSongMsg;
    public GameObject loadingMsg;
    [Space]
    public Image previewSongCover;
    public TMP_Text previewSongName;
    public TMP_Text previewSongComposer;
    public TMP_Text previewSongCharter;
    public TMP_Text previewSongDifficulty;
    public TMP_Text previewSongDescription;
    public Button playSongButton;
    [Space] public RectTransform songDetailsLayout;
    public RectTransform songMiscDetailsLayout;
    public RectTransform songListLayout;

    [Space] public Transform player1Notes;
    public List<List<NoteObject>> player1NotesObjects;
    public Animator[] player1NotesAnimators;
    public Transform player1Left;
    public Transform player1Down;
    public Transform player1Up;
    public Transform player1Right;
    [Space]
    public Transform player2Notes;
    public List<List<NoteObject>> player2NotesObjects;
    public Animator[] player2NotesAnimators;
    public Transform player2Left;
    public Transform player2Down;
    public Transform player2Up;
    public Transform player2Right;

    [Header("Prefabs")]
    public GameObject leftArrow;
    public GameObject leftArrowHold;
    public Sprite leftArrowHoldEnd;
    public GameObject downArrow;
    public GameObject downArrowHold;
    public Sprite downArrowHoldEnd;
    public GameObject upArrow;
    public GameObject upArrowHold;
    public Sprite upArrowHoldEnd;
    public GameObject rightArrow;
    public GameObject rightArrowHold;
    public Sprite rightArrowHoldEnd;

    [Header("Enemy")] public GameObject enemyObj;
    public string enemyName;
    public Animator enemyAnimation;
    public float enemyIdleTimer = .3f;
    private float _currentEnemyIdleTimer;
    public float enemyNoteTimer = .25f;
    private readonly float[] _currentEnemyNoteTimers = {0,0,0,0};
    private readonly float[] _currentDemoNoteTimers = {0,0,0,0};

    [Header("Boyfriend")] public GameObject bfObj;
    public Animator boyfriendAnimation;
    public float boyfriendIdleTimer = .3f;
    private float _currentBoyfriendIdleTimer;

    private FNFSong _song;

    public static Song instance;

    [Space] public float health = 100;

    private const float MAXHealth = 200;
    public float healthLerpSpeed;
    public GameObject healthBar;
    public Image boyfriendHealthIcon;
    public Image boyfriendHealthBar;
    public Image enemyHealthIcon;
    public Image enemyHealthBar;

    
    [Space]
    public NoteObject lastNote;
    public float stepCrochet;
    public float beatsPerSecond;
    public int currentBeat;
    public bool beat;
    
    private float _bfRandomDanceTimer;
    private float _enemyRandomDanceTimer;

    private string _songsFolder;
    private string _selectedSongDir;

    [HideInInspector] public SongListObject selectedSong;
    private bool _songStarted;

    private bool _onlineMode;

    #endregion

    private void Start()
    {
        instance = this;

        _songsFolder = Application.dataPath + "/Songs";
        
        player1Notes.gameObject.SetActive(false);
        player2Notes.gameObject.SetActive(false);
        battleCanvas.enabled = false;
        healthBar.SetActive(false);

        musicSources[0].clip = menuClip;
        musicSources[0].Play();
        musicSources[0].loop = true;

        musicSources[0].volume = PlayerPrefs.GetFloat("Music Volume", .75f);
        instVolumeSlider.value = PlayerPrefs.GetFloat("Music Volume", .75f);
        
        vocalSource.volume = PlayerPrefs.GetFloat("Voice Volume", .75f);
        voiceVolumeSlider.value = PlayerPrefs.GetFloat("Voice Volume", .75f);

        keysetDropdown.value = PlayerPrefs.GetInt("Key Set", 0);

        myPlayerName = PlayerPrefs.GetString("Player Name", "Player");
    }

    #region Menu

    public void PlaySolo()
    {
        StartTransition(songListScreen, menuScreen);
        _onlineMode = false;
    }

    public void PlayOnline()
    {
        StartTransition(multiplayerScreen, menuScreen);
        _onlineMode = true;
    }

    #endregion


    #region Song Gameplay

    public void PlaySong()
    {
        _currentRatingLayer = 0; 
        _highestSickCombo = 0;
        _currentSickCombo = 0;
        _hitNotes = 0;
        _totalNoteHits = 0;
        _currentScore = 0;
        _missedHits = 0;
        currentBeat = 0;
        
        UpdateScoringInfo();
        
        _selectedSongDir = _songsFolder + "/" + selectedSong.SongName;
        jsonDir = _selectedSongDir + "/Chart.json";


        
        battleCanvas.enabled = true;
        generatingSongMsg.SetActive(true);
        
        menuCanvas.enabled = false;
        menuScreen.SetActive(true);
        songListScreen.SetActive(false);
        chooseSongMsg.SetActive(true);
        songDetails.SetActive(false);



        StartCoroutine(nameof(SetupSong));

    }

    IEnumerator SetupSong()
    {
        
        WWW www1 = new WWW(_selectedSongDir+"/Inst.ogg");
        if (www1.error != null)
        {
            Debug.LogError(www1.error);
        }
        else
        {
            musicClip = www1.GetAudioClip();
            while (musicClip.loadState != AudioDataLoadState.Loaded)
                yield return new WaitForSeconds(0.1f);
            WWW www2 = new WWW(_selectedSongDir+"/Voices.ogg");
            if (www2.error != null)
            {
                Debug.LogError(www2.error);
            }
            else
            {
                vocalClip = www2.GetAudioClip();
                while (vocalClip.loadState != AudioDataLoadState.Loaded)
                    yield return new WaitForSeconds(0.1f);
                print("Sounds loaded, generating song.");
                GenerateSong();
            }
        }
    }
    
    void GenerateSong()
    {

        
        health = MAXHealth / 2;

        _song = new FNFSong(jsonDir);

        beatsPerSecond = (float)_song.Bpm / 60;

        stepCrochet = (60 / (float) _song.Bpm * 1000 / 4);
        
        //GENERATE PLAYER ONE NOTES
        player1NotesObjects = new List<List<NoteObject>>
        {
            new List<NoteObject>(), new List<NoteObject>(), new List<NoteObject>(), new List<NoteObject>()
        };

        //GENERATE PLAYER TWO NOTES
        player2NotesObjects = new List<List<NoteObject>>
        {
            new List<NoteObject>(), new List<NoteObject>(), new List<NoteObject>(), new List<NoteObject>()
        };

        if (_song == null)
        {
            Debug.LogError("Error with song data");
            return;
        }
        foreach (FNFSong.FNFSection section in _song.Sections)
        {
            foreach (var noteData in section.Notes)
            {
                GameObject newNoteObj;
                List<decimal> data = noteData.ConvertToNote();
                bool mustHitNote = section.MustHitSection;
                if (data[1] > 3)
                    mustHitNote = !section.MustHitSection;
                int noteType = Convert.ToInt32(data[1] % 4);
                Vector3 spawnPos;

                float susLength = (float) data[2];

                /*
                if (susLength > 0)
                {
                    isSusNote = true;
                    
                }
                */
                
                susLength = susLength / stepCrochet;
                print("Sus length is " + susLength);
                

                switch (noteType)
                {
                    case 0: //Left
                        newNoteObj = Instantiate(leftArrow);
                        spawnPos = mustHitNote ? player1Left.position : player2Left.position;
                        break;
                    case 1: //Down
                        newNoteObj = Instantiate(downArrow);
                        spawnPos = mustHitNote ? player1Down.position : player2Down.position;
                        break;
                    case 2: //Up
                        newNoteObj = Instantiate(upArrow);
                        spawnPos = mustHitNote ? player1Up.position : player2Up.position;
                        break;
                    case 3: //Right
                        newNoteObj = Instantiate(rightArrow);
                        spawnPos = mustHitNote ? player1Right.position : player2Right.position;
                        break;
                    default:
                        Debug.LogError("Invalid note data.");
                        return;
                }


                spawnPos += Vector3.down * (Convert.ToSingle(data[0] / (decimal) notesOffset) + (_song.Speed * noteDelay));
                spawnPos.y -= (_song.Bpm / 60) * startSound.length * _song.Speed;
                newNoteObj.transform.position = spawnPos;
                //newNoteObj.transform.position += Vector3.down * Convert.ToSingle(secNoteData[0] / notesOffset);
                NoteObject nObj = newNoteObj.GetComponent<NoteObject>();

                nObj.ScrollSpeed = -_song.Speed;
                nObj.strumTime = (float) data[0];
                nObj.type = noteType;
                nObj.mustHit = mustHitNote;
                nObj.dummyNote = false;
                
                if (mustHitNote)
                    player1NotesObjects[noteType].Add(nObj);
                else
                    player2NotesObjects[noteType].Add(nObj);
                lastNote = nObj;
                for (int i = 0; i < Math.Floor(susLength); i++)
                {
                    GameObject newSusNoteObj;
                    Vector3 susSpawnPos;

                    bool setAsLastSus = false;
                    
                    switch (noteType)
                    {
                        case 0: //Left
                            newSusNoteObj = Instantiate(leftArrowHold);
                            if ((i + 1) == Math.Floor(susLength))
                            {
                                newSusNoteObj.GetComponent<SpriteRenderer>().sprite = leftArrowHoldEnd;
                                setAsLastSus = true;
                            }
                            susSpawnPos = mustHitNote ? player1Left.position : player2Left.position;
                            break;
                        case 1: //Down
                            newSusNoteObj = Instantiate(downArrowHold);
                            if ((i + 1) == Math.Floor(susLength))
                            {
                                newSusNoteObj.GetComponent<SpriteRenderer>().sprite = downArrowHoldEnd;
                                setAsLastSus = true;
                            }
                            susSpawnPos = mustHitNote ? player1Down.position : player2Down.position;
                            break;
                        case 2: //Up
                            newSusNoteObj = Instantiate(upArrowHold);
                            if ((i + 1) == Math.Floor(susLength))
                            {
                                newSusNoteObj.GetComponent<SpriteRenderer>().sprite = upArrowHoldEnd;
                                setAsLastSus = true;
                            }
                            susSpawnPos = mustHitNote ? player1Up.position : player2Up.position;
                            break;
                        case 3: //Right
                            newSusNoteObj = Instantiate(rightArrowHold);
                            if ((i + 1) == Math.Floor(susLength))
                            {
                                newSusNoteObj.GetComponent<SpriteRenderer>().sprite = rightArrowHoldEnd;
                                setAsLastSus = true;
                            }
                            susSpawnPos = mustHitNote ? player1Right.position : player2Right.position;
                            break;
                        default:
                            newSusNoteObj = Instantiate(leftArrowHold);
                            if ((i + 1) == Math.Floor(susLength))
                            {
                                newSusNoteObj.GetComponent<SpriteRenderer>().sprite = leftArrowHoldEnd;
                                setAsLastSus = true;
                            }
                            susSpawnPos = mustHitNote ? player1Left.position : player2Left.position;
                            break;
                    }
                    susSpawnPos += Vector3.down * (Convert.ToSingle(data[0] / (decimal) notesOffset) + (_song.Speed * noteDelay));
                    susSpawnPos.y -= (_song.Bpm / 60) * startSound.length * _song.Speed;
                    newSusNoteObj.transform.position = susSpawnPos;
                    NoteObject susObj = newSusNoteObj.GetComponent<NoteObject>();
                    susObj.type = noteType;
                    susObj.ScrollSpeed = -_song.Speed;
                    susObj.mustHit = mustHitNote;
                    susObj.strumTime = (float) data[0] + (stepCrochet * i) + stepCrochet;
                    susObj.susNote = true;
                    susObj.dummyNote = false;
                    susObj.lastSusNote = setAsLastSus;
                    susObj.GenerateHold(lastNote);
                    if (mustHitNote)
                        player1NotesObjects[noteType].Add(susObj);
                    else
                        player2NotesObjects[noteType].Add(susObj);
                    lastNote = susObj;
                }
                
                

                
                
            }
            
            
        }

        /*foreach (List<NoteObject> nte in player1NotesObjects)
        {
            foreach (NoteObject nte2 in nte)
            {
                print(nte2.transform.position);
            }
        }*/

        player1Notes.gameObject.SetActive(true);
        player2Notes.gameObject.SetActive(true);

        generatingSongMsg.SetActive(false);

        if(!Player.demoMode)
        {
            healthBar.SetActive(true);
            currentScoringText.enabled = true;
        }
        else
        {
            currentScoringText.enabled = false;
        }
        
        hasStarted = true;
        _songStarted = false;

        musicSources[0].loop = false;
        musicSources[0].Stop();

        soundSource.clip = startSound;
        soundSource.Play();

        menuCanvas.enabled = false;
        battleCanvas.enabled = true;
        
        StartCoroutine(nameof(SongStart), startSound.length);
    }
    
    IEnumerator SongStart(float delay)
    {
        if (Player.demoMode)
        {
            File.Delete(Application.dataPath + "/tmp/ok.json");
            Directory.Delete(Application.dataPath + "/tmp");
        }

        yield return new WaitForSeconds(delay);


        beatStopwatch = new Stopwatch();
        beatStopwatch.Start();
        
        

        musicSources[0].clip = musicClip;
        vocalSource.clip = vocalClip;

        foreach (AudioSource source in musicSources)
        {
            source.Play();
        }
        
        
        
        vocalSource.Play();

        _songStarted = true;
        
        notesObject.isActive = true;
        
        stopwatch = new Stopwatch();
        stopwatch.Start();
    }

    #endregion

    #region Game Options

    public void TestVolume()
    {
        Player.demoMode = true;


        Directory.CreateDirectory(Application.dataPath + "/tmp");
        
        StreamWriter testFile = File.CreateText(Application.dataPath + "/tmp/ok.json");
        testFile.Write(testData);
        testFile.Close();

        vocalClip = testVoices;
        musicClip = testInst;

        voiceVolumeSlider.transform.parent.gameObject.SetActive(true);
        instVolumeSlider.transform.parent.gameObject.SetActive(true);

        jsonDir = Application.dataPath + "/tmp/ok.json";
        GenerateSong();
    }

    public void OnKeySetChange(Int32 val)
    {
        PlayerPrefs.SetInt("Key Set", val);
        Player.UpdateKeySet();
    }

    #endregion
    
    public void QuitGame()
    {
        Application.Quit();
    }


    #region Song List

    #region Song Importing

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
            string newFolder = _songsFolder + "/" + discoveredSong.info.songName + " ("+difficulty+")";
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
        foreach (string dir in Directory.GetDirectories(_songsFolder, "*", option))
        {

            if (File.Exists(dir + "/meta.json") & File.Exists(dir + "/Voices.ogg") & File.Exists(dir + "/Inst.ogg") & File.Exists(dir + "/Chart.json"))
            {
                
                
                SongMeta meta = JsonConvert.DeserializeObject<SongMeta>(File.ReadAllText(dir + "/meta.json"));

                if (meta == null)
                {
                    Debug.LogError("Error whilst trying to read JSON file! " + dir + "/meta.json");
                    break;
                }
                
                SongListObject newSong = Instantiate(songListObject,songListTransform).GetComponent<SongListObject>();

                
                
                newSong.Author = meta.authorName;
                newSong.Charter = meta.charterName;
                newSong.Difficulty = meta.difficultyName;
                newSong.DifficultyColor = meta.difficultyColor;
                newSong.SongName = meta.songName;
                newSong.Description = meta.songDescription;
                newSong.InsturmentalPath = dir + "/Inst.ogg";
                
                string coverDir = dir + "/Cover.png";
                
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



            }
            else
            {
                Debug.LogError("Failed to find required files in " + dir);
            }
        }

        LayoutRebuilder.ForceRebuildLayoutImmediate(songListLayout);
    }

    #endregion

    #region Screen Transitions

    public void SetFromScreen(GameObject screen) => fromScreen = screen;
    public void SetToScreen(GameObject screen) => toScreen = screen;
    public void StartTransition() => StartTransition(toScreen, fromScreen);

    public void StartTransition(GameObject screenTwo, GameObject screenOne = null)
    {
        transitionImage.raycastTarget = true;
        LeanTween.alpha(transitionImage.GetComponent<RectTransform>(), 1, .45f).setOnComplete(() =>
        {
            if(screenOne != null)
                screenOne.SetActive(false);
            screenTwo.SetActive(true);
            LeanTween.alpha(transitionImage.GetComponent<RectTransform>(), 0, .45f).setOnComplete(() =>
            {
                transitionImage.raycastTarget = false;
            });
        });
    }

    #endregion



    #region Animating

    public void EnemyPlayAnimation(string animationName)
    {
        enemyAnimation.Play(enemyName + " " + animationName,0,0);
        enemyAnimation.speed = 0;
        
        enemyAnimation.Play(enemyName + " " + animationName);
        enemyAnimation.speed = 1;
        
        _currentEnemyIdleTimer = enemyIdleTimer;
    }

    private void BoyfriendPlayAnimation(string animationName)
    {
        boyfriendAnimation.Play("BF " + animationName,0,0);
        boyfriendAnimation.speed = 0;
        
        boyfriendAnimation.Play("BF " + animationName);
        boyfriendAnimation.speed = 1;

        
        _currentBoyfriendIdleTimer = boyfriendIdleTimer;
    }
    
    public void AnimateNote(int player, int type, string animName)
    {
        switch (player)
        {
            case 1: //Boyfriend
                player1NotesAnimators[type].Play(animName);

                if (animName == "Activated" & Player.demoMode)
                {
                    _currentDemoNoteTimers[type] = enemyNoteTimer;
                }

                break;
            case 2: //Opponent
                player2NotesAnimators[type].Play(animName);

                if (animName == "Activated")
                {
                    _currentEnemyNoteTimers[type] = enemyNoteTimer;
                }
                break;
        }
    }

    #endregion

    #region Note & Score Registration

    public enum Rating
    {
        Sick = 1,
        Good = 2,
        Bad = 3,
        Shit = 4
    }

    public void UpdateScoringInfo()
    {
        float accuracy;
        float accuracyPercent;
        if(_totalNoteHits != 0)
        {
            accuracy = (float)_hitNotes / _totalNoteHits;
            accuracyPercent = (float) Math.Round(accuracy, 4);
            accuracyPercent *= 100;
        }
        else
        {
            accuracyPercent = 0;
        }

        currentScoringText.text =
            $"Score: {_currentScore}\nAccuracy: {accuracyPercent}%\nCombo: {_currentSickCombo} ({_highestSickCombo})\nMisses: {_missedHits}";
    }
    
    public void NoteHit(int note)
    {
        vocalSource.mute = false;

        switch (note)
        {
            case 0:
                //Left
                BoyfriendPlayAnimation("Sing Left");
                break;
            case 1:
                //Down
                BoyfriendPlayAnimation("Sing Down");
                break;
            case 2:
                //Up
                BoyfriendPlayAnimation("Sing Up");
                break;
            case 3:
                //Right
                BoyfriendPlayAnimation("Sing Right");
                break;
        }

        AnimateNote(1, note,"Activated");
        

        NoteObject tmpObj = player1NotesObjects[note][0];

        if(!tmpObj.susNote)
        {
            _totalNoteHits++;

            float yPos = tmpObj.transform.position.y;

            Rating rating;
            RatingObject ratingObjectScript;

            GameObject newRatingObject = Instantiate(ratingObject);

            ratingObjectScript = newRatingObject.GetComponent<RatingObject>();
            if (yPos <= 4.85 & yPos >= 4)
            {
                rating = Rating.Sick;

                ratingObjectScript.sprite.sprite = sickSprite;

                health += 5;

                _currentSickCombo++;

                _currentScore += 10;

                
            }
            else if ((yPos < 5.30 & yPos >= 4.85) || (yPos < 4 & yPos >= 3.45))
            {
                rating = Rating.Good;

                ratingObjectScript.sprite.sprite = goodSprite;

                health += 2;
                _currentScore += 5;
                _currentSickCombo++;
            }
            else if (yPos < 5.50 & yPos >= 5.30)
            {
                rating = Rating.Bad;

                ratingObjectScript.sprite.sprite = badSprite;
                health += 1;

                _currentScore += 1;
                _currentSickCombo++;
            }
            else if (yPos > 5.50)
            {
                rating = Rating.Shit;

                ratingObjectScript.sprite.sprite = shitSprite;

                _currentSickCombo = 0;
            }
            
            if (_highestSickCombo < _currentSickCombo)
            {
                _highestSickCombo = _currentSickCombo;
            }
            _hitNotes++;


            _currentRatingLayer++;
            ratingObjectScript.sprite.sortingOrder = _currentRatingLayer;
            ratingLayerTimer = _ratingLayerDefaultTime;
        }

        UpdateScoringInfo();
        player1NotesObjects[note].Remove(tmpObj);
        Destroy(tmpObj.gameObject);
    }

    public void NoteMiss(int note)
    {
        print("MISS!!!");
        vocalSource.mute = true;
        oopsSource.PlayOneShot(noteMissClip[Random.Range(0, noteMissClip.Length)]);

        switch (note)
        {
            case 0:
                //Left
                BoyfriendPlayAnimation("Sing Left Miss");
                break;
            case 1:
                //Down
                BoyfriendPlayAnimation("Sing Down Miss");
                break;
            case 2:
                //Up
                BoyfriendPlayAnimation("Sing Up Miss");
                break;
            case 3:
                //Right
                BoyfriendPlayAnimation("Sing Right Miss");
                break;
        }

        health -= 3;
        _currentScore -= 5;
        _currentSickCombo = 0;
        _missedHits++;
        _totalNoteHits++;
        
        UpdateScoringInfo();

    }

        #endregion

    


    // Update is called once per frame
    void Update()
    {
        if (hasStarted)
        {

            if (Player.demoMode & _songStarted)
            {
            
                musicSources[0].volume = instVolumeSlider.value;
                vocalSource.volume = voiceVolumeSlider.value;

                if (Input.GetKeyDown(KeyCode.Return))
                {
                    PlayerPrefs.SetFloat("Music Volume", instVolumeSlider.value);
                    PlayerPrefs.SetFloat("Voice Volume", voiceVolumeSlider.value);
                    musicSources[0].Stop();
                    vocalSource.Stop();
                    
                    
                    
                    voiceVolumeSlider.transform.parent.gameObject.SetActive(false);
                    instVolumeSlider.transform.parent.gameObject.SetActive(false);

                    Player.demoMode = false;
                }
            }

            if (_songStarted)
            {
                /*if ((float)beatStopwatch.ElapsedMilliseconds / 1000 >= beatsPerSecond)
                {
                    beatStopwatch.Restart();
                    currentBeat++;
                    float offset;
                    offset = beat ? 0.5f : -0.5f;
                    beat = !beat;
                    if (currentBeat % 8 != 0)
                    {
                        LeanTween.moveX(player1Notes.gameObject, 4.45f + offset, beatsPerSecond / 2)
                            .setEase(LeanTweenType.easeOutExpo).setOnComplete((
                                () =>
                                {
                                    LeanTween.moveX(player1Notes.gameObject, 4.45f, beatsPerSecond / 2)
                                        .setEase(LeanTweenType.easeInExpo);
                                }));
                        LeanTween.moveX(player2Notes.gameObject, -4.45f + offset, beatsPerSecond / 2)
                            .setEase(LeanTweenType.easeOutExpo).setOnComplete((
                                () =>
                                {
                                    LeanTween.moveX(player2Notes.gameObject, -4.45f, beatsPerSecond / 2)
                                        .setEase(LeanTweenType.easeInExpo);
                                }));                    
                    }
                    else
                    {
                        LeanTween.moveY(player1Notes.gameObject, 4.45f + 1, beatsPerSecond / 2)
                            .setEase(LeanTweenType.easeOutExpo).setOnComplete((
                                () =>
                                {
                                    LeanTween.moveY(player1Notes.gameObject, 4.45f, beatsPerSecond / 2)
                                        .setEase(LeanTweenType.easeInExpo);
                                }));
                        LeanTween.moveY(player2Notes.gameObject, 4.45f + 1, beatsPerSecond / 2)
                            .setEase(LeanTweenType.easeOutExpo).setOnComplete((
                                () =>
                                {
                                    LeanTween.moveY(player2Notes.gameObject, 4.45f, beatsPerSecond / 2)
                                        .setEase(LeanTweenType.easeInExpo);
                                }));  
                    }
                }*/
            }
            
            if (health > MAXHealth)
                health = MAXHealth;
            if (health < 0)
                health = 0;

            float healthPercent = health / MAXHealth;
            boyfriendHealthBar.fillAmount = healthPercent;
            enemyHealthBar.fillAmount = 1 - healthPercent;

            var rectTransform = enemyHealthIcon.rectTransform;
            var anchoredPosition = rectTransform.anchoredPosition;
            Vector2 enemyPortraitPos = anchoredPosition;
            enemyPortraitPos.x = -(healthPercent * 594 - (300)) - 50;

            Vector2 boyfriendPortraitPos = anchoredPosition;
            boyfriendPortraitPos.x = -(healthPercent * 594 - (300)) + 50;



            anchoredPosition = enemyPortraitPos;
            rectTransform.anchoredPosition = anchoredPosition;
            boyfriendHealthIcon.rectTransform.anchoredPosition = boyfriendPortraitPos;

            if (!musicSources[0].isPlaying & _songStarted)
            {
                //Song is done.

                stopwatch.Stop();
                beatStopwatch.Stop();

                Player.demoMode = false;

                hasStarted = false;
                foreach (List<NoteObject> noteList in player1NotesObjects.ToList())
                {
                    foreach (NoteObject noteObject in noteList.ToList())
                    {
                        noteList.Remove(noteObject);
                        Destroy(noteObject.gameObject);
                    }
                }
                
                
                foreach (List<NoteObject> noteList in player2NotesObjects.ToList())
                {
                    foreach (NoteObject noteObject in noteList.ToList())
                    {
                        noteList.Remove(noteObject);
                        Destroy(noteObject.gameObject);
                    }
                }
                
                battleCanvas.enabled = false;
                
                player1Notes.gameObject.SetActive(false);
                player2Notes.gameObject.SetActive(false);

                healthBar.SetActive(false);

                
                menuScreen.SetActive(false);
                StartTransition(menuScreen);

        
                menuCanvas.enabled = true;

                musicSources[0].clip = menuClip;
                musicSources[0].loop = true;
                musicSources[0].Play();
            }
        }
        else
        {
            _bfRandomDanceTimer -= Time.deltaTime;
            _enemyRandomDanceTimer -= Time.deltaTime;

            if (_bfRandomDanceTimer <= 0)
            {
                switch (Random.Range(0, 4))
                {
                    case 1:
                        BoyfriendPlayAnimation("Sing Left");
                        break;
                    case 2:
                        BoyfriendPlayAnimation("Sing Down");
                        break;
                    case 3:
                        BoyfriendPlayAnimation("Sing Up");
                        break;
                    case 4:
                        BoyfriendPlayAnimation("Sing Right");
                        break;
                    default:
                        BoyfriendPlayAnimation("Sing Left");
                        break;
                }

                _bfRandomDanceTimer = Random.Range(.5f, 3f);
            }
            if (_enemyRandomDanceTimer <= 0)
            {
                switch (Random.Range(0, 4))
                {
                    case 1:
                        EnemyPlayAnimation("Sing Left");
                        break;
                    case 2:
                        EnemyPlayAnimation("Sing Down");
                        break;
                    case 3:
                        EnemyPlayAnimation("Sing Up");
                        break;
                    case 4:
                        EnemyPlayAnimation("Sing Right");
                        break;
                    default:
                        EnemyPlayAnimation("Sing Left");
                        break;
                }

                _enemyRandomDanceTimer = Random.Range(.5f, 3f);
            }
        }

        for (int i = 0; i < _currentEnemyNoteTimers.Length; i++)
        {
            if (player2NotesAnimators[i].GetCurrentAnimatorStateInfo(0).IsName("Activated"))
            {
                _currentEnemyNoteTimers[i] -= Time.deltaTime;
                if (_currentEnemyNoteTimers[i] <= 0)
                {
                    AnimateNote(2, i, "Normal");
                }
            }

        }

        if (ratingLayerTimer > 0)
        {
            ratingLayerTimer -= Time.deltaTime;
            if (ratingLayerTimer < 0)
                _currentRatingLayer = 0;
        }
        
        if(Player.demoMode)
            for (int i = 0; i < _currentDemoNoteTimers.Length; i++)
            {
                if (player1NotesAnimators[i].GetCurrentAnimatorStateInfo(0).IsName("Activated"))
                {
                    _currentDemoNoteTimers[i] -= Time.deltaTime;
                    if (_currentDemoNoteTimers[i] <= 0)
                    {
                        AnimateNote(1, i, "Normal");
                    }
                }

            }
        

        if (!enemyAnimation.GetCurrentAnimatorStateInfo(0).IsName(enemyName + " Idle"))
        {

            _currentEnemyIdleTimer -= Time.deltaTime;
            if (_currentEnemyIdleTimer <= 0)
            {
                print("Attempting to play dad idle.");
                enemyAnimation.Play(enemyName + " Idle");
                _currentEnemyIdleTimer = enemyIdleTimer;
            }
        }

        if (!boyfriendAnimation.GetCurrentAnimatorStateInfo(0).IsName("BF Idle"))
        {

            _currentBoyfriendIdleTimer -= Time.deltaTime;
            if (_currentBoyfriendIdleTimer <= 0)
            {
                print("Attempting to play bf idle.");
                boyfriendAnimation.Play("BF Idle");
                _currentBoyfriendIdleTimer = boyfriendIdleTimer;
            }
        }

        
    }
}
