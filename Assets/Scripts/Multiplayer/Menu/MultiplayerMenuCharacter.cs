using System.Collections;
using System.Collections.Generic;
using Mirror;
using TMPro;
using UnityEngine;

public class MultiplayerMenuCharacter : NetworkBehaviour
{
    public TMP_Text usernameText;

    [Space] public bool defaultToGuest;

    [SyncVar(hook = nameof(UsernameChange))]
    public string username;

    private void UsernameChange(string _Old, string _New)
    {
        usernameText.text = username;
    }

    public override void OnStartAuthority()
    {
        if (defaultToGuest & MultiplayerMenu.isHost) return;
        username = MultiplayerMenuNetworking.myUsername;
        usernameText.text = MultiplayerMenuNetworking.myUsername;
    }
    
    
    

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    
    
}
