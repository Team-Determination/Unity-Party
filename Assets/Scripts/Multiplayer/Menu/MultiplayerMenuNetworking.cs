using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class MultiplayerMenuNetworking : NetworkBehaviour
{
    public GameObject hostCharacter;
    private NetworkIdentity _hostCharIdentity;

    public GameObject guestCharacter;
    private NetworkIdentity _guestCharIdentity;

    public static string myUsername;
    // Start is called before the first frame update
    void Start()
    {
        _hostCharIdentity = hostCharacter.GetComponent<NetworkIdentity>();
        _guestCharIdentity = guestCharacter.GetComponent<NetworkIdentity>();
        
        
    }

    [Command]
    public void CmdTakeGuestCharacter(NetworkConnectionToClient connection)
    {
        _guestCharIdentity.AssignClientAuthority(connection);
    }
    [Command]
    public void CmdTakeHostCharacter(NetworkConnectionToClient connection)
    {
        _hostCharIdentity.AssignClientAuthority(connection);
    }
    
    // Update is called once per frame
    void Update()
    {
        
    }
}
