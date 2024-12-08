using Epic.OnlineServices;
using Epic.OnlineServices.Lobby;
using EpicTransport;
using Il2CppTMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;



/// <summary>
/// SR2MP Lobby UI.
/// SR2MP uses one class for all UI unlike SRMP.
/// </summary>
[RegisterTypeInIl2Cpp(false)]
public class MultiplayerLobbyUI : EOSLobby
{
    
    // EOS Premade
    private List<Epic.OnlineServices.Lobby.Attribute> lobbyData = new List<Epic.OnlineServices.Lobby.Attribute>();

    private void OnEnable() {
        //subscribe to events
        CreateLobbySucceeded += OnCreateLobbySuccess;
        JoinLobbySucceeded += OnJoinLobbySuccess;
        LeaveLobbySucceeded += OnLeaveLobbySuccess;
    }

    //deregister events
    private void OnDisable() {
        //unsubscribe from events
        CreateLobbySucceeded -= OnCreateLobbySuccess;
        JoinLobbySucceeded -= OnJoinLobbySuccess;
        LeaveLobbySucceeded -= OnLeaveLobbySuccess;
    }

    //when the lobby is successfully created, start the host
    private void OnCreateLobbySuccess(List<Epic.OnlineServices.Lobby.Attribute> attributes) {
        lobbyData = attributes;

        SRNetworkManager.singleton.StartHost();
    }

    //when the user joined the lobby successfully, set network address and connect
    private void OnJoinLobbySuccess(List<Epic.OnlineServices.Lobby.Attribute> attributes) {
        lobbyData = attributes;

        NetworkManager netManager = SRNetworkManager.singleton;
        netManager.networkAddress = attributes.Find((x) => x.Data.Key == hostAddressKey).Data.Value.AsUtf8;
        netManager.StartClient();
    }

    //when the lobby was left successfully, stop the host/client
    private void OnLeaveLobbySuccess() {
        NetworkManager netManager = SRNetworkManager.singleton;
        netManager.StopHost();
        netManager.StopClient();
    }
    
    // SRMP Stuff
    
    void Start()
    {
        rootObject = gameObject.getObjRec<GameObject>("Root"); 

        connectPanel = gameObject.getObjRec<GameObject>("ConnectPanel");
        lobbyCodeInput = gameObject.getObjRec<GameObject>("LobbyCode").GetComponentInChildren<TMP_InputField>();
        joinViaCodeButton = connectPanel.transform.FindChild("Button").GetComponent<Button>(); 
        lobbyCodePaste = gameObject.getObjRec<Button>("PasteButton");

        hostPanel = gameObject.getObjRec<GameObject>("HostPanel");
        hostButton = hostPanel.transform.FindChild("Button").GetComponent<Button>();
        
        lobbyPanel = gameObject.getObjRec<GameObject>("LobbyPanel");
        adminSubpanel = gameObject.getObjRec<GameObject>("LobbyAdminPanel");
        lobbyCodeText = gameObject.getObjRec<GameObject>("LobbyAdminPanel").transform.GetChild(0).GetComponent<TMP_Text>();
        
        // Button Functions
        hostButton.onClick.AddListener(new System.Action(() =>
        {
            lobbyCode = GenerateServerCode();
            CreateLobby(10, LobbyPermissionLevel.Publicadvertised, false, new AttributeData[] { new AttributeData { Key = "code", Value = lobbyCode }, });
            isHost = true;
        }));
        
        joinViaCodeButton.onClick.AddListener(new System.Action(() =>
        {
            FindLobbyByCode(lobbyCodeInput.m_Text);
        }));
        
        lobbyCodePaste.onClick.AddListener(new System.Action(() =>
        {
            lobbyCodeInput.SetText(GUIUtility.systemCopyBuffer);
        }));
        
        // Temp
        lobbyCodeInput.interactable = false;
    }
    public void FindLobbyByCode(string lobbyCode) {
        LobbySearch search = new LobbySearch();

        EOSSDKComponent.GetLobbyInterface().CreateLobbySearch(new CreateLobbySearchOptions { MaxResults = 1 }, out search);

        search.SetParameter(new LobbySearchSetParameterOptions {
            ComparisonOp = ComparisonOp.Equal,
            Parameter = new AttributeData { Key = "code", Value = lobbyCode }
        });
        
        search.Find(new LobbySearchFindOptions { LocalUserId = EOSSDKComponent.LocalUserProductId }, null, (LobbySearchFindCallbackInfo callback) => {
            if (callback.ResultCode != Result.Success) {
                SRMP.Log($"There was an error while finding lobbies. Error code: {callback.ResultCode}");
                return;
            }

            LobbyDetails lobbyInformation;
            search.CopySearchResultByIndex(new LobbySearchCopySearchResultByIndexOptions { LobbyIndex = 0}, out lobbyInformation);
            
            JoinLobby(lobbyInformation);
        });
    }
    
    public static MultiplayerLobbyUI instance;
    
    /// <summary>
    /// Current menu for UI. Should automatically be set by the behaviour.
    /// </summary>
    public enum LobbyUIState
    {
        CLOSED,
        MAIN_MENU,
        HOST_MENU,
        LOBBY_MENU,
        LOBBY_HOST_MENU,
        SETTINGS_MENU,
    }
    
    public GameObject rootObject;

    /// <summary>
    /// This handles all logic for which menu to leave open. Settings menu has not yet been implemented.
    /// </summary>
    public LobbyUIState state
    {
        get
        {
            if (isForceClosed) return LobbyUIState.CLOSED;
            
            if (isInMainMenu) return LobbyUIState.MAIN_MENU;
            if (!isPaused) return LobbyUIState.CLOSED;
            if (isConnected && isHost) return LobbyUIState.LOBBY_HOST_MENU;
            if (isConnected) return LobbyUIState.LOBBY_MENU;
            if (!isConnected && isPaused) return LobbyUIState.HOST_MENU;
            if (isInSettings) return LobbyUIState.SETTINGS_MENU;

            return LobbyUIState.CLOSED;
        }
    }

    public GameObject connectPanel;
    public TMP_InputField lobbyCodeInput;
    public Button lobbyCodePaste;
    public Button joinViaCodeButton;
    

    public GameObject hostPanel;
    public Button hostButton;
    
    public bool isHost = false;
    public GameObject lobbyPanel;
    public GameObject adminSubpanel;
    public TMP_Text lobbyCodeText;

    public GameObject settingsPanel;
    
    public bool isPaused = false;
    public bool isInSettings = false;
    public bool isInMainMenu = false;
    public bool isConnected = false;
    
    public bool isForceClosed = false;
    
    public string lobbyCode;

    void CheckFlags()
    {
        isPaused = Time.timeScale == 0;
        isInMainMenu = SystemContext.Instance.SceneLoader._currentSceneGroup.name.Contains("MainMenu"); // Using name because there are 2 main menu scene groups for some reason.
        isConnected = ConnectedToLobby;
    }

    internal bool isCodeInputSelected = false;
    
    void Update()
    {
        CheckFlags();
        
        lobbyPanel.SetActive(false);
        adminSubpanel.SetActive(false);
        connectPanel.SetActive(false);
        hostPanel.SetActive(false);

        switch (state)
        {
            case LobbyUIState.MAIN_MENU:
                connectPanel.SetActive(true);
                break;
            case LobbyUIState.LOBBY_MENU:
                lobbyPanel.SetActive(true);
                break;
            case LobbyUIState.LOBBY_HOST_MENU:
                lobbyPanel.SetActive(true);
                adminSubpanel.SetActive(true);
                lobbyCodeText.SetText($"Lobby Code: {lobbyCode}");
                break;
            case LobbyUIState.HOST_MENU:
                hostPanel.SetActive(true);
                break;
            default:
                // CLOSED, or unimplemented.
                break;
        }

        if (isCodeInputSelected)
        {
            if (Keyboard.current.escapeKey.isPressed || Keyboard.current.enterKey.isPressed || state != LobbyUIState.MAIN_MENU)
            {
                isCodeInputSelected = false;
            }
        }

        if (Keyboard.current.f4Key.wasPressedThisFrame) isForceClosed = !isForceClosed;
    }
}
