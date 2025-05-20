
using System.Collections;
using System.Reflection;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using Il2CppMonomiPark.SlimeRancher.DataModel;
using UnityEngine;
using UnityEngine.SceneManagement;

using Il2CppMonomiPark.SlimeRancher.Event;
using Il2CppMonomiPark.SlimeRancher.Persist;
using Il2CppMonomiPark.SlimeRancher.Platform.Steam;
using Il2CppMonomiPark.SlimeRancher.SceneManagement;
using Il2CppMonomiPark.UnitPropertySystem;
using Il2CppMonomiPark.World;
using Il2CppTMPro;
using NewSR2MP.EpicSDK;
using NewSR2MP.Patches;
using SR2E;
using SR2E.Managers;
using SR2E.Menus;
using UnityEngine.Serialization;

namespace NewSR2MP
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
            InitializeCommandExtensions();

            Instance = this;

            gameObject.AddComponent<NetworkUI>();
            gameObject.AddComponent<EpicApplication>();
            NetworkHandler.Initialize();
        }
        private void Start()
        {
            SR2ECommandManager.RegisterCommand(new HostCommand());
            SR2ECommandManager.RegisterCommand(new JoinCommand());
            //SR2ECommandManager.RegisterCommand(new SplitScreenDebugCommand());
            SR2ECommandManager.RegisterCommand(new DevModifySyncTimerCommand());
            SR2ECommandManager.RegisterCommand(new ShowSRMPErrorsCommand());
            SR2ECommandManager.RegisterCommand(new GivePlayerCommand());
        }

        public void GeneratePlayerModel()
        {
            var found = GameObject.Find("BeatrixMainMenu");

            onlinePlayerPrefab = Instantiate(found);

            onlinePlayerPrefab.SetActive(false);

            var comp = onlinePlayerPrefab.AddComponent<NetworkPlayer>();
            onlinePlayerPrefab.AddComponent<TransformSmoother>();

            var text = new GameObject("Username") { transform = { parent = onlinePlayerPrefab.transform } };
            onlinePlayerPrefab.transform.localScale = Vector3.one * .85f;

            comp.enabled = false;
            comp.usernamePanel = text.AddComponent<TextMesh>();

            text.transform.localPosition = Vector3.up * 3f;
            text.transform.localEulerAngles = Vector3.up * 180f;

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
                    var playerAnim = player.gameObject.gameObject.GetComponent<Animator>();
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
        public static void PlayerJoin(ushort conn, Guid savingID, string username)
        {
            SRMP.Debug("A client is attempting to join!");

            clientToGuid.Add(conn, savingID);

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

                var upgrades = new Dictionary<byte, sbyte>();
                var upgModel = sceneContext.PlayerState._model.upgradeModel;
                foreach (var upg in upgModel.upgradeDefinitions.items)
                {
                    upgrades.Add((byte)upg._uniqueId, (sbyte)upgModel.GetUpgradeLevel(upg));
                }

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

                        if (actors.FirstOrDefault(x => x == data) == null)
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
                            id = g._id,
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
                    if (player.playerID != 65535 && player.playerID != conn)
                    {

                        var p = new InitPlayerData()
                        {
                            id = player.playerID,
                            username = playerUsernamesReverse[player.playerID]
                        };
                        initPlayers.Add(p);
                    }
                }

                var p2 = new InitPlayerData()
                {
                    id = 65535,
                    username = Main.data.Username
                };
                initPlayers.Add(p2);



                // Plots
                foreach (var landplot in sceneContext.GameModel.landPlots)
                {
                    var plot = landplot.value;
                    try
                    {
                        Dictionary<string, InitSiloData> silos = new();
                        foreach (var siloStorage in plot.gameObj.GetComponentsInChildren<SiloStorage>())
                        {
                            // Silos

                            InitSiloData s = new InitSiloData
                            {
                                ammo = new HashSet<AmmoData>()
                            }; // Empty


                            HashSet<AmmoData> ammo = new HashSet<AmmoData>();
                            var idx = 0;
                            foreach (var slot in siloStorage.LocalAmmo.Slots)
                            {
                                if (slot != null)
                                {
                                    var ammoSlot = new AmmoData()
                                    {
                                        slot = idx,
                                        id = GetIdentID(slot._id),
                                        count = slot.Count,
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

                            s = new InitSiloData
                            {
                                slots = siloStorage.LocalAmmo.Slots.Count,
                                ammo = ammo
                            };
                            
                            silos.Add(siloStorage.AmmoSetReference.name, s);
                        }

                        int cropIdent = 9;
                        if (plot.resourceGrowerDefinition != null)
                        {
                            cropIdent = GetIdentID(plot.resourceGrowerDefinition._primaryResourceType);
                        }

                        var p = new InitPlotData()
                        {
                            id = landplot.key,
                            type = plot.typeId,
                            upgrades = plot.upgrades,
                            cropIdent = cropIdent,

                            siloData = silos,
                        };
                        plots.Add(p);
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

                Dictionary<int, int> refineryItems = new Dictionary<int, int>();
                foreach (var item in sceneContext.GadgetDirector._model._itemCounts)
                    refineryItems.Add(GetIdentID(item.Key), item.Value);

                Dictionary<int, TreasurePod.State> pods = new();
                foreach (var pod in sceneContext.GameModel.pods)
                {
                    pods.Add(int.Parse(pod.key.Replace("pod","")), pod.value.state);
                }
                
                // Send save data.
                var saveMessage = new LoadMessage()
                {
                    initActors = actors,
                    initPlayers = initPlayers,
                    initPlots = plots,
                    initGordos = gordos,
                    initPedias = pedias,
                    initAccess = access, initPods = pods, initSwitches = switches, money = money, time = time,
                    initMaps = fogEvents, playerID = conn, localPlayerSave = localPlayerData, upgrades = upgrades,
                    marketPrices = prices, refineryItems = refineryItems,
                };

                NetworkSend(saveMessage, ServerSendOptions.SendToPlayer(conn));
                SRMP.Debug("The world data has been sent to the client!");

            }
            catch (Exception ex)
            {
                clientToGuid.Remove(conn);
                SRMP.Error(ex.ToString());
            }

            try
            {
                var newAmmo = CreateNewPlayerAmmo();
                newAmmo.RegisterAmmoPointer($"player_{savingID}");

                var savedAmmo = playerData.ammo;
                if (savedAmmo.Count < 7)
                {
                    savedAmmo = new Il2CppSystem.Collections.Generic.List<AmmoDataV01>();
                    for (var i = 0; i < 7; i++)
                    {
                        savedAmmo.Add(new AmmoDataV01()
                        {
                            Appearance = SlimeAppearance.AppearanceSaveSet.NONE,
                            Count = 0,
                            ID = 9,
                        });
                    }
                }

                newAmmo._ammoModel.slots = new Il2CppReferenceArray<Ammo.Slot>(AmmoDataToSlotsSRMP(savedAmmo));
                newAmmo._ammoModel.slotMaxCountFunction = sceneContext.PlayerState.Ammo._ammoModel.slotMaxCountFunction;

                int slotC = 0;
                foreach (var slot in newAmmo._ammoModel.slots)
                {
                    slot.Definition = newAmmo._ammoSlotDefinitions[slotC];
                    slot._maxCountValue = sceneContext.PlayerState.Ammo.Slots[slotC]._maxCountValue;
                    slot._isUnlockedValue = sceneContext.PlayerState.Ammo.Slots[slotC]._isUnlockedValue;
                    slotC++;
                }
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

        public void Connect(string lobby)
        {
            if (ServerActive())
            {
                SRMP.Error("You can't join a server while hosting!");
                return;
            }
            
            EpicApplication.Instance.Lobby.JoinLobby(lobby);
        }

        public static void BeginWaitingForSaveData() => Instance.waitingForSave = true;
        
        
        bool waitingForSave = false;
        bool waitingForSceneLoad = false;

        bool WaitForSaveData()
        {
            if (!waitingForSave) return false;
            if (latestSaveJoined == null) return false;
            if (!waitingForSceneLoad)
            {
                if (latestSaveJoined?.localPlayerSave == null)
                {
                    //SRMP.Error("Failed to get the client's player data from save!");
                    //Shutdown();
                    return false;
                }

                gameContext.AutoSaveDirector.SavedGame.CreateNew("SR2MPLatestSave", "Multiplayer", -1,
                    CreateEmptyGameSettingsModel());

                systemContext.SceneLoader.LoadSceneGroup(sceneGroups[latestSaveJoined.localPlayerSave.sceneGroup]);
                waitingForSceneLoad = true;
                return false;
            }

            clientLoading = true;
            
            handlingPacket = true;

            if (systemContext.SceneLoader.IsSceneLoadInProgress) return false;
            
            handlingPacket = false;

            isJoiningAsClient = true;


            SRMP.Debug("Received the save data!");

            ammoByPlotID.Clear();

            MelonCoroutines.Start(Main.OnSaveLoaded());

            if (systemContext.SceneLoader.IsCurrentSceneGroupDefault())
            {
                Main.OnRanchSceneGroupLoaded();
            }

            isJoiningAsClient = false;
            waitingForSceneLoad = false;

            return true;
        }

        public void OnConnectionSuccessful(object? sender, EventArgs args)
        {
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

        public void Host()
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

            EpicApplication.Instance.Lobby.CreateLobby();
        }

        public bool loadingZone = false;

        

        private float networkUpdateInterval = .15f;
        private float nextNetworkUpdate = -1f;

        public int sceneLoadingFrameCounter = -1;

        void Update()
        {

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
                    x.value.Transform == null);
            else
                return true;

            MelonCoroutines.Start(LoadZoneActors(actors.ToList()));

            return true;
        }

        IEnumerator LoadZoneActors(
            List<Il2CppSystem.Collections.Generic.Dictionary<ActorId, IdentifiableModel>.Entry> actorEntries)
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
            EpicApplication.Instance.Lobby.LeaveLobby();
            EpicApplication.Instance.Lobby.DestroyLobby();
            
            EpicApplication.Instance.Lobby.NetworkClient?.Shutdown();
            EpicApplication.Instance.Lobby.NetworkServer?.Shutdown();
            
            EraseValues();
        }
    }
}
