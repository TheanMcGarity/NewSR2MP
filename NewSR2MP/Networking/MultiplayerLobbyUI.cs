using Epic.OnlineServices;
using Epic.OnlineServices.Lobby;
using EpicTransport;
using Il2CppTMPro;
using UnityEngine;
using UnityEngine.UI;

[RegisterTypeInIl2Cpp(false)]
public class MultiplayerLobbyUI : EOSLobby
{
    void Start()
    {
        rootObject = gameObject.getObjRec<GameObject>("Root");
        connectPanel = gameObject.getObjRec<GameObject>("ConnectPanel");
        lobbyCodeInput = gameObject.getObjRec<GameObject>("LobbyCode").GetComponentInChildren<TMP_InputField>();
        joinViaCodeButton = connectPanel.transform.FindChild("Button").GetComponent<Button>();
        hostPanel = gameObject.getObjRec<GameObject>("HostPanel");
        hostButton = connectPanel.transform.FindChild("Button").GetComponent<Button>();
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

    public LobbyUIState state
    {
        get
        {
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
    
    public string lobbyCode;

    void CheckFlags()
    {
        isPaused = Time.timeScale == 0;
        isInMainMenu = SystemContext.Instance.SceneLoader._currentSceneGroup == SystemContext.Instance.SceneLoader._mainMenuSceneGroup;
        isConnected = ConnectedToLobby;
    }
    
    void Update()
    {
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
    }
}
