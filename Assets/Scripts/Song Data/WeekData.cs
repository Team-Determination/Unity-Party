using UnityEngine;
[CreateAssetMenu(menuName = "Create New Week", fileName = "Week")]
public class WeekData : ScriptableObject
{
    public string weekName;
    public SongData[] songs;
}
