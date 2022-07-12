 using System;
 using System.Collections;
 using System.Collections.Generic;
 using System.Diagnostics;
 using System.IO;
 using System.Linq;
 using FridayNightFunkin;
 using Newtonsoft.Json;
 using QFSW.MOP2;
 using SimpleSpriteAnimator;
 using Slowsharp;
 using TMPro;
 using UnityEngine;
 using UnityEngine.SceneManagement;
 using UnityEngine.Serialization;
 using UnityEngine.UI;
 using Debug = UnityEngine.Debug;
 using Random = UnityEngine.Random;
 using System.Reflection;

 // ReSharper disable IdentifierTypo
// ReSharper disable PossibleNullReferenceException

public class Song : MonoBehaviour
{

    #region Variables

    public AudioSource soundSource;
    public AudioClip startSound;
    [Space] public AudioSource[] musicSources;
    public AudioSource vocalSource;
    public AudioSource oopsSource;
    public AudioClip musicClip;
    public AudioClip vocalClip;
    public AudioClip menuClip;
    public AudioClip[] noteMissClip;
    public bool hasVoiceLoaded;    
    public HybInstance modInstance;


    [Space] public bool songSetupDone;
        
    [Space] public GameObject[] defaultSceneObjects;

    [Space] public GameObject ratingObject;
    public GameObject liteRatingObjectP1;
    public GameObject liteRatingObjectP2;
    public Sprite sickSprite;
    public Sprite goodSprite;
    public Sprite badSprite;
    public Sprite shitSprite;
    [Header("Player 1 Stats")]
    public GameObject playerOneScoringObject;
    public TMP_Text playerOneScoringText;
    public Image playerOneCornerImage;
    public TMP_Text playerOneComboText;
    public LTDescr playerOneComboTween;
    public LTDescr playerOneScoreTween;
    public float playerOneScoreLerpSpeed;
    [Header("Player 2 Stats")]
    public GameObject playerTwoScoringObject;
    public TMP_Text playerTwoScoringText;
    public Image playerTwoCornerImage;
    public TMP_Text playerTwoComboText;
    public LTDescr playerTwoComboTween;
    public LTDescr playerTwoScoreTween;
    public float playerTwoScoreLerpSpeed;
    [Space]
    public float ratingLayerTimer;
    private float _ratingLayerDefaultTime = 2.2f;
    private int _currentRatingLayer;
    public PlayerStat playerOneStats;
    public PlayerStat playerTwoStats;

    public Stopwatch stopwatch;
    public Stopwatch beatStopwatch;
    [Space] public Camera mainCamera;
    public Camera uiCamera;
    public float beatZoomTime;
    private float _defaultZoom;
    public float defaultGameZoom;

    [Space, TextArea(2, 12)] public string jsonDir;
    public float notesOffset;
    public float noteDelay;
    [Range(-1f, 1f)] public float speedDifference;

    [Space] public Canvas battleCanvas;
    public Canvas menuCanvas;
    public GameObject generatingSongMsg;
    public GameObject songListScreen;
    
    [Space] public GameObject menuScreen;

    [Header("Death Mechanic")] public Camera deadCamera;
    public GameObject deadBoyfriend;
    public Animator deadBoyfriendAnimator;
    public AudioClip deadNoise;
    public AudioClip deadTheme;
    public AudioClip deadConfirm;
    public Image deathBlackout;
    public bool isDead;
    public bool respawning;

    

   
    
    private float _currentInterval;
    
    [Space] public Transform player1Notes;
    public SpriteRenderer[] player1NoteSprites;
    public List<List<NoteObject>> player1NotesObjects;
    public Animator[] player1NotesAnimators;
    public Transform player1Left;
    public Transform player1Down;
    public Transform player1Up;
    public Transform player1Right;
    [Space] public Transform player2Notes;
    public SpriteRenderer[] player2NoteSprites;
    public List<List<NoteObject>> player2NotesObjects;
    public Animator[] player2NotesAnimators;
    public Transform player2Left;
    public Transform player2Down;
    public Transform player2Up;
    public Transform player2Right;
    private List<NoteBehaviour> _noteBehaviours = new List<NoteBehaviour>();
    
    [Header("Prefabs")] public GameObject leftArrow;
    public GameObject downArrow;
    public GameObject upArrow;
    public GameObject rightArrow;
    [Space] public GameObject holdNote;
    public Sprite holdNoteEnd;
    public Sprite holdNoteSprite;
    [Header("Object Pools")] 
    public ObjectPool leftNotesPool;
    public ObjectPool rightNotesPool;
    public ObjectPool downNotesPool;
    public ObjectPool upNotesPool;
    public ObjectPool holdNotesPool;
    
    [Header("Characters")]
    public string[] characterNames;
    public Character[] characters;
    public Dictionary<string, Character> charactersDictionary;
    public Character defaultEnemy;
    [Space] public GameObject girlfriendObject;
    public SpriteAnimator girlfriendAnimator;
    public bool altDance;

    [FormerlySerializedAs("enemyObj")] [Header("Enemy")] public GameObject opponentObject;
    public Character enemy;
    public string enemyName;
    [FormerlySerializedAs("enemyAnimator")] public SpriteAnimator opponentAnimator;
    public float enemyIdleTimer = .3f;
    private float _currentEnemyIdleTimer;
    public float enemyNoteTimer = .25f;
    private Vector3 _enemyDefaultPos;
    private readonly float[] _currentEnemyNoteTimers = {0, 0, 0, 0};
    private readonly float[] _currentDemoNoteTimers = {0, 0, 0, 0};
    private LTDescr _enemyFloat;



    [FormerlySerializedAs("bfObj")] [Header("Boyfriend")] public GameObject boyfriendObject;
    public SpriteAnimator boyfriendAnimator;
    public float boyfriendIdleTimer = .3f;
    public Sprite boyfriendPortraitNormal;
    public Sprite boyfriendPortraitDead;
    private float _currentBoyfriendIdleTimer;

    private FNFSong _song;

    public static Song instance;

    [Header("Scenes")] public Dictionary<string, SceneData> scenes;

    [Header("Health")] public float health = 100;

    private const float MAXHealth = 200;
    public float healthLerpSpeed;
    public GameObject healthBar;
    public RectTransform boyfriendHealthIconRect;
    public Image boyfriendHealthIcon;
    public Image boyfriendHealthBar;
    public RectTransform enemyHealthIconRect;
    public Image enemyHealthIcon;
    public Image enemyHealthBar;

    [Space] public GameObject songDurationObject;
    public TMP_Text songDurationText;
    public Image songDurationBar;

    [Space] public GameObject startSongTooltip;
    
    [Space] public NoteObject lastNote;
    public float stepCrochet;
    public float beatsPerSecond;
    public int currentBeat;
    public bool beat;

    private float _bfRandomDanceTimer;
    private float _enemyRandomDanceTimer;

    private bool _portraitsZooming;
    private bool _cameraZooming;

    public string songsFolder;
    public string selectedSongDir;

    public static SongMetaV2 currentSongMeta;
    public static string difficulty;
    public static int modeOfPlay;

    [HideInInspector] public SongListObject selectedSong;


    public bool songStarted;

    [Header("Subtitles")]

    public SubtitleDisplayer subtitleDisplayer;
    public bool usingSubtitles;

    [Header("Custom Notes")]
    public List<Assembly> assembliesCustomNotes = new List<Assembly>();
    public List<object> assemblyObjects = new List<object>();
    public List<MethodInfo> methodsGetter = new List<MethodInfo>();
    public MethodInfo methodNoteOnClick;
    public MethodInfo methodNoteOnMiss;
    public Sprite test;

    public Dictionary<string, Sprite[]> noteSprites = new Dictionary<string, Sprite[]>();
    public Dictionary<int, CustomNote> customNotes = new Dictionary<int, CustomNote>();
    #endregion

    private void Start()
    {
        /*
         * To allow other scripts to access the Song script without needing the
         * script to be found or referenced, we set a static variable within the
         * Song script itself that can be used at anytime to access the singular used Song
         * script instance.
         */
        instance = this;

        /*
         * Sets the "songs folder" to %localappdata%/Rei/FridayNight/Songs.
         * This is used to find and load any found songs.
         *
         * This can only be changed within the editor itself and not in-game,
         * though it would not be hard to make that possible.
         */
        songsFolder = Application.persistentDataPath + "/Songs";

        /*
         * Makes sure the UI for the song gameplay is disabled upon load.
         *
         * This disables the notes for both players and the UI for the gameplay.
         */
        player1Notes.gameObject.SetActive(false);
        player2Notes.gameObject.SetActive(false);
        playerOneScoringText.enabled = false;
        playerTwoScoringText.enabled = false;
        battleCanvas.enabled = true;
        healthBar.SetActive(false);
        songDurationObject.SetActive(false);

        mainCamera = Camera.main;
        
        
        /*
         * Grabs the subtitle displayer.
         */
        subtitleDisplayer = GetComponent<SubtitleDisplayer>();

        if (OptionsV2.DesperateMode)
        {
            boyfriendObject.SetActive(false);
            opponentObject.SetActive(false);

            boyfriendHealthIcon.enabled = false;
            enemyHealthIcon.enabled = false;
        }

        if (OptionsV2.LiteMode)
        {
            girlfriendObject.SetActive(false);
        }

        
        /*
         * In case we want to reset the enemy position later on,
         * we will save their current position.
         */
        _enemyDefaultPos = opponentObject.transform.position;

        /*
         * We'll make a dictionary of characters via the two arrays of character names
         * and character classes.
         *
         * This is later on used to load a character based on their name for "Player2"
         * in an FNF chart.
         */
        charactersDictionary = new Dictionary<string, Character>();
        for (int i = 0; i < characters.Length; i++)
        {
            charactersDictionary.Add(characterNames[i], characters[i]);
        }


        _defaultZoom = uiCamera.orthographicSize;

        bool doAuto = false;
        
        switch (modeOfPlay)
        {
            //Boyfriend
            case 1:
                Player.playAsEnemy = false;
                Player.twoPlayers = false;
                break;
            //Opponent
            case 2:
                Player.playAsEnemy = true;
                Player.twoPlayers = false;
                break;
            //Local Multiplayer
            case 3:
                Player.playAsEnemy = false;
                Player.twoPlayers = true;
                break;
            //Auto
            case 4:
                doAuto = true;
                Player.playAsEnemy = false;
                Player.twoPlayers = false;
                break;
        }
        
        PlaySong(doAuto, difficulty,currentSongMeta.songPath,currentSongMeta.haveCustomNotes,currentSongMeta.customNotes);
    }

    #region Song Gameplay

    public void PlaySong(bool auto, string difficulty = "", string directory = "", bool haveCustomNotes = false, List<string> customNotesDllNames = null)
    {
        /*
         * If the player wants the song to play itself,
         * we'll set the Player script to be on demo mode.
         */
        Player.demoMode = auto;
        
        /*
         * We'll reset any stats then update the UI based on it.
         */
        _currentRatingLayer = 0;
        playerOneStats = new PlayerStat();
        playerTwoStats = new PlayerStat();
        currentBeat = 0;

        UpdateScoringInfo();

        /*
         * Grabs the current song's directory and saves it to a variable.
         *
         * We'll then use it to grab the chart file.
         */
        selectedSongDir = string.IsNullOrWhiteSpace(directory) ? selectedSong.directory : directory;
        
        jsonDir = selectedSongDir + $"/Chart-{difficulty.ToLower()}.json";

        if (haveCustomNotes)
        {
            foreach(string namePath in customNotesDllNames)
            {
                assemblyObjects.Clear();
                noteSprites.Clear();
                Assembly assembly = Assembly.Load(File.ReadAllBytes(Path.Combine(Application.persistentDataPath, "Notes", namePath) + ".dll"));  //Assembly.Load(File.ReadAllBytes(Path.Combine(Application.persistentDataPath, "Notes", nameDll)));
                assembliesCustomNotes.Add(assembly);

                string pathAssets = Path.Combine(Application.persistentDataPath, "Notes", namePath);

                assemblyObjects.Add(assembly.CreateInstance("LibraryGetter", false, BindingFlags.ExactBinding, null, null, null, null));
                assemblyObjects.Add(assembly.CreateInstance("Note", false, BindingFlags.ExactBinding, null, null, null, null));

                MethodInfo getInfos = assembly.GetType("LibraryGetter").GetMethod("GetMeta");
                MethodInfo getImage = assembly.GetType("LibraryGetter").GetMethod("GetSprite");
                methodNoteOnClick = assembly.GetType("Note").GetMethod("OnClick");
                methodNoteOnMiss = assembly.GetType("Note").GetMethod("OnMiss");

                CustomNote cn = JsonConvert.DeserializeObject<CustomNote>((string)getInfos.Invoke(assemblyObjects[0], null));

                noteSprites.Add(namePath, new Sprite[] {
                    (Sprite)getImage.Invoke(assemblyObjects[0], new object[] {"Note.png"}),
                    (Sprite)getImage.Invoke(assemblyObjects[0], new object[] {"Note-Hold.png"}),
                    (Sprite)getImage.Invoke(assemblyObjects[0], new object[] {"Note-HoldEnd.png"})
                });

                cn.methodTriggerOnClick = methodNoteOnClick;
                cn.methodTriggerOnMiss = methodNoteOnMiss;
                cn.assemblyObj = assemblyObjects[1];
                cn.sprites = noteSprites[namePath].ToList();

                customNotes.Add(cn.index, cn);
            }
        }

        /*
         * We'll enable the gameplay UI.
         *
         * We'll also hide the Menu UI but also reset it
         * so we can instantly go back to the menu
         */
        battleCanvas.enabled = true;
        generatingSongMsg.SetActive(true);

        menuCanvas.enabled = false;
        songListScreen.SetActive(false);
        
        /*
         * We'll check and load subtitltes.
         */
        if(File.Exists(selectedSongDir+"/Subtitles.txt"))
        {
            TextAsset textAsset = new TextAsset(File.ReadAllText(selectedSongDir+"/Subtitles.txt"));
            subtitleDisplayer.Subtitle = textAsset;
            usingSubtitles = true;
        }

        if (File.Exists(selectedSongDir + "/ModScript.csx"))
        {
            modInstance = CScript.CreateRunner(File.ReadAllText(selectedSongDir + "/ModScript.csx")).Instantiate("ModScript");
            modInstance?.Invoke("OnSongStarting");

            if (File.Exists(selectedSongDir + "/Events.json"))
            {
                string eventsData = File.ReadAllText(selectedSong + "/Events.json");
                SongEvents events = JsonConvert.DeserializeObject<SongEvents>(eventsData);
                SongEventsHandler.instance.songEvents = events.events;
            }

        }

        /*
         * Now we start the song setup.
         *
         * This is a Coroutine so we can make use
         * of the functions to pause it for certain time.
         */
        StartCoroutine(nameof(SetupSong));

    }

    IEnumerator SetupSong()
    {
        /*
         * First, we have to load the instrumentals from the
         * local file. We use the, although deprecated, WWW function for this.
         *
         * In case of an error, we just stop and output it.
         * Otherwise, we set the clip as the instrumental.
         *
         * Then we wait until it is fully loaded, WaitForSeconds allows us to pause
         * the coroutine for .1 seconds then check if the clip is loaded again.
         * If not, keep waiting until it is loaded.
         *
         * Once the instrumentals is loaded, we repeat the exact same thing with
         * the voices. Then, we generate the rest of the song from the chart file.
         */
        WWW www1 = new WWW(selectedSongDir + "/Inst.ogg")
        {
            threadPriority = ThreadPriority.High
        };
        if (www1.error != null)
        {
            Debug.LogError(www1.error);
        }
        else
        {
            musicClip = www1.GetAudioClip();
            while (musicClip.loadState != AudioDataLoadState.Loaded)
                yield return new WaitForSeconds(0.1f);
            if(File.Exists(selectedSongDir + "/Voices.ogg"))
            {
            
                WWW www2 = new WWW(selectedSongDir + "/Voices.ogg");
                if (www2.error != null)
                {
                    Debug.LogError(www2.error);
                }
                else
                {
                    vocalClip = www2.GetAudioClip();
                    while (vocalClip.loadState != AudioDataLoadState.Loaded)
                        yield return new WaitForSeconds(0.1f);
                    hasVoiceLoaded = true;
                    print("Sounds loaded, generating song.");
                    GenerateSong();
                }
            }
            else
            {
                hasVoiceLoaded = false;
                print("Sounds loaded, generating song.");
                GenerateSong();
            }
        }
    }

    
    
    
    
    public void GenerateSong()
    {

        for (int i = 0; i < OptionsV2.instance.colorPickers.Length; i++)
        {
            player1NoteSprites[i].color = OptionsV2.instance.colorPickers[i].color;
            player2NoteSprites[i].color = OptionsV2.instance.colorPickers[i].color;
        }

        /*
         * Set the health the half of the max so it's smack dead in the
         * middle.
         */

        health = MAXHealth / 2;

        /*
         * Special thanks to KadeDev for creating the .NET FNF Song parsing library.
         *
         * With it, we can load the song as a whole class full of chart information
         * via the chart file.
         */
        _song = new FNFSong(jsonDir);

        /*
         * We grab the BPM to calculate the BPS and the Step Crochet.
         */
        beatsPerSecond = 60 / (float) _song.Bpm;

        stepCrochet = (60 / (float) _song.Bpm * 1000 / 4);

        /*
         * Just in case, we'll force player 1 and player 2 notes to be wiped to a
         * clean slate.
         */

        if (player1NotesObjects != null)
        {
            foreach (List<NoteObject> list in player1NotesObjects)
            {

                list.Clear();
            }
            


            player1NotesObjects.Clear();
        }

        if (player2NotesObjects != null)
        {
            foreach (List<NoteObject> list in player2NotesObjects)
            {

                list.Clear();
            }

            player2NotesObjects.Clear();
        }

        leftNotesPool.ReleaseAll();
        downNotesPool.ReleaseAll();
        upNotesPool.ReleaseAll();
        rightNotesPool.ReleaseAll();
        holdNotesPool.ReleaseAll();
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

        /*
         * If somehow we fucked up, we stop the song generation process entirely.
         *
         * If we didn't, we keep going.
         */

        if (_song == null)
        {
            Debug.LogError("Error with song data");
            return;
        }
        
        /*
         * Shift the UI for downscroll or not
         */
        if (OptionsV2.Downscroll)
        {
            healthBar.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 140f);

            uiCamera.transform.position = new Vector3(0, 7,-10);

            playerOneScoringObject.GetComponent<RectTransform>().anchoredPosition = new Vector3(-160, 315, 0);
            playerTwoScoringObject.GetComponent<RectTransform>().anchoredPosition = new Vector3(160, 315, 0);
        }
        else
        {
            healthBar.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -140f);

            uiCamera.transform.position = new Vector3(0, 2,-10);

            playerOneScoringObject.GetComponent<RectTransform>().anchoredPosition = new Vector3(-160, 45, 0);
            playerTwoScoringObject.GetComponent<RectTransform>().anchoredPosition = new Vector3(160, 45, 0);
        }

        /*
         * Shift the UI or not for Middlescroll
         */
        
        if(OptionsV2.Middlescroll)
        {
            if (!Player.twoPlayers)
            {
                if (Player.playAsEnemy)
                {
                    foreach (SpriteRenderer sprite in player1NoteSprites)
                    {
                        sprite.enabled = false;
                    }
                    
                    
                    foreach (SpriteRenderer sprite in player2NoteSprites)
                    {
                        sprite.enabled = true;
                    }

                    player2Notes.transform.position = new Vector3(0f, 4.45f, 15);
                }
                else
                {
                    foreach (SpriteRenderer sprite in player1NoteSprites)
                    {
                        sprite.enabled = true;
                    }
                    foreach (SpriteRenderer sprite in player2NoteSprites)
                    {
                        sprite.enabled = false;
                    }

                    player1Notes.transform.position = new Vector3(0f, 4.45f, 15);
                }
            }
        }
        else
        {
            foreach (SpriteRenderer sprite in player2NoteSprites)
            {
                sprite.enabled = true;
            }
            
            foreach (SpriteRenderer sprite in player1NoteSprites)
            {
                sprite.enabled = true;
            }
            
            player2Notes.transform.position = new Vector3(-3.6f, 4.45f, 15);
            player1Notes.transform.position = new Vector3(3.6f, 4.45f, 15);

        }

        /*
         * "foreach" allows us to go through each and every single section in the
         * chart. Then a nested "foreach" allows to go through every notes in that
         * specific section.
         */
        foreach ( FNFSong.FNFSection section in _song.Sections ) {
            foreach ( var noteData in section.Notes ) {
                _noteBehaviours.Add( new NoteBehaviour( section, noteData ) );
            }
        }
        /*
         * Charts tend to not have organized notes, so we have to sort notes
         * for the game so inputs do not get screwed up.
         *
         * The notes for each player are sorted in ascending order based on strum time.
         */

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

        /*
         * We now enable the notes UI for both players and disable the
         * generating song message UI.
         */
        player1Notes.gameObject.SetActive(true);
        player2Notes.gameObject.SetActive(true);

        generatingSongMsg.SetActive(false);

        healthBar.SetActive(true);
        playerOneScoringText.enabled = true;
        playerTwoScoringText.enabled = true;

        /*
         * Tells the entire script and other attached scripts that the song
         * started to play but has not fully started.
         */
        songSetupDone = true;
        songStarted = false;
        
        /*
         * Reset the stopwatch entirely.
         */
        stopwatch = new Stopwatch();

        /*
         * Stops any current music playing and sets it to not loop.
         */
        musicSources[0].loop = false;
        musicSources[0].volume = OptionsV2.instVolume;
        musicSources[0].Stop();

       /*
        * Initialize combo texts.
        */
       playerOneComboText.alpha = 0;
       playerTwoComboText.alpha = 0;

        /*
         * Disable the entire Menu UI and enable the entire Gameplay UI.
         */
        menuCanvas.enabled = false;
        battleCanvas.enabled = true;
        
        if (!OptionsV2.SongDuration)
            songDurationObject.SetActive(false);
        else
        {
            songDurationObject.SetActive(true);
            
            if (OptionsV2.Downscroll)
            {
                RectTransform rect = songDurationObject.GetComponent<RectTransform>();
                
                rect.anchoredPosition = new Vector3(0,-165,0);
            }
        }
        
        
        /*
         * If the player 2 in the chart exists in this engine,
         * we'll change player 2 to the correct character.
         *
         * If not, keep any existing character we selected.
         */
        if(!OptionsV2.DesperateMode)
        {
            print("Checking for and applying " + _song.Player2 + ". Result is " +
                  Cache.cachedOpponents.ContainsKey(_song.Player2));
            if (Cache.cachedOpponents.ContainsKey(_song.Player2))
            {
                enemy = Cache.cachedOpponents[_song.Player2];
                opponentAnimator.spriteAnimations = enemy.animations;

                /*
                 * Yes, opponents can float if enabled in their
                 * configuration file.
                 */
                if (enemy.doesFloat)
                {
                    _enemyFloat = LeanTween.moveLocalY(opponentObject, enemy.floatToOffset, enemy.floatSpeed)
                        .setEaseInOutExpo()
                        .setLoopPingPong();
                }
                else
                {
                    /*
                     * In case any previous enemy floated before and this new one does not,
                     * we reset their position and cancel the floating tween.
                     */
                    if (_enemyFloat != null && LeanTween.isTweening(_enemyFloat.id))
                    {
                        LeanTween.cancel(_enemyFloat.id);
                        opponentObject.transform.position = _enemyDefaultPos;
                    }
                }

                enemyHealthIcon.sprite = enemy.portrait;
                enemyHealthIconRect.sizeDelta = enemy.portraitSize;
                enemyHealthBar.color = enemy.healthColor;
                playerTwoCornerImage.color = enemy.healthColor;

                Vector3 offset = enemy.cameraOffset;
                offset.z = -10;
                enemy.cameraOffset = offset;

                EnemyPlayAnimation("Idle");

                opponentAnimator.transform.localScale = new Vector2(enemy.scale, enemy.scale);

                CameraMovement.instance.playerTwoOffset = enemy.cameraOffset;
            }
            else
            {
                string charDir = selectedSongDir + "/Opponent";
                Dictionary<string, List<Sprite>> CharacterAnimations = new Dictionary<string, List<Sprite>>();

                if (Directory.Exists(charDir))
                {
                    opponentAnimator.spriteAnimations = new List<SpriteAnimation>();

                    // BEGIN ANIMATIONS IMPORT

                    var charMetaPath = charDir + "/char-meta.json";
                    var currentMeta = File.Exists(charMetaPath)
                        ? JsonConvert.DeserializeObject<CharacterMeta>(File.ReadAllText(charMetaPath))
                        : null;

                    foreach (string directoryPath in Directory.GetDirectories(charDir))
                    {
                        DirectoryInfo directoryInfo = new DirectoryInfo(directoryPath);

                        var files = directoryInfo.GetFiles("*.png");

                        List<Sprite> sprites = new List<Sprite>();

                        foreach (var file in files)
                        {
                            byte[] imageData = File.ReadAllBytes(file.ToString());

                            Texture2D imageTexture = new Texture2D(2, 2);
                            imageTexture.LoadImage(imageData);

                            var sprite = Sprite.Create(imageTexture,
                                new Rect(0, 0, imageTexture.width, imageTexture.height), new Vector2(0.5f, 0.0f), 100);
                            sprites.Add(sprite);

                        }

                        CharacterAnimations.Add(directoryInfo.Name, sprites);
                    }

                    foreach (string animationName in CharacterAnimations.Keys)
                    {
                        List<Vector2> offsets = new List<Vector2>();
                        SpriteAnimation newAnimation = ScriptableObject.CreateInstance<SpriteAnimation>();
                        List<SpriteAnimationFrame> frames = new List<SpriteAnimationFrame>();
                        for (var index = 0; index < CharacterAnimations[animationName].Count; index++)
                        {
                            Sprite sprite = CharacterAnimations[animationName][index];
                            Vector2 animationOffset = Vector2.zero;
                            if (currentMeta != null)
                            {
                                if (currentMeta.Offsets.ContainsKey(animationName))
                                {
                                    animationOffset = currentMeta.Offsets[animationName][index];
                                }
                            }
                            else
                            {
                                offsets.Add(animationOffset);
                            }

                            SpriteAnimationFrame newFrame = new SpriteAnimationFrame
                            {
                                Sprite = sprite,
                                Offset = animationOffset
                            };

                            frames.Add(newFrame);
                        }

                        newAnimation.Frames = frames;
                        newAnimation.Name = animationName;
                        newAnimation.FPS = 24;
                        newAnimation.SpriteAnimationType = SpriteAnimationType.PlayOnce;

                        opponentAnimator.spriteAnimations.Add(newAnimation);

                    }

                    Character newCharacter = ScriptableObject.CreateInstance<Character>();
                    newCharacter = currentMeta.Character;
                    if (File.Exists(charDir + "/Portrait.png"))
                    {
                        byte[] portraitFile = File.ReadAllBytes(charDir + "/Portrait.png");
                        Texture2D newTexture = new Texture2D(5, 5);
                        newTexture.LoadImage(portraitFile);
                        newCharacter.portrait = Sprite.Create(newTexture,
                            new Rect(0, 0, newTexture.width, newTexture.height),
                            Vector2.zero);
                    }
                    else
                    {
                        newCharacter.portrait = defaultEnemy.portrait;
                    }

                    if (File.Exists(charDir + "/Dead Portrait.png"))
                    {
                        byte[] portraitFile = File.ReadAllBytes(charDir + "/Dead Portrait.png");
                        Texture2D newTexture = new Texture2D(5, 5);
                        newTexture.LoadImage(portraitFile);
                        newCharacter.portraitDead = Sprite.Create(newTexture,
                            new Rect(0, 0, newTexture.width, newTexture.height),
                            Vector2.zero);
                    }
                    else
                    {
                        newCharacter.portraitDead = defaultEnemy.portraitDead;
                    }

                    enemy = newCharacter;


                    opponentAnimator.transform.localScale = new Vector2(newCharacter.scale, newCharacter.scale);

                    enemyHealthIcon.sprite = enemy.portrait;
                    enemyHealthIconRect.sizeDelta = enemy.portraitSize;
                    enemyHealthBar.color = enemy.healthColor;
                    playerTwoCornerImage.color = enemy.healthColor;
                    

                    Vector3 offset = enemy.cameraOffset;
                    offset.z = -10;
                    enemy.cameraOffset = offset;

                    EnemyPlayAnimation("Idle");

                    CameraMovement.instance.playerTwoOffset = enemy.cameraOffset;
                    newCharacter.animations = opponentAnimator.spriteAnimations;
                    Cache.cachedOpponents.Add(_song.Player2, newCharacter);

                    /*
                    if (charactersDictionary.ContainsKey(currentMeta.Character.name))
                    {
                        enemy = charactersDictionary[currentMeta.Character.name];
                        enemyAnimator.spriteAnimations = enemy.animations;
                        
                        enemy.doesFloat)
                        {
                            _enemyFloat = LeanTween.moveLocalY(enemyObj, enemy.floatToOffset, enemy.floatSpeed).setEaseInOutExpo()
                                .setLoopPingPong();
                        }
                        else
                        {
                             
                            if (_enemyFloat != null && LeanTween.isTweening(_enemyFloat.id))
                            {
                                LeanTween.cancel(_enemyFloat.id);
                                enemyObj.transform.position = _enemyDefaultPos;
                            }
                        }
    
                        enemyHealthIcon.sprite = enemy.portrait;
                        enemyHealthIconRect.sizeDelta = enemy.portraitSize;
    
                        CameraMovement.instance.playerTwoOffset = enemy.cameraOffset;
                    } 
                    else
                    {
    
                        
                    }
                    */
                }
            }
            string sceneDir = selectedSongDir + "/Scene";
            if (Directory.Exists(sceneDir))
            {
                SceneData data = JsonConvert.DeserializeObject<SceneData>(File.ReadAllText(sceneDir + "/scene.json"));

                if (data != null)
                {
                    if(!Cache.cachedScenes.ContainsKey(data.sceneName))
                    {
                        string imagesDirectory = sceneDir + "/images";
                        if (Directory.Exists(imagesDirectory))
                        {
                            Dictionary<SceneObject, Sprite> sprites = new Dictionary<SceneObject, Sprite>();
                            foreach (SceneObject sceneObject in data.objects)
                            {
                                string path = imagesDirectory + "/" + sceneObject.fileName;
                                if (File.Exists(path))
                                {
                                    byte[] imageData = File.ReadAllBytes(path);


                                    Texture2D imageTexture = new Texture2D(2, 2);
                                    imageTexture.LoadImage(imageData);

                                    GameObject newImage = new GameObject();
                                    SpriteRenderer renderer = newImage.AddComponent<SpriteRenderer>();
                                    Sprite newSprite = Sprite.Create(imageTexture,
                                        new Rect(0, 0, imageTexture.width, imageTexture.height), Vector2.zero, 100);
                                    renderer.sprite = newSprite;
                                    renderer.sortingOrder = sceneObject.layer;
                                    newImage.name = Path.GetFileName(path);

                                    newImage.transform.position = sceneObject.position;
                                    newImage.transform.localScale = sceneObject.size;
                                    newImage.transform.rotation = sceneObject.rotation;

                                    sprites.Add(sceneObject, newSprite);
                                }
                            }

                            Cache.cachedScenes.Add(data.sceneName, sprites);
                        }
                    }
                    else
                    {
                        Dictionary<SceneObject, Sprite> sceneObjectsCache = Cache.cachedScenes[data.sceneName];

                        foreach (SceneObject sceneObject in sceneObjectsCache.Keys)
                        {
                            GameObject newImage = new GameObject();
                            SpriteRenderer renderer = newImage.AddComponent<SpriteRenderer>();
                            renderer.sprite = sceneObjectsCache[sceneObject];
                            renderer.sortingOrder = sceneObject.layer;
                            newImage.name = sceneObject.fileName;

                            newImage.transform.position = sceneObject.position;
                            newImage.transform.localScale = sceneObject.size;
                            newImage.transform.rotation = sceneObject.rotation;
                        }
                    }

                    foreach (GameObject sceneObject in defaultSceneObjects) Destroy(sceneObject);
                }
            }
        }

        

        if (isDead)
        {
            isDead = false;
            respawning = false;

            deadCamera.enabled = false;

            
        }
        if(OptionsV2.SongDuration)
        {
            float time = musicClip.length - musicSources[0].time;

            int seconds = (int) (time % 60); // return the remainder of the seconds divide by 60 as an int
            time /= 60; // divide current time y 60 to get minutes
            int minutes = (int) (time % 60); //return the remainder of the minutes divide by 60 as an int

            songDurationText.text = minutes + ":" + seconds.ToString("00");

            songDurationBar.fillAmount = 0;
        }

        mainCamera.enabled = true;
        uiCamera.enabled = true;
        /*
         * Now we can fully start the song in a coroutine.
         */
        StartCoroutine(nameof(SongStart), startSound.length);
    }

    
    
    IEnumerator SongStart(float delay)
    {
        /*
         * If we are in demo mode, delete any temp charts.
         */
        if (Player.demoMode)
        {
            if(File.Exists(Application.persistentDataPath + "/tmp/ok.json"))
                File.Delete(Application.persistentDataPath + "/tmp/ok.json");
            if(Directory.Exists(Application.persistentDataPath + "/tmp"))
                Directory.Delete(Application.persistentDataPath + "/tmp");
        }

        startSongTooltip.SetActive(true);
        startSongTooltip.GetComponentInChildren<TMP_Text>().text = $"Press {Player.keybinds.startSongKeyCode} to start the song.";

        LoadingTransition.instance.Hide();
        
        yield return new WaitUntil(() => Input.GetKeyDown(Player.keybinds.startSongKeyCode));
        startSongTooltip.SetActive(false);
        /*
        * Start the countdown audio.
        *
        * Unlike FNF, this does not dynamically change based on BPM.
        */
        soundSource.clip = startSound;
        soundSource.Play();
        
        /*
         * Wait for the countdown to finish.
         */
        yield return new WaitForSeconds(delay);

        if(!OptionsV2.LiteMode)
            mainCamera.orthographicSize = 4;
        
        /*
         * Start the beat stopwatch.
         *
         * This is used to precisely calculate when a beat happens based
         * on the BPM or BPS.
         */
        beatStopwatch = new Stopwatch();
        beatStopwatch.Start();


        /*
         * Sets the voices and music audio sources clips to what
         * they should have.
         */
        musicSources[0].clip = musicClip;
        if(hasVoiceLoaded)
            vocalSource.clip = vocalClip;

        /*
         * In case we have more than one audio source,
         * let's tell them all to play.
         */
        foreach (AudioSource source in musicSources)
        {
            source.Play();
        }


        /*
         * Plays the vocal audio source then tells this script and other
         * attached scripts that the song fully started.
         */
        if(hasVoiceLoaded)
            vocalSource.Play();

        songStarted = true;
        
        modInstance?.Invoke("OnSongStarted");


        /*
         * Start subtitles.
         */
        if(usingSubtitles)
        {
            subtitleDisplayer.paused = false;
            subtitleDisplayer.StartSubtitles();
        }
        /*
         * Restart the stopwatch for the song itself.
         */
        stopwatch.Restart();
    }
    
    public void GenNote( FNFSong.FNFSection section, List<decimal> note ) {
        /*
                 * The .NET FNF Chart parsing library already has something specific
                 * to tell us if the note is a must hit.
                 *
                 * But previously I already kind of reverse engineered the FNF chart
                 * parsing process so I used the "ConvertToNote" function in the .NET
                 * library to grab "note data".
                 */
        GameObject newNoteObj;
        List<decimal> data = note;

        /*
         * It sets the "must hit note" boolean depending if the note
         * is in a section focusing on the boyfriend or not, and
         * if the note is for the other section.
         */
        bool mustHitNote = section.MustHitSection;
        if ( data[ 1 ] > 3 )
            mustHitNote = !section.MustHitSection;
        int noteType = Convert.ToInt32( data[ 1 ] % 4 );
        print(data[3]);

        /*
         * We make a spawn pos variable to later set the spawn
         * point of this note.
         */
        Vector3 spawnPos;

        /*
         * We get the length of this note's hold length.
         */
        float susLength = (float)data[ 2 ];

        /*
        if (susLength > 0)
        {
            isSusNote = true;

        }
        */

        /*
         * Then we adjust it to fit the step crochet to get the TRUE
         * hold length.
         */
        susLength /= stepCrochet;

        /*
         * It checks the type of note this is and spawns in a note gameobject
         * tailored for it then sets the spawn point for it depending on if it's
         * a note belonging to player 1 or player 2.
         *
         * If somehow this is the wrong data type, it fails and stops the song generation.
         */
        switch ( noteType ) {
            case 0: //Left
                newNoteObj = leftNotesPool.GetObject();
                spawnPos = mustHitNote ? player1Left.position : player2Left.position;
                break;
            case 1: //Down
                newNoteObj = downNotesPool.GetObject();
                spawnPos = mustHitNote ? player1Down.position : player2Down.position;
                break;
            case 2: //Up
                newNoteObj = upNotesPool.GetObject();
                spawnPos = mustHitNote ? player1Up.position : player2Up.position;
                break;
            case 3: //Right
                newNoteObj = rightNotesPool.GetObject();
                spawnPos = mustHitNote ? player1Right.position : player2Right.position;
                break;
            default:
                Debug.LogError( "Invalid note data." );
                return;
        }

        /*
         * We then move the note to a specific position in the game world.
         */
        spawnPos += Vector3.down *
                    ( Convert.ToSingle( data[ 0 ] / (decimal)notesOffset ) + ( _song.Speed * noteDelay ) );
        spawnPos.y -= ( _song.Bpm / 60 ) * startSound.length * _song.Speed;
        newNoteObj.transform.position = spawnPos;
        //newNoteObj.transform.position += Vector3.down * Convert.ToSingle(secNoteData[0] / notesOffset);

        /*
         * Each note gameobject has a special component named "NoteObject".
         * It controls the note's movement based on the data provided.
         * It also allows Player 2 to hit their notes.
         *
         * Below we set this note's component data. Simple.
         *
         * DummyNote is always false if generated via a JSON.
         */
        NoteObject nObj = newNoteObj.GetComponent<NoteObject>( );

        nObj.ScrollSpeed = -_song.Speed;
        nObj.strumTime = (float)data[ 0 ];
        nObj.type = noteType;
        nObj.mustHit = mustHitNote;
        nObj.dummyNote = false;
        nObj.layer = section.MustHitSection ? 1 : 2;

        /*
         * We add this new note to a list of either player 1's notes
         * or player 2's notes, depending on who it belongs to.
         */
        if ( mustHitNote )
            player1NotesObjects[ noteType ].Add( nObj );
        else
            player2NotesObjects[ noteType ].Add( nObj );

        if (customNotes.ContainsKey((int)data[3]))
        {
            nObj.custom = customNotes[(int)data[3]];
        } 
        else
        {
            nObj.custom = null;
        }
        nObj.Start();
        /*
         * This below is for hold notes generation. It tells the future
         * hold note what the previous note is.
         */
        lastNote = nObj;
        /*
         * Now we generate hold notes depending on this note's hold length.
         * The generation of hold notes is more or less the same as normal
         * notes. Hold notes, though, use a different gameobject as it's not
         * a normal note.
         *
         * If there's nothing, we skip.
         */
        for ( int i = 0; i < Math.Floor( susLength ); i++ ) {
            GameObject newSusNoteObj;
            Vector3 susSpawnPos;

            bool setAsLastSus = false;

            /*
             * Math.floor returns the largest integer less than or equal to a given number.
             *
             * I uh... have no clue why this is needed or what it does but we need this
             * in or else it won't do hold notes right so...
             */
            newSusNoteObj = holdNotesPool.GetObject();
            if ( ( i + 1 ) == Math.Floor( susLength ) ) {
                newSusNoteObj.GetComponent<SpriteRenderer>( ).sprite = holdNoteEnd;
                setAsLastSus = true;
            }
            else
            {
                setAsLastSus = false;
                newSusNoteObj.GetComponent<SpriteRenderer>().sprite = holdNoteSprite;
            }

            switch ( noteType ) {
                case 0: //Left
                    susSpawnPos = mustHitNote ? player1Left.position : player2Left.position;
                    break;
                case 1: //Down
                    susSpawnPos = mustHitNote ? player1Down.position : player2Down.position;
                    break;
                case 2: //Up
                    susSpawnPos = mustHitNote ? player1Up.position : player2Up.position;
                    break;
                case 3: //Right
                    susSpawnPos = mustHitNote ? player1Right.position : player2Right.position;
                    break;
                default:
                    susSpawnPos = mustHitNote ? player1Left.position : player2Left.position;
                    break;
            }


            susSpawnPos += Vector3.down *
                           ( Convert.ToSingle( data[ 0 ] / (decimal)notesOffset ) + ( _song.Speed * noteDelay ) );
            susSpawnPos.y -= ( _song.Bpm / 60 ) * startSound.length * _song.Speed;
            newSusNoteObj.transform.position = susSpawnPos;
            NoteObject susObj = newSusNoteObj.GetComponent<NoteObject>( );
            susObj.type = noteType;
            susObj.ScrollSpeed = -_song.Speed;
            susObj.mustHit = mustHitNote;
            susObj.strumTime = (float)data[ 0 ] + ( stepCrochet * i ) + stepCrochet;
            susObj.susNote = true;
            susObj.dummyNote = false;
            susObj.lastSusNote = setAsLastSus;
            susObj.layer = section.MustHitSection ? 1 : 2;
            if (customNotes.ContainsKey((int)data[3]))
            {
                susObj.custom = customNotes[(int)data[3]];
            }
            else
            {
                susObj.custom = null;
            }
            susObj.Start();
            susObj.GenerateHold( lastNote );
            if ( mustHitNote )
                player1NotesObjects[ noteType ].Add( susObj );
            else
                player2NotesObjects[ noteType ].Add( susObj );
            lastNote = susObj;
        }
    }
    

    #region Pause Menu
    public void PauseSong()
    {
        

        subtitleDisplayer.paused = true;
        
        stopwatch.Stop();
        beatStopwatch.Stop();

        foreach (AudioSource source in musicSources)
        {
            source.Pause();
        }

        if(hasVoiceLoaded)
            vocalSource.Pause();

        Pause.instance.pauseScreen.SetActive(true);
    }

    public void ContinueSong()
    {
        stopwatch.Start();
        beatStopwatch.Start();
        
        subtitleDisplayer.paused = false;

        foreach (AudioSource source in musicSources)
        {
            source.UnPause();
        }

        if(hasVoiceLoaded)
            vocalSource.UnPause();
        
        Pause.instance.pauseScreen.SetActive(false);
    }

    public void RestartSong()
    {
        subtitleDisplayer.StopSubtitles();
        PlaySong(false,difficulty,currentSongMeta.songPath);
        Pause.instance.pauseScreen.SetActive(false);
    }

    public void QuitSong()
    {
        ContinueSong();
        subtitleDisplayer.StopSubtitles();
        foreach (AudioSource source in musicSources)
        {
            source.Stop();
        }

        if(hasVoiceLoaded)
            vocalSource.Stop();
    }

    #endregion
    #endregion

    public void QuitGame()
    {
        Application.Quit();
    }


    
    #region Animating

    public void EnemyPlayAnimation(string animationName)
    {
        if (enemy.idleOnly || OptionsV2.DesperateMode) return;
        opponentAnimator.Play(animationName);
        _currentEnemyIdleTimer = enemyIdleTimer;
    }

    private void BoyfriendPlayAnimation(string animationName)
    {
        if (OptionsV2.DesperateMode) return;
        boyfriendAnimator.Play("BF " + animationName);

        
        _currentBoyfriendIdleTimer = boyfriendIdleTimer;
    }
    
    public void AnimateNote(int player, int type, string animName)
    {
        switch (player)
        {
            case 1: //Boyfriend
                
                player1NotesAnimators[type].Play(animName,0,0);
                player1NotesAnimators[type].speed = 0;
                        
                player1NotesAnimators[type].Play(animName);
                player1NotesAnimators[type].speed = 1;

                if (animName == "Activated" & !Player.twoPlayers)
                {
                    if(Player.demoMode)
                        _currentDemoNoteTimers[type] = enemyNoteTimer;
                    else if(Player.playAsEnemy)
                        _currentEnemyNoteTimers[type] = enemyNoteTimer;

                }

                break;
            case 2: //Opponent
                
                player2NotesAnimators[type].Play(animName,0,0);
                player2NotesAnimators[type].speed = 0;
                        
                player2NotesAnimators[type].Play(animName);
                player2NotesAnimators[type].speed = 1;

                if (animName == "Activated" & !Player.twoPlayers)
                {
                    if(!Player.playAsEnemy)
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
        
        if (!Player.playAsEnemy || Player.twoPlayers || Player.demoMode)
        {
            /*
            float accuracyPercent;
            if(playerOneStats.totalNoteHits != 0)
            {
                float sickScore = playerOneStats.totalSicks * 4;
                float goodScore = playerOneStats.totalGoods * 3;
                float badScore = playerOneStats.totalBads * 2;
                float shitScore = playerOneStats.totalShits;

                float totalAccuracyScore = sickScore + goodScore + badScore + shitScore;

                var accuracy = totalAccuracyScore / (playerOneStats.totalNoteHits * 4);
                
                accuracyPercent = (float) Math.Round(accuracy, 4);
                accuracyPercent *= 100;
            }
            else
            {
                accuracyPercent = 0;
            }

            playerOneScoringText.text =
                $"Score: {playerOneStats.currentScore}\nAccuracy: {accuracyPercent}%\nCombo: {playerOneStats.currentCombo} ({playerOneStats.highestCombo})\nMisses: {playerOneStats.missedHits}";
        */
            playerOneScoringText.text = playerOneStats.currentScore.ToString("00000000");
        }
        else
        {
            playerOneScoringText.text = string.Empty;
        }

        if (Player.playAsEnemy || Player.twoPlayers || Player.demoMode)
        {
            /*
            float accuracyPercent;
            if(playerTwoStats.totalNoteHits != 0)
            {
                float sickScore = playerTwoStats.totalSicks * 4;
                float goodScore = playerTwoStats.totalGoods * 3;
                float badScore = playerTwoStats.totalBads * 2;
                float shitScore = playerTwoStats.totalShits;

                float totalAccuracyScore = sickScore + goodScore + badScore + shitScore;

                var accuracy = totalAccuracyScore / (playerTwoStats.totalNoteHits * 4);
                
                accuracyPercent = (float) Math.Round(accuracy, 4);
                accuracyPercent *= 100;
            }
            else
            {
                accuracyPercent = 0;
            }

            playerTwoScoringText.text =
                $"Score: {playerTwoStats.currentScore}\nAccuracy: {accuracyPercent}%\nCombo: {playerTwoStats.currentCombo} ({playerTwoStats.highestCombo})\nMisses: {playerTwoStats.missedHits}";
        */
            playerTwoScoringText.text = playerTwoStats.currentScore.ToString("00000000");

        }
        else
        {
            playerTwoScoringText.text = string.Empty;
        }
    }
    
    public void NoteHit(NoteObject note)
    {
        if (note == null) return;


        var player = note.mustHit ? 1 : 2;
        if (note.custom != null && player == 2)
            return;
        
        if(hasVoiceLoaded)
            vocalSource.mute = false;

        bool invertHealth = false;

        int noteType = note.type;
        switch (player)
        {
            case 1:
                if(!Player.playAsEnemy || Player.demoMode || Player.twoPlayers)
                    invertHealth = false;
                switch (noteType)
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
                AnimateNote(1, noteType,"Activated");
                if (note.custom != null)
                    note.custom.methodTriggerOnClick.Invoke(note.custom.assemblyObj, null);
                break;
            case 2:
                if(Player.playAsEnemy || Player.demoMode || Player.twoPlayers)
                    invertHealth = true;
                switch (noteType)
                {
                    case 0:
                        //Left
                        EnemyPlayAnimation("Sing Left");
                        break;
                    case 1:
                        //Down
                        EnemyPlayAnimation("Sing Down");
                        break;
                    case 2:
                        //Up
                        EnemyPlayAnimation("Sing Up");
                        break;
                    case 3:
                        //Right
                        EnemyPlayAnimation("Sing Right");
                        break;
                }
                AnimateNote(2, noteType,"Activated");
                break;
        }

        bool modifyScore = true;

        if (player == 1 & Player.playAsEnemy & !Player.twoPlayers)
            modifyScore = false;
        else if (player == 2 & !Player.playAsEnemy & !Player.twoPlayers)
            modifyScore = false;

        if (Player.demoMode) modifyScore = true;

        CameraMovement.instance.focusOnPlayerOne = note.layer == 1;

        Rating rating;
        if(!note.susNote & modifyScore)
        {
            if (player == 1)
            {
                playerOneStats.totalNoteHits++;
            }
            else
            {
                playerTwoStats.totalNoteHits++;
            }

            float yPos = note.transform.position.y;

            var newRatingObject = !OptionsV2.LiteMode ? Instantiate(ratingObject) : liteRatingObjectP1;
            Vector3 ratingPos = new Vector3(1, 1, 0);
            newRatingObject.transform.position = ratingPos;
            if (OptionsV2.LiteMode & player == 2)
            {
                newRatingObject = liteRatingObjectP2;
                ratingPos = newRatingObject.transform.position;
            }
            
            ratingPos.y = OptionsV2.Downscroll ? 6 : 1;
            if (player == 2)
            {
                
                if (!OptionsV2.LiteMode)
                {
                    ratingPos.x = -ratingPos.x;
                }
            }
            
            newRatingObject.transform.position = ratingPos;

            var ratingObjectScript = newRatingObject.GetComponent<RatingObject>();

            if (OptionsV2.LiteMode)
            {
                ratingObjectScript.liteTimer = 2.15f;
            }
            

            /*
             * Rating and difference calulations from FNF Week 6 update
             */
            
            float noteDiff = Math.Abs(note.strumTime - stopwatch.ElapsedMilliseconds + Player.visualOffset+Player.inputOffset);

            if (noteDiff > 0.9 * Player.safeZoneOffset)
            {
                // way early or late
                rating = Rating.Shit;
            }
            else if (noteDiff > .75 * Player.safeZoneOffset)
            {
                // early or late
                rating = Rating.Bad;
            }
            else if (noteDiff > .35 * Player.safeZoneOffset)
            {
                // your kinda there
                rating = Rating.Good;
            }
            else
            {
                rating = Rating.Sick;
            }

            if (Player.demoMode)
            {
                rating = Rating.Sick;
            }

            switch (rating)
            {
                case Rating.Sick:
                {
                    ratingObjectScript.sprite.sprite = sickSprite;

                    if(!invertHealth)
                        health += 5;
                    else
                        health -= 5;
                    if (player == 1)
                    {
                        playerOneStats.currentCombo++;
                        playerOneStats.totalSicks++;
                        playerOneStats.currentScore += 10;
                    }
                    else
                    {
                        playerTwoStats.currentCombo++;
                        playerTwoStats.totalSicks++;
                        playerTwoStats.currentScore += 10;
                    }
                    break;
                }
                case Rating.Good:
                {
                    ratingObjectScript.sprite.sprite = goodSprite;

                    if (!invertHealth)
                        health += 2;
                    else
                        health -= 2;
                
                    if (player == 1)
                    {
                        playerOneStats.currentCombo++;
                        playerOneStats.totalGoods++;
                        playerOneStats.currentScore += 5;
                    }
                    else
                    {
                        playerTwoStats.currentCombo++;
                        playerTwoStats.totalGoods++;
                        playerTwoStats.currentScore += 5;
                    }
                    break;
                }
                case Rating.Bad:
                {
                    ratingObjectScript.sprite.sprite = badSprite;

                    if (!invertHealth)
                        health += 1;
                    else
                        health -= 1;

                    if (player == 1)
                    {
                        playerOneStats.currentCombo++;
                        playerOneStats.totalBads++;
                        playerOneStats.currentScore += 1;
                    }
                    else
                    {
                        playerTwoStats.currentCombo++;
                        playerTwoStats.totalBads++;
                        playerTwoStats.currentScore += 1;
                    }
                    break;
                }
                case Rating.Shit:
                    ratingObjectScript.sprite.sprite = shitSprite;

                    if (player == 1)
                    {
                        playerOneStats.currentCombo = 0;
                        playerOneStats.totalShits++;
                    }
                    else
                    {
                        playerTwoStats.currentCombo = 0;
                        playerTwoStats.totalShits++;
                    }
                    break;
            }
            
            if (player == 1)
            {
                if (playerOneStats.highestCombo < playerOneStats.currentCombo)
                {
                    playerOneStats.highestCombo = playerOneStats.currentCombo;
                }
                playerOneStats.hitNotes++;

                if (playerOneComboTween != null)
                {
                    LeanTween.cancel(playerOneComboTween.id);
                }

                playerOneComboText.text = playerOneStats.currentCombo.ToString("000");

                playerOneComboText.alpha = 1;
                
                playerOneComboTween = LeanTween.value(playerOneComboText.gameObject, 1, 0, .2f).setDelay(1).setOnUpdate(
                    value =>
                    {
                        playerOneComboText.alpha = value;
                    });

                if (playerOneScoreTween != null)
                {
                    LeanTween.cancel(playerOneScoreTween.id);
                }

                playerOneScoreTween = LeanTween.value(playerOneScoringText.gameObject, new Vector3(1.2f, 1.2f, 1.2f),
                    new Vector3(1f, 1f, 1f), .25f).setOnUpdate(
                    value =>
                    {
                        value.z = 1;
                        playerOneScoringText.rectTransform.localScale = value;
                    });

            }
            else
            {
                if (playerTwoStats.highestCombo < playerTwoStats.currentCombo)
                {
                    playerTwoStats.highestCombo = playerTwoStats.currentCombo;
                }
                playerTwoStats.hitNotes++;
                
                if (playerTwoComboTween != null)
                {
                    LeanTween.cancel(playerTwoComboTween.id);
                }

                playerTwoComboText.text = playerTwoStats.currentCombo.ToString("000");

                playerTwoComboText.alpha = 1;
                
                
                playerTwoComboTween = LeanTween.value(playerTwoComboText.gameObject, 1, 0, .2f).setDelay(1).setOnUpdate(
                    value =>
                    {
                        playerTwoComboText.alpha = value;
                    });
                if (playerTwoScoreTween != null)
                {
                    LeanTween.cancel(playerTwoScoreTween.id);
                }

                playerTwoScoreTween = LeanTween.value(playerTwoScoringText.gameObject, new Vector3(1.2f, 1.2f, 1.2f),
                    new Vector3(1f, 1f, 1f), .25f).setOnUpdate(
                    value =>
                    {
                        value.z = 1;
                        playerTwoScoringText.rectTransform.localScale = value;
                    });
            }

            


            _currentRatingLayer++;
            ratingObjectScript.sprite.sortingOrder = _currentRatingLayer;
            ratingLayerTimer = _ratingLayerDefaultTime;
        }

        UpdateScoringInfo();
        if (player == 1)
        {
            player1NotesObjects[noteType].Remove(note);
        }
        else
        {
            player2NotesObjects[noteType].Remove(note);
        }

        if (note.susNote)
        {
            holdNotesPool.Release(note.gameObject);
        } else
        {

            switch (note.type)
            {
                case 0:
                    leftNotesPool.Release(note.gameObject);
                    break;
                case 1:
                    downNotesPool.Release(note.gameObject);
                    break;
                case 2:
                    upNotesPool.Release(note.gameObject);
                    break;
                case 3:
                    rightNotesPool.Release(note.gameObject);
                    break;
            }
        }

    }

    public void NoteMiss(NoteObject note)
    {
        note.custom.methodTriggerOnMiss.Invoke(note.custom.assemblyObj, null);
        if (note.canMiss)
                return;
        if (!note.canMiss)
        {
            print("MISS!!!");


            if (hasVoiceLoaded)
                vocalSource.mute = true;
            oopsSource.clip = noteMissClip[Random.Range(0, noteMissClip.Length)];
            oopsSource.Play();

            var player = note.mustHit ? 1 : 2;


            bool invertHealth = player == 2;

            int noteType = note.type;
            switch (player)
            {
                case 1:
                    switch (noteType)
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
                    break;
                default:
                    switch (noteType)
                    {
                        case 0:
                            //Left
                            EnemyPlayAnimation("Sing Left");
                            break;
                        case 1:
                            //Down
                            EnemyPlayAnimation("Sing Down");
                            break;
                        case 2:
                            //Up
                            EnemyPlayAnimation("Sing Up");
                            break;
                        case 3:
                            //Right
                            EnemyPlayAnimation("Sing Right");
                            break;
                    }
                    break;
            }

            bool modifyHealth = true;

            if (player == 1 & Player.playAsEnemy & !Player.twoPlayers)
                modifyHealth = false;
            else if (player == 2 & !Player.playAsEnemy & !Player.twoPlayers)
                modifyHealth = false;

            if (modifyHealth)
            {
                if (!invertHealth)
                    health -= 8;
                else
                    health += 8;
            }

            if (player == 1)
            {
                playerOneStats.currentScore -= 5;
                playerOneStats.currentCombo = 0;
                playerOneStats.missedHits++;
                playerOneStats.totalNoteHits++;
            }
            else
            {
                playerTwoStats.currentScore -= 5;
                playerTwoStats.currentCombo = 0;
                playerTwoStats.missedHits++;
                playerTwoStats.totalNoteHits++;
            }

            UpdateScoringInfo();

        }
    }

    #endregion

    


    // Update is called once per frame
    void Update()
    {
        if (songSetupDone)
        {
            modInstance?.Invoke("Update");
            
            if ( _noteBehaviours.Count > 0)
                foreach (NoteBehaviour nBeh in _noteBehaviours)
                    if (nBeh.count < 1)
                        nBeh.GenerateNote();
            
            if (songStarted & musicSources[0].isPlaying)
            {
                
                if(OptionsV2.SongDuration)
                {
                    float t = musicClip.length - musicSources[0].time;

                    int seconds = (int) (t % 60); // return the remainder of the seconds divide by 60 as an int
                    t /= 60; // divide current time y 60 to get minutes
                    int minutes = (int) (t % 60); //return the remainder of the minutes divide by 60 as an int

                    songDurationText.text = minutes + ":" + seconds.ToString("00");

                    songDurationBar.fillAmount = musicSources[0].time / musicClip.length;
                }
                if ((float)beatStopwatch.ElapsedMilliseconds / 1000 >= beatsPerSecond)
                {
                    beatStopwatch.Restart();
                    currentBeat++;
                    
                    modInstance?.Invoke("OnBeat",currentBeat);
                    if (_currentBoyfriendIdleTimer <= 0 & currentBeat % 2 == 0)
                    {
                        boyfriendAnimator.Play("BF Idle");
                    }

                    if (_currentEnemyIdleTimer <= 0 & currentBeat % 2 == 0)
                    {
                        opponentAnimator.Play("Idle");
                    }

                    
                    
                    if (!OptionsV2.LiteMode)
                    {
                        if (altDance)
                        {
                            girlfriendAnimator.Play("GF Dance Left");
                            altDance = false;
                        }
                        else
                        {
                            girlfriendAnimator.Play("GF Dance Right");
                            altDance = true;
                        }

                        if (!_portraitsZooming)
                        {
                            _portraitsZooming = true;
                            LeanTween.value(1.25f, 1, .15f).setOnComplete(() => { _portraitsZooming = false; })
                                .setOnUpdate(f =>
                                {
                                    boyfriendHealthIconRect.localScale = new Vector3(-f, f, 1);
                                    enemyHealthIconRect.localScale = new Vector3(f, f, 1);
                                });
                        }

                        if (!_cameraZooming)
                        {
                            if (currentBeat % 4 == 0)
                            {
                                LeanTween.value(uiCamera.gameObject, _defaultZoom - .1f, _defaultZoom,
                                        beatZoomTime).setOnUpdate(f => { uiCamera.orthographicSize = f; })
                                    .setOnComplete(() => { _cameraZooming = false; });

                                LeanTween.value(mainCamera.gameObject, defaultGameZoom - .1f, defaultGameZoom,
                                        beatZoomTime).setOnUpdate(f => { mainCamera.orthographicSize = f; })
                                    .setOnComplete(() => { _cameraZooming = false; });
                            }
                        }
                    }
                }
            }
            else if (!songStarted & !musicSources[0].isPlaying)
            {
                if (Input.GetKeyDown(Player.keybinds.pauseKeyCode))
                {
                    LoadingTransition.instance.Show(() => SceneManager.LoadScene("Title"));
                }
            }
            
            
            if (health > MAXHealth)
                health = MAXHealth;
            if (health <= 0)
            {
                health = 0;
                if(!Player.playAsEnemy & !Player.twoPlayers & !Player.demoMode)
                {
                    if (isDead)
                    {
                        if (!respawning)
                        {
                            if (Input.GetKeyDown(Player.pauseKey))
                            {
                                musicSources[0].Stop();
                                respawning = true;

                                deadBoyfriendAnimator.Play("Dead Confirm");

                                musicSources[0].PlayOneShot(deadConfirm);

                                deathBlackout.rectTransform.LeanAlpha(1, 3).setDelay(1).setOnComplete(() =>
                                {
                                    LoadingTransition.instance.Show(() => SceneManager.LoadScene("Game_Backup3"));
                                });
                            } else if (Input.GetKeyDown(KeyCode.Escape))
                            {
                                musicSources[0].Stop();
                                respawning = true;

                                LoadingTransition.instance.Show(() => SceneManager.LoadScene("Title"));
                            }
                        }
                    }
                    else
                    {
                        isDead = true;
                        
                        modInstance?.Invoke("OnDeath");


                        deathBlackout.color = Color.clear;

                        foreach (AudioSource source in musicSources)
                        {
                            source.Stop();
                        }

                        
                        if(hasVoiceLoaded)
                            vocalSource.Stop();

                        musicSources[0].PlayOneShot(deadNoise);

                        battleCanvas.enabled = false;

                        uiCamera.enabled = false;
                        mainCamera.enabled = false;
                        deadCamera.enabled = true;

                        beatStopwatch.Reset();
                        stopwatch.Reset();

                        subtitleDisplayer.StopSubtitles();
                        subtitleDisplayer.paused = false;

                        deadBoyfriend.transform.position = boyfriendObject.transform.position;
                        deadBoyfriend.transform.localScale = boyfriendObject.transform.localScale;

                        deadCamera.orthographicSize = mainCamera.orthographicSize;
                        deadCamera.transform.position = mainCamera.transform.position;

                        deadBoyfriendAnimator.Play("Dead Start");

                        Vector3 newPos = deadBoyfriend.transform.position;
                        newPos.y += 2.95f;
                        newPos.z = -10;

                        LeanTween.move(deadCamera.gameObject, newPos, .5f).setEaseOutExpo();

                        LeanTween.delayedCall(2.417f, () =>
                        {
                            if (!respawning)
                            {
                                musicSources[0].clip = deadTheme;
                                musicSources[0].loop = true;
                                musicSources[0].Play();
                                deadBoyfriendAnimator.Play("Dead Loop");
                            }
                        });
                    }
                }
            }
            

            float healthPercent = health / MAXHealth;
            boyfriendHealthBar.fillAmount = healthPercent;
            enemyHealthBar.fillAmount = 1 - healthPercent;

            var rectTransform = enemyHealthIcon.rectTransform;
            var anchoredPosition = rectTransform.anchoredPosition;
            Vector2 enemyPortraitPos = anchoredPosition;
            enemyPortraitPos.x = -(healthPercent * 394 - (200)) - 50;

            Vector2 boyfriendPortraitPos = anchoredPosition;
            boyfriendPortraitPos.x = -(healthPercent * 394 - (200)) + 50;
            
            if (healthPercent >= .80f)
            {
                enemyHealthIcon.sprite = enemy.portraitDead;
                boyfriendHealthIcon.sprite = boyfriendPortraitNormal; 
            } else if (healthPercent <= .20f)
            {
                enemyHealthIcon.sprite = enemy.portrait;
                boyfriendHealthIcon.sprite = boyfriendPortraitDead; 
            }
            else
            {
                enemyHealthIcon.sprite = enemy.portrait;
                boyfriendHealthIcon.sprite = boyfriendPortraitNormal; 
            }



            anchoredPosition = enemyPortraitPos;
            rectTransform.anchoredPosition = anchoredPosition;
            boyfriendHealthIcon.rectTransform.anchoredPosition = boyfriendPortraitPos;

            if (!musicSources[0].isPlaying & songStarted & !isDead & !respawning & !Pause.instance.pauseScreen.activeSelf & !Pause.instance.editingVolume)
            {
                //Song is done.
                
                MenuV2.startPhase = MenuV2.StartPhase.SongList;
                

                LeanTween.cancelAll();

                stopwatch.Stop();
                beatStopwatch.Stop();

                if (usingSubtitles)
                {
                    subtitleDisplayer.StopSubtitles();
                    subtitleDisplayer.paused = false;
                    usingSubtitles = false;

                }

                girlfriendAnimator.Play("GF Dance Loop");
                boyfriendAnimator.Play("BF Idle Loop");


                Player.demoMode = false;

                songSetupDone = false;
                songStarted = false;
                foreach (List<NoteObject> noteList in player1NotesObjects.ToList())
                {
                    foreach (NoteObject noteObject in noteList.ToList())
                    {
                        noteList.Remove(noteObject);
                    }
                }
                
                
                foreach (List<NoteObject> noteList in player2NotesObjects.ToList())
                {
                    foreach (NoteObject noteObject in noteList.ToList())
                    {
                        noteList.Remove(noteObject);
                        
                    }
                }
                
                leftNotesPool.ReleaseAll();
                downNotesPool.ReleaseAll();
                upNotesPool.ReleaseAll();
                rightNotesPool.ReleaseAll();
                holdNotesPool.ReleaseAll();
                
                battleCanvas.enabled = false;
                
                player1Notes.gameObject.SetActive(false);
                player2Notes.gameObject.SetActive(false);

                healthBar.SetActive(false);

                
                menuScreen.SetActive(false);
                
                string highScoreSave = currentSongMeta.songName + currentSongMeta.bundleMeta.bundleName +
                    difficulty.ToLower() +
                    modeOfPlay;

                int overallScore = 0;
                
                int currentHighScore = PlayerPrefs.GetInt(highScoreSave, 0);

                switch (modeOfPlay)
                {
                    //Boyfriend
                    case 1:
                        overallScore = playerOneStats.currentScore;
                        break;
                    //Opponent
                    case 2:
                        overallScore = playerTwoStats.currentScore;
                        break;
                    //Local Multiplayer
                    case 3:
                        overallScore = playerOneStats.currentScore + playerTwoStats.currentScore;
                        break;
                    //Auto
                    case 4:
                        overallScore = 0;
                        break;
                }

                if (overallScore > currentHighScore)
                {
                    PlayerPrefs.SetInt(highScoreSave, overallScore);
                    PlayerPrefs.Save();
                }
                
                LoadingTransition.instance.Show(() => { SceneManager.LoadScene("Title"); });



                
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
            if (Player.twoPlayers) continue;
            if (!Player.playAsEnemy)
            {
                if (player2NotesAnimators[i].GetCurrentAnimatorStateInfo(0).IsName("Activated"))
                {
                    _currentEnemyNoteTimers[i] -= Time.deltaTime;
                    if (_currentEnemyNoteTimers[i] <= 0)
                    {
                        AnimateNote(2, i, "Normal");
                    }
                }
            } else
            {
                if (player1NotesAnimators[i].GetCurrentAnimatorStateInfo(0).IsName("Activated"))
                {
                    _currentEnemyNoteTimers[i] -= Time.deltaTime;
                    if (_currentEnemyNoteTimers[i] <= 0)
                    {
                        AnimateNote(1, i, "Normal");
                    }
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

        if (OptionsV2.DesperateMode) return;
        if ((opponentAnimator.CurrentAnimation == null || !opponentAnimator.CurrentAnimation.Name.Contains("Idle")) & !songStarted)
        {
            _currentEnemyIdleTimer -= Time.deltaTime;
            if (_currentEnemyIdleTimer <= 0)
            {
                opponentAnimator.Play("Idle Loop");
                _currentEnemyIdleTimer = enemyIdleTimer;
            }
        }
        else
        {
            _currentEnemyIdleTimer -= Time.deltaTime;
        }

        if ((!boyfriendAnimator.CurrentAnimation.Name.Contains("Idle") || boyfriendAnimator.CurrentAnimation == null) & !songStarted)
        {

            _currentBoyfriendIdleTimer -= Time.deltaTime;
            if (_currentBoyfriendIdleTimer <= 0)
            {
                boyfriendAnimator.Play("BF Idle Loop");
                _currentBoyfriendIdleTimer = boyfriendIdleTimer;
            }
        }
        else
        {
            _currentBoyfriendIdleTimer -= Time.deltaTime;
        }


    }
}

[System.Serializable]
public class CustomNote
{
    public string name = "";
    public int index = 0;
    public List<Sprite> sprites = new List<Sprite>();
    public object assemblyObj = null;
    public MethodInfo methodTriggerOnClick = null;
    public MethodInfo methodTriggerOnMiss = null;
    public bool canMiss = false;
}