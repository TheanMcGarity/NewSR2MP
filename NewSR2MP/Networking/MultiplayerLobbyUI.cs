
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
public class MultiplayerLobbyUI : MonoBehaviour
{
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
            MultiplayerManager.Instance.Host(7777);
            isHost = true;
        }));
        
        joinViaCodeButton.onClick.AddListener(new System.Action(() =>
        {
            MultiplayerManager.Instance.Connect("127.0.0.1",7777);
        }));
        
        lobbyCodePaste.onClick.AddListener(new System.Action(() =>
        {
            lobbyCodeInput.SetText(GUIUtility.systemCopyBuffer);
        }));
        
        // Temp
        lobbyCodeInput.interactable = false;
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
        isConnected = ClientActive();
        isHost = ServerActive();
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
