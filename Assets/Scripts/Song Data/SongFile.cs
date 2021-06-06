using UnityEngine;
using UnityEngine.SceneManagement;


[CreateAssetMenu(fileName = "Song", menuName = "Create New Song", order = 0)]
    public class SongFile : ScriptableObject
    {
        [TextArea(5,8)]
        public string songJsonEasy;
        [TextArea(5,8)]
        public string songJsonNormal;
        [TextArea(5,8)]
        public string songJsonHard;

        [Space]
        public AudioClip instrumentalClip;
        public AudioClip boyfriendClip;
        public AudioClip cyeClip;

        [Space] public string backgroundScene;

    }
