﻿
using System.Collections;
using System.Reflection;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using Il2CppMonomiPark.SlimeRancher.DataModel;
using UnityEngine;
using UnityEngine.SceneManagement;

using Il2CppMonomiPark.SlimeRancher.Event;
using Il2CppMonomiPark.SlimeRancher.SceneManagement;
using Il2CppMonomiPark.UnitPropertySystem;
using Il2CppMonomiPark.World;
using NewSR2MP.Networking.Patches;
using Riptide.Transports.Tcp;
using Riptide.Transports.Udp;
using Riptide.Utils;
using SR2E;
using SR2E.Managers;
using SR2E.Menus;
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
            RiptideLogger.Initialize(SRMP.Debug, SRMP.Log, SRMP.Warn, SRMP.Error, false);
            Instance = this;

            Message.MaxPayloadSize = 2048000;

        }



        private void Start()
        {
            SR2ECommandManager.RegisterCommand(new HostCommand());
            SR2ECommandManager.RegisterCommand(new JoinCommand());
            SR2ECommandManager.RegisterCommand(new SplitScreenDebugCommand());
            SR2ECommandManager.RegisterCommand(new ShowSRMPErrorsCommand());
        }

        // Prototype Player model
        public void GeneratePlayerBean()
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

            var material = Resources.FindObjectsOfTypeAll<Material>()
                .FirstOrDefault((mat) => mat.name == "slimePinkBase");
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

        public void GeneratePlayerModel()
        {
            var found = GameObject.Find("BeatrixMainMenu");

            onlinePlayerPrefab = Instantiate(found);


            onlinePlayerPrefab.AddComponent<NetworkPlayer>();
            onlinePlayerPrefab.AddComponent<TransformSmoother>();
            onlinePlayerPrefab.GetComponent<NetworkPlayer>().enabled = false;

            onlinePlayerPrefab.transform.localScale = Vector3.one * .85f;

            DontDestroyOnLoad(onlinePlayerPrefab);
        }

        public void SetupPlayerAnimations()
        {
            var animator = sceneContext.Player.GetComponent<Animator>();

            var prefabAnim = onlinePlayerPrefab.GetComponent<Animator>();
            prefabAnim.avatar = animator.avatar;
            prefabAnim.runtimeAnimatorController = animator.runtimeAnimatorController;
            
            if (ClientActive())
            {
                foreach (var player in players)
                {
                    var playerAnim = player.Value.gameObject.GetComponent<Animator>();
                    playerAnim.avatar = animator.avatar;
                    playerAnim.runtimeAnimatorController = animator.runtimeAnimatorController;
                }
            }
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

            var pedias = sceneContext.PediaDirector._pediaModel.unlocked;

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
            SRMP.Debug("A client is attempting to join!");


            clientToGuid.Add(nctc.Id, savingID);

            var newPlayer = !savedGame.savedPlayers.playerList.TryGetValue(savingID, out var playerData);
            if (newPlayer)
            {
                playerData = new NetPlayerV01();

                savedGame.savedPlayers.playerList.Add(savingID, playerData);
            }

            try
            {
                // Variables
                double time = sceneContext.TimeDirector.CurrTime();
                List<InitActorData> actors = new List<InitActorData>();
                HashSet<InitGordoData> gordos = new HashSet<InitGordoData>();
                List<InitPlayerData> initPlayers = new List<InitPlayerData>();
                List<InitPlotData> plots = new List<InitPlotData>();
                List<InitSwitchData> switches = new List<InitSwitchData>();
                List<string> pedias = new List<string>();


                foreach (var pedia in sceneContext.PediaDirector._pediaModel.unlocked)
                {
                    pedias.Add(pedia.name);
                }

                var upgrades = sceneContext.PlayerState._model.upgradeModel.upgradeLevels;

                // Actors
                foreach (var typeDict in sceneContext.GameModel.identifiablesByIdent)
                foreach (var a in typeDict.value)
                {
                    try
                    {
                        var data = new InitActorData()
                        {
                            id = a.actorId.Value,
                            ident = GetIdentID(a.ident),
                            pos = a.lastPosition,
                            scene = sceneGroupsReverse[a.sceneGroup.name]
                        };
                        actors.Add(data);
                    }
                    catch
                    {
                    }
                }



                // Gordos
                foreach (var g in Resources.FindObjectsOfTypeAll<GordoEat>())
                {
                    try
                    {
                        if (g.gameObject.hideFlags != HideFlags.HideAndDontSave && g.gameObject.scene.name != "")
                        {

                        }

                        InitGordoData data = new InitGordoData()
                        {
                            id = g.Id,
                            eaten = g.GordoModel.gordoEatCount
                        };
                        gordos.Add(data);
                    }
                    catch
                    {
                    }
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
                foreach (var landplot in sceneContext.GameModel.landPlots)
                {
                    var plot = landplot.value;
                    try
                    {



                        if (plot.siloAmmo._count != 0)
                        {

                            // TODO Get multiple ammo datas
                            string firstAmmoId = "";

                            foreach (var _ in plot.siloAmmo)
                            {
                                firstAmmoId = _.key;
                                break;
                            }

                            // Silos
                            InitSiloData s = new InitSiloData()
                            {
                                ammo = new HashSet<AmmoData>()
                            }; // Empty

                            var silo = plot.siloAmmo[firstAmmoId];

                            if (silo != null)
                            {
                                HashSet<AmmoData> ammo = new HashSet<AmmoData>();
                                var idx = 0;
                                foreach (var a in silo.slots)
                                {
                                    if (a != null)
                                    {
                                        var ammoSlot = new AmmoData()
                                        {
                                            slot = idx,
                                            id = GetIdentID(a.Id),
                                            count = a.Count,
                                        };
                                        ammo.Add(ammoSlot);
                                    }
                                    else
                                    {
                                        var ammoSlot = new AmmoData()
                                        {
                                            slot = idx,
                                            id = 9,
                                            count = 0,
                                        };
                                        ammo.Add(ammoSlot);
                                    }

                                    idx++;
                                }

                                s = new InitSiloData()
                                {
                                    slots = silo.slots.Count,
                                    ammo = ammo
                                };
                            }

                            int cropIdent = 9;
                            if (plot.resourceGrowerDefinition != null)
                            {
                                cropIdent = GetIdentID(plot.resourceGrowerDefinition._primaryResourceType);
                            }

                            var p = new InitPlotData()
                            {
                                id = plot.gameObj.GetComponent<LandPlotLocation>().Id,
                                type = plot.typeId,
                                upgrades = plot.upgrades,
                                cropIdent = cropIdent,

                                siloData = s,
                            };
                            plots.Add(p);
                        }
                    }
                    catch (Exception ex)
                    {
                        SRMP.Error($"Landplot failed to send! This will cause major desync.\n{ex}");
                    }
                }

                // Slime Gates || Ranch expansions
                List<InitAccessData> access = new List<InitAccessData>();
                foreach (var accessDoor in sceneContext.GameModel.doors)
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
                        id = ammoSlot.ID,
                        count = ammoSlot.Count,
                    };
                    playerAmmoData.Add(playerSlot);
                    i++;
                }

                LocalPlayerData localPlayerData = new LocalPlayerData()
                {
                    pos = playerData.position.Value,
                    rot = playerData.rotation.Value,
                    ammo = playerAmmoData,
                    sceneGroup = playerData.sceneGroup
                };


                // First time ever coding a local function.... its not good, shouldve just used a normal one
                List<string> GetListFromFogEvents(
                    Il2CppSystem.Collections.Generic.Dictionary<string, EventRecordModel.Entry> events)
                {
                    var ret = new List<string>();
                    foreach (var e in events)
                        ret.Add(e.key);
                    return ret;
                }
                List<string> fogEvents = new List<string>();
                if (sceneContext.eventDirector._model.table.TryGetValue("fogRevealed", out var table))
                    fogEvents = GetListFromFogEvents(table);


                var money = sceneContext.PlayerState._model.currency;

                var prices = new List<float>();
                foreach (var price in sceneContext.EconomyDirector._currValueMap)
                    prices.Add(price.value.CurrValue);

                foreach (var sw in sceneContext.GameModel.switches)
                    switches.Add(new InitSwitchData
                    {
                        id = sw.Key,
                        state = (byte)sw.Value.state,
                    });
                
                // Send save data.
                var saveMessage = new LoadMessage()
                {
                    initActors = actors,
                    initPlayers = initPlayers,
                    initPlots = plots,
                    initGordos = gordos,
                    initPedias = pedias,
                    initAccess = access,
                    initMaps = fogEvents,
                    playerID = nctc.Id,
                    money = money,
                    time = time,
                    localPlayerSave = localPlayerData,
                    upgrades = upgrades,
                    marketPrices = prices,
                    initSwitches = switches
                };
                
                NetworkSend(saveMessage, ServerSendOptions.SendToPlayer(nctc.Id));
                SRMP.Debug("The world data has been sent to the client!");

            }
            catch (Exception ex)
            {
                clientToGuid.Remove(nctc.Id);
                SRMP.Error(ex.ToString());
            }

            try
            {
                var newAmmo = CreateNewPlayerAmmo();
                newAmmo.RegisterAmmoPointer($"player_{savingID}");
                newAmmo._ammoModel.slots = new Il2CppReferenceArray<Ammo.Slot>(AmmoDataToSlotsSRMP(playerData.ammo));
            }
            catch (Exception ex)
            {
                SRMP.Error($"Post join error!\n{ex}");
            }
            
        }


        /// <summary>
        /// Shows SRMP errors on networking related stuff.
        /// </summary>
        public void ShowSRMPErrors()
        {
            ShowErrors = true;
        }

        public static void ClientLeave()
        {
            systemContext.SceneLoader.LoadSceneGroup(systemContext.SceneLoader._mainMenuSceneGroup);
        }

        public void Connect(string ip, ushort port)
        {
            if (ServerActive())
            {
                SRMP.Error("You can't join a server while hosting!");
                return;
            }

            var transport = new TcpClient();
            
            client = new Client(transport);
            client.Connect($"{ip}:{port}");

            client.TimeoutTime = 30000;
            
            client.Connected += OnConnectionSuccessful;
            client.ConnectionFailed += OnClientConnectionFail;
            client.Disconnected += OnClientDisconnect;
        }

        bool waitingForSave = false;
        bool waitingForSceneLoad = false;

        bool WaitForSaveData()
        {
            if (!waitingForSave) return false;
            if (latestSaveJoined == null) return false;
            if (!waitingForSceneLoad)
            {
                if (latestSaveJoined.localPlayerSave == null)
                {
                    SRMP.Error("Failed to get the client's player data from save!");
                    Shutdown();
                    return false;
                }

                gameContext.AutoSaveDirector.SavedGame.CreateNew("SR2MPLatestSave", "Multiplayer", -1, CreateEmptyGameSettingsModel());

                systemContext.SceneLoader.LoadSceneGroup(sceneGroups[latestSaveJoined.localPlayerSave.sceneGroup]);
                waitingForSceneLoad = true;
                return false;
            }

            if (systemContext.SceneLoader.IsSceneLoadInProgress) return false;


            isJoiningAsClient = true;


            SRMP.Debug("Received the save data!");

            if (ServerActive())
            {
                server.Stop();
                server = null;
            }

            ammoByPlotID.Clear();

            MelonCoroutines.Start(Main.OnSaveLoaded());

            if (systemContext.SceneLoader.IsCurrentSceneGroupDefault())
            {
                Main.OnRanchSceneGroupLoaded(SceneContext.Instance);
            }

            isJoiningAsClient = false;
            waitingForSceneLoad = false;

            return true;
        }

        public void OnConnectionSuccessful(object? sender, EventArgs args)
        {
            client.TimeoutTime = 10000;

            client.Connection.MaxSendAttempts = 75;
            var saveRequestPacket = new ClientUserMessage()
            {
                guid = Main.data.Player,
                name = Main.data.Username
            };
            NetworkSend(saveRequestPacket);

            AutoSaveDirectorSaveGame.isClient = true;
            waitingForSave = true;
        }

        public void OnClientDisconnect(object? sender, EventArgs args)
        {
            systemContext.SceneLoader.LoadMainMenuSceneGroup();
            Shutdown();
        }
        public void OnClientConnectionFail(object? sender, EventArgs args)
        {
            Shutdown();
        }

        public void Host(ushort port)
        {
            if (!SystemContext.Instance.SceneLoader.IsCurrentSceneGroupGameplay())
            {
                SRMP.Error("You can't host a server while not being in a world!");
                return;
            }

            if (ClientActive())
            {
                SRMP.Error("You can't host a server while in one!");
                return;
            }

            var transport = new TcpServer();
            
            server = new Server(transport);
            server.Start(port, 10);

            server.TimeoutTime = 30000;

            RegisterAllSilos();

            StartHosting();
        }

        public bool loadingZone = false;
        
        private void UpdateNetwork()
        {
            try
            {
                if (ServerActive()) server.Update();
                if (ClientActive()) client.Update();
            }
            catch (Exception ex)
            {
                SRMP.Error($"Network error!\n{ex}");
            }
        }

        private float networkUpdateInterval = .15f;
        private float nextNetworkUpdate = -1f;

        public int sceneLoadingFrameCounter = -1;
        
        void Update()
        {
            if (nextNetworkUpdate <= Time.unscaledTime)
            {
                UpdateNetwork();
                nextNetworkUpdate = Time.unscaledTime + networkUpdateInterval;
            }
            
            if (WaitForSaveData())
            {
                waitingForSave = false;
            }
            
            if (WaitForZoneLoad())
            {
                loadingZone = false;
            }
            
            if (systemContext.SceneLoader.IsSceneLoadInProgress)
                if (sceneLoadingFrameCounter >= 8)
                    loadingZone = true;
                else
                    sceneLoadingFrameCounter++;
            else
                sceneLoadingFrameCounter = 0;
        }

        bool WaitForZoneLoad()
        {
            if (!loadingZone)
                return false;
            if (systemContext.SceneLoader.IsSceneLoadInProgress)
                return false;
            if (!systemContext.SceneLoader._previousGroup._isGameplay)
                return false;
            if (!systemContext.SceneLoader._currentSceneGroup._isGameplay)
                return false;

            IEnumerable<Il2CppSystem.Collections.Generic.Dictionary<ActorId, IdentifiableModel>.Entry> actors = null;
            
            if (ClientActive())
                actors = sceneContext.GameModel.identifiables._entries.Where(x =>
                    x != null &&
                    x.value != null &&
                    x.value.TryCast<ActorModel>() != null && 
                    x.value.sceneGroup == systemContext.SceneLoader._currentSceneGroup);
            else if (ServerActive())
                actors = sceneContext.GameModel.identifiables._entries.Where(x =>
                    x != null &&
                    x.value != null &&
                    x.value.TryCast<ActorModel>() != null && 
                    x.value.sceneGroup == systemContext.SceneLoader._currentSceneGroup &&
                    multiplayerSpawnedActorsIDs.Contains(x.key.Value));
            else
                return true;
            
            MelonCoroutines.Start(LoadZoneActors(actors.ToList()));
            
            return true;
        }

        IEnumerator LoadZoneActors(List<Il2CppSystem.Collections.Generic.Dictionary<ActorId, IdentifiableModel>.Entry> actorEntries)
        {
            int yeildCounter = 0;
            int i = 0;
            foreach (var t in actorEntries)
            {
                var actor = InstantiateActorFromModel(t.value.Cast<ActorModel>());
                actor.transform.position = t.value.lastPosition;
                
                yeildCounter++;
                i++;
                if (i >= actorEntries.Count)
                    break;
                if (yeildCounter == 50)
                {
                    yeildCounter = 0;
                    yield return null;
                }
            }
        }
        public static void Shutdown()
        {
            if (ServerActive()) server.Stop();
            if (ClientActive()) client.Disconnect();
            
            server = null;
            client = null;
            
            EraseValues();
        }
    }
}
