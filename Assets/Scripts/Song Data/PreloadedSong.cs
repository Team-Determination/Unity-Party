using UnityEngine;
[CreateAssetMenu(menuName =  "Make Preload Song",fileName = "New Song")]
public class PreloadedSong : ScriptableObject
{
    [TextArea(4, 6)] public string jsonData;
    public AudioClip instClip;
    public AudioClip voiceClip;
}
