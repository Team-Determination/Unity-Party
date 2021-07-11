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
using TMPro;
using TMPro.SpriteAssetUtilities;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using Debug = UnityEngine.Debug;
using Random = UnityEngine.Random;
// ReSharper disable IdentifierTypo

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
    [Space] public Camera mainCamera;
    public Camera uiCamera;
    public float beatZoomTime;
    private float _defaultZoom;
    public float defaultGameZoom;

    [Space, TextArea(2, 12)] public string jsonDir;
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

    [Header("Bundle Downloader")] public GameObject downloadWindow;
    public GameObject bundleUrlPrompt;
    public GameObject downloadingBundlePrompt;
    public GameObject extractingBundlePrompt;
    public GameObject bundleDownloadFailPrompt;
    public GameObject bundleIncompatiblePrompt;
    public GameObject bundleExtractFailPrompt;

    [Header("Volume Testing")] public AudioClip testVoices;
    public AudioClip testInst;
    public string testData;
    public Slider voiceVolumeSlider;
    public Slider instVolumeSlider;
    public GameObject saveTooltip;
    private bool _isTesting;
    [Space] public GameObject menuScreen;

    [Header("Death Mechanic")] public Camera deadCamera;
    public GameObject deadBoyfriend;
    public Animator deadBoyfriendAnimator;
    public AudioClip deadNoise;
    public AudioClip deadTheme;
    public AudioClip deadConfirm;
    public Image deathBlackout;
    private bool _isDead = false;
    private bool _respawning = false;

    [Header("Keybinding")] public TMP_Text primaryLeftKeybindText;
    public TMP_Text primaryDownKeybindText;
    public TMP_Text primaryUpKeybindText;
    public TMP_Text primaryRightKeybindText;
    public TMP_Text secondaryLeftKeybindText;
    public TMP_Text secondaryDownKeybindText;
    public TMP_Text secondaryUpKeybindText;
    public TMP_Text secondaryRightKeybindText;
    private KeybindSet _currentKeybindSet;
    private bool _settingKeybind;

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

    [Header("Pause")] public GameObject pauseScreen;
    private float _currentInterval;
    
    [Space] public Transform player1Notes;
    public List<List<NoteObject>> player1NotesObjects;
    public Animator[] player1NotesAnimators;
    public Transform player1Left;
    public Transform player1Down;
    public Transform player1Up;
    public Transform player1Right;
    [Space] public Transform player2Notes;
    public List<List<NoteObject>> player2NotesObjects;
    public Animator[] player2NotesAnimators;
    public Transform player2Left;
    public Transform player2Down;
    public Transform player2Up;
    public Transform player2Right;

    [Header("Prefabs")] public GameObject leftArrow;
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

    public string[] characterNames;
    public Character[] characters;
    public Dictionary<string, Character> charactersDictionary;

    [Header("Enemy")] public GameObject enemyObj;
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

    private string _songsFolder;
    private string _selectedSongDir;

    [HideInInspector] public SongListObject selectedSong;


    public bool songStarted;

    private bool _onlineMode;

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
        _songsFolder = Application.persistentDataPath + "/Songs";

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
         * This makes the "theme" song (which is Breakfast via the FNF' OST)
         * play and makes it loop.
         */
        musicSources[0].clip = menuClip;
        musicSources[0].Play();
        musicSources[0].loop = true;

        /*
         * Grabs the player's saved values for the volume of both the
         * and the voices then sets it for the Audio Sources respectively.
         * It also sets the UI Slider values to the saved values.
         *
         * If there are no saved values, it sets it to 75% by default.
         *
         * PlayerPrefs allows you to save/load a user preference.
         * Although, it's limited as it can only save strings, ints, and floats.
         */
        musicSources[0].volume = PlayerPrefs.GetFloat("Music Volume", .75f);
        instVolumeSlider.value = PlayerPrefs.GetFloat("Music Volume", .75f);

        vocalSource.volume = PlayerPrefs.GetFloat("Voice Volume", .75f);
        voiceVolumeSlider.value = PlayerPrefs.GetFloat("Voice Volume", .75f);

        /*
         * This is something I am not proud of and I am pretty sure
         * there's a possible way to make it better, but this is what
         * I came up with and it works so fuck it.
         *
         * It grabs a saved JSON string and tries to convert it from JSON to a SavedKeybinds class.
         * If there's no JSON string, it'll be empty so the game will auto-generate and save
         * a default SavedKeybinds class JSON value.
         */
        string keys = PlayerPrefs.GetString("Saved Keybinds", String.Empty);

        SavedKeybinds savedKeybinds = new SavedKeybinds();

        if (!string.IsNullOrWhiteSpace(keys))
        {
            savedKeybinds = JsonConvert.DeserializeObject<SavedKeybinds>(keys);
        }
        else
        {
            PlayerPrefs.SetString("Saved Keybinds", JsonConvert.SerializeObject(savedKeybinds));
            PlayerPrefs.Save();
        }

        /*
         * It will then take each keybind in the referenced SavedKeybinds class
         * and assign them to the Player class variables respectively.
         *
         * We will also update the text in the Game Options for the KeyBinds.
         */
        Player.leftArrowKey = savedKeybinds.primaryLeftKeyCode;
        Player.downArrowKey = savedKeybinds.primaryDownKeyCode;
        Player.upArrowKey = savedKeybinds.primaryUpKeyCode;
        Player.rightArrowKey = savedKeybinds.primaryRightKeyCode;
        Player.secLeftArrowKey = savedKeybinds.secondaryLeftKeyCode;
        Player.secDownArrowKey = savedKeybinds.secondaryDownKeyCode;
        Player.secUpArrowKey = savedKeybinds.secondaryUpKeyCode;
        Player.secRightArrowKey = savedKeybinds.secondaryRightKeyCode;

        primaryLeftKeybindText.text = "LEFT\n" + savedKeybinds.primaryLeftKeyCode;
        primaryDownKeybindText.text = "DOWN\n" + savedKeybinds.primaryDownKeyCode;
        primaryUpKeybindText.text = "UP\n" + savedKeybinds.primaryUpKeyCode;
        primaryRightKeybindText.text = "RIGHT\n" + savedKeybinds.primaryRightKeyCode;
        secondaryLeftKeybindText.text = "LEFT\n" + savedKeybinds.secondaryLeftKeyCode;
        secondaryDownKeybindText.text = "DOWN\n" + savedKeybinds.secondaryDownKeyCode;
        secondaryUpKeybindText.text = "UP\n" + savedKeybinds.secondaryUpKeyCode;
        secondaryRightKeybindText.text = "RIGHT\n" + savedKeybinds.secondaryRightKeyCode;

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

    #region Menu

    /*
     * This below here are remnants of an attempt to add an
     * online multiplayer functionality into this game.
     *
     * As you can tell already, there was never a multiplayer mode.
     */
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

    public void PlaySong(bool auto)
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
         * Grabs the current song's directory and saves it to a variable.
         *
         * We'll then use it to grab the chart file.
         */
        _selectedSongDir = selectedSong.directory;
        jsonDir = _selectedSongDir + "/Chart.json";

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
        chooseSongMsg.SetActive(true);
        songDetails.SetActive(false);


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
        WWW www1 = new WWW(_selectedSongDir + "/Inst.ogg");
        if (www1.error != null)
        {
            Debug.LogError(www1.error);
        }
        else
        {
            musicClip = www1.GetAudioClip();
            while (musicClip.loadState != AudioDataLoadState.Loaded)
                yield return new WaitForSeconds(0.1f);
            WWW www2 = new WWW(_selectedSongDir + "/Voices.ogg");
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

        /*
         * If the player is playing in demo mode, disable the scoring.
         * Otherwise, enable the health bar and the scoring.
         */
        if (!Player.demoMode)
        {
            healthBar.SetActive(true);
            currentScoringText.enabled = true;
        }
        else
        {
            currentScoringText.enabled = false;
        }

        /*
         * Tells the entire script and other attached scripts that the song
         * started to play but has not fully started.
         */
        hasStarted = true;
        songStarted = false;

        /*
         * Stops any current music playing and sets it to not loop.
         */
        musicSources[0].loop = false;
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
        if (charactersDictionary.ContainsKey(_song.Player2))
        {
            Character enemy = charactersDictionary[_song.Player2];
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

            CameraMovement.instance.playerTwoOffset = enemy.offset;
        }

        if (_isDead)
        {
            _isDead = false;
            _respawning = false;

            deadCamera.enabled = false;

            
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

        /*
         * Wait for the countdown to finish.
         */
        yield return new WaitForSeconds(delay);

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

        notesObject.isActive = true;

        /*
         * Start the stopwatch for the song itself.
         */
        stopwatch = new Stopwatch();
        stopwatch.Start();


    }

    public void PauseSong()
    {
        print(_isTesting);
        if (_isTesting) return;
        
        stopwatch.Stop();
        beatStopwatch.Stop();

        foreach (AudioSource source in musicSources)
        {
            source.Pause();
        }

        vocalSource.Pause();
        
        uiCamera.enabled = false;
        
        pauseScreen.SetActive(true);
    }

    public void ContinueSong()
    {
        stopwatch.Start();
        beatStopwatch.Start();

        foreach (AudioSource source in musicSources)
        {
            source.UnPause();
        }

        vocalSource.UnPause();
        
        uiCamera.enabled = true;
        
        pauseScreen.SetActive(false);
    }

    public void RestartSong()
    {
        PlaySong(false);
        pauseScreen.SetActive(false);
    }

    public void QuitSong()
    {
        ContinueSong();
        foreach (AudioSource source in musicSources)
        {
            source.Stop();
        }

        vocalSource.Stop();
    }

    #endregion

    #region Game Options

    public void TestVolume()
    {
        Player.demoMode = true;


        Directory.CreateDirectory(Application.persistentDataPath + "/tmp");

        StreamWriter testFile = File.CreateText(Application.persistentDataPath + "/tmp/ok.json");
        testFile.Write(testData);
        testFile.Close();

        vocalClip = testVoices;
        musicClip = testInst;

        voiceVolumeSlider.transform.parent.gameObject.SetActive(true);
        instVolumeSlider.transform.parent.gameObject.SetActive(true);
        saveTooltip.SetActive(true);

        jsonDir = Application.persistentDataPath + "/tmp/ok.json";
        GenerateSong();

        _isTesting = true;
    }

    public enum KeybindSet
    {
        PrimaryLeft = 1,
        PrimaryDown = 2,
        PrimaryUp = 3,
        PrimaryRight = 4,
        SecondaryLeft = 5,
        SecondaryDown = 6,
        SecondaryUp = 7,
        SecondaryRight = 8
    }

    public void ChangeKeybind(int key)
    {
        KeybindSet keybind = (KeybindSet) Enum.ToObject(typeof(KeybindSet), key);

        _currentKeybindSet = keybind;
        _settingKeybind = true;

        switch (keybind)
        {
            case KeybindSet.PrimaryLeft:
                primaryLeftKeybindText.text = "LEFT\nPress a Key";
                break;
            case KeybindSet.PrimaryDown:
                primaryDownKeybindText.text = "DOWN\nPress a Key";
                break;
            case KeybindSet.PrimaryUp:
                primaryUpKeybindText.text = "UP\nPress a Key";
                break;
            case KeybindSet.PrimaryRight:
                primaryRightKeybindText.text = "RIGHT\nPress a Key";
                break;
            case KeybindSet.SecondaryLeft:
                secondaryLeftKeybindText.text = "LEFT\nPress a Key";
                break;
            case KeybindSet.SecondaryDown:
                secondaryDownKeybindText.text = "DOWN\nPress a Key";
                break;
            case KeybindSet.SecondaryUp:
                secondaryUpKeybindText.text = "UP\nPress a Key";
                break;
            case KeybindSet.SecondaryRight:
                secondaryRightKeybindText.text = "RIGHT\nPress a Key";
                break;
        }
    }

    #endregion

    public void QuitGame()
    {
        Application.Quit();
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
                string newBundlePath = _songsFolder + "/" + meta.bundleName;
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

        if (!Directory.Exists(_songsFolder + "/Imported"))
        {
            Directory.CreateDirectory(_songsFolder + "/Imported");
        }

        if (!File.Exists(_songsFolder + "/Imported/bundle-meta.json"))
        {
            BundleMeta bundleMeta = new BundleMeta {bundleName = "Imported Songs", authorName = Environment.UserName};
            StreamWriter metaFile = File.CreateText(_songsFolder + "/Imported/bundle-meta.json");
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
            string newFolder = _songsFolder + "/Imported/" + discoveredSong.info.songName + " ("+difficulty+")";
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

                if (animName == "Activated" & Player.demoMode)
                {
                    _currentDemoNoteTimers[type] = enemyNoteTimer;
                }

                break;
            case 2: //Opponent
                
                player2NotesAnimators[type].Play(animName,0,0);
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

        CameraMovement.instance.focusOnPlayerOne = tmpObj.layer == 1;

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

            

            if (songStarted)
            {
                if (Input.GetKeyDown(KeyCode.Return) & !Player.demoMode & !_isDead)
                {
                    if(!pauseScreen.activeSelf)
                        PauseSong();
                }
                
                if (Player.demoMode)
                {
                    if(_isTesting)
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
                            saveTooltip.SetActive(false);

                            Player.demoMode = false;

                            _isTesting = false;
                        }
                    }
                    else
                    {
                        if (Input.GetKeyDown(KeyCode.Return))
                        {
                            QuitSong();
                        }
                    }
                }
                
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
                            
                            LeanTween.value(mainCamera.gameObject, defaultGameZoom-.1f, defaultGameZoom,
                                    beatZoomTime).setOnUpdate(f => { mainCamera.orthographicSize = f; })
                                .setOnComplete(() => { _cameraZooming = false; });
                        }
                    }
                }
            }
            
            if (health > MAXHealth)
                health = MAXHealth;
            if (health <= 0)
            {
                if (_isDead)
                {
                    if(!_respawning)
                    {
                        if (Input.GetKeyDown(KeyCode.KeypadEnter) || Input.GetKeyDown(KeyCode.Return))
                        {
                            musicSources[0].Stop();
                            _respawning = true;

                            deadBoyfriendAnimator.Play("Dead Confirm");

                            musicSources[0].PlayOneShot(deadConfirm);
                            
                            deathBlackout.rectTransform.LeanAlpha(1, 3).setDelay(1).setOnComplete(() =>
                            {
                                PlaySong(false);
                            });
                        }
                    }
                } else
                {
                    _isDead = true;

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

                    beatStopwatch.Reset();
                    stopwatch.Reset();

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
                        if (!_respawning)
                        {
                            musicSources[0].clip = deadTheme;
                            musicSources[0].loop = true;
                            musicSources[0].Play();
                            deadBoyfriendAnimator.Play("Dead Loop");
                        }
                    });
                }
            }

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

            if (!musicSources[0].isPlaying & songStarted & !_isDead & !_respawning & !pauseScreen.activeSelf)
            {
                //Song is done.

                stopwatch.Stop();
                beatStopwatch.Stop();

                Player.demoMode = false;
                
                voiceVolumeSlider.transform.parent.gameObject.SetActive(false);
                instVolumeSlider.transform.parent.gameObject.SetActive(false);
                saveTooltip.SetActive(false);

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

        if (_settingKeybind)
        {
            if (!Input.anyKeyDown) return;

            KeyCode newKey = KeyCode.A;
            
            foreach(KeyCode kcode in Enum.GetValues(typeof(KeyCode)))
            {
                if (Input.GetKeyDown(kcode))
                {
                    newKey = kcode;
                    break;
                }
            }
            
            switch (_currentKeybindSet)
            {
                case KeybindSet.PrimaryLeft:
                    primaryLeftKeybindText.text = "LEFT\n" + newKey;
                    Player.leftArrowKey = newKey;
                    break;
                case KeybindSet.PrimaryDown:
                    primaryDownKeybindText.text = "DOWN\n" + newKey;
                    Player.downArrowKey = newKey;
                    break;
                case KeybindSet.PrimaryUp:
                    primaryUpKeybindText.text = "UP\n" + newKey;
                    Player.upArrowKey = newKey;
                    break;
                case KeybindSet.PrimaryRight:
                    primaryRightKeybindText.text = "RIGHT\n" + newKey;
                    Player.rightArrowKey = newKey;
                    break;
                case KeybindSet.SecondaryLeft:
                    secondaryLeftKeybindText.text = "LEFT\n" + newKey;
                    Player.secLeftArrowKey = newKey;
                    break;
                case KeybindSet.SecondaryDown:
                    secondaryDownKeybindText.text = "DOWN\n" + newKey;
                    Player.secDownArrowKey = newKey;
                    break;
                case KeybindSet.SecondaryUp:
                    secondaryUpKeybindText.text = "UP\n" + newKey;
                    Player.secUpArrowKey = newKey;
                    break;
                case KeybindSet.SecondaryRight:
                    secondaryRightKeybindText.text = "RIGHT\n" + newKey;
                    Player.secRightArrowKey = newKey;
                    break;
            }

            Player.SaveKeySet();
            _settingKeybind = false;
        }
        
    }
}
