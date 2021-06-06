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
using UnityEngine.SceneManagement;
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
    public AudioClip secVocalClip;
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
    private float _ratingLayerDefaultTime = 2.2f;
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

    [Header("DATA")] public static SongFile songFile;
    public static bool freePlay;
    public static int week;
    public static List<SongFile> songFiles;
    public static int currentSongIndex = 0;
    public static int difficulty;
    public bool autoPlay;
    public bool debug;
    public SongFile debugFile;
    
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
    public SpriteRenderer[] player1SparkleRenderers;
    private List<Animator> player1SparkleAnimators = new List<Animator>();
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
    public SpriteRenderer enemySpriteRenderer;
    public float enemyIdleTimer = .3f;
    private float _currentEnemyIdleTimer;
    public float enemyNoteTimer = .25f;
    private readonly float[] _currentEnemyNoteTimers = {0,0,0,0};
    private readonly float[] _currentDemoNoteTimers = {0,0,0,0};

    [Header("Boyfriend")] public GameObject bfObj;
    public Animator boyfriendAnimation;
    public float boyfriendIdleTimer = .3f;
    public SpriteRenderer boyfriendSpriteRenderer;
    private float _currentBoyfriendIdleTimer;

    private FNFSong _song;

    public static Song instance;
    [Space] public GameObject girlfriend;
    

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

    [Header("Camera Beat")] public LTDescr zoomTween;
    public Camera beatCamera;
    private float _defaultZoom;
    public Camera gameCamera;
    public float defaultGameZoom;
    public float beatZoomTime;
    public bool cameraZooming = false;
    [Space] public RectTransform healthBarTransform;
    public bool healthZooming;
    public float healthZoomSpeed;

    [Space] public RectTransform player1Icon;
    public RectTransform player2Icon;
    public bool playerIconsZooming;
    public float playerZoomTime;

    [Header("Mobile Controls")] public GameObject leftTouch;
    public GameObject downTouch;
    public GameObject upTouch;
    public GameObject rightTouch;

    #endregion

    private void Start()
    {
        instance = this;

        _songsFolder = Application.dataPath + "/Songs";
        
        player1Notes.gameObject.SetActive(true);
        player2Notes.gameObject.SetActive(true);
        battleCanvas.enabled = true;
        healthBar.SetActive(true);


        musicSources[0].volume = PlayerPrefs.GetFloat("Music Volume", 1f);
        musicSources[1].volume = PlayerPrefs.GetFloat("Music Volume", 1f);
        
        vocalSource.volume = PlayerPrefs.GetFloat("Music Volume", 1f);

        oopsSource.volume = .60f * PlayerPrefs.GetFloat("Music Volume", 1f);

        if(!debug)
        {
            PlayFromFile(freePlay ? songFile : songFiles[currentSongIndex]);
        }
        else
        {
            difficulty = 3;
            PlayFromFile(debugFile);
        }

        Player.demoMode = autoPlay;

        _defaultZoom = beatCamera.orthographicSize;
        defaultGameZoom = gameCamera.orthographicSize;

        foreach (SpriteRenderer spriteRenderer in player1SparkleRenderers)
        {
            player1SparkleAnimators.Add(spriteRenderer.GetComponent<Animator>());
            spriteRenderer.enabled = false;
        }

    }


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

        beatsPerSecond = 60 / (float)_song.Bpm;

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
                nObj.layer = section.MustHitSection ? 1 : 2;
                
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

        for (int i = 0; i < 4; i++)
        {
            player1NotesObjects[i] = player1NotesObjects[i].OrderBy(s => s.strumTime).ToList();
            player2NotesObjects[i] = player2NotesObjects[i].OrderBy(s => s.strumTime).ToList();

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

        musicSources[1].loop = false;

        soundSource.clip = startSound;
        soundSource.Play();

        menuCanvas.enabled = false;
        battleCanvas.enabled = true;
        
        StartCoroutine(nameof(SongStart), startSound.length);
    }
    
    IEnumerator SongStart(float delay)
    {
        

        yield return new WaitForSeconds(delay);

        foreach (SpriteRenderer spriteRenderer in player1SparkleRenderers)
        {
            spriteRenderer.enabled = true;
        }
        
        beatStopwatch = new Stopwatch();
        beatStopwatch.Start();
        
        

        musicSources[0].clip = musicClip;
        musicSources[1].clip = secVocalClip;
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
        
        
        File.Delete(Application.dataPath + "/tmp/ok.json");
    }

    #endregion

    #region Game Options

    public void PlayFromFile(SongFile file)
    {
        SceneManager.LoadScene(file.backgroundScene, LoadSceneMode.Additive);

        if(!Directory.Exists(Application.dataPath + "/tmp"))
            Directory.CreateDirectory(Application.dataPath + "/tmp");
        
        StreamWriter testFile = File.CreateText(Application.dataPath + "/tmp/ok.json");
        switch (difficulty)
        {
            case 1:
                testFile.Write(file.songJsonEasy);
                break;
            case 2:
                testFile.Write(file.songJsonNormal);
                break;
            case 3:
                testFile.Write(file.songJsonHard);
                break;
        }

        testFile.Close();

        vocalClip = file.boyfriendClip;
        
        musicClip = file.instrumentalClip;

        secVocalClip = file.cyeClip;
        
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

    public void PlaySparkle(int note)
    {
        player1SparkleAnimators[note].Play("Hit " + Random.Range(1,3),0,0);
        enemyAnimation.speed = 0;
        
        player1SparkleAnimators[note].Play("Hit " + Random.Range(1,3));
        enemyAnimation.speed = 1;
    }

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
                player1NotesAnimators[type].Play(animName, 0, 0);
                player1NotesAnimators[type].speed = 0;
                
                player1NotesAnimators[type].Play(animName);
                player1NotesAnimators[type].speed = 1;

                if (animName == "Activated" & Player.demoMode)
                {
                    _currentDemoNoteTimers[type] = enemyNoteTimer;
                }

                break;
            case 2: //Opponent
                player2NotesAnimators[type].Play(animName, 0, 0);
                player2NotesAnimators[type].speed = 0;
                
                player2NotesAnimators[type].Play(animName);
                player2NotesAnimators[type].speed = 1;

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
            $"Score: {_currentScore} | Accuracy: {accuracyPercent}% | Combo: {_currentSickCombo} ({_highestSickCombo}) | Misses: {_missedHits}";
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

        tmpObj.SyncCamera();

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

                PlaySparkle(note);

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
            if (_songStarted)
            {
                if ((float)beatStopwatch.ElapsedMilliseconds / 1000 >= beatsPerSecond)
                {
                    beatStopwatch.Restart();
                    currentBeat++;
                    float offset;
                    offset = beat ? 0.5f : -0.5f;
                    beat = !beat;
                    if (currentBeat % 4 == 0)
                    {
                        if (!cameraZooming)
                        {
                            cameraZooming = true;
                            LeanTween.value(beatCamera.gameObject, _defaultZoom-.1f, _defaultZoom,
                                    beatZoomTime).setOnUpdate(f => { beatCamera.orthographicSize = f; })
                                .setOnComplete(() => { cameraZooming = false; });
                            
                            LeanTween.value(gameCamera.gameObject, defaultGameZoom-.1f, defaultGameZoom,
                                    beatZoomTime).setOnUpdate(f => { gameCamera.orthographicSize = f; })
                                .setOnComplete(() => { cameraZooming = false; });
                        }

                        if (!healthZooming)
                        {
                            healthZooming = true;
                            healthBarTransform.localScale = new Vector3(.68f, .68f, .68f);

                            LeanTween.value(healthBar, .68f, .65f, healthZoomSpeed).setOnUpdate(f =>
                            {
                                healthBarTransform.localScale = new Vector3(f, f, f);
                            }).setOnComplete(() =>
                            {
                                healthZooming = false;
                            });
                            
                        }
                    }

                    if(!playerIconsZooming)
                    {
                        playerIconsZooming = true;

                        player1Icon.localScale = new Vector3(-1.2f, 1.2f, 1.2f);
                        player2Icon.localScale = new Vector3(1.2f, 1.2f, 1.2f);

                        LeanTween.value(player1Icon.gameObject, 1.2f, 1f, playerZoomTime).setOnUpdate(f =>
                        {
                            player1Icon.localScale = new Vector3(-f, f, f);
                            player2Icon.localScale = new Vector3(f, f, f);

                        }).setOnComplete(() =>
                        {
                            playerIconsZooming = false;
                        });

                        
                    }
                    
                }
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

                if (freePlay)
                {
                    Intro.skipIntro = true;
                    SceneManager.LoadScene("Menu");
                }
                else
                {
                    currentSongIndex++;
                    
                    
                    
                    if (!(currentSongIndex > songFiles.Count - 1))
                    {
                        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
                    }
                    else
                    {
                        int unlockedWeek = week + 1;
                        
                        PlayerPrefs.SetString($"Week {unlockedWeek} Unlocked", "true");
                        PlayerPrefs.SetString($"Week {unlockedWeek} X Unlocked", "true");
                        PlayerPrefs.Save();
                        
                        currentSongIndex = 0;
                        SceneManager.LoadScene("Menu");

                    }
                }
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
