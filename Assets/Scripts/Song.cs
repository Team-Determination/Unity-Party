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
 using UnityEngine.Video;
 using raonyreis13.Utils;

 // ReSharper disable IdentifierTypo
// ReSharper disable PossibleNullReferenceException

public class Song : MonoBehaviour
{

    #region Variables
    public static bool inEditorMode = false;
    public static FNFSong editedSong = null;
    public static FNFSong currentSong;
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
    public GameObject cutsceneParent;
    public VideoPlayer videoPlayerCutscene;
    public bool hcsD = false;
    public GameObject blankGameObject;
    public GameObject modCanvas;
    public bool dontback = false;
    public static List<SongMetaV2> metas = new List<SongMetaV2>();

    public GameObject p1, p2;


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
    public float cameraBopLerpSpeed;
    public float portraitBopLerpSpeed;
    public float _defaultZoom;
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
    public List<NoteBehaviour> _noteBehaviours = new List<NoteBehaviour>();
    public CameraMovement cameraMov;
    public List<Sprite> numbersOfRating;
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

    public float basehealthPercent = 0;

    [FormerlySerializedAs("bfObj")] [Header("Boyfriend")] public GameObject boyfriendObject;
    public SpriteAnimator boyfriendAnimator;
    public float boyfriendIdleTimer = .3f;
    public Sprite boyfriendPortraitNormal;
    public Sprite boyfriendPortraitDead;
    private float _currentBoyfriendIdleTimer;

    private FNFSong _song;
    public float timeOffset = 1000;
    public float noteSpawnOffsetHandlerValue = 505.86206896551724137931034482759f;

    public static Song instance;
    public ObjectPool ratingObjectPool;
    public Transform ratingP1Parent;
    public Transform ratingP2Parent;
    public int currentComboP1, currentComboP2;

    [Header("Scenes")] public Dictionary<string, SceneData> scenes;

    [Header("Health")] public float health = 100;

    private const float MAXHealth = 200;
    public float healthLerpSpeed;
    public GameObject healthBar;
    public GameObject healthBarBase;
    public RectTransform boyfriendHealthIconRect;
    public Image boyfriendHealthIcon;
    public Image boyfriendHealthBar;
    public RectTransform enemyHealthIconRect;
    public GameObject enemyHealthIcon;
    public SpriteAnimator enemyHealthIconAnimator;
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
    public SongMetaV2 metaSong;

    public SongListObject selectedSong;

    public SpriteAnimator[] splshes;

    public bool songStarted;
    public bool cutsceneIsPlaying;

    [Header("Subtitles")]

    public SubtitleDisplayer subtitleDisplayer;
    public bool usingSubtitles;

    [Header("Cutscene Parser")]
    public CutsceneParser cutsceneParser;

    [Header("Custom Notes")]
    public Dictionary<int, CustomNote> customNotes = new Dictionary<int, CustomNote>();
    List<int> customNotesIndex = new List<int>();
    public bool haveCustomNotes = false;
    public List<Sprite> defaultNoteSprites = new List<Sprite>();
    public List<HybInstance> notesModScript = new List<HybInstance>();
    #endregion

    [Header("Modhelper")]
    public List<SpriteAnimator> playerStaticNotes;
    public List<SpriteAnimator> enemyStaticNotes;

    private void Awake()
    {
        instance = this;
        metaSong = currentSongMeta;
        print(metaSong.songPath);
        ModHelper.canvasMod = modCanvas;
        ModHelper.song = this;
        ModHelper.gameplayCamera = uiCamera.gameObject;
    }

    public Sprite GetSprite(string path, Vector2 pivot) => UsefulFunctions.GetSprite(path, pivot);

    private void Start()
    {
        /*
         * To allow other scripts to access the Song script without needing the
         * script to be found or referenced, we set a static variable within the
         * Song script itself that can be used at anytime to access the singular used Song
         * script instance.
         */

        selectedSongDir = string.IsNullOrWhiteSpace(currentSongMeta.songPath) ? selectedSong.directory.Replace('\\', '/') : currentSongMeta.songPath.Replace('\\', '/');

        jsonDir = Path.Combine(selectedSongDir, $"Chart-{difficulty.ToLower()}.json").Replace('\\', '/');

        if (Preloaded.preloadedAssets["Songs Data"].ContainsKey(jsonDir) && !inEditorMode) {
            _song = Preloaded.preloadedAssets["Songs Data"][jsonDir] as FNFSong;
            print("Finded " + currentSongMeta.songName + " data in Preloaded / Chached dictionary in \"Songs Data\" applying it.");
        } else if (!inEditorMode) {
            _song = new FNFSong(jsonDir);
            print("Not Finded " + currentSongMeta.songName + " data in Preloaded / Chached dictionary in \"Songs Data\" finding it in " + jsonDir + " and adding this to Cache ... ");
            Preloaded.preloadedAssets["Songs Data"].Add(jsonDir, _song);
        } else {
            _song = editedSong;
        }

        currentSong = _song;

        timeOffset = (noteSpawnOffsetHandlerValue);

        foreach (SpriteAnimator s in playerStaticNotes) {
            s.Play("Normal");
        }

        foreach (SpriteAnimator s in enemyStaticNotes) {
            s.Play("Normal");
        }
        /*
         * Sets the "songs folder" to %localappdata%/Rei/FridayNight/Songs.
         * This is used to find and load any found songs.
         *
         * This can only be changed within the editor itself and not in-game,
         * though it would not be hard to make that possible.
         */
        songsFolder = Path.Combine(Application.persistentDataPath, "Songs");

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
        * Initialize LeanTween
        */
        
       LeanTween.init(int.MaxValue);
        
        /*
         * Grabs the subtitle displayer.
         */
        subtitleDisplayer = GetComponent<SubtitleDisplayer>();

        if (OptionsV2.DesperateMode)
        {
            boyfriendObject.SetActive(false);
            opponentObject.SetActive(false);

            boyfriendHealthIcon.enabled = false;
            enemyHealthIcon.SetActive(false);
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
        
        PlaySong(doAuto, difficulty, currentSongMeta.songPath, currentSongMeta.hasCutscene, currentSongMeta.hasCustomNotes, currentSongMeta.customNotesName);
    }

    #region Song Gameplay

    public void PlaySong(bool auto, string difficulty = "", string directory = "", bool hasCutscene = false, bool hasCustomNotes = false, List<string> customNotesInUse = null)
    {
        if (hasCutscene)
        {
            cutsceneParser.ParseCutscene(Path.Combine(directory, "Cutscenes", "Start"));
            hcsD = true;
        }

        if (hasCustomNotes) {
            if (customNotesInUse != null) customNotes = CustomNoteParser.instance.ParseCustomNotes(directory, customNotesInUse);
            haveCustomNotes = true;
        }

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
        if(File.Exists(Path.Combine(selectedSongDir, "Subtitles.txt").Replace('\\', '/')))
        {
            TextAsset textAsset = new TextAsset(File.ReadAllText(Path.Combine(selectedSongDir, "Subtitles.txt")));
            subtitleDisplayer.Subtitle = textAsset;
            usingSubtitles = true;
        }

        if (File.Exists(Path.Combine(selectedSongDir, "ModScript.csx").Replace('\\', '/')))
        {
            modInstance = CScript.CreateRunner(File.ReadAllText(Path.Combine(selectedSongDir, "ModScript.csx"))).Instantiate("ModScript");
            modInstance?.Invoke("OnSongStarting");

            if (File.Exists(Path.Combine(selectedSongDir, "Events.json")))
            {
                string eventsData = File.ReadAllText(Path.Combine(selectedSongDir, "Events.json").Replace('\\', '/'));
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
        StartCoroutine(nameof(SetupSong), hasCutscene);

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

        if (Preloaded.preloadedAssets["Audios"].ContainsKey(Path.Combine(selectedSongDir, "Inst.ogg").Replace('\\', '/'))) {
            musicClip = Preloaded.preloadedAssets["Audios"][Path.Combine(selectedSongDir, "Inst.ogg").Replace('\\', '/')] as AudioClip;
            print("finded inst");
        } else {
            WWW www1 = new WWW(Path.Combine(selectedSongDir, "Inst.ogg").Replace('\\', '/')) {
                threadPriority = ThreadPriority.High
            };
            musicClip = www1.GetAudioClip();
            yield return new WaitUntil(() => musicClip.loadState == AudioDataLoadState.Loaded);
        }

        if (Preloaded.preloadedAssets["Audios"].ContainsKey(Path.Combine(selectedSongDir, "Voices.ogg").Replace('\\', '/'))) {
            if (_song.NeedVoices) {
                vocalClip = Preloaded.preloadedAssets["Audios"][Path.Combine(selectedSongDir, "Voices.ogg").Replace('\\', '/')] as AudioClip;
                hasVoiceLoaded = true;
                print("finded voices");
                GenerateSong();
            } else {
                hasVoiceLoaded = false;
                GenerateSong();
            }
        } else {
            if (_song.NeedVoices) {
                WWW www2 = new WWW(Path.Combine(selectedSongDir, "Voices.ogg").Replace('\\', '/'));
                vocalClip = www2.GetAudioClip();
                yield return new WaitUntil(() => vocalClip.loadState == AudioDataLoadState.Loaded);
                hasVoiceLoaded = true;
                GenerateSong();
            } else {
                hasVoiceLoaded = false;
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

        if (hcsD)
        {
            LoadingTransition.instance.Hide();
            cutsceneParser.PlayCutscene();
        }
        
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
        string charDir = selectedSongDir + "/Opponent";
        string charDirJson = Path.Combine(charDir, "char-meta.json").Replace('\\', '/');
        if (!OptionsV2.DesperateMode) {
            if (File.Exists(charDirJson)) {
                
                string charDirAnimations = Path.Combine(charDir, "Animations").Replace('\\', '/');
                string portraitDir = Path.Combine(charDir, "Portraits").Replace('\\', '/');
                string portraitJsonDir = Path.Combine(charDir, "Portraits", "portraits-controller.json").Replace('\\', '/');
                Dictionary<string, List<Sprite>> CharacterAnimations = new Dictionary<string, List<Sprite>>();
                opponentAnimator.spriteAnimations = new List<SpriteAnimation>();
                CharacterMeta currentCharacter = new CharacterMeta();

                if (Preloaded.preloadedAssets["Characters"].ContainsKey(charDirJson)) {
                    currentCharacter = Preloaded.preloadedAssets["Characters"][charDirJson] as CharacterMeta;
                    print("Finded character data in Preloaded / Chached dictionary in \"Characters\" applying it.");
                } else {
                    currentCharacter.Character = defaultEnemy;
                    print("Not Finded character data in Preloaded / Chached dictionary in \"Characters\" applying it.");
                }

                Character newCharacter = ScriptableObject.CreateInstance<Character>();
                newCharacter = currentCharacter.Character;

                PortraitRoot portraitRoot;
                if (Preloaded.preloadedAssets["Portraits"].ContainsKey(currentCharacter.Character.characterName)) {
                    portraitRoot = Preloaded.preloadedAssets["Portraits"][currentCharacter.Character.characterName] as PortraitRoot;
                } else {
                    portraitRoot = JsonConvert.DeserializeObject<PortraitRoot>(File.ReadAllText(portraitJsonDir));
                    portraitRoot.path = portraitDir;
                    portraitRoot.GetAnimations();
                    Preloaded.preloadedAssets["Portraits"].Add(currentCharacter.Character.characterName, portraitRoot);
                }

                if (portraitRoot != null) {
                    newCharacter.portraitData = portraitRoot.portraitData;
                } else {
                    newCharacter.portraitData = defaultEnemy.portraitData;
                }

                enemy = newCharacter;

                opponentAnimator.transform.localScale = new Vector2(newCharacter.scale, newCharacter.scale);

                enemyHealthIconAnimator.spriteAnimations = enemy.portraitData.animations;
                enemyHealthIconRect.sizeDelta = enemyHealthIconAnimator.currentFrame.SizeDelta;
                enemyHealthBar.color = enemy.healthColor;
                playerTwoCornerImage.color = enemy.healthColor;


                Vector3 offset = enemy.cameraOffset;
                offset.z = -10;
                enemy.cameraOffset = offset;

                EnemyPlayAnimation("Idle");
            } else {
                enemy = defaultEnemy;
                Character newCharacter = ScriptableObject.CreateInstance<Character>();
                newCharacter = enemy;
                opponentAnimator.transform.localScale = new Vector2(newCharacter.scale, newCharacter.scale);

                enemyHealthIconAnimator.spriteAnimations = enemy.portraitData.animations;
                enemyHealthIconRect.sizeDelta = enemyHealthIconAnimator.currentFrame.SizeDelta;
                enemyHealthBar.color = enemy.healthColor;
                playerTwoCornerImage.color = enemy.healthColor;


                Vector3 offset = enemy.cameraOffset;
                offset.z = -10;
                enemy.cameraOffset = offset;

                EnemyPlayAnimation("Idle");
            }
            cameraMov.playerTwoOffset = 
                enemy.cameraOffset;
            cameraMov.dplayerTwoOffset = 
                enemy.cameraOffset;

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

                cameraMov.instance.playerTwoOffset = enemy.cameraOffset;
            } 
            else
            {


            }
            */
            if (Directory.Exists(Path.Combine(currentSongMeta.songPath, "Scene"))) {
                SceneParser.instance.ParseScene(currentSongMeta.songPath);
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
        if (hcsD)
        {
            modInstance?.Invoke("OnCutsceneStart");
            yield return new WaitUntil(() => !cutsceneIsPlaying);
            modInstance?.Invoke("OnCutsceneEnd");
        }

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
        LoadingTransition.instance.Hide();
        startSongTooltip.SetActive(true);
        startSongTooltip.GetComponentInChildren<TMP_Text>().text = $"Press {Player.keybinds.startSongKeyCode} to start the song.";

        DiscordController.instance.EnableGameStateLoop = true;
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
    
    
    public void GenNote( FNFSong.FNFSection section, List<decimal> note, int toKill ) {
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
        int noteDataCustom = Convert.ToInt32(note[3]);

        CustomNote currentCustomNote = null;
        if (haveCustomNotes) {
            try {
                currentCustomNote = customNotes[noteDataCustom];
            } catch {
                currentCustomNote = null;
            }
        }

        /*
         * It sets the "must hit note" boolean depending if the note
         * is in a section focusing on the boyfriend or not, and
         * if the note is for the other section.
         */
        bool mustHitNote = section.MustHitSection;
        if ( data[ 1 ] > 3 )
            mustHitNote = !section.MustHitSection;
        int noteType = Convert.ToInt32( data[ 1 ] % 4 );

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
        nObj.customNoteData = currentCustomNote;
        nObj.isCustomNote = currentCustomNote != null;
        if (currentCustomNote != null) {
            if (currentCustomNote.noteSprites != null) {
                if (currentCustomNote.noteSprites.ContainsKey("Note Normal")) {
                    nObj._sprite.sprite = currentCustomNote.noteSprites["Note Normal"];
                } else {
                    nObj._sprite.sprite = defaultNoteSprites[0];
                }
            } else {
                nObj._sprite.sprite = defaultNoteSprites[0];
            }
        } else {
            nObj._sprite.sprite = defaultNoteSprites[0];
        }
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
            SpriteRenderer sprender = newSusNoteObj.GetComponent<SpriteRenderer>();
            if ( ( i + 1 ) == Math.Floor( susLength ) ) {
                if (currentCustomNote != null) {
                    if (currentCustomNote.noteSprites != null) {
                        if (currentCustomNote.noteSprites.ContainsKey("Hold End")) {
                            sprender.sprite = currentCustomNote.noteSprites["Hold End"];
                        } else {
                            sprender.sprite = defaultNoteSprites[2];
                        }
                    } else {
                        sprender.sprite = defaultNoteSprites[2];
                    }
                } else {
                    sprender.sprite = defaultNoteSprites[2];
                }
                setAsLastSus = true;
            }
            else
            {
                if (currentCustomNote != null) {
                    if (currentCustomNote.noteSprites != null) {
                        if (currentCustomNote.noteSprites.ContainsKey("Hold Middle")) {
                            sprender.sprite = currentCustomNote.noteSprites["Hold Middle"];
                        } else {
                            sprender.sprite = defaultNoteSprites[1];
                        }
                    } else {
                        sprender.sprite = defaultNoteSprites[1];
                    }
                } else {
                    sprender.sprite = defaultNoteSprites[1];
                }
                setAsLastSus = false;
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
            susObj.customNoteData = currentCustomNote;
            susObj.isCustomNote = currentCustomNote != null;
            susObj.type = noteType;
            susObj.ScrollSpeed = -_song.Speed;
            susObj.mustHit = mustHitNote;
            susObj.strumTime = (float)data[ 0 ] + ( stepCrochet * i ) + stepCrochet;
            susObj.susNote = true;
            susObj.dummyNote = false;
            susObj.lastSusNote = setAsLastSus;
            susObj.layer = section.MustHitSection ? 1 : 2;
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
                
                playerStaticNotes[type].Play(animName);

                if (animName == "Actived" & !Player.twoPlayers)
                {
                    if(Player.demoMode)
                        _currentDemoNoteTimers[type] = enemyNoteTimer;
                    else if(Player.playAsEnemy)
                        _currentEnemyNoteTimers[type] = enemyNoteTimer;

                }

                break;
            case 2: //Opponent

                enemyStaticNotes[type].Play(animName);

                if (animName == "Actived" & !Player.twoPlayers)
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
                $"Score: {playerOneStats.currentScore}\nAccuracy: {accuracyPercent.ToString("0.00")}%\nMisses: {playerOneStats.missedHits}";
        
            //playerOneScoringText.text = playerOneStats.currentScore.ToString("00000000");
        }
        else
        {
            playerOneScoringText.text = string.Empty;
        }

        if (Player.playAsEnemy || Player.twoPlayers || Player.demoMode)
        {
            
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
                $"Score: {playerTwoStats.currentScore}\nAccuracy: {accuracyPercent}%\nMisses: {playerTwoStats.missedHits}";
        
            //playerTwoScoringText.text = playerTwoStats.currentScore.ToString("00000000");

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

        if (note.customNoteData != null) {
            if (player == 1) note.customNoteData.noteScript.Invoke("OnHit", note);
            if (player == 2) note.customNoteData.noteScript.Invoke("OnBotHit", note);
        }
    
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
                        if (cameraMov.playerOneOffset.x > cameraMov.dplayerOneOffset.x - 0.2f)
                            cameraMov.playerOneOffset = new Vector2(cameraMov.playerOneOffset.x - 0.2f, cameraMov.playerOneOffset.y);
                        break;
                    case 1:
                        //Down
                        BoyfriendPlayAnimation("Sing Down");
                        if (cameraMov.playerOneOffset.y > cameraMov.dplayerOneOffset.y - 0.2f)
                            cameraMov.playerOneOffset = new Vector2(cameraMov.playerOneOffset.x, cameraMov.playerOneOffset.y - 0.2f);
                        break;
                    case 2:
                        //Up
                        BoyfriendPlayAnimation("Sing Up");
                        if (cameraMov.playerOneOffset.y < cameraMov.dplayerOneOffset.y + 0.2f)
                            cameraMov.playerOneOffset = new Vector2(cameraMov.playerOneOffset.x, cameraMov.playerOneOffset.y + 0.2f);
                        break;
                    case 3:
                        //Right
                        BoyfriendPlayAnimation("Sing Right");
                        if (cameraMov.playerOneOffset.x < cameraMov.dplayerOneOffset.x + 0.2f)
                            cameraMov.playerOneOffset = new Vector2(cameraMov.playerOneOffset.x + 0.2f, cameraMov.playerOneOffset.y);
                        break;
                }
                AnimateNote(1, noteType, "Actived");
                break;
            case 2:
                if(Player.playAsEnemy || Player.demoMode || Player.twoPlayers)
                    invertHealth = true;
                switch (noteType)
                {
                    case 0:
                        //Left
                        EnemyPlayAnimation("Sing Left");
                        if (cameraMov.playerTwoOffset.x > cameraMov.dplayerTwoOffset.x - 0.2f)
                            cameraMov.playerTwoOffset = new Vector2(cameraMov.playerTwoOffset.x - 0.2f, cameraMov.playerTwoOffset.y);
                        break;
                    case 1:
                        //Down
                        EnemyPlayAnimation("Sing Down");
                        if (cameraMov.playerTwoOffset.y > cameraMov.dplayerTwoOffset.y - 0.2f)
                            cameraMov.playerTwoOffset = new Vector2(cameraMov.playerTwoOffset.x, cameraMov.playerTwoOffset.y - 0.2f);
                        break;
                    case 2:
                        //Up
                        EnemyPlayAnimation("Sing Up");
                        if (cameraMov.playerTwoOffset.y < cameraMov.dplayerTwoOffset.y + 0.2f)
                            cameraMov.playerTwoOffset = new Vector2(cameraMov.playerTwoOffset.x, cameraMov.playerTwoOffset.y + 0.2f);
                        break;
                    case 3:
                        //Right
                        EnemyPlayAnimation("Sing Right");
                        if (cameraMov.playerTwoOffset.x < cameraMov.dplayerTwoOffset.x + 0.2f)
                            cameraMov.playerTwoOffset = new Vector2(cameraMov.playerTwoOffset.x + 0.2f, cameraMov.playerTwoOffset.y);
                        break;
                }
                AnimateNote(2, noteType,"Actived");
                break;
        }

        bool modifyScore = true;

        if (player == 1 & Player.playAsEnemy & !Player.twoPlayers)
            modifyScore = false;
        else if (player == 2 & !Player.playAsEnemy & !Player.twoPlayers)
            modifyScore = false;

        if (Player.demoMode) modifyScore = true;

        cameraMov.focusOnPlayerOne = note.layer == 1;

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

            var newRatingObject = player == 1 ? liteRatingObjectP1 : liteRatingObjectP2;
            Vector3 ratingPos = newRatingObject.transform.position;
            
            ratingPos.y = OptionsV2.Downscroll ? 6 : 1;
            
            newRatingObject.transform.position = ratingPos;

            var ratingObjectScript = newRatingObject.GetComponent<RatingObject>();

            

            /*
             * Rating and difference calulations from FNF Week 6 update
             */
            
            float noteDiff = Math.Abs(note.strumTime - stopwatch.ElapsedMilliseconds + Player.visualOffset+Player.inputOffset);

            if (noteDiff > 0.9 * Player.safeZoneOffset)
            {
                // way early or late
                rating = Rating.Shit;
                modInstance?.Invoke("OnNoteHit", "Shit", note.type, note.mustHit);
            }
            else if (noteDiff > .75 * Player.safeZoneOffset)
            {
                // early or late
                rating = Rating.Bad;
                modInstance?.Invoke("OnNoteHit", "Bad", note.type, note.mustHit);
            }
            else if (noteDiff > .35 * Player.safeZoneOffset)
            {
                // your kinda there
                rating = Rating.Good;
                modInstance?.Invoke("OnNoteHit", "Good", note.type, note.mustHit);
            }
            else
            {
                rating = Rating.Sick;
                modInstance?.Invoke("OnNoteHit", "Sick", note.type, note.mustHit);
            }

            if (Player.demoMode)
            {
                rating = Rating.Sick;
            }
            currentComboP1 = playerOneStats.currentCombo;
            currentComboP2 = playerTwoStats.currentCombo;
            switch (rating)
            {
                case Rating.Sick:
                {
                    RatingInstancer.instance.Rate(player, 3);
                    //ratingObjectScript.sprite.sprite = sickSprite;

                    if(!invertHealth)
                        if (note.customNoteData == null || !note.customNoteData.IgnoreHit)
                            health += 5;
                    else
                        if (note.customNoteData == null || !note.customNoteData.IgnoreHit)
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
                    RatingInstancer.instance.Rate(player, 2);
                    //ratingObjectScript.sprite.sprite = goodSprite;

                    if (!invertHealth)
                        if (note.customNoteData == null || !note.customNoteData.IgnoreHit)
                            health += 2;
                    else
                        if (note.customNoteData == null || !note.customNoteData.IgnoreHit)
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
                    RatingInstancer.instance.Rate(player, 1);
                    //ratingObjectScript.sprite.sprite = badSprite;

                    if (!invertHealth)
                        if (note.customNoteData == null || !note.customNoteData.IgnoreHit)
                            health += 1;
                    else
                        if (note.customNoteData == null || !note.customNoteData.IgnoreHit)
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
                    RatingInstancer.instance.Rate(player, 0);
                    //ratingObjectScript.sprite.sprite = shitSprite;

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
            ratingObjectScript.ShowRating();
            
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
        print("MISS!!!");

        if (note.customNoteData != null) {
            note.customNoteData.noteScript.Invoke("OnMiss", note);
            if (note.customNoteData.IgnoreMiss)
                return;
        }

        if(hasVoiceLoaded)
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
                if (enemyHealthIconRect.sizeDelta != enemyHealthIconAnimator.currentFrame.SizeDelta) {
                    enemyHealthIconRect.sizeDelta = enemyHealthIconAnimator.currentFrame.SizeDelta;
                }

                if (Input.GetKeyDown(KeyCode.Alpha7)) {
                    musicSources[0].Stop();
                    respawning = true;

                    LoadingTransition.instance.Show(() =>
                    {
                        SongConfigs.song = _song;
                        SceneManager.LoadScene("Song Configurations");
                        DiscordController.instance.EnableGameStateLoop = false;
                    });
                }

                if (OptionsV2.SongDuration)
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
                    if (enemyHealthIconAnimator.CurrentAnimation.SpriteAnimationType == SpriteAnimationType.PlayOnce) {
                        enemyHealthIconAnimator.Play(enemyHealthIconAnimator.CurrentAnimation);
                    }
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
                        
                        boyfriendHealthIconRect.localScale = new Vector3(-1.25f, 1.25f, 1);
                        if (enemy.portraitData.useDefaultBeats) enemyHealthIconRect.localScale = new Vector3(1.25f, 1.25f, 1);

                        if (currentBeat % 4 == 0)
                        {
                            mainCamera.orthographicSize = defaultGameZoom - .1f;
                            uiCamera.orthographicSize = _defaultZoom - .1f;
                        }
                    }
                }

                boyfriendHealthIconRect.localScale =
                    Vector3.Lerp(boyfriendHealthIconRect.localScale, new Vector3(-1, 1, 1),portraitBopLerpSpeed);
                if (enemy.portraitData.useDefaultBeats)
                    enemyHealthIconRect.localScale = 
                    Vector3.Lerp(enemyHealthIconRect.localScale, new Vector3(1, 1, 1),portraitBopLerpSpeed);

                mainCamera.orthographicSize = Mathf.Lerp(mainCamera.orthographicSize, defaultGameZoom,cameraBopLerpSpeed);
                uiCamera.orthographicSize = Mathf.Lerp(uiCamera.orthographicSize, _defaultZoom, cameraBopLerpSpeed);
            }
            else if (!songStarted & !musicSources[0].isPlaying)
            {
                if (Input.GetKeyDown(Player.keybinds.pauseKeyCode))
                {
                    LoadingTransition.instance.Show(() =>
                    {
                        SceneManager.LoadScene("Title");
                        DiscordController.instance.EnableGameStateLoop = false;
                    });
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
                                    LoadingTransition.instance.Show(() =>
                                    {
                                        SceneManager.LoadScene("Game_Backup3");
                                        DiscordController.instance.EnableGameStateLoop = false;
                                    });
                                });
                            } else if (Input.GetKeyDown(KeyCode.Escape))
                            {
                                musicSources[0].Stop();
                                respawning = true;

                                LoadingTransition.instance.Show(() =>
                                {
                                    SceneManager.LoadScene("Title");
                                    DiscordController.instance.EnableGameStateLoop = false;
                                });
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

            if (!musicSources[0].isPlaying & songStarted & !isDead & !respawning & !Pause.instance.pauseScreen.activeSelf & !Pause.instance.editingVolume) {
                StartCoroutine(End());
            }


            float healthPercent = health / MAXHealth;
            boyfriendHealthBar.fillAmount = healthPercent;
            enemyHealthBar.fillAmount = 1 - healthPercent;

            RectTransform rectTransform = enemyHealthIcon.GetComponent<RectTransform>();
            var anchoredPosition = rectTransform.anchoredPosition;
            Vector2 enemyPortraitPos = anchoredPosition;
            enemyPortraitPos.x = -(healthPercent * 394 - (200)) - 50;

            Vector2 boyfriendPortraitPos = anchoredPosition;
            boyfriendPortraitPos.x = -(healthPercent * 394 - (200)) + 50;
            
            if (healthPercent >= .80f)
            {
                if (enemyHealthIconAnimator.CurrentAnimation != enemyHealthIconAnimator.GetAnimationByName("Losing")) {
                    enemyHealthIconAnimator.Play("Losing");
                }

                boyfriendHealthIcon.sprite = boyfriendPortraitNormal; 
            } 
            else if (healthPercent <= .20f)
            {
                if (enemyHealthIconAnimator.CurrentAnimation != enemyHealthIconAnimator.GetAnimationByName("Winning")) {
                    if (enemyHealthIconAnimator.spriteAnimations.Contains(enemyHealthIconAnimator.GetAnimationByName("Winning"))) {
                        enemyHealthIconAnimator.Play("Winning");
                    } else {
                        enemyHealthIconAnimator.Play("Normal");
                    }
                }

                boyfriendHealthIcon.sprite = boyfriendPortraitDead; 
            }
            else
            {
                if (enemyHealthIconAnimator.CurrentAnimation != enemyHealthIconAnimator.GetAnimationByName("Normal")) {
                    enemyHealthIconAnimator.Play("Normal");
                }

                boyfriendHealthIcon.sprite = boyfriendPortraitNormal; 
            }



            anchoredPosition = enemyPortraitPos;
            rectTransform.anchoredPosition = anchoredPosition;
            boyfriendHealthIcon.rectTransform.anchoredPosition = boyfriendPortraitPos;
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
                if (enemyStaticNotes[i].CurrentAnimation.Name == "Actived")
                {
                    _currentEnemyNoteTimers[i] -= Time.deltaTime;
                    if (_currentEnemyNoteTimers[i] <= 0)
                    {
                        AnimateNote(2, i, "Normal");
                    }
                }
            }
            else
            {
                if (playerStaticNotes[i].CurrentAnimation.Name == "Actived")
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
                if (playerStaticNotes[i].CurrentAnimation.Name == "Actived")
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

    IEnumerator End() {
        if (File.Exists(Path.Combine(currentSongMeta.songPath, "Cutscenes/End/cutscene.json"))) {
            cutsceneParser.ParseCutscene(Path.Combine(currentSongMeta.songPath, "Cutscenes/End"), true);
            yield return new WaitUntil(() => !cutsceneIsPlaying);
        }
            //Song is done.

            MenuV2.startPhase = MenuV2.StartPhase.SongList;


            LeanTween.cancelAll();

            stopwatch.Stop();
            beatStopwatch.Stop();

            if (usingSubtitles) {
                subtitleDisplayer.StopSubtitles();
                subtitleDisplayer.paused = false;
                usingSubtitles = false;

            }

            girlfriendAnimator.Play("GF Dance Loop");
            boyfriendAnimator.Play("BF Idle Loop");


            Player.demoMode = false;

            songSetupDone = false;
            songStarted = false;
            foreach (List<NoteObject> noteList in player1NotesObjects.ToList()) {
                foreach (NoteObject noteObject in noteList.ToList()) {
                    noteList.Remove(noteObject);
                }
            }


            foreach (List<NoteObject> noteList in player2NotesObjects.ToList()) {
                foreach (NoteObject noteObject in noteList.ToList()) {
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

            switch (modeOfPlay) {
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

            if (overallScore > currentHighScore && !inEditorMode) {
                PlayerPrefs.SetInt(highScoreSave, overallScore);
                PlayerPrefs.Save();
            }
            LoadingTransition.instance.Show(() => {
                SceneManager.LoadScene("Title");
                DiscordController.instance.EnableGameStateLoop = false;
            });
    }
}
