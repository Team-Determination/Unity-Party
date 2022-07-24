using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MultiplayerButton : MonoBehaviour
{
    public void GoToMultiplayer(string args) {
        LoadingTransition.instance.Show(() =>
            SceneManager.LoadScene("Online")
        );
    }
}
