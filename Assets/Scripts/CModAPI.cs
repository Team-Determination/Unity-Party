using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CModAPI
{
    public static int Remainder(int first, int second)
    {
        return first % second;
    }
    public static float Remainder(float first, float second)
    {
        return first % second;
    }

    public static Color RandomColor(bool randomAlpha)
    {
        Color randomColor = Random.ColorHSV();
        if (!randomAlpha) randomColor.a = 1;
        return randomColor;
    }

    public static void ResetPlayerNotes()
    {
        Options.instance.LoadNotePrefs();

        Song.instance.player1NoteSprites[0].transform.position = new Vector3(-2.7f, 0, -10);
        Song.instance.player2NoteSprites[0].transform.position = new Vector3(-2.7f, 0, -10);
        
        Song.instance.player1NoteSprites[1].transform.position = new Vector3(-2.7f, 0, -10);
        Song.instance.player2NoteSprites[1].transform.position = new Vector3(-2.7f, 0, -10);
        
        Song.instance.player1NoteSprites[3].transform.position = new Vector3(2.7f, 0, -10);
        Song.instance.player2NoteSprites[3].transform.position = new Vector3(2.7f, 0, -10);
    }
}
