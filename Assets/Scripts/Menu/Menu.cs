using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Menu : MonoBehaviour
{
    public bool blockAllInputs = false;
    
    [Header("Background")] public Image backgroundImage;
    public AudioSource musicSource;
    public Sprite dayBackground;
    public AudioClip dayMusic;
    public Sprite nightBackground;
    public AudioClip nightMusic;
    
    [Header("Main Menu Selections")]
    public List<string> selections;
    public int currentIndex;
    public Intro intro;

    [Space] public TMP_Text prevSelection;
    public TMP_Text currSelection;
    public TMP_Text nextSelection;
    private bool _changing = false;

    [Space] public RectTransform selectionContainerRectTransform;
    public RectTransform selectionsRectTransform;
    public float selectionsTransitionTime;
    public RectTransform selectionsGroup;

    [Header("Story Mode")] public GameObject storyModeMenu; 
    public GameObject[] storyModeSelections;
    public float storyModeSelectionXOffset;
    public float storyModeSelectionYOffset;
    public int currentStoryModeIndex;
    public string selectedWeek;
    public int difficulty = 3;

    [Space] public SongFile[] tutorialSongFiles;
    public SongFile[] week1SongFiles;
    public SongFile[] week1RemixedSongFiles;
    
    // Start is called before the first frame update
    void Start()
    {
        
        if (DateTime.Now.Hour < 7 || DateTime.Now.Hour > 19)
        {
            musicSource.clip = nightMusic;
            backgroundImage.sprite = nightBackground;
        }
        else
        {
            musicSource.clip = dayMusic;
            backgroundImage.sprite = dayBackground;
        }
        
        musicSource.Play();
        musicSource.volume = PlayerPrefs.GetFloat("Music Volume", 1f);

        foreach (GameObject week in storyModeSelections)
        {
            TMP_Text theText = week.GetComponent<TMP_Text>();

            if (theText.text.ToLower() == "tutorial") continue;

            if (PlayerPrefs.GetString($"{theText.text} Unlocked", "false") != "true")
            {
                theText.text += " <sprite=\"Text_UI\" name=\"Lock\">";
            }
        }
    }

    public void ChangeSelection(bool next)
    {
        if (_changing)
            return;
        _changing = true;
        //Next = +1
        //!Next = -1

        if(next)
        {
            string selection = selections[0];
            selections.RemoveAt(0);
            selections.Add(selection);
        } else {
            string selection = selections[selections.Count - 1];
            selections.RemoveAt(selections.Count - 1);
            selections.Insert(0,selection);
        }

        LeanTween.moveY(selectionContainerRectTransform, -76, selectionsTransitionTime).setEaseInExpo().setOnComplete(
            () =>
            {
                prevSelection.text = selections[selections.Count - 1];
                currSelection.text = selections[0];
                nextSelection.text = selections[1];
                LayoutRebuilder.ForceRebuildLayoutImmediate(selectionsGroup);
                LeanTween.moveY(selectionContainerRectTransform, 0, selectionsTransitionTime).setOnComplete(() =>
                {
                    _changing = false;
                });
            });
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Minus))
        {
            float musVol = PlayerPrefs.GetFloat("Music Volume", 1);

            musVol -= .1f;
            if (musVol < 0)
                musVol = 0;

            PlayerPrefs.SetFloat("Music Volume", musVol);
            PlayerPrefs.Save();

            print($"Setting volume to {musVol}");

            musicSource.volume = musVol;
        } else if (Input.GetKeyDown(KeyCode.Equals))
        {
            float musVol = PlayerPrefs.GetFloat("Music Volume", 1);

            musVol += .1f;
            if (musVol > 1)
                musVol = 1;

            PlayerPrefs.SetFloat("Music Volume", musVol);
            PlayerPrefs.Save();

            print($"Setting volume to {musVol}");
            musicSource.volume = musVol;
        }
        
        if (blockAllInputs)
            return;
        
        
        if (intro.selections.activeSelf)
        {
            if (Input.GetKeyDown(KeyCode.RightArrow))
                ChangeSelection(true);
            else if (Input.GetKeyDown(KeyCode.LeftArrow))
                ChangeSelection(false);
            else if (Input.GetKeyDown(KeyCode.Return))
            {
                blockAllInputs = true;
                LeanTween.moveY(selectionsRectTransform, -50f, .25f).setEaseInExpo().setOnComplete(() =>
                {
                    intro.selections.SetActive(false);
                    switch (currSelection.text.ToLower())
                    {
                        case "story mode":
                            storyModeMenu.SetActive(true);
                            UpdateStoryModeSelections();
                            LeanTween.moveX(storyModeMenu.GetComponent<RectTransform>(), -275, 0);
                            LeanTween.moveX(storyModeMenu.GetComponent<RectTransform>(), 0, 2f).setEaseInExpo().setOnComplete(
                                () =>
                                {
                                    blockAllInputs = false;
                                });
                            break;
                            
                    }
                });
                
            }
        }

        if (storyModeMenu.activeSelf)
        {
            if (Input.GetKeyDown(KeyCode.UpArrow))
            {
                print("Story mode Up Arrow");
                currentStoryModeIndex--;
                if (currentStoryModeIndex > storyModeSelections.Length - 1) currentStoryModeIndex = 0;
                else if (currentStoryModeIndex < 0) currentStoryModeIndex = storyModeSelections.Length - 1;

                UpdateStoryModeSelections();

            } else if (Input.GetKeyDown(KeyCode.DownArrow))
            {
                print("Story mode Down Arrow");
                currentStoryModeIndex++;
                if (currentStoryModeIndex > storyModeSelections.Length - 1) currentStoryModeIndex = 0;
                else if (currentStoryModeIndex < 0) currentStoryModeIndex = storyModeSelections.Length - 1;

                UpdateStoryModeSelections();
            } else if (Input.GetKeyDown(KeyCode.Return))
            {
                if (!selectedWeek.ToLower().Contains("lock"))
                {
                    
                    Song.difficulty = difficulty;
                    Song.freePlay = false;
                    
                    switch (selectedWeek.ToLower())
                    {
                        case "tutorial":
                            Song.songFiles = tutorialSongFiles.ToList();
                            Song.week = 0;
                            SceneManager.LoadScene("Tutorial");
                            break;
                        case "week 1":
                            Song.songFiles = week1SongFiles.ToList();
                            Song.week = 1;
                            SceneManager.LoadScene("Forest");
                            break;
                    }


                }
            }
        }
    }

    

    public void UpdateStoryModeSelections()
    {
        int location = 0;

        selectedWeek = storyModeSelections[currentStoryModeIndex].GetComponent<TMP_Text>().text;
                
        foreach (GameObject gObject in storyModeSelections)
        {
            RectTransform gTransform = gObject.GetComponent<RectTransform>();
            int position = location - currentStoryModeIndex;
            print("Position is " + position);

            float xOffset = storyModeSelectionXOffset;

            if (location > currentStoryModeIndex)
            {
                xOffset *= -1;
            }

            LeanTween.moveY(gTransform, -storyModeSelectionYOffset * position, .1f);
            LeanTween.moveX(gTransform, xOffset * position, .1f);
            location++;
        }
    }
}
