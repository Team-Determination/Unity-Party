using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(menuName = "Create Week",fileName = "New Week")]
public class WeekData : ScriptableObject
{
    public SongData[] songs;
}
