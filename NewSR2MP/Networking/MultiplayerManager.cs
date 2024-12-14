
using System.Reflection;
using UnityEngine;
using UnityEngine.SceneManagement;

using Il2CppMonomiPark.SlimeRancher.Event;
using Il2CppMonomiPark.SlimeRancher.SceneManagement;
using Il2CppMonomiPark.World;
using Riptide.Utils;
using SR2E;
using UnityEngine.Serialization;

namespace NewSR2MP.Networking
{
    [RegisterTypeInIl2Cpp(false)]
    public partial class MultiplayerManager : SRBehaviour
    {
        //public EOSLobbyGUI prototypeLobbyGUI;


        public GameObject onlinePlayerPrefab;


        GUIStyle guiStyle;

        public static MultiplayerManager Instance;

        public void Awake()
        {
            // Dont make that mistake again
            // i submitted a incorrect bug report :sob:
            RiptideLogger.Initialize(SRMP.Debug,SRMP.Log,SRMP.Warn,SRMP.Error,false);
            Instance = this;
            
            
        }


        private void Start()
        {
            SR2EConsole.RegisterCommand(new HostCommand());
            SR2EConsole.RegisterCommand(new JoinCommand());
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
        public static void PlayerJoin(Connection nctc, Guid savingID, string username)
        {
            SRMP.Log("connecting client.");




            try
            {
                clientToGuid.Add(nctc.Id, savingID);
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
                    playerID = nctc.Id,
                    money = money,
                    time = time,
                    localPlayerSave = localPlayerData,
                    upgrades = upgrades,
                };
                NetworkSend(saveMessage, ServerSendOptions.SendToPlayer(nctc.Id));
                SRMP.Log("sent world");

                Ammo currentHostAmmo = SceneContext.Instance.PlayerState.Ammo;
                NetworkAmmo netAmmo = new NetworkAmmo($"player_{savingID}",SceneContext.Instance.PlayerState._ammoSlotDefinitions);

                netAmmo._ammoModel.slots = NetworkAmmo.SRMPAmmoDataToSlots(playerData.ammo);

                // Spawn player for host
                try
                {
                    var player = Instantiate(Instance.onlinePlayerPrefab);
                    player.name = $"Player{nctc.Id}";
                    var netPlayer = player.GetComponent<NetworkPlayer>();
                    players.Add(nctc.Id, netPlayer);
                    netPlayer.id = nctc.Id;
                    player.SetActive(true);
                    var packet = new PlayerJoinMessage()
                    {
                        id = nctc.Id,
                        local = false
                    };
                    NetworkSend(packet, ServerSendOptions.SendToAllExcept(nctc.Id));
                }
                catch
                { }
            }
            catch (Exception ex)
            {
                clientToGuid.Remove(nctc.Id);
                SRMP.Error(ex.ToString());
            }
        }


        public static void ClientLeave()
        {
            SystemContext.Instance.SceneLoader.LoadSceneGroup(SystemContext.Instance.SceneLoader._mainMenuSceneGroup);
        }

        public void Connect(string ip, ushort port)
        {
            client = new Client();
            client.Connect($"{ip}:{port}");
        }
        public void Host(ushort port)
        {
            server = new Server();
            server.Start(port,10); 
        }

        System.Collections.IEnumerator UpdateNetwork()
        {
            while (true)
            {
                if (ServerActive()) server.Update();
                if (ClientActive()) client.Update();
                yield return null;
            }
        }

        private void FixedUpdate()
        {            
        }

        public static void Shutdown()
        {
            // How do i shut them down?????
            if (ServerActive()) server.Stop();
            if (ClientActive()) client.Disconnect();
            
            server = null;
            client = null;
        }
    }
}
