using Il2CppTMPro;
using UnityEngine;
using UnityEngine.UI;

public class MultiplayerLobbyUI : EOSLobby
{
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
