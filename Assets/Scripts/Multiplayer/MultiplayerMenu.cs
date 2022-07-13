using System.Collections;
using System.Collections.Generic;
using Epic.OnlineServices;
using Epic.OnlineServices.Friends;
using Epic.OnlineServices.Lobby;
using Epic.OnlineServices.UserInfo;
using EpicTransport;
using Mirror;
using ModIO;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MultiplayerMenu : MonoBehaviour
{
    private EOSLobby _lobby;
    private LobbyInterface _interface;
    private FriendsInterface _friends;
    private NetworkManager _manager;
    private UserInfoInterface _userInfo;
    private MultiplayerMenuNetworking _menuNetworking;

    public static bool isHost;


    public GameObject menuScreen;
    public GameObject waitScreen;
    public GameObject lobbyScreen;
    [Space] public Button[] menuButtons;
    [Space] public GameObject[] networkedObjects;
    [Header("Friends")] public RectTransform friendsListRect;
    public GameObject friendButtonPrefab;

    private static Dictionary<EpicAccountId, FriendsStatus> _friendsList;

    // Start is called before the first frame update
    void Start()
    {
        _lobby = FindObjectOfType<EOSLobby>();
        _lobby.CreateLobbySucceeded += OnLobbyCreated;
        _lobby.JoinLobbySucceeded += OnLobbyJoined;

        _userInfo = EOSSDKComponent.GetUserInfoInterface();

        _friends = EOSSDKComponent.GetFriendsInterface();

        _interface = EOSSDKComponent.GetLobbyInterface();
        _interface.AddNotifyLobbyInviteAccepted(new AddNotifyLobbyInviteAcceptedOptions(), null, OnLobbyInviteAccepted);

        _menuNetworking = FindObjectOfType<MultiplayerMenuNetworking>();

        _manager = NetworkManager.singleton;
    }

    private void OnLobbyJoined(List<Attribute> attributes)
    {
        if (isHost) return;

        menuScreen.SetActive(false);
        waitScreen.SetActive(true);

        _manager.networkAddress = _lobby.ConnectedLobbyDetails.GetLobbyOwner(new LobbyDetailsGetLobbyOwnerOptions())
            .ToString();

        print(attributes.Find(x => x.Data.Key == "Using ModIO").Data.Value == (AttributeDataValue)false
            ? "WE ARE NOT USING MOD.IO"
            : "We ARE USING MOD.IO");

        UpdateMultiplayerUsername();

        _manager.onStartClientEvent.AddListener(() =>
        {
            EnableNetworkedObjects();
            _menuNetworking = FindObjectOfType<MultiplayerMenuNetworking>();

            _menuNetworking.CmdTakeGuestCharacter(NetworkClient.connection as NetworkConnectionToClient);

            _menuNetworking.guestCharacter.GetComponent<MultiplayerMenuCharacter>().username =
                MultiplayerMenuNetworking.myUsername;

            _manager.onStartClientEvent.RemoveAllListeners();
        });



        _manager.StartClient();
    }

    private void OnLobbyInviteAccepted(LobbyInviteAcceptedCallbackInfo data)
    {
        _lobby.JoinLobbyByID(data.LobbyId);
    }

    public void EnableNetworkedObjects()
    {
        foreach (GameObject networkObject in networkedObjects)
        {
            networkObject.SetActive(true);
        }
    }

    private void OnLobbyCreated(List<Attribute> attributes)
    {
        _manager.onStartHostEvent.AddListener(() =>
        {
            isHost = true;
            _lobby.UpdateLobbyAttribute("Using ModIO", false);

            _manager.onStartHostEvent.RemoveAllListeners();
        });

        _manager.onStartClientEvent.AddListener(() =>
        { 
            waitScreen.SetActive(false);
            lobbyScreen.SetActive(true);
            EnableNetworkedObjects();
            _menuNetworking = FindObjectOfType<MultiplayerMenuNetworking>();

            _menuNetworking.CmdTakeHostCharacter(NetworkClient.connection as NetworkConnectionToClient);
            
            _menuNetworking.hostCharacter.GetComponent<MultiplayerMenuCharacter>().username =
                MultiplayerMenuNetworking.myUsername;
            _manager.onStartClientEvent.RemoveAllListeners();
        });
        
        _manager.StartHost();
    }
    
    

    public void CreateGame()
    {
        menuScreen.SetActive(false);
        waitScreen.SetActive(true);

        UpdateMultiplayerUsername();

        _lobby.CreateLobby(2,LobbyPermissionLevel.Inviteonly,true);
    }

    private void UpdateMultiplayerUsername()
    {
        QueryUserInfoOptions options = new QueryUserInfoOptions
        {
            LocalUserId = EOSSDKComponent.LocalUserAccountId,
            TargetUserId = EOSSDKComponent.LocalUserAccountId
        };
        _userInfo.QueryUserInfo(options, null, callback =>
        {
            CopyUserInfoOptions copyOptions = new CopyUserInfoOptions
            {
                LocalUserId = options.LocalUserId,
                TargetUserId = options.TargetUserId
            };
            _userInfo.CopyUserInfo(copyOptions, out var userInfo);

            MultiplayerMenuNetworking.myUsername = userInfo.DisplayName;
        });
    }
    

    public void LoadFriends()
    {
        if(_friendsList == null)
        {
            _friendsList = new Dictionary<EpicAccountId, FriendsStatus>();
            QueryFriendsOptions options = new QueryFriendsOptions
            {
                LocalUserId = EOSSDKComponent.LocalUserAccountId
            };
            _friends.QueryFriends(options, null,
                data =>
                {
                    _friends.AddNotifyFriendsUpdate(new AddNotifyFriendsUpdateOptions(), null, OnFriendsUpdated);
                    RefreshFriendsList();
                });
        }
        else
        {
            RefreshFriendsList();
        }
    }

    private void RefreshFriendsList()
    {
        foreach (EpicAccountId accountId in _friendsList.Keys)
        {
            if (_friendsList[accountId] == FriendsStatus.Friends)
            {
                Instantiate(friendButtonPrefab, friendsListRect);
                LayoutRebuilder.ForceRebuildLayoutImmediate(friendsListRect);

                FriendListObject friendListObject = friendButtonPrefab.GetComponent<FriendListObject>();
                
                QueryUserInfoOptions options = new QueryUserInfoOptions
                {
                    LocalUserId = EOSSDKComponent.LocalUserAccountId,
                    TargetUserId = accountId
                };
                _userInfo.QueryUserInfo(options, null, callback =>
                {
                    CopyUserInfoOptions copyOptions = new CopyUserInfoOptions
                    {
                        LocalUserId = options.LocalUserId,
                        TargetUserId = options.TargetUserId
                    };
                    _userInfo.CopyUserInfo(copyOptions, out var userInfo);

                    friendListObject.friendName.text = userInfo.DisplayName;
                });
                friendListObject.inviteButton.onClick.AddListener(() =>
                {
                    
                });
            }
        }
    }

    private void OnFriendsUpdated(OnFriendsUpdateInfo data)
    {
        if (_friendsList.ContainsKey(data.TargetUserId))
        {
            _friendsList.Remove(data.TargetUserId);
        } 
        _friendsList.Add(data.TargetUserId, data.CurrentStatus);
    }


    // Update is called once per frame
    void Update()
    {
        if (EOSSDKComponent.Initialized)
            foreach (Button btn in menuButtons)
                btn.interactable = true;
        else
            foreach (Button btn in menuButtons)
                btn.interactable = false;
    }
}
