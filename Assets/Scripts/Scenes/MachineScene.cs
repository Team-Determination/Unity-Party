using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class MachineScene : MonoBehaviour
{
    public List<SpriteRenderer> screens;
    public SpriteRenderer currentScreen;
    public int endOfIndex;

    public bool faded;
    public Song song;
    private void OnEnable()
    {
        song = Song.instance;
        song.onBeatHit.AddListener(BeatHit);
        song.onNoteHit.AddListener(NoteHit);
        endOfIndex = screens.Count - 1;
        faded = false;

        song.girlfriendObject.SetActive(false);
    }

    private void OnDisable()
    {
        song.onBeatHit.RemoveListener(BeatHit);
        song.onNoteHit.RemoveListener(NoteHit);
        song.enemyObj.GetComponent<SpriteRenderer>().color = Color.white;
        song.enemyHealthIcon.color = Color.white;
        if(!Options.LiteMode)
            song.girlfriendObject.SetActive(true);

    }

    public void NoteHit(NoteObject note)
    {
        if(Player.playAsEnemy)
        {
            if(note.mustHit)
            {
                if(song.health + 5 > 101)
                {
                    song.health = 100;
                }
                else
                {
                    song.health += 5;
                }
            }
        }
        else
        {
            //Play as Protagonist
            if(!note.mustHit)
            {
                if(song.health - 5 <= 1)
                {
                    song.health = 2;
                }
                else
                {
                    song.health -= 5;
                }
            }
        }
    }

    private void BeatHit(int currentBeat)
    {
        if (currentBeat % 32 == 0)
        {
            LeanTween.value(currentScreen.gameObject, currentScreen.color, Color.clear, 1).setOnUpdate(val => currentScreen.color = val);

            screens.Remove(currentScreen);
            screens.Insert(endOfIndex, currentScreen);

            currentScreen = screens[Random.Range(0, endOfIndex)];
            
            LeanTween.value(currentScreen.gameObject, currentScreen.color, Color.white, 1).setOnUpdate(val => currentScreen.color = val);
        }
        
    }
    
    
}
