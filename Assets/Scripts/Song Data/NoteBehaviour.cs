using System.Collections;
using System.Collections.Generic;
using FridayNightFunkin;
using UnityEngine;

public class NoteBehaviour
{
    public FNFSong.FNFSection section;
    public FNFSong.FNFNote noteData;
    List<decimal> note;
    public int count;

    public NoteBehaviour( FNFSong.FNFSection section, FNFSong.FNFNote noteData ) {
        this.section = section;
        this.noteData = noteData;
        note = noteData.ConvertToNote( );
    }

    public void GenerateNote( ) {
        if ( Song.instance.stopwatch.ElapsedMilliseconds >= (float)note[ 0 ] - 2000 && count < 1 ) {
            Song.instance.GenNote( section, note );
            count++;
        }
    }
}






















//raonyreis13 :)
