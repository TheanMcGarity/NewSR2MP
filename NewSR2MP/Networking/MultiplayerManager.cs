using Mirror.Discovery;
using System.Reflection;
using UnityEngine;
using UnityEngine.SceneManagement;

using EpicTransport;
using Il2CppMonomiPark.SlimeRancher.Event;
using Il2CppMonomiPark.World;

namespace NewSR2MP.Networking
{
    [RegisterTypeInIl2Cpp(false)]
    public class MultiplayerManager : SRBehaviour
    {
        /// <summary>
        /// The delegate for the 'OnPlayerConnecting' event.
        /// </summary>
        /// <param name="connection">Connection ID for the player. This should be on its way to the clients when this event is called.</param>
        public delegate void OnPlayerConnect(NetworkConnectionToClient connection);

        /// <summary>
        /// The delegate for the 'OnJoinAttempt' event.
        /// </summary>
        /// <param name="connection">Connection ID for the player. this gets sent to all clients on join.</param>
        /// <param name="errorIfOccurred">If an error occurred, this is what the exception ToString() is.</param>
        public delegate void OnPlayerAttemptedJoin(NetworkConnectionToClient connection, string errorIfOccurred = null);
        
        /// <summary>
        /// The delegate for the 'OnPlayerLeave...' events.
        /// </summary>
        /// <param name="id">The player id of the disconnecting player. This is actually the server side connection id.</param>
        public delegate void OnPlayerLeft(int id);


        /// <summary>
        /// This event occurrs after a player connects into the server, right before the save data is sent over.
        /// The parameters for this event are a client.
        /// </summary>
        public static event OnPlayerConnect OnPlayerConnecting;

        /// <summary>
        /// This event occurrs after a player leaves the server.
        /// The parameters for this event are the player id.
        /// This runs on both client and server.
        /// </summary>
        public static event OnPlayerLeft OnPlayerLeaveCommon;

        /// <summary>
        /// This event occurrs after a player leaves the server.
        /// The parameters for this event are the player id.
        /// This runs only on the client.
        /// </summary>
        public static event OnPlayerLeft OnPlayerLeaveClient;

        /// <summary>
        /// This event occurrs after a player leaves the server.
        /// The parameters for this event are the player id.
        /// This runs only on the server.
        /// </summary>
        public static event OnPlayerLeft OnPlayerLeaveServer;

        /// <summary>
        /// This event occurrs after a player attempts to join, when the code for sending a save errors or finishes.
        /// The parameters for this event are a client and an error if occurred.
        /// </summary>
        public static event OnPlayerAttemptedJoin OnJoinAttempt;


        private NetworkManager networkManager;

        public EOSLobbyUI prototypeLobbyUI;

        private NetworkDiscovery discoveryManager;

        public GameObject onlinePlayerPrefab;

        public Transport transport;

        GUIStyle guiStyle;

        public static NetworkManager NetworkManager
        {
            get
            {
                return Instance.networkManager;
            }
        }

        public static NetworkDiscovery DiscoveryManager
        {
            get
            {
                return Instance.discoveryManager;
            }
        }

        public static MultiplayerManager Instance;

        public void Awake()
        {
            Instance = this;
            gameObject.AddComponent<EOSSDKComponent>();
        }


        private void Start()
        {
            transport = gameObject.AddComponent<EosTransport>();
            

            WriterBugfix.FixWriters();
            ReaderBugfix.FixReaders();
            
            networkManager = gameObject.AddComponent<SRNetworkManager>();

            networkManager.maxConnections = 100;
            // networkManager.playerPrefab = onlinePlayerPrefab; need to use asset bundles to fix error
            networkManager.autoCreatePlayer = false;


            networkManager.transport = transport;
            Transport.active = transport;

            prototypeLobbyUI = gameObject.AddComponent<EOSLobbyUI>();
            
            NetworkManager.dontDestroyOnLoad = true;
            discoveryManager.enableActiveDiscovery = true;



            NetworkClient.OnDisconnectedEvent += ClientLeave;
        }

        void GeneratePlayerBean()
        {
            onlinePlayerPrefab = new GameObject("PlayerDefault");
            var playerModel = GameObject.CreatePrimitive(PrimitiveType.Capsule); // Prototype player.
            var playerFace = GameObject.CreatePrimitive(PrimitiveType.Cube);
            playerFace.transform.parent = playerModel.transform;
            playerFace.transform.localPosition = new Vector3(0f, 0.5f, 0.25f);
            playerFace.transform.localScale = Vector3.one * 0.5f;
            onlinePlayerPrefab.AddComponent<NetworkPlayer>();
            onlinePlayerPrefab.AddComponent<TransformSmoother>();
            onlinePlayerPrefab.GetComponent<NetworkPlayer>().enabled = false;
            DontDestroyOnLoad(onlinePlayerPrefab);
            onlinePlayerPrefab.SetActive(false);
            playerModel.transform.parent = onlinePlayerPrefab.transform;

            var material = Resources.FindObjectsOfTypeAll<Material>().FirstOrDefault((mat) => mat.name == "slimePinkBase");
            playerFace.GetComponent<MeshRenderer>().material = material;
            playerModel.GetComponent<MeshRenderer>().material = material;

            Destroy(playerFace.GetComponent<BoxCollider>());

            playerModel.transform.localPosition = Vector3.up;

            var viewcam = new GameObject("CharaCam").AddComponent<Camera>();

            viewcam.transform.parent = playerFace.transform;
            viewcam.transform.localPosition = new Vector3(0, 0, 1.3f);
            viewcam.enabled = false;

            onlinePlayerPrefab.GetComponent<NetworkPlayer>().InitCamera();

        }

        public RenderTexture playerCameraPreviewImage = new RenderTexture(250, 250, 24);

        public NetworkPlayer currentPreviewRenderer;

        public void OnDestroy()
        {
            SRMP.Error("THIS SHOULD NOT APPEAR!!!!");
            SRMP.Error("SR2MP has quit unexpectedly, restart your game to play multiplayer.");
        }

        HashSet<string> getPediaEntries()
        {
            
            var pedias = SceneContext.Instance.PediaDirector._pediaModel.unlocked;
            
            var ret = new HashSet<string>();

            foreach (var pedia in pedias)
            {
                ret.Add(pedia.name);
            }

            return ret;
        }
        
        // Hefty code
        public static void PlayerJoin(NetworkConnectionToClient nctc, Guid savingID, string username)
        {
            SRMP.Log("connecting client.");

            OnPlayerConnecting?.Invoke(nctc);



            try
            {
                clientToGuid.Add(nctc.connectionId, savingID);
                // Variables
                double time = SceneContext.Instance.TimeDirector.CurrTime();
                List<InitActorData> actors = new List<InitActorData>();
                HashSet<InitGordoData> gordos = new HashSet<InitGordoData>();
                List<InitPlayerData> initPlayers = new List<InitPlayerData>();
                List<InitPlotData> plots = new List<InitPlotData>();
                List<string> pedias = new List<string>();


                var upgrades = SceneContext.Instance.PlayerState._model.upgradeModel.upgradeLevels;

                var newPlayer = !savedGame.savedPlayers.playerList.TryGetValue(savingID, out var playerData);
                if (newPlayer)
                {
                    playerData = new NetPlayerV01();

                    savedGame.savedPlayers.playerList.Add(savingID, playerData);
                }




                // Actors
                foreach (var a in Resources.FindObjectsOfTypeAll<Identifiable>())
                {
                    try
                    {

                        if (a.gameObject.scene.name == "worldGenerated")
                        {
                            var data = new InitActorData()
                            {
                                id = a.GetActorId().Value,
                                ident = a.identType.name,
                                pos = a.transform.position
                            };
                            actors.Add(data);
                        }
                    }
                    catch { }
                }

                // Gordos
                foreach (var g in Resources.FindObjectsOfTypeAll<GordoEat>())
                {
                    try
                    {
                        if (g.gameObject.hideFlags != HideFlags.HideAndDontSave && g.gameObject.activeInHierarchy)
                        {
                            
                        }
                        InitGordoData data = new InitGordoData()
                        {
                            id = g.Id,
                            eaten = g.GordoModel.gordoEatCount
                        };
                        gordos.Add(data);
                    }
                    catch { }
                }

                // Current Players
                foreach (var player in players)
                {
                    if (player.Key != 0)
                    {

                        var p = new InitPlayerData()
                        {
                            id = player.Key,
                        };
                        initPlayers.Add(p);
                    }
                }
                var p2 = new InitPlayerData()
                {
                    id = 0
                };
                initPlayers.Add(p2);



                // Plots
                foreach (var plot in Resources.FindObjectsOfTypeAll<LandPlot>())
                {
                    if (plot.gameObject.scene.name == "worldGenerated")
                    {
                        try
                        {
                            var silo = plot.gameObject.GetComponentInChildren<SiloStorage>();

                            // Silos
                            InitSiloData s = new InitSiloData()
                            {
                                ammo = new HashSet<AmmoData>()
                            }; // Empty
                            if (silo != null)
                            {
                                HashSet<AmmoData> ammo = new HashSet<AmmoData>();
                                var idx = 0;
                                foreach (var a in silo.Ammo.Slots)
                                {
                                    if (a != null)
                                    {
                                        var ammoSlot = new AmmoData()
                                        {
                                            slot = idx,
                                            id = a.Id.name,
                                            count = a.Count,
                                        };
                                        ammo.Add(ammoSlot);
                                    }
                                    else
                                    {
                                        var ammoSlot = new AmmoData()
                                        {
                                            slot = idx,
                                            id = "",
                                            count = 0,
                                        };
                                        ammo.Add(ammoSlot);
                                    }
                                    idx++;
                                }
                                s = new InitSiloData()
                                {
                                    slots = silo.LocalAmmo.Slots.Count,
                                    ammo = ammo
                                };
                            }

                            var p = new InitPlotData()
                            {
                                id = plot._model.gameObj.GetComponent<LandPlotLocation>().Id,
                                type = plot._model.typeId,
                                upgrades = plot._model.upgrades,
                                cropIdent = plot.GetAttachedCropId().name,

                                siloData = s,
                            };
                            plots.Add(p);
                        }
                        catch { }
                    }
                }

                // Slime Gates || Ranch expansions
                List<InitAccessData> access = new List<InitAccessData>();
                foreach (var accessDoor in SceneContext.Instance.GameModel.doors)
                {
                    access.Add(new InitAccessData()
                    {
                        open = (accessDoor.Value.state == AccessDoor.State.OPEN),
                        id = accessDoor.Key
                    });
                }
                List<AmmoData> playerAmmoData = new List<AmmoData>();
                int i = 0;
                foreach (var ammoSlot in playerData.ammo)
                {

                    var playerSlot = new AmmoData()
                    {
                        slot = i,
                        id = GetStringFromPersistentID_IdentifiableType(ammoSlot.ID),
                        count = ammoSlot.Count,
                    };
                    playerAmmoData.Add(playerSlot);
                    i++;
                }

                LocalPlayerData localPlayerData = new LocalPlayerData()
                {
                    pos = playerData.position.Value,
                    rot = playerData.rotation.Value,
                    ammo = playerAmmoData
                };


                List<string> GetListFromFogEvents(Il2CppSystem.Collections.Generic.Dictionary<string, EventRecordModel.Entry>.KeyCollection events)
                {
                    var ret = new List<string>();
                    foreach (var e in events)
                        ret.Add(e);
                    return ret;
                }
                

                var money = SceneContext.Instance.PlayerState._model.currency;

                // Send save data.
                var saveMessage = new LoadMessage()
                {
                    initActors = actors,
                    initPlayers = initPlayers,
                    initPlots = plots,
                    initGordos = gordos,
                    initPedias = pedias,
                    initAccess = access,
                    initMaps = GetListFromFogEvents(SceneContext.Instance.eventDirector._model.table["fogRevealed"]._keys),
                    playerID = nctc.connectionId,
                    money = money,
                    time = time,
                    localPlayerSave = localPlayerData,
                    upgrades = upgrades,
                };
                NetworkServer.SRMPSend(saveMessage, nctc);
                SRMP.Log("sent world");

                Ammo currentHostAmmo = SceneContext.Instance.PlayerState.Ammo;
                NetworkAmmo netAmmo = new NetworkAmmo($"player_{savingID}",SceneContext.Instance.PlayerState._ammoSlotDefinitions);

                netAmmo._ammoModel.slots = NetworkAmmo.SRMPAmmoDataToSlots(playerData.ammo);

                // Spawn player for host
                try
                {
                    var player = Instantiate(Instance.onlinePlayerPrefab);
                    player.name = $"Player{nctc.connectionId}";
                    var netPlayer = player.GetComponent<NetworkPlayer>();
                    players.Add(nctc.connectionId, netPlayer);
                    netPlayer.id = nctc.connectionId;
                    player.SetActive(true);
                    var packet = new PlayerJoinMessage()
                    {
                        id = nctc.connectionId,
                        local = false
                    };
                    NetworkServer.SRMPSendToConnections(packet, NetworkServer.NetworkConnectionListExcept(nctc));

                }
                catch
                { }
                OnJoinAttempt?.Invoke(nctc);
            }
            catch (Exception ex)
            {
                clientToGuid.Remove(nctc.connectionId);
                OnJoinAttempt?.Invoke(nctc,ex.ToString());
                SRMP.Log(ex.ToString());
            }
        }


        public static void ClientLeave()
        {
            SceneManager.LoadScene("MainMenu");
        }

        public void Connect(string ip, ushort port)
        {
            // Please use EOS
        }
        public void Host()
        {
            // Please use EOS
        }


        private void Update()
        {
            // Ticks
            if (NetworkServer.activeHost)
            {
                transport.ServerEarlyUpdate();
                transport.ServerLateUpdate();
            }
            else if (NetworkClient.active || NetworkClient.isConnecting)
            {
                transport.ClientEarlyUpdate();
                transport.ClientLateUpdate();
            }
            
        }
    }
}
