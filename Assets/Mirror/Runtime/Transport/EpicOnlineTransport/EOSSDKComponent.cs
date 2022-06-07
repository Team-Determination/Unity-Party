using System;
using System.Runtime.InteropServices;
using Epic.OnlineServices;
using Epic.OnlineServices.Achievements;
using Epic.OnlineServices.Auth;
using Epic.OnlineServices.Connect;
using Epic.OnlineServices.Ecom;
using Epic.OnlineServices.Friends;
using Epic.OnlineServices.Leaderboards;
using Epic.OnlineServices.Lobby;
using Epic.OnlineServices.Logging;
using Epic.OnlineServices.Metrics;
using Epic.OnlineServices.Mods;
using Epic.OnlineServices.P2P;
using Epic.OnlineServices.Platform;
using Epic.OnlineServices.PlayerDataStorage;
using Epic.OnlineServices.Presence;
using Epic.OnlineServices.Sessions;
using Epic.OnlineServices.TitleStorage;
using Epic.OnlineServices.UI;
using Epic.OnlineServices.UserInfo;
using UnityEngine;
using Credentials = Epic.OnlineServices.Auth.Credentials;
using LoginCallbackInfo = Epic.OnlineServices.Auth.LoginCallbackInfo;
using LoginOptions = Epic.OnlineServices.Auth.LoginOptions;

/// <summary>
/// Manages the Epic Online Services SDK
/// Do not destroy this component!
/// The Epic Online Services SDK can only be initialized once,
/// after releasing the SDK the game has to be restarted in order to initialize the SDK again.
/// In the unity editor the OnDestroy function will not run so that we dont have to restart the editor after play.
/// </summary>
namespace EpicTransport {
    [DefaultExecutionOrder(-32000)]
    public class EOSSDKComponent : MonoBehaviour {

        // Unity Inspector shown variables
        
        [SerializeField]
        private EosApiKey apiKeys;

        [Header("User Login")]
        public bool authInterfaceLogin;
        public LoginCredentialType authInterfaceCredentialType = LoginCredentialType.AccountPortal;
        public uint devAuthToolPort = 7878;
        public string devAuthToolCredentialName = "";
        public ExternalCredentialType connectInterfaceCredentialType = ExternalCredentialType.DeviceidAccessToken;
        public string deviceModel = "PC Windows 64bit";
        [SerializeField] private string displayName = "User";
        public static string DisplayName {
            get {
                return Instance.displayName;
            }
            set {
                Instance.displayName = value;
            }
        }

        [Header("Misc")]
        public LogLevel epicLoggerLevel = LogLevel.Error;

        [SerializeField] private bool collectPlayerMetrics = true;
        public static bool CollectPlayerMetrics {
            get {
                return Instance.collectPlayerMetrics;
            }
        }

        public bool checkForEpicLauncherAndRestart;
        public bool delayedInitialization;
        public float platformTickIntervalInSeconds;
        private float platformTickTimer;
        public uint tickBudgetInMilliseconds;

        // End Unity Inspector shown variables

        private ulong authExpirationHandle;


        private string authInterfaceLoginCredentialId;
        public static void SetAuthInterfaceLoginCredentialId(string credentialId) => Instance.authInterfaceLoginCredentialId = credentialId;
        private string authInterfaceCredentialToken;
        public static void SetAuthInterfaceCredentialToken(string credentialToken) => Instance.authInterfaceCredentialToken = credentialToken;
        private string connectInterfaceCredentialToken;
        public static void SetConnectInterfaceCredentialToken(string credentialToken) => Instance.connectInterfaceCredentialToken = credentialToken;

        private PlatformInterface EOS;

        // Interfaces
        public static AchievementsInterface GetAchievementsInterface() => Instance.EOS.GetAchievementsInterface();
        public static AuthInterface GetAuthInterface() => Instance.EOS.GetAuthInterface();
        public static ConnectInterface GetConnectInterface() => Instance.EOS.GetConnectInterface();
        public static EcomInterface GetEcomInterface() => Instance.EOS.GetEcomInterface();
        public static FriendsInterface GetFriendsInterface() => Instance.EOS.GetFriendsInterface();
        public static LeaderboardsInterface GetLeaderboardsInterface() => Instance.EOS.GetLeaderboardsInterface();
        public static LobbyInterface GetLobbyInterface() => Instance.EOS.GetLobbyInterface();
        public static MetricsInterface GetMetricsInterface() => Instance.EOS.GetMetricsInterface(); // Handled by the transport automatically, only use this interface if Mirror is not used for singleplayer
        public static ModsInterface GetModsInterface() => Instance.EOS.GetModsInterface();
        public static P2PInterface GetP2PInterface() => Instance.EOS.GetP2PInterface();
        public static PlayerDataStorageInterface GetPlayerDataStorageInterface() => Instance.EOS.GetPlayerDataStorageInterface();
        public static PresenceInterface GetPresenceInterface() => Instance.EOS.GetPresenceInterface();
        public static SessionsInterface GetSessionsInterface() => Instance.EOS.GetSessionsInterface();
        public static TitleStorageInterface GetTitleStorageInterface() => Instance.EOS.GetTitleStorageInterface();
        public static UIInterface GetUIInterface() => Instance.EOS.GetUIInterface();
        public static UserInfoInterface GetUserInfoInterface() => Instance.EOS.GetUserInfoInterface();


        protected EpicAccountId localUserAccountId;
        public static EpicAccountId LocalUserAccountId {
            get {
                return Instance.localUserAccountId;
            }
        }

        protected string localUserAccountIdString;
        public static string LocalUserAccountIdString {
            get {
                return Instance.localUserAccountIdString;
            }
        }

        protected ProductUserId localUserProductId;
        public static ProductUserId LocalUserProductId {
            get {
                return Instance.localUserProductId;
            }
        }

        protected string localUserProductIdString;
        public static string LocalUserProductIdString {
            get {
                return Instance.localUserProductIdString;
            }
        }

        protected bool initialized;
        public static bool Initialized {
            get {
                return Instance.initialized;
            }
        }

        protected bool isConnecting;
        public static bool IsConnecting {
            get {
                return Instance.isConnecting;
            }
        }

        protected static EOSSDKComponent instance;
        protected static EOSSDKComponent Instance {
            get
            {
                if (instance == null) {
                    return new GameObject("EOSSDKComponent").AddComponent<EOSSDKComponent>();
                }

                return instance;
            }
        }

        public static void Tick() {
            instance.platformTickTimer -= Time.deltaTime;
            instance.EOS.Tick();
        }

        // If we're in editor, we should dynamically load and unload the SDK between play sessions.
        // This allows us to initialize the SDK each time the game is run in editor.
#if UNITY_EDITOR
        [DllImport("Kernel32.dll")]
        private static extern IntPtr LoadLibrary(string lpLibFileName);

        [DllImport("Kernel32.dll")]
        private static extern int FreeLibrary(IntPtr hLibModule);

        [DllImport("Kernel32.dll")]
        private static extern IntPtr GetProcAddress(IntPtr hModule, string lpProcName);

        private IntPtr libraryPointer;
#endif

        private void Awake() {
            // Prevent multiple instances
            if (instance != null) {
                Destroy(gameObject);
                return;
            }
            instance = this;

#if UNITY_EDITOR
            var libraryPath = "Assets/Mirror/Runtime/Transport/EpicOnlineTransport/EOSSDK/" + Config.LibraryName;

            libraryPointer = LoadLibrary(libraryPath);
            if (libraryPointer == IntPtr.Zero) {
                throw new Exception("Failed to load library" + libraryPath);
            }

            Bindings.Hook(libraryPointer, GetProcAddress);
#endif

            if (!delayedInitialization) {
                Initialize();
            }
        }

        protected void InitializeImplementation() {
            isConnecting = true;

            var initializeOptions = new InitializeOptions
            {
                ProductName = apiKeys.epicProductName,
                ProductVersion = apiKeys.epicProductVersion
            };

            var initializeResult = PlatformInterface.Initialize(initializeOptions);

            // This code is called each time the game is run in the editor, so we catch the case where the SDK has already been initialized in the editor.
            var isAlreadyConfiguredInEditor = Application.isEditor && initializeResult == Result.AlreadyConfigured;
            if (initializeResult != Result.Success && !isAlreadyConfiguredInEditor) {
                throw new Exception("Failed to initialize platform: " + initializeResult);
            }

            // The SDK outputs lots of information that is useful for debugging.
            // Make sure to set up the logging interface as early as possible: after initializing.
            LoggingInterface.SetLogLevel(LogCategory.AllCategories, epicLoggerLevel);
            LoggingInterface.SetCallback(message => Logger.EpicDebugLog(message));

            var options = new Options
            {
                ProductId = apiKeys.epicProductId,
                SandboxId = apiKeys.epicSandboxId,
                DeploymentId = apiKeys.epicDeploymentId,
                ClientCredentials = new ClientCredentials
                {
                    ClientId = apiKeys.epicClientId,
                    ClientSecret = apiKeys.epicClientSecret
                },
                TickBudgetInMilliseconds = tickBudgetInMilliseconds
            };

            EOS = PlatformInterface.Create(options);
            if (EOS == null) {
                throw new Exception("Failed to create platform");
            }

            if (checkForEpicLauncherAndRestart) {
                Result result = EOS.CheckForLauncherAndRestart();

                // If not started through epic launcher the app will be restarted and we can quit 
                if (result != Result.NoChange) {

                    // Log error if launcher check failed, but still quit to prevent hacking
                    if (result == Result.UnexpectedError) {
                        Debug.LogError("Unexpected Error while checking if app was started through epic launcher");
                    }

                    Application.Quit();
                }
            }

            // If we use the Auth interface then only login into the Connect interface after finishing the auth interface login
            // If we don't use the Auth interface we can directly login to the Connect interface
            if (authInterfaceLogin) {
                if (authInterfaceCredentialType == LoginCredentialType.Developer) {
                    authInterfaceLoginCredentialId = "localhost:" + devAuthToolPort;
                    authInterfaceCredentialToken = devAuthToolCredentialName;
                }

                // Login to Auth Interface
                LoginOptions loginOptions = new LoginOptions
                {
                    Credentials = new Credentials
                    {
                        Type = authInterfaceCredentialType,
                        Id = authInterfaceLoginCredentialId,
                        Token = authInterfaceCredentialToken
                    },
                    ScopeFlags = AuthScopeFlags.BasicProfile | AuthScopeFlags.FriendsList | AuthScopeFlags.Presence
                };

                EOS.GetAuthInterface().Login(loginOptions, null, OnAuthInterfaceLogin);
            } else {
                // Login to Connect Interface
                if (connectInterfaceCredentialType == ExternalCredentialType.DeviceidAccessToken) {
                    CreateDeviceIdOptions createDeviceIdOptions = new CreateDeviceIdOptions();
                    createDeviceIdOptions.DeviceModel = deviceModel;
                    EOS.GetConnectInterface().CreateDeviceId(createDeviceIdOptions, null, OnCreateDeviceId);
                } else {
                    ConnectInterfaceLogin();
                }
            }

        }
        public static void Initialize() {
            if (Instance.initialized || Instance.isConnecting) {
                return;
            }

            Instance.InitializeImplementation();
        }

        private void OnAuthInterfaceLogin(LoginCallbackInfo loginCallbackInfo) {
            if (loginCallbackInfo.ResultCode == Result.Success) {
                Debug.Log("Auth Interface Login succeeded");

                string accountIdString;
                Result result = loginCallbackInfo.LocalUserId.ToString(out accountIdString);
                if (Result.Success == result) {
                    Debug.Log("EOS User ID:" + accountIdString);

                    localUserAccountIdString = accountIdString;
                    localUserAccountId = loginCallbackInfo.LocalUserId;
                }
                
                ConnectInterfaceLogin();
            } else if(Epic.OnlineServices.Common.IsOperationComplete(loginCallbackInfo.ResultCode)){
                Debug.Log("Login returned " + loginCallbackInfo.ResultCode);
                LoginOptions loginOptions = new LoginOptions
                {
                    Credentials = new Credentials
                    {
                        Type = LoginCredentialType.AccountPortal,
                        Id = authInterfaceLoginCredentialId,
                        Token = authInterfaceCredentialToken
                    },
                    ScopeFlags = AuthScopeFlags.BasicProfile | AuthScopeFlags.FriendsList | AuthScopeFlags.Presence
                };

                EOS.GetAuthInterface().Login(loginOptions, null, OnAuthInterfaceLogin);
            }
        }

        private void OnCreateDeviceId(CreateDeviceIdCallbackInfo createDeviceIdCallbackInfo) {
            if (createDeviceIdCallbackInfo.ResultCode == Result.Success || createDeviceIdCallbackInfo.ResultCode == Result.DuplicateNotAllowed) {
                ConnectInterfaceLogin();
            } else if(Epic.OnlineServices.Common.IsOperationComplete(createDeviceIdCallbackInfo.ResultCode)) {
                Debug.Log("Device ID creation returned " + createDeviceIdCallbackInfo.ResultCode);
            }
        }

        private void ConnectInterfaceLogin() {
            var loginOptions = new Epic.OnlineServices.Connect.LoginOptions();

            if (connectInterfaceCredentialType == ExternalCredentialType.Epic) {
                Token token;
                Result result = EOS.GetAuthInterface().CopyUserAuthToken(new CopyUserAuthTokenOptions(), localUserAccountId, out token);

                if (result == Result.Success) {
                    connectInterfaceCredentialToken = token.AccessToken;
                } else {
                    Debug.LogError("Failed to retrieve User Auth Token");
                }
            } else if (connectInterfaceCredentialType == ExternalCredentialType.DeviceidAccessToken) {
                loginOptions.UserLoginInfo = new UserLoginInfo();
                loginOptions.UserLoginInfo.DisplayName = displayName;
            }

            loginOptions.Credentials = new Epic.OnlineServices.Connect.Credentials();
            loginOptions.Credentials.Type = connectInterfaceCredentialType;
            loginOptions.Credentials.Token = connectInterfaceCredentialToken;

            EOS.GetConnectInterface().Login(loginOptions, null, OnConnectInterfaceLogin);
        }

        private void OnConnectInterfaceLogin(Epic.OnlineServices.Connect.LoginCallbackInfo loginCallbackInfo) {
            if (loginCallbackInfo.ResultCode == Result.Success) {
                Debug.Log("Connect Interface Login succeeded");

                string productIdString;
                Result result = loginCallbackInfo.LocalUserId.ToString(out productIdString);
                if (Result.Success == result) {
                    Debug.Log("EOS User Product ID:" + productIdString);

                    localUserProductIdString = productIdString;
                    localUserProductId = loginCallbackInfo.LocalUserId;
                }
                
                initialized = true;
                isConnecting = false;

                var authExpirationOptions = new AddNotifyAuthExpirationOptions();
                authExpirationHandle = EOS.GetConnectInterface().AddNotifyAuthExpiration(authExpirationOptions, null, OnAuthExpiration);
            } else if (Epic.OnlineServices.Common.IsOperationComplete(loginCallbackInfo.ResultCode)) {
                Debug.Log("Login returned " + loginCallbackInfo.ResultCode + "\nRetrying...");
                EOS.GetConnectInterface().CreateUser(new CreateUserOptions { ContinuanceToken = loginCallbackInfo.ContinuanceToken }, null, cb => {
                    if (cb.ResultCode != Result.Success) { Debug.Log(cb.ResultCode); return; }
                    localUserProductId = cb.LocalUserId;
                    ConnectInterfaceLogin();
                });
            }
        }
        
        private void OnAuthExpiration(AuthExpirationCallbackInfo authExpirationCallbackInfo) {
            Debug.Log("AuthExpiration callback");
            EOS.GetConnectInterface().RemoveNotifyAuthExpiration(authExpirationHandle);
            ConnectInterfaceLogin();
        }

        // Calling tick on a regular interval is required for callbacks to work.
        private void LateUpdate() {
            if (EOS != null) {
                platformTickTimer += Time.deltaTime;

                if (platformTickTimer >= platformTickIntervalInSeconds) {
                    platformTickTimer = 0;
                    EOS.Tick();
                }
            }
        }

        private void OnApplicationQuit() {
            if (EOS != null) {
                EOS.Release();
                EOS = null;
                PlatformInterface.Shutdown();
            }

            // Unhook the library in the editor, this makes it possible to load the library again after stopping to play
#if UNITY_EDITOR
            if (libraryPointer != IntPtr.Zero) {
                Bindings.Unhook();

                // Free until the module ref count is 0
                while (FreeLibrary(libraryPointer) != 0) { }

                libraryPointer = IntPtr.Zero;
            }
#endif
        }
    }
}