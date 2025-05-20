using SR2E.Managers;

namespace NewSR2MP.Component;

[RegisterTypeInIl2Cpp(false)]
public class NetworkUI : MonoBehaviour
{
    public Color guiColor = Color.white;
    public Rect uiBox = Rect.zero;
    public bool customBoxSize = false;
    public enum MainUIState
    {
        // In game
        HOST,
        CLIENT,
        SINGLEPLAYER,

        // Out of game
        MAIN_MENU,
        LOADING,
        CHANGIMG_USERNAME,
        RESTART,
        
        // Hidden
        HIDDEN,
    }

    public bool AlreadyHasUsername => Main.data.HasSavedUsername;

    private string serverCodeInput = "";

    public bool SteamMode => false;//Main.data.UseSteam;

    public bool mustRestart = false;
    public bool changingUsername;

    
    public MainUIState CurrentState 
    {
        get
        {
            if (mustRestart)
                return MainUIState.RESTART;
            
            if (systemContext.SceneLoader.IsSceneLoadInProgress || gameContext == null)
                return MainUIState.LOADING;
            
            if (changingUsername)
                return MainUIState.CHANGIMG_USERNAME;

            if (Time.timeScale == 0.0f)
            {
                if (ClientActive())
                    return MainUIState.CLIENT;
                if (ServerActive())
                    return MainUIState.HOST;

                if (inGame)
                    return MainUIState.SINGLEPLAYER;
            }

            if (!inGame)
                return MainUIState.MAIN_MENU;
                
            
            return MainUIState.HIDDEN;
        }
    }
    
    public string inputCode = "";
    
    void UsernameInput()
    {
        if (!customBoxSize)
            uiBox = new Rect(5, 5,  285, 65);
        
        Main.data.Username = GUI.TextField(new Rect(10, 10, 275, 25), Main.data.Username);
        if (GUI.Button(new Rect(10, 35, 275, 25),SR2ELanguageManger.translation("ui.saveusername")))
        {
            changingUsername = false;
            Main.data.HasSavedUsername = true;
            Main.modInstance.SaveData();
        }
        
    }

    void MainMenuUI()
    {
        if (!customBoxSize)
            uiBox = new Rect(5, 5, 285, 215);

        if (GUI.Button(new Rect(10, 10, 275, 25), SR2ELanguageManger.translation("ui.changeusername")))
        {
            changingUsername = true;
        }

        inputCode = GUI.TextField(new Rect(10, 45, 275, 25), inputCode);

        if (GUI.Button(new Rect(10, 80, 275, 25), SR2ELanguageManger.translation("ui.join")))
        {
            MultiplayerManager.Instance.Connect(inputCode);
        }


        GUI.Label(new Rect(10, 185, 275, 25), SR2ELanguageManger.translation("ui.joinsave"));

    }

    void SinglePlayerUI()
    {
        if (!customBoxSize)
            uiBox = new Rect(5, 5, 285, 50);
            
        if (GUI.Button(new Rect(10, 10, 275, 25), SR2ELanguageManger.translation("ui.host")))
        {
            MultiplayerManager.Instance.Host();
        }
    }

    void LoadingUI()
    {
        if (!customBoxSize)
            uiBox = new Rect(5, 5,  285, 35);
        
        GUI.Label(new Rect(10, 10, 275, 25), SR2ELanguageManger.translation("ui.loading"));
    }
    void HostUI()
    {
        if (!customBoxSize)
            uiBox = new Rect(5, 5,  285, 35);
        
        GUI.TextField(new Rect(10, 10, 275, 25), SR2ELanguageManger.translation("steam.ui.servercode", EpicApplication.Instance.Lobby.LobbyId));
    }
    void SingleplayerUI()
    {
        if (!customBoxSize)
            uiBox = new Rect(5, 5,  285, 35);
        
        if (GUI.Button(new Rect(10, 10, 275, 25), SR2ELanguageManger.translation("ui.host")))
        {
            MultiplayerManager.Instance.Host();
        }
    }
    void RestartUI()
    {
        if (!customBoxSize)
            uiBox = new Rect(5, 5,  285, 35);
        
        GUI.Label(new Rect(10, 10, 275, 25), SR2ELanguageManger.translation("ui.restartgame"));
    }

    void OnGUI()
    {    
        GUI.color = guiColor;
        
        if (CurrentState == MainUIState.RESTART)
        {            
            GUI.Box(uiBox, "");
            RestartUI();
            return;
        }
        
        if (CurrentState != MainUIState.HIDDEN)            
            GUI.Box(uiBox, "");

        if (!AlreadyHasUsername || CurrentState == MainUIState.CHANGIMG_USERNAME)
            UsernameInput();
        else if (CurrentState == MainUIState.MAIN_MENU)
            MainMenuUI();
        else if (CurrentState == MainUIState.SINGLEPLAYER)
            SinglePlayerUI();
        else if (CurrentState == MainUIState.LOADING)
            LoadingUI();
        else if (CurrentState == MainUIState.HOST)
            HostUI();
        
        // Implement Host kick menu, chat menu, and Client menu
    }
}