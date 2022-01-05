 using System;
 using System.Collections;
 using System.Collections.Generic;
 using System.Diagnostics;
 using System.IO;
 using System.Linq;
 using FridayNightFunkin;
 using MoonSharp.VsCodeDebugger.SDK;
 using Newtonsoft.Json;
 using SimpleSpriteAnimator;
 using Slowsharp;
 using TMPro;
 using UnityEngine;
 using UnityEngine.Events;
 using UnityEngine.SceneManagement;
 using UnityEngine.UI;
 using Debug = UnityEngine.Debug;
 using Random = UnityEngine.Random;

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
    public AudioClip victoryClip;
    public AudioClip[] noteMissClip;
    public bool hasVoiceLoaded;    
    public HybInstance modInstance;


    [Space] public bool songSetupDone;
    public WeekData[] weeks;
    public WeekData weekData;
    public SongData[] songs;
    public SongData freeplaySong;
    public int difficulty = 1;
    public int currentSong;
    public bool freeplay;
    public string loadedScene;
   

    [Space] public GameObject ratingObject;
    public GameObject liteRatingObjectP1;
    public GameObject liteRatingObjectP2;
    public Sprite sickSprite;
    public Sprite goodSprite;
    public Sprite badSprite;
    public Sprite shitSprite;
    public GameObject playerOneScoringObject;
    public TMP_Text playerOneScoringText;
    public GameObject playerTwoScoringObject;
    public TMP_Text playerTwoScoringText;
    public float ratingLayerTimer;
    private float _ratingLayerDefaultTime = 2.2f;
    private int _currentRatingLayer;
    public PlayerStat overallStats;
    public PlayerStat playerOneStats;
    public PlayerStat playerTwoStats;

    public Stopwatch stopwatch;
    public Stopwatch beatStopwatch;
    public Stopwatch stepStopwatch;
    [Space] public Camera mainCamera;
    public Camera uiCamera;
    public float beatZoomTime;
    public float defaultZoom;
    public float defaultGameZoom;

    [Space, TextArea(2, 12)] public string jsonDir;
    public float notesOffset;
    public float noteDelay;
    [Range(-1f, 1f)] public float speedDifference;

    [Space] public Canvas battleCanvas;
    public Canvas menuCanvas;
    public GameObject generatingSongMsg;
    public GameObject songListScreen;

    [Header("Dialogue")] public Canvas dialogueCanvas; 
    public AudioSource dialogueMusicSource;
    public AudioClip dialogueTalkSound;
    public Image[] dialogueImages;
    public Image dialoguePortrait;
    public TMP_Text dialogueText;
    public int currentDialogue;
    public Coroutine dialogueProcess;
    public bool dialogueInProgress;
    public bool typewriterDone;
    
    [Space] public GameObject menuScreen;
    public GameObject victoryScreen;

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

    [Header("Prefabs")] public GameObject leftArrow;
    public GameObject downArrow;
    public GameObject upArrow;
    public GameObject rightArrow;
    [Space] public GameObject holdNote;
    public Sprite holdNoteEnd;
    
    [Header("Characters")]
    public string[] characterNames;
    public Character[] characters;
    public Dictionary<string, Character> charactersDictionary;
    [Space] public string[] protagonistNames;
    public Protagonist[] protagonists;
    public Dictionary<string, Protagonist> protagonistsDictionary;
    public Character defaultEnemy;
    public Protagonist defaultProtagonist;
    [Space] public GameObject girlfriendObject;
    public SpriteAnimator girlfriendAnimator;
    public bool altDance;

    [Header("Enemy")] public GameObject enemyObj;
    public Character enemy;
    public string enemyName;
    public SpriteAnimator enemyAnimator;
    public float enemyIdleTimer = .3f;
    private float _currentEnemyIdleTimer;
    public float enemyNoteTimer = .25f;
    private Vector3 _enemyDefaultPos;
    private readonly float[] _currentEnemyNoteTimers = {0, 0, 0, 0};
    private readonly float[] _currentDemoNoteTimers = {0, 0, 0, 0};
    private LTDescr _enemyFloat;



    [Header("Boyfriend")] public GameObject bfObj;
    public Protagonist boyfriend;
    
    public SpriteAnimator boyfriendAnimator;
    public float boyfriendIdleTimer = .3f;
    public Sprite boyfriendPortraitNormal;
    public Sprite boyfriendPortraitDead;
    private float _currentBoyfriendIdleTimer;

    private FNFSong _song;

    public static Song instance;

    [Header("Events")] public UnityEvent<int> onBeatHit;
    public UnityEvent<int> onStepHit;
    public UnityEvent<NoteObject> onNoteHit;

    [Space] public float health = 100;

    private const float MAXHealth = 200;
    public float healthLerpSpeed;
    public GameObject healthBar;
    public RectTransform boyfriendHealthIconRect;
    public Image boyfriendHealthIcon;
    public Image boyfriendHealthBar;
    public RectTransform enemyHealthIconRect;
    public Image enemyHealthIcon;
    public Image enemyHealthBar;

    [Space] public NoteObject lastNote;
    public float stepCrochet;
    public float beatsPerSecond;
    public int currentBeat;
    public int currentStep;
    public bool beat;

    private float _bfRandomDanceTimer;
    private float _enemyRandomDanceTimer;

    private bool _portraitsZooming;
    private bool _cameraZooming;

    public string songsFolder;
    public string selectedSongDir;

    [HideInInspector] public SongListObject selectedSong;


    public bool songStarted;

    private bool _onlineMode;
    public SubtitleDisplayer subtitleDisplayer;
    public bool usingSubtitles;

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
        battleCanvas.enabled = false;
        healthBar.SetActive(false);

        mainCamera = Camera.main;
        
        
        /*
         * Grabs the subtitle displayer.
         */
        subtitleDisplayer = GetComponent<SubtitleDisplayer>();
        
        /*
         * Check if the player wants to load with Lite mode.
         */
        if (PlayerPrefs.GetInt("Lite Mode", 0) == 1)
        {
            Options.LiteMode = true;
        }
        
        

        /*
         * In case we want to reset the enemy position later on,
         * we will save their current position.
         */
        _enemyDefaultPos = enemyObj.transform.position;

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
        protagonistsDictionary = new Dictionary<string, Protagonist>();
        for (int i = 0; i < protagonists.Length; i++)
        {
            protagonistsDictionary.Add(protagonistNames[i], protagonists[i]);
        }


        defaultZoom = uiCamera.orthographicSize;

        
    }

    #region Song Gameplay

    public void PlaySong()
    { 
		
        string songSceneName;
        songSceneName = !freeplay ? songs[currentSong].sceneName : freeplaySong.sceneName;
        if (SceneManager.GetSceneByName(loadedScene).isLoaded)
        {
            SceneManager.UnloadSceneAsync(loadedScene);
        }

        SceneManager.LoadScene(songSceneName,LoadSceneMode.Additive);
        loadedScene = songSceneName;
		

        /*
         * We'll reset any stats then update the UI based on it.
         */
        _currentRatingLayer = 0;
        playerOneStats = new PlayerStat();
        playerTwoStats = new PlayerStat();
        currentBeat = 0;

        UpdateScoringInfo();

        /*
         * We'll enable the gameplay UI.
         *
         * We'll also hide the Menu UI but also reset it
         * so we can instantly go back to the menu
         */
        battleCanvas.enabled = true;
        generatingSongMsg.SetActive(true);

        menuCanvas.enabled = false;
        mainCamera.enabled = true;
        menuScreen.SetActive(true);
        songListScreen.SetActive(false);
        Menu.instance.chooseSongMsg.SetActive(true);
        Menu.instance.songDetails.SetActive(false);
        
        /*
         * Now we start the song setup.
         *
         * This is a Coroutine so we can make use
         * of the functions to pause it for certain time.
         */
        SetupSong();

    }

    void SetupSong()
    {
        var songData = !freeplay ? songs[currentSong] : freeplaySong;
        musicClip = songData.instrumentals;
        if(songData.vocals != null)
        {
            vocalClip = songData.vocals;
            hasVoiceLoaded = true;
        }
        musicSources[1].clip = songData.nikoVocals;
        musicSources[1].volume = Options.voiceVolume;

        musicSources[0].volume = Options.instVolume;

        vocalSource.volume = Options.voiceVolume;

        musicSources[1].mute = false;
        vocalSource.mute = false;

        mainCamera.orthographicSize = songData.cameraZoom;
        defaultGameZoom = songData.cameraZoom;
        
        GenerateSong();
    }

    public void GenerateSong()
    {

        for (int i = 0; i < Options.instance.colorPickers.Length; i++)
        {
            player1NoteSprites[i].color = Options.instance.colorPickers[i].color;
            player2NoteSprites[i].color = Options.instance.colorPickers[i].color;
        }

        /*
         * Set the health the half of the max so it's smack dead in the
         * middle.
         */

        if(!Options.NoHealthGain)
        {
            health = MAXHealth / 2;
        }
        else
        {
            if(!Player.playAsEnemy)
                health = MAXHealth;
            else
                health = -MAXHealth;
        }
        /*
         * Special thanks to KadeDev for creating the .NET FNF Song parsing library.
         *
         * With it, we can load the song as a whole class full of chart information
         * via the chart file.
         */
        var songData = !freeplay ? songs[currentSong] : freeplaySong;
        var jsonData = difficulty == 1 ? songData.normalData : songData.hardData;
        

        _song = new FNFSong(jsonData,FNFSong.DataReadType.AsRawJson);

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
                foreach (var t in list)
                {
                    Destroy(t.gameObject);
                }

                list.Clear();
            }

            player1NotesObjects.Clear();
        }

        if (player2NotesObjects != null)
        {
            foreach (List<NoteObject> list in player2NotesObjects)
            {
                foreach (var t in list)
                {
                    Destroy(t.gameObject);
                }

                list.Clear();
            }

            player2NotesObjects.Clear();
        }

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
        if (Options.Downscroll)
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
        
        if(Options.Middlescroll)
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
        foreach (FNFSong.FNFSection section in _song.Sections)
        {
            foreach (var noteData in section.Notes)
            {
                /*
                 * The .NET FNF Chart parsing library already has something specific
                 * to tell us if the note is a must hit.
                 *
                 * But previously I already kind of reverse engineered the FNF chart
                 * parsing process so I used the "ConvertToNote" function in the .NET
                 * library to grab "note data".
                 */
                GameObject newNoteObj;
                List<decimal> data = noteData.ConvertToNote();

                /*
                 * It sets the "must hit note" boolean depending if the note
                 * is in a section focusing on the boyfriend or not, and
                 * if the note is for the other section.
                 */
                bool mustHitNote = section.MustHitSection;
                if (data[1] > 3)
                    mustHitNote = !section.MustHitSection;
                int noteType = Convert.ToInt32(data[1] % 4);

                if(Options.RandomNotes)
                {
                    noteType = Random.Range(0,4);
                }

                /*
                 * We make a spawn pos variable to later set the spawn
                 * point of this note.
                 */
                Vector3 spawnPos;

                /*
                 * We get the length of this note's hold length.
                 */
                float susLength = (float) data[2];

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
                susLength = susLength / stepCrochet;

                /*
                 * It checks the type of note this is and spawns in a note gameobject
                 * tailored for it then sets the spawn point for it depending on if it's
                 * a note belonging to player 1 or player 2.
                 *
                 * If somehow this is the wrong data type, it fails and stops the song generation.
                 */
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

                /*
                 * We then move the note to a specific position in the game world.
                 */
                spawnPos += Vector3.down *
                            (Convert.ToSingle(data[0] / (decimal) notesOffset) + (_song.Speed * noteDelay));
                spawnPos.y -= (_song.Bpm / 60) * startSound.length * _song.Speed;
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
                NoteObject nObj = newNoteObj.GetComponent<NoteObject>();

                nObj.ScrollSpeed = -_song.Speed;
                nObj.strumTime = (float) data[0];
                nObj.type = noteType;
                nObj.mustHit = mustHitNote;
                nObj.dummyNote = false;
                nObj.layer = section.MustHitSection ? 1 : 2;

                /*
                 * We add this new note to a list of either player 1's notes
                 * or player 2's notes, depending on who it belongs to.
                 */
                if (mustHitNote)
                    player1NotesObjects[noteType].Add(nObj);
                else
                    player2NotesObjects[noteType].Add(nObj);

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
                for (int i = 0; i < Math.Floor(susLength); i++)
                {
                    GameObject newSusNoteObj;
                    Vector3 susSpawnPos;

                    bool setAsLastSus = false;

                    /*
                     * Math.floor returns the largest integer less than or equal to a given number.
                     *
                     * I uh... have no clue why this is needed or what it does but we need this
                     * in or else it won't do hold notes right so...
                     */
                    newSusNoteObj = Instantiate(holdNote);
                    if ((i + 1) == Math.Floor(susLength))
                    {
                        newSusNoteObj.GetComponent<SpriteRenderer>().sprite = holdNoteEnd;
                        setAsLastSus = true;
                    }

                    switch (noteType)
                    {
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
                                   (Convert.ToSingle(data[0] / (decimal) notesOffset) + (_song.Speed * noteDelay));
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
                    susObj.layer = section.MustHitSection ? 1 : 2;
                    susObj.GenerateHold(lastNote);
                    if (mustHitNote)
                        player1NotesObjects[noteType].Add(susObj);
                    else
                        player2NotesObjects[noteType].Add(susObj);
                    lastNote = susObj;
                }





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
        musicSources[0].volume = Options.instVolume;
        musicSources[0].Stop();

       

        /*
         * Disable the entire Menu UI and enable the entire Gameplay UI.
         */
        menuCanvas.enabled = false;
        battleCanvas.enabled = true;
        
        
        /*
         * If the player 2 in the chart exists in this engine,
         * we'll change player 2 to the correct character.
         *
         * If not, keep any existing character we selected.
         */

        print("Checking for and applying " + _song.Player2 +". Result is " + charactersDictionary.ContainsKey(_song.Player2));
        if (charactersDictionary.ContainsKey(_song.Player2))
        {
            enemy = charactersDictionary[_song.Player2];
            enemyAnimator.spriteAnimations = enemy.animations;
            enemyAnimator.Play("Idle");


            /*
             * Yes, opponents can float if enabled in their
             * configuration file.
             */
            if (enemy.doesFloat)
            {
                _enemyFloat = LeanTween.moveLocalY(enemyObj, enemy.floatToOffset, enemy.floatSpeed).setEaseInOutExpo()
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
                    enemyObj.transform.position = _enemyDefaultPos;
                }
            }

            enemyHealthIcon.sprite = enemy.portrait;
            enemyHealthIconRect.sizeDelta = enemy.portraitSize;

            enemyHealthBar.color = enemy.healthColor;

            CameraMovement.instance.playerTwoOffset = enemy.cameraOffset;
        }
        else
        {
#if !UNITY_WEBGL
            string charDir = selectedSongDir+"/Opponent";
            Dictionary<string, List<Sprite>> CharacterAnimations = new Dictionary<string, List<Sprite>>();

            if (Directory.Exists(charDir))
            {
                enemyAnimator.spriteAnimations = new List<SpriteAnimation>();

                // BEGIN ANIMATIONS IMPORT

                var charMetaPath = charDir + "/char-meta.json";
                var currentMeta = File.Exists(charMetaPath)
                    ? JsonConvert.DeserializeObject<CharacterMeta>(File.ReadAllText(charMetaPath))
                    : null;

                if (charactersDictionary.ContainsKey(currentMeta.Character.name))
                {
                    enemy = charactersDictionary[currentMeta.Character.name];
                    enemyAnimator.spriteAnimations = enemy.animations;
                    
                    /*
                    * Yes, opponents can float if enabled in their
                    * configuration file.
                    */
                    if (enemy.doesFloat)
                    {
                        _enemyFloat = LeanTween.moveLocalY(enemyObj, enemy.floatToOffset, enemy.floatSpeed).setEaseInOutExpo()
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
                            enemyObj.transform.position = _enemyDefaultPos;
                        }
                    }

                    enemyHealthIcon.sprite = enemy.portrait;
                    enemyHealthIconRect.sizeDelta = enemy.portraitSize;

                    CameraMovement.instance.playerTwoOffset = enemy.cameraOffset;
                } 
                else
                {

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

                        enemyAnimator.spriteAnimations.Add(newAnimation);

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



                    charactersDictionary.Add(newCharacter.characterName, newCharacter);
                    enemy = newCharacter;

                    enemyHealthIcon.sprite = enemy.portrait;
                    enemyHealthIconRect.sizeDelta = enemy.portraitSize;

                    Vector3 offset = enemy.cameraOffset;
                    offset.z = -10;
                    enemy.cameraOffset = offset;
                    
                    CameraMovement.instance.playerTwoOffset = enemy.cameraOffset;
                }
            }
#endif
        }
        
        print("Checking for and applying " + _song.Player1 +". Result is " + protagonistsDictionary.ContainsKey(_song.Player1));


        boyfriend = protagonistsDictionary.ContainsKey(_song.Player1) ? protagonistsDictionary[_song.Player1] : defaultProtagonist;
        boyfriendAnimator.spriteAnimations = boyfriend.animations;

        boyfriendHealthIcon.sprite = boyfriend.portrait;
        boyfriendHealthIconRect.sizeDelta = boyfriend.portraitSize;

        boyfriendHealthBar.color = boyfriend.healthColor;

        deadNoise = boyfriend.deathStartSound;
        deadTheme = boyfriend.deathLoopMusic;
        deadConfirm = boyfriend.deathConfirmSound;

        boyfriendAnimator.GetComponent<SpriteRenderer>().flipX = !boyfriend.doNotFlip;

        boyfriendAnimator.Play("Idle");

        deadBoyfriendAnimator.runtimeAnimatorController = boyfriend.deathAnimator;

        CameraMovement.instance.playerOneOffset = boyfriend.cameraOffset;

        if (isDead)
        {
            isDead = false;
            respawning = false;

            deadCamera.enabled = false;

            
        }

        

        //mainCamera.enabled = true;
        uiCamera.enabled = true;
        /*
         * Now we can fully start the song in a coroutine.
         */

        if (songData.DialogueData.dialogues.Length != 0)
        {
            battleCanvas.enabled = false;
            dialogueCanvas.enabled = true;

            dialogueMusicSource.clip = songData.dialogueMusic;
            dialogueMusicSource.volume = Options.menuVolume;
            dialogueMusicSource.Play();

            foreach (Image img in dialogueImages)
            {
                Color currentColor = img.color;
                currentColor.a = 0;
                img.color = currentColor;

                LeanTween.alpha(img.rectTransform, 1, .75f).setEaseOutExpo();
            }

            currentDialogue = 0;
            var dialogue = songData.DialogueData.dialogues[currentDialogue];
            dialoguePortrait.sprite = dialogue.portrait;

            LeanTween.delayedCall(.85f, NextDialogue);
        }
        else
        {
            StartCoroutine(nameof(SongStart), startSound.length);
        }

    }

    public void NextDialogue()
    {
        if (!dialogueInProgress)
        {
            dialogueInProgress = true;

            currentDialogue = 0;
            
            
        }
        else
        {
            SongData songData = !freeplay ? songs[currentSong] : freeplaySong;
            if (currentDialogue + 1 == songData.DialogueData.dialogues.Length)
            {
                dialogueInProgress = false;
                if (dialogueProcess != null)
                    StopCoroutine(dialogueProcess);

                dialogueText.text = string.Empty;
                
                foreach (Image img in dialogueImages)
                {
                    LeanTween.alpha(img.rectTransform, 0, .75f).setEaseOutExpo().setOnComplete(() =>
                    {
                        battleCanvas.enabled = true;
                        dialogueCanvas.enabled = false;

                        dialogueMusicSource.Stop();
                        
                        StartCoroutine(nameof(SongStart), startSound.length);
                    });
                }

                return;
            }
            currentDialogue++;
        }
        
        if (dialogueProcess != null)
            StopCoroutine(dialogueProcess);

        dialogueProcess = StartCoroutine(nameof(TypewriteText));
    }

    IEnumerator TypewriteText()
    { 
        SongData songData = !freeplay ? songs[currentSong] : freeplaySong;

        dialogueText.text = string.Empty;
        var dialogue = songData.DialogueData.dialogues[currentDialogue];
        dialoguePortrait.sprite = dialogue.portrait;
        foreach (Char c in dialogue.dialog)
        {
            dialogueText.text += c;
            vocalSource.PlayOneShot(dialogueTalkSound);
            yield return new WaitForSeconds(.045f);
        }
        
    }

    IEnumerator SongStart(float delay)
    {
        /*
         * If we are in demo mode, delete any temp charts.
         */
        if (Player.autoPlay)
        {
#if !UNITY_WEBGL
            if(File.Exists(Application.persistentDataPath + "/tmp/ok.json"))
                File.Delete(Application.persistentDataPath + "/tmp/ok.json");
            if(Directory.Exists(Application.persistentDataPath + "/tmp"))
                Directory.Delete(Application.persistentDataPath + "/tmp");
#endif
        }
        
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

        if(!Options.LiteMode)
            mainCamera.orthographicSize = defaultGameZoom;
        
        /*
         * Start the beat stopwatch.
         *
         * This is used to precisely calculate when a beat happens based
         * on the BPM or BPS.
         */
        beatStopwatch = new Stopwatch();
        beatStopwatch.Start();

        stepStopwatch = new Stopwatch();
        stepStopwatch.Start();

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

    #region Pause Menu
    public void PauseSong()
    {
        if (Options.instance.isTesting)
        {
            subtitleDisplayer.StopSubtitles();
            return;
        }

        subtitleDisplayer.paused = true;
        
        stopwatch.Stop();
        beatStopwatch.Stop();
        stepStopwatch.Stop();

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
        stepStopwatch.Start();
        
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

        vocalSource.Stop();
        musicSources[1].Stop();

        PlaySong();
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
        if (enemy.idleOnly) return;
        enemyAnimator.Play(animationName);
        _currentEnemyIdleTimer = enemyIdleTimer;
    }

    private void BoyfriendPlayAnimation(string animationName)
    {
        boyfriendAnimator.Play( animationName);

        
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
                    if(Player.autoPlay)
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
        
        if (!Player.playAsEnemy || Player.twoPlayers || Player.autoPlay)
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
                $"Score: {playerOneStats.currentScore}\nAccuracy: {accuracyPercent}%\nCombo: {playerOneStats.currentCombo} ({playerOneStats.highestCombo})\nMisses: {playerOneStats.missedHits}";
        }
        else
        {
            playerOneScoringText.text = string.Empty;
        }

        if (Player.playAsEnemy || Player.twoPlayers || Player.autoPlay)
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
                $"Score: {playerTwoStats.currentScore}\nAccuracy: {accuracyPercent}%\nCombo: {playerTwoStats.currentCombo} ({playerTwoStats.highestCombo})\nMisses: {playerTwoStats.missedHits}";
        }
        else
        {
            playerTwoScoringText.text = string.Empty;
        }
    }
    
    public void NoteHit(NoteObject note)
    {
        if (note == null) return;

        onNoteHit?.Invoke(note);

        var player = note.mustHit ? 1 : 2;
    
        
        if (note.mustHit)
        {
            vocalSource.mute = false;
        }
        else
        {
            if (freeplay)
            {
                if(freeplaySong.noNikoVocals)
                {
                    vocalSource.mute = false;
                }
                else
                {
                    musicSources[1].mute = false;
                }
            }
            else
            {
                if(songs[currentSong].noNikoVocals)
                {
                    vocalSource.mute = false;
                }
                else
                {
                    musicSources[1].mute = false;
                }
            }
            
        }

        bool invertHealth = false;

        int noteType = note.type;
        switch (player)
        {
            case 1:
                if(!Player.playAsEnemy || Player.autoPlay || Player.twoPlayers)
                    invertHealth = false;
                string altAnimation = string.Empty;
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
                break;
            case 2:
                if(Player.playAsEnemy || Player.autoPlay || Player.twoPlayers)
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

        if (Player.autoPlay) modifyScore = true;

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
            
            var newRatingObject = !Options.LiteMode ? Instantiate(ratingObject) : liteRatingObjectP1;
            Vector3 ratingPos = newRatingObject.transform.position;

            if (Options.LiteMode & player == 2)
            {
                newRatingObject = liteRatingObjectP2;
                ratingPos = newRatingObject.transform.position;
            }
            
            ratingPos.y = Options.Downscroll ? 6 : 1;
            if (player == 2)
            {
                
                if (!Options.LiteMode)
                {
                    ratingPos.x = -ratingPos.x;
                }
            }
            
            newRatingObject.transform.position = ratingPos;

            var ratingObjectScript = newRatingObject.GetComponent<RatingObject>();

            if (Options.LiteMode)
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

            switch (rating)
            {
                case Rating.Sick:
                {
                    ratingObjectScript.sprite.sprite = sickSprite;

                    float healthDifference = 5;

                    if(Options.MoreHealthGain)
                    {
                        healthDifference = 10;
                    }
                    else if(Options.LessHealthGain)
                    {
                        healthDifference = 2.5f;
                    }

                    if(!Options.NoHealthGain)
                    {
                        if(!invertHealth)
                            health += healthDifference;
                        else
                            health -= healthDifference;
                    }

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

                    float healthDifference = 2;

                    if(Options.MoreHealthGain)
                    {
                        healthDifference = 4;
                    }
                    else if(Options.LessHealthGain)
                    {
                        healthDifference = 1;
                    }

                    if(!Options.NoHealthGain)
                    {
                        if(!invertHealth)
                            health += healthDifference;
                        else
                            health -= healthDifference;
                    }
                
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

                    float healthDifference = 1;

                    if(Options.MoreHealthGain)
                    {
                        healthDifference = 2;
                    }
                    else if(Options.LessHealthGain)
                    {
                        healthDifference = 0.5f;
                    }

                    if(!Options.NoHealthGain)
                    {
                        if(!invertHealth)
                            health += healthDifference;
                        else
                            health -= healthDifference;
                    }
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
                        playerOneStats.totalShits = 0;
                    }
                    else
                    {
                        playerTwoStats.currentCombo = 0;
                        playerTwoStats.totalShits = 0;
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
            }
            else
            {
                if (playerTwoStats.highestCombo < playerTwoStats.currentCombo)
                {
                    playerTwoStats.highestCombo = playerTwoStats.currentCombo;
                }
                playerTwoStats.hitNotes++;
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

        Destroy(note.gameObject);

    }

    public void NoteMiss(NoteObject note)
    {
        
        if (note.mustHit)
        {
            vocalSource.mute = true;
        }
        else
        {
            if (freeplay)
            {
                if(freeplaySong.noNikoVocals)
                {
                    vocalSource.mute = true;
                }
                else
                {
                    musicSources[1].mute = true;
                }
            }
            else
            {
                if(songs[currentSong].noNikoVocals)
                {
                    vocalSource.mute = true;
                }
                else
                {
                    musicSources[1].mute = true;
                }
            }
            
        }
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
                        BoyfriendPlayAnimation(boyfriend.hasMissAnimations ? "Sing Left Miss" : "Sing Left");
                        break;
                    case 1:
                        //Down
                        BoyfriendPlayAnimation(boyfriend.hasMissAnimations ? "Sing Down Miss" : "Sing Down");
                        break;
                    case 2:
                        //Up
                        BoyfriendPlayAnimation(boyfriend.hasMissAnimations ? "Sing Up Miss" : "Sing Up");
                        break;
                    case 3:
                        //Right
                        BoyfriendPlayAnimation(boyfriend.hasMissAnimations ? "Sing Right Miss" : "Sing Right");
                        break;
                }
                break;
            case 2:
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
            float healthDifference = 8;
            if(Options.LessHealthLoss)
            {
                healthDifference = 4;
            }
            else if(Options.MoreHealthLoss)
            {
                healthDifference = 14;
            }

            if (!invertHealth)
                health -= healthDifference;
            else
                health += healthDifference;

            if(Options.PerfectMode)
            {
                if(player == 1)
                {
                    health = 0;
                }
            }
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
            if (dialogueInProgress)
            {
                if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
                {
                    NextDialogue();
                }
            }
            if (songStarted)
            {
                if(Input.GetKeyDown(Player.resetKey))
                {
                    health = 0;
                }

                if ((float) stepStopwatch.ElapsedMilliseconds >= stepCrochet)
                {
                    stepStopwatch.Restart();
                    currentStep++;
                    onStepHit?.Invoke(currentStep);
                    
                }
                if ((float)beatStopwatch.ElapsedMilliseconds / 1000 >= beatsPerSecond)
                {
                    beatStopwatch.Restart();
                    currentBeat++;

                    onBeatHit?.Invoke(currentBeat);
                    modInstance?.Invoke("OnBeat",currentBeat);
                    if (_currentBoyfriendIdleTimer <= 0 & currentBeat % 2 == 0)
                    {
                        boyfriendAnimator.Play("Idle");
                    }

                    if (_currentEnemyIdleTimer <= 0 & currentBeat % 2 == 0)
                    {
                        enemyAnimator.Play("Idle");
                    }

                    
                    if (Options.LiteMode) return;
                    
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
                        LeanTween.value(1.25f, 1, .15f).setOnComplete(() =>
                        {
                            _portraitsZooming = false;
                        }).setOnUpdate(f =>
                        {
                            boyfriendHealthIconRect.localScale = new Vector3(-f, f, 1);
                            enemyHealthIconRect.localScale = new Vector3(f, f, 1);
                        });
                    }

                    if (!_cameraZooming)
                    {
                        if(currentBeat % 4 == 0)
                        {
                            LeanTween.value(uiCamera.gameObject, defaultZoom-.1f, defaultZoom,
                                    beatZoomTime).setOnUpdate(f => { uiCamera.orthographicSize = f; })
                                .setOnComplete(() => { _cameraZooming = false; });
                            
                            LeanTween.value(mainCamera.gameObject, defaultGameZoom-.1f, defaultGameZoom,
                                    beatZoomTime).setOnUpdate(f => { mainCamera.orthographicSize = f; })
                                .setOnComplete(() => { _cameraZooming = false; });
                        }
                    }
                }
            }
            
            if (health > MAXHealth)
                health = MAXHealth;
            if (health <= 0 & !Options.NoDeath)
            {
                health = 0;
                if(!Player.playAsEnemy & !Player.twoPlayers & !Player.autoPlay)
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
                                    PlaySong();
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
                        stepStopwatch.Reset();

                        subtitleDisplayer.StopSubtitles();
                        subtitleDisplayer.paused = false;

                        deadBoyfriend.transform.position = bfObj.transform.position;
                        deadBoyfriend.transform.localScale = bfObj.transform.localScale;

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
                boyfriendHealthIcon.sprite = boyfriend.portraitWinning; 
            } else if (healthPercent <= .20f)
            {
                enemyHealthIcon.sprite = enemy.portraitWinning;
                boyfriendHealthIcon.sprite = boyfriend.portraitDead; 
            }
            else
            {
                enemyHealthIcon.sprite = enemy.portrait;
                boyfriendHealthIcon.sprite = boyfriend.portrait; 
            }



            anchoredPosition = enemyPortraitPos;
            rectTransform.anchoredPosition = anchoredPosition;
            boyfriendHealthIcon.rectTransform.anchoredPosition = boyfriendPortraitPos;

            if (!musicSources[0].isPlaying & songStarted & !isDead & !respawning & !Pause.instance.pauseScreen.activeSelf & !Pause.instance.editingVolume)
            {
                //Song is done.

                modInstance?.Invoke("OnSongDone");

                stopwatch.Stop();
                beatStopwatch.Stop();
                stepStopwatch.Stop();

                enemyAnimator.spriteAnimations = defaultEnemy.animations;

                if (usingSubtitles)
                {
                    subtitleDisplayer.StopSubtitles();
                    subtitleDisplayer.paused = false;
                    usingSubtitles = false;

                }

                girlfriendAnimator.Play("GF Dance");
                boyfriendAnimator.Play("Idle");
                enemyAnimator.Play("Idle");



                songSetupDone = false;
                songStarted = false;
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

                if (Pause.instance.quitting)
                {
                    battleCanvas.enabled = false;
                
                    player1Notes.gameObject.SetActive(false);
                    player2Notes.gameObject.SetActive(false);

                    healthBar.SetActive(false);

                
                    menuScreen.SetActive(false);
                    ScreenTransition.instance.StartTransition(menuScreen);

                    menuCanvas.enabled = true;

                    musicSources[0].clip = menuClip;
                    musicSources[0].loop = true;
                    musicSources[0].volume = Options.menuVolume;
                    musicSources[0].Play();
                    Pause.instance.quitting = false;
                    Options.instance.ClearModifiers();
                    return;
                }
                
                if(!Player.playAsEnemy)
                {
                    overallStats.currentScore += playerOneStats.currentScore;
                    overallStats.totalSicks += playerOneStats.totalSicks;
                    overallStats.totalGoods += playerOneStats.totalGoods;
                    overallStats.totalBads += playerOneStats.totalBads;
                    overallStats.totalShits += playerOneStats.totalShits;
                    overallStats.totalNoteHits += playerOneStats.totalNoteHits;
                    overallStats.hitNotes += playerOneStats.hitNotes;
                    overallStats.missedHits += playerOneStats.missedHits;
                }
                else
                {
                    overallStats.currentScore += playerTwoStats.currentScore;
                    overallStats.totalSicks += playerTwoStats.totalSicks;
                    overallStats.totalGoods += playerTwoStats.totalGoods;
                    overallStats.totalBads += playerTwoStats.totalBads;
                    overallStats.totalShits += playerTwoStats.totalShits;
                    overallStats.totalNoteHits += playerTwoStats.totalNoteHits;
                    overallStats.hitNotes += playerTwoStats.hitNotes;
                    overallStats.missedHits += playerTwoStats.missedHits;
                }

                if (currentSong + 1 == songs.Length || freeplay)
                {
                    if(!Player.autoPlay)
                    {
                        battleCanvas.enabled = false;

                        player1Notes.gameObject.SetActive(false);
                        player2Notes.gameObject.SetActive(false);

                        healthBar.SetActive(false);


                        menuScreen.SetActive(false);
                        VictoryScreen.Instance.ResetScreen();
                        ScreenTransition.instance.StartTransition(victoryScreen);

                        menuCanvas.enabled = true;

                        musicSources[0].clip = victoryClip;
                        musicSources[0].loop = false;
                        musicSources[0].volume = Options.completedVolume;
                        musicSources[0].Play();

                        Options.instance.ClearModifiers();

                        if (weekData?.weekName.ToLower() == "week one" & freeplay == false)
                        {
                            if(!Options.NoDeath & !Options.MoreHealthGain & !Options.LessHealthLoss)
                            {
                                PlayerPrefs.SetInt("Niko Week 1 Done", 1);
                                PlayerPrefs.Save();
                                
                                Menu.instance.weekTwoButton.SetActive(true);
                                Menu.instance.weekTwoFreeplay.SetActive(true);

                                LayoutRebuilder.ForceRebuildLayoutImmediate(Menu.instance.menuSelections);
                                LayoutRebuilder.ForceRebuildLayoutImmediate(Menu.instance.freeplaySelections);
                            }
                        }
                    }
                    else
                    {
                        battleCanvas.enabled = false;
                
                        player1Notes.gameObject.SetActive(false);
                        player2Notes.gameObject.SetActive(false);

                        healthBar.SetActive(false);

                
                        menuScreen.SetActive(false);
                        ScreenTransition.instance.StartTransition(menuScreen);

                        menuCanvas.enabled = true;

                        musicSources[0].clip = menuClip;
                        musicSources[0].loop = true;
                        musicSources[0].volume = Options.menuVolume;
                        musicSources[0].Play();

                        Options.instance.ClearModifiers();

                    }
                }
                else
                {
                
                    currentSong++;
                    PlaySong();
                }
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
                /*switch (Random.Range(0, 4))
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

                _enemyRandomDanceTimer = Random.Range(.5f, 3f);*/
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
        
        if(Player.autoPlay)
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

     
        
        _currentEnemyIdleTimer -= Time.deltaTime;
        _currentBoyfriendIdleTimer -= Time.deltaTime;


    }
}
