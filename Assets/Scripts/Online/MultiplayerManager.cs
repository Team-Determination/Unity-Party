using Photon.Pun;
using Photon.Realtime;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Online
{
    public class MultiplayerManager : MonoBehaviourPunCallbacks
    {
        private static readonly string[] AmericanServers =
        {
            "a22378db-eb7c-4e1a-9dca-4e4aa3023495",
            "1f8632bb-c4a8-4ea4-a14c-540eaff86dee",
            "66890387-b62c-435c-9737-f59061901707",
            "9208d99a-bfbf-4aa5-a7c8-8b2cef0165f6",
            "94999d11-71e8-4177-a7d2-952e0e88ae5f"
        };

        private static readonly string[] EuropeanServers =
        {
            "b8a3864a-ca5c-47c0-be9a-456afccd9406",
            "48a5eb95-cfd8-4e40-8124-51aecfae960f",
            "61aef01e-5634-4877-9f3b-970cdcd10460",
            "d65095dd-9e3c-4ea1-abed-7d3446d34166",
            "589385ee-4994-4676-97eb-1862193202ed"
        };

        public int selectedServer;

        [Header("Screens")] public GameObject connectingScreen;
        public GameObject mainScreen;
        public GameObject lobbyScreen;

        [Header("Main Menu")] public TMP_Text serverConnectedText;

        [Header("Lobby")] public TMP_Text boyfriendNameText;
        public TMP_Text opponentNameText;
        [Space] public TMP_Text chatText;
        [TextArea(3, 10)] public string defaultChatText;
        [Space] public Button changeSongButton;

        [Header("Gameplay")] public Camera gameplayCamera;
        public Canvas gameplayCanvas;
        [Space]
        public RectTransform boyfriendHealthIconRect;
        public Image boyfriendHealthBar;
        public RectTransform enemyHealthIconRect;
        public Image enemyHealthBar;
        
        //MULTIPLAYER VARIABLES
        private Photon.Realtime.Player boyfriendPlayer;
        private Photon.Realtime.Player opponentPlayer; 
        
        // Start is called before the first frame update
        void Start()
        {
            gameplayCanvas.enabled = false;
            gameplayCamera.enabled = false;
            mainScreen.SetActive(false);
            connectingScreen.SetActive(true);

            string region = PlayerPrefs.GetString("Multiplayer Region");

            if (string.IsNullOrWhiteSpace(region))
            {               
                region = "us";

                PlayerPrefs.SetString("Multiplayer Region", region);
                PlayerPrefs.Save();
            } 
            int server = PlayerPrefs.GetInt("Multiplayer Server",0);

            PhotonNetwork.PhotonServerSettings.AppSettings.AppIdRealtime = region == "us" ? AmericanServers[server] : EuropeanServers[server];
            PhotonNetwork.PhotonServerSettings.AppSettings.AppVersion = "1.0";
            PhotonNetwork.PhotonServerSettings.AppSettings.FixedRegion = region;

            PhotonNetwork.ConnectUsingSettings();
        }

        public override void OnConnectedToMaster()
        {
            mainScreen.SetActive(true);
            connectingScreen.SetActive(false);
        }

        public void CreateMatch()
        {
            RoomOptions options = new RoomOptions {MaxPlayers = 2};

            PhotonNetwork.NickName = "Master Client";
            
            PhotonNetwork.CreateRoom(null, options);
            
            connectingScreen.SetActive(true);
            mainScreen.SetActive(false);
            
            
        }

        public override void OnJoinedRoom()
        {
            connectingScreen.SetActive(false);
            lobbyScreen.SetActive(true);

            chatText.text = defaultChatText;

            boyfriendPlayer = PhotonNetwork.MasterClient;

            if (!PhotonNetwork.IsMasterClient)
            {
                changeSongButton.interactable = false;
                opponentPlayer = PhotonNetwork.LocalPlayer;

                opponentNameText.text = opponentPlayer.NickName;
            }
            
        }

        public override void OnPlayerEnteredRoom(Photon.Realtime.Player newPlayer)
        {
            if (PhotonNetwork.CurrentRoom.PlayerCount == 2)
            {
                opponentPlayer = newPlayer;
                
                opponentNameText.text = opponentPlayer.NickName;
            }
        }

        // Update is called once per frame
        void Update()
        {
        
        }
    }
}