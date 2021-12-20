using UnityEngine;
[CreateAssetMenu(menuName = "Create New Song", fileName = "Song")]
public class SongData : ScriptableObject
{
    [TextArea(4,6)]
    public string normalData;
    [TextArea(4,6)]

    public string hardData;

    [Space] public AudioClip instrumentals;
    public AudioClip vocals;
    public AudioClip nikoVocals;

    public string sceneName;
}
