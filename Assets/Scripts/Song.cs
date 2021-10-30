using System;
using System.CodeDom;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using FridayNightFunkin;
using Newtonsoft.Json;
using Slowsharp;
using TMPro;
using TMPro.SpriteAssetUtilities;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Networking;
using UnityEngine.UI;
using Debug = UnityEngine.Debug;
using Random = UnityEngine.Random;
// ReSharper disable IdentifierTypo
// ReSharper disable PossibleNullReferenceException

public class Song : MonoBehaviour
{
    public PreloadedSong preloadedSong;
    [Space]

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

    [Space] public bool liteMode;

    [Space] public bool hasStarted;

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
    [Space] public Camera mainCamera;
    public Camera uiCamera;
    public float beatZoomTime;
    private float _defaultZoom;
    public float defaultGameZoom;

    [Space, TextArea(2, 12)] public string jsonDir;
    public float notesOffset;
    public float noteDelay;

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
    public bool isDead = false;
    public bool respawning = false;

    

   
    
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

    [Header("Enemy")] public GameObject enemyObj;
    public Character enemy;
    public string enemyName;
    public Animator enemyAnimation;
    public float enemyIdleTimer = .3f;
    private float _currentEnemyIdleTimer;
    public float enemyNoteTimer = .25f;
    private Vector3 _enemyDefaultPos;
    private readonly float[] _currentEnemyNoteTimers = {0, 0, 0, 0};
    private readonly float[] _currentDemoNoteTimers = {0, 0, 0, 0};
    private LTDescr _enemyFloat;



    [Header("Boyfriend")] public GameObject bfObj;
    public Animator boyfriendAnimation;
    public float boyfriendIdleTimer = .3f;
    public Sprite boyfriendPortraitNormal;
    public Sprite boyfriendPortraitDead;
    private float _currentBoyfriendIdleTimer;

    private FNFSong _song;

    public static Song instance;

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
            liteMode = true;
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


        _defaultZoom = uiCamera.orthographicSize;

        
    }

    #region Song Gameplay

    public void PlaySong(bool auto, string directory = "")
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
        _highestSickCombo = 0;
        _currentSickCombo = 0;
        _hitNotes = 0;
        _totalNoteHits = 0;
        _currentScore = 0;
        _missedHits = 0;
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
        menuScreen.SetActive(true);
        songListScreen.SetActive(false);
        


        /*
         * Now we start the song setup.
         *
         * This is a Coroutine so we can make use
         * of the functions to pause it for certain time.
         */
        SetupSong();

    }

    public void SetupSong()
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
        musicClip = preloadedSong.instClip;
        vocalClip = preloadedSong.voiceClip;
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

        health = MAXHealth / 2;

        /*
         * Special thanks to KadeDev for creating the .NET FNF Song parsing library.
         *
         * With it, we can load the song as a whole class full of chart information
         * via the chart file.
         */
        string tempPath = Application.persistentDataPath + "/Temp";
        if (!Directory.Exists(tempPath))
        {
            Directory.CreateDirectory(tempPath);
        }

        var tmpJson = tempPath + "/tmp.json";
        if (File.Exists(tmpJson))
        {
            File.Delete(tmpJson);
        }

        File.Create(tmpJson).Dispose();
        File.WriteAllText(tmpJson, preloadedSong.jsonData);
        
        
        _song = new FNFSong(tmpJson);

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
                print("Sus length is " + susLength);

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
        currentScoringText.enabled = true;

        /*
         * Tells the entire script and other attached scripts that the song
         * started to play but has not fully started.
         */
        hasStarted = true;
        songStarted = false;
        
        /*
         * Start the stopwatch so that it can move the notes during the countdown.
         */
        stopwatch = new Stopwatch();
        stopwatch.Start();

        /*
         * Stops any current music playing and sets it to not loop.
         */
        musicSources[0].loop = false;
        musicSources[0].volume = Options.instVolume;
        musicSources[0].Stop();

        /*
         * Start the countdown audio.
         *
         * Unlike FNF, this does not dynamically change based on BPM.
         */
        soundSource.clip = startSound;
        soundSource.Play();

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
            enemyAnimation.runtimeAnimatorController = enemy.animator;

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

        }

        if (isDead)
        {
            isDead = false;
            respawning = false;

            deadCamera.enabled = false;

            battleCanvas.enabled = true;
        }
        mainCamera.enabled = true;
        uiCamera.enabled = true;

        var setUiCameraPos = Options.downscroll ? new Vector3(0, 7, -10) : new Vector3(0, 2, -10);
        uiCamera.transform.position = setUiCameraPos;
        var setHealthPos = Options.downscroll ? new Vector3(0, 140, 0) : new Vector3(0, -140, 0);
        healthBar.GetComponent<RectTransform>().anchoredPosition3D = setHealthPos;
        
        if (File.Exists(tmpJson))
        {
            File.Delete(tmpJson);
        }
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
        

        /*
         * Wait for the countdown to finish.
         */
        yield return new WaitForSeconds(delay);

        mainCamera.orthographicSize = 6;
        
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
        vocalSource.Play();

        songStarted = true;

        /*
         * Start subtitles.
         */
        if(usingSubtitles)
        {
            subtitleDisplayer.paused = false;
            subtitleDisplayer.StartSubtitles();
        }
        /*
         * Start the stopwatch for the song itself.
         */
        stopwatch = new Stopwatch();
        stopwatch.Start();


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

        foreach (AudioSource source in musicSources)
        {
            source.Pause();
        }

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

        vocalSource.UnPause();
        
        Pause.instance.pauseScreen.SetActive(false);
    }

    public void RestartSong()
    {
        subtitleDisplayer.StopSubtitles();
        PlaySong(false);
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
        enemyAnimation.Play(animationName,0,0);
        enemyAnimation.speed = 0;
        
        enemyAnimation.Play(animationName);
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
    
    public void NoteHit(NoteObject note)
    {
        vocalSource.mute = false;
        
        int player;

        player = note.mustHit ? 1 : 2;

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


        Rating rating;
        if(!note.susNote & modifyScore)
        {
            _totalNoteHits++;

            float yPos = note.transform.position.y;

            GameObject newRatingObject = Instantiate(ratingObject);

            Vector3 ratingPos = newRatingObject.transform.position;
            if (player == 2)
            {
                ratingPos.x = -ratingPos.x;
                newRatingObject.transform.position = ratingPos;
            }
            

            var ratingObjectScript = newRatingObject.GetComponent<RatingObject>();

            /*
             * Rating and difference calulations from FNF Week 6 update
             */
            
            float noteDiff = Math.Abs(note.strumTime - stopwatch.ElapsedMilliseconds + Player.visualOffset+Player.inputOffset);
            
            if (noteDiff > 0.9 * Player.safeZoneOffset) // way early or late
                rating = Rating.Shit;
            else if (noteDiff > .75 * Player.safeZoneOffset) // early or late
                rating = Rating.Bad;
            else if (noteDiff > .35 * Player.safeZoneOffset) // your kinda there
                rating = Rating.Good;
            else
                rating = Rating.Sick;
            
            switch (rating)
            {
                case Rating.Sick:
                {
                    ratingObjectScript.sprite.sprite = sickSprite;

                    if(!invertHealth)
                        health += 5;
                    else
                        health -= 5;
                    _currentScore += 10;
                    _currentSickCombo++;
                    break;
                }
                case Rating.Good:
                {
                    ratingObjectScript.sprite.sprite = goodSprite;

                    if (!invertHealth)
                        health += 2;
                    else
                        health -= 2;
                
                    _currentScore += 5;
                    _currentSickCombo++;
                    break;
                }
                case Rating.Bad:
                {
                    ratingObjectScript.sprite.sprite = badSprite;

                    if (!invertHealth)
                        health += 1;
                    else
                        health -= 1;

                    _currentScore += 1;
                    _currentSickCombo++;
                    break;
                }
                case Rating.Shit:
                    ratingObjectScript.sprite.sprite = shitSprite;

                    _currentSickCombo = 0;
                    break;
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
        if (player == 1)
        {
            player1NotesObjects[note.type].Remove(note);
        }
        else
        {
            player2NotesObjects[note.type].Remove(note);
        }

        Destroy(note.gameObject);

    }

    public void NoteMiss(NoteObject note)
    {
        print("MISS!!!");
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
            if (songStarted)
            {

                if ((float)beatStopwatch.ElapsedMilliseconds / 1000 >= beatsPerSecond)
                {
                    beatStopwatch.Restart();
                    currentBeat++;

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
                            LeanTween.value(uiCamera.gameObject, _defaultZoom-.1f, _defaultZoom,
                                    beatZoomTime).setOnUpdate(f => { uiCamera.orthographicSize = f; })
                                .setOnComplete(() => { _cameraZooming = false; });
                            
                            /*LeanTween.value(mainCamera.gameObject, defaultGameZoom-.1f, defaultGameZoom,
                                    beatZoomTime).setOnUpdate(f => { mainCamera.orthographicSize = f; })
                                .setOnComplete(() => { _cameraZooming = false; });*/
                        }
                    }
                }
            }
            
            if (health > MAXHealth)
                health = MAXHealth;
            if (health <= 0 || (Input.GetKeyDown(Player.resetKey) & songStarted))
            {
                health = 0;
                if(!Player.playAsEnemy & !Player.twoPlayers)
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
                                    PlaySong(false);
                                });
                            }
                        }
                    }
                    else
                    {
                        isDead = true;

                        deathBlackout.color = Color.clear;

                        foreach (AudioSource source in musicSources)
                        {
                            source.Stop();
                        }

                        vocalSource.Stop();

                        musicSources[0].PlayOneShot(deadNoise);

                        uiCamera.enabled = false;
                        mainCamera.enabled = false;
                        deadCamera.enabled = true;

                        battleCanvas.enabled = false;

                        beatStopwatch.Reset();
                        stopwatch.Reset();

                        subtitleDisplayer.StopSubtitles();
                        subtitleDisplayer.paused = false;

                        deadBoyfriend.transform.position = bfObj.transform.position;
                        deadBoyfriend.transform.localScale = bfObj.transform.localScale;

                        deadCamera.orthographicSize = mainCamera.orthographicSize;
                        deadCamera.transform.position = mainCamera.transform.position;

                        deadBoyfriendAnimator.Play("Dead Start");

                        Vector3 newPos = deadBoyfriend.transform.position;
                        newPos.y = 0;
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

                stopwatch.Stop();
                beatStopwatch.Stop();

                if (usingSubtitles)
                {
                    subtitleDisplayer.StopSubtitles();
                    subtitleDisplayer.paused = false;
                    usingSubtitles = false;

                }


                Player.demoMode = false;

                hasStarted = false;
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
        

        if (!enemyAnimation.GetCurrentAnimatorStateInfo(0).IsName("Idle"))
        {

            _currentEnemyIdleTimer -= Time.deltaTime;
            if (_currentEnemyIdleTimer <= 0)
            {
                enemyAnimation.Play("Idle");
                _currentEnemyIdleTimer = enemyIdleTimer;
            }
        }

        if (!boyfriendAnimation.GetCurrentAnimatorStateInfo(0).IsName("BF Idle"))
        {

            _currentBoyfriendIdleTimer -= Time.deltaTime;
            if (_currentBoyfriendIdleTimer <= 0)
            {
                boyfriendAnimation.Play("BF Idle");
                _currentBoyfriendIdleTimer = boyfriendIdleTimer;
            }
        }

        
    }
}
