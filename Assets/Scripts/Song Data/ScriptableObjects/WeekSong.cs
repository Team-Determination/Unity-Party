using UnityEngine;
using UnityEngine.Video;

[CreateAssetMenu(fileName = "New Song",menuName = "Create New Song")]
public class WeekSong : ScriptableObject
{
    public string songName;
    public string sceneName;
    
    [Space]
    public TextAsset chart;
    public AudioClip instrumentals;
    public AudioClip vocals;

    [Space] public VideoClip cutscene;

}