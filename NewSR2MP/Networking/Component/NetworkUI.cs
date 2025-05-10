using SR2E;
using SR2E.Managers;
using SR2E.Menus;

namespace NewSR2MP.Networking.Component;

[RegisterTypeInIl2Cpp(false)]
public class NetworkUI : MonoBehaviour
{
    public Color guiColor = Color.white;
    public Rect uiBox = Rect.zero;
    public bool customBoxSize = false;
    public enum MainUIState
    {
        HOST,
        CLIENT,
        MAIN_MENU,
        LOADING,
        SINGLEPLAYER,
        CHANGIMG_USERNAME,
        HIDDEN
    }

    public bool AlreadyHasUsername => Main.data.HasSavedUsername;
    public bool changingUsername;
    public MainUIState CurrentState 
    {
        get
        {
            if (systemContext.SceneLoader.IsSceneLoadInProgress || gameContext == null)
                return MainUIState.LOADING;
            
            if (changingUsername)
                return MainUIState.CHANGIMG_USERNAME;
            
            if (Time.timeScale == 0.0f)
            {
                if (ServerActive())
                    return MainUIState.HOST;
                if (ClientActive())
                    return MainUIState.CLIENT;
                
                if (inGame)
                    return MainUIState.SINGLEPLAYER;
            }
            
            if (!inGame)
                return MainUIState.MAIN_MENU;
            
            
            return MainUIState.HIDDEN;
        }
    }
    
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
            uiBox = new Rect(5, 5,  285, 200);
        
        if (GUI.Button(new Rect(10, 10, 275, 25), SR2ELanguageManger.translation("ui.changeusername")))
        {
           changingUsername = true;
        }
        
        Main.data.LastIP = GUI.TextField(new Rect(10, 45, 275, 25), Main.data.LastIP);
        Main.data.LastPort = GUI.TextField(new Rect(10, 80, 275, 25), Main.data.LastPort);

        if (!ushort.TryParse(Main.data.LastPort, out var portParsed))
            GUI.Label(new Rect(10, 115, 275, 25), SR2ELanguageManger.translation("ui.invalidport"));
        else
        {
            if (GUI.Button(new Rect(10, 140, 275, 25), SR2ELanguageManger.translation("ui.join")))
            {
                MultiplayerManager.Instance.Connect(Main.data.LastIP, portParsed);
                
                Main.modInstance.SaveData();
            }
            
        }
        
        GUI.Label(new Rect(10, 185, 275, 25), SR2ELanguageManger.translation("ui.joinsave"));

    }
    void SinglePlayerUI()
    {
        if (!customBoxSize)
            uiBox = new Rect(5, 5,  285, 80);
        
        Main.data.LastPortHosted = GUI.TextField(new Rect(10, 10, 275, 25), Main.data.LastPortHosted);
        if (!ushort.TryParse(Main.data.LastPortHosted, out var portParsed))
            GUI.Label(new Rect(10, 45, 275, 25), SR2ELanguageManger.translation("ui.invalidport"));
        else
        {
            if (GUI.Button(new Rect(10, 45, 275, 25), SR2ELanguageManger.translation("ui.host")))
            {
                MultiplayerManager.Instance.Host(portParsed);
                
                Main.modInstance.SaveData();
            }
        }
    }

    void LoadingUI()
    {
        if (!customBoxSize)
            uiBox = new Rect(5, 5,  285, 35);
        
        GUI.Label(new Rect(10, 10, 275, 25), SR2ELanguageManger.translation("ui.loading"));
    }

    void OnGUI()
    {    
        GUI.color = guiColor;
        
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
        // Implement Host and Client
    }
}