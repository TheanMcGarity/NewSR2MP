using System.Collections;
using System.Reflection;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using Il2CppMonomiPark.ScriptedValue;
using Main = NewSR2MP.Main;



using Il2CppMonomiPark.SlimeRancher.DataModel;
using Il2CppMonomiPark.SlimeRancher.Event;
using Il2CppMonomiPark.SlimeRancher.Options;
using Il2CppMonomiPark.SlimeRancher.Pedia;
using Il2CppMonomiPark.SlimeRancher.Player.CharacterController;
using Il2CppMonomiPark.SlimeRancher.UI.Map;
using Il2CppMonomiPark.SlimeRancher.UI.Refinery;
using Il2CppMonomiPark.SlimeRancher.Util.Extensions;
using Il2CppMonomiPark.SlimeRancher.World;
using Il2CppMonomiPark.World;
using Newtonsoft.Json;
using SR2E;
using SR2E.Buttons;
using SR2E.Commands;
using SR2E.Expansion;
using SR2E.Managers;
using UnityEngine;
using BuildInfo = MelonLoader.Properties.BuildInfo;
using Exception = System.Exception;
using Guid = System.Guid;


namespace NewSR2MP
{
    public class HostCommand : SR2ECommand
    {
        public override bool Execute(string[] args)
        {
            MultiplayerManager.Instance.Host();
            return true;
        }

        public override string ID => "host";
        public override string Usage => "host <port>";
    }
    public class DevModifySyncTimerCommand : SR2ECommand
    {
        public override List<string> GetAutoComplete(int argIndex, string[] args)
        {
            return Enum.GetNames<SyncTimerType>().ToList();
        }

        public override bool Execute(string[] args)
        {
            SetTimer(Enum.Parse<SyncTimerType>(args[0]), float.Parse(args[1]));
            
            return true;
        }

        public override string ID => "dev_modifysynctimer";
        public override string Usage => "dev_modifysynctimer <synctype>";
    }
    public class GivePlayerCommand : SR2ECommand
    {
        public override string ID => "playergive";
        public override string Usage => "playergive <playerid> <item> <count>";
        
        public override List<string> GetAutoComplete(int argIndex, string[] args)
        {
            if (argIndex == 0)
            {
                var list = new List<string>();
                foreach (var name in playerUsernames)
                {
                    list.Add(name.Key.Replace(" ", ""));
                }
                return list;
            }
            if (argIndex == 1)
                return getVaccableListByPartialName(args == null ? null : args[0], true);
            if (argIndex == 2)
                return new List<string> { "1", "5", "10", "20", "30", "50" };

            return null;
        }

        public override bool Execute(string[] args)
        {
            if (!args.IsBetween(2,3)) return SendUsage();
            if (!inGame) return SendLoadASaveFirst();

            if (!ServerActive())
                return false;

            Guid player = clientToGuid[playerUsernames.First(x => x.Key.Replace(" ", "") == args[0]).Value];
            string identifierTypeName = args[1];
            IdentifiableType type = getIdentByName(identifierTypeName);
            if (type == null) return SendNotValidIdentType(identifierTypeName);
            string itemName = type.getName();
            if (type.isGadget()) return SendIsGadgetNotItem(itemName);
        
            int amount = 1;
            if (args.Length == 3) if(!this.TryParseInt(args[2], out amount,0, false)) return false;

            for (int i = 0; i < amount; i++)
                ammoByPlotID[$"player_{player}"].MaybeAddToSlot(type, null);

            return true;
        }
    }
    public class ShowSRMPErrorsCommand : SR2ECommand
    {
        public override bool Execute(string[] args)
        {
            ShowErrors = true;
            return true;
        }

        public override string ID => "showsrmperrors";
        public override string Usage => "showsrmperrors";
    }
    public class JoinCommand : SR2ECommand
    {
        public override bool Execute(string[] args)
        {
            MultiplayerManager.Instance.Connect(args[0]);
            return true;
        }

        public override string ID => "join";
        public override string Usage => "join <code>";
    }
    public class SplitScreenDebugCommand : SR2ECommand
    {
        public override bool Execute(string[] args)
        {
            MultiplayerManager.Instance.Connect(args[0]);
            return true;
        }

        public override string ID => "debug_ss";
        public override string Usage => "debug_ss <code>";
    }
    public static class Extentions
    {
        public static void RemoveComponent<T>(this GameObject go) where T : UnityEngine.Component => UnityEngine.Object.Destroy(go.GetComponent<T>());

        public static bool TryRemoveComponent<T>(this GameObject go) where T : UnityEngine.Component
        {
            T component = go.GetComponent<T>();
            
            if (component != null)
            {
                go.RemoveComponent<T>();
                return true;
            }
            
            return false;
        }
    }
    
    public class Main : SR2EExpansionV1
    {

        public override void OnNormalInitializeMelon()
        {
            SR2EEntryPoint.RegisterOptionMenuButtons += RegisterSR2ESettings;
            SR2ELanguageManger.AddLanguages(LoadTextFile("NewSR2MP.translations.csv"));
        }

        internal static void RegisterSR2ESettings(object o, EventArgs e)
        {
            scriptedAutoHostPort = CustomSettingsCreator.CreateScriptedInt(0);
            
            CustomSettingsCreator.Create(CustomSettingsCreator.BuiltinSettingsCategory.GameSettings, AddTranslationFromSR2E("setting.mpautohost", "b.autohost", "UI"),AddTranslationFromSR2E("setting.mpautohost.desc", "b.autohostdescription", "UI"), "autoHost", 0, true, false, false, ((_,_,_) => { }), new CustomSettingsCreator.OptionValue("off",AddTranslationFromSR2E("setting.mpautohost.off", "b.autohostoff", "UI"),scriptedAutoHostPort, 0), new CustomSettingsCreator.OptionValue("val1",AddTranslationFromSR2E("setting.mpautohost.val1", "b.autohostval1", "UI"),scriptedAutoHostPort, 7777), new CustomSettingsCreator.OptionValue("val1",AddTranslationFromSR2E("setting.mpautohost.val2", "b.autohostval2", "UI"),scriptedAutoHostPort, 16500));
        }

        public override void OnEarlyInitializeMelon()
        {
            var eosAudioPath = Path.Combine(Application.dataPath, "..", "UserLibs", "xaudio2_9redist.dll");
            if (!File.Exists(EOS_SDK_PATH))
            {
                File.WriteAllBytes(EOS_SDK_PATH, ExtractResource("NewSR2MP.SatyEOS.EpicSDK.Libs.EOSSDK-Win64-Shipping.dll"));
            }
            //if (!File.Exists(eosAudioPath))
            //{
            //    File.WriteAllBytes(eosAudioPath, ExtractResource("NewSR2MP.SatyEOS.SRMP.EpicSDK.Libs.xaudio2_9redist.dll"));
            //}
        }

        public static Main modInstance;
        
        public static AssetBundle ui;
        

        public override void OnLateInitializeMelon()
        {
            modInstance = this;
            LoadData();
            Application.add_quitting(new System.Action(SaveData));
        }
        public static UserData data;

        public static readonly string DataPath = Path.Combine(Application.persistentDataPath, "MultiplayerData.json");

        private void LoadData()
        {
            if (File.Exists(Path.Combine(Application.persistentDataPath, "MultiplayerData.json")))
            {
                try { data = JsonConvert.DeserializeObject<UserData>(File.ReadAllText(DataPath)); }
                catch { CreateData(); }
            }
            else
                CreateData();
        }
        private void CreateData()
        {
            var dat = new UserData();

            var rand = new System.Random();

            var i = rand.Next(100000, 999999);

            var guid = Guid.NewGuid();
            var guid2 = Guid.NewGuid();

            dat.Username = $"User{i}";
            dat.Player = guid;
            dat.Debug_Player2 = guid2;
            
            data = dat;

            SaveData();
        }
        public void SaveData()
        {
            File.WriteAllText(DataPath, JsonConvert.SerializeObject(data, Formatting.Indented));
        }

        internal AssetBundle modifiedGameUI;

        public static GameObject modSettingsUI;

        public static Il2CppSystem.Collections.Generic.IEnumerable<LandPlot.Upgrade> ConvertToIEnumerable(Il2CppSystem.Collections.Generic.HashSet<LandPlot.Upgrade> upgrades)
        {
            var list = new Il2CppSystem.Collections.Generic.List<LandPlot.Upgrade>();

            foreach (var upgrade in upgrades) list.Add(upgrade);
            
            return list.Cast<Il2CppSystem.Collections.Generic.IEnumerable<LandPlot.Upgrade>>();
        }

        public static void OnRanchSceneGroupLoaded()
        {
            var save = latestSaveJoined;

            foreach (var plot in save.initPlots)
            {
                try
                {
                    var model = sceneContext.GameModel.landPlots[plot.id];
                    
                    model.typeId = plot.type;
                    foreach (var upgrade in plot.upgrades)
                    {
                        model.upgrades.Add(upgrade);
                    }
                    
                    handlingPacket = true;
                    model.gameObj.GetComponent<LandPlotLocation>().Replace(
                        model.gameObj.GetComponentInChildren<LandPlot>(),
                        GameContext.Instance.LookupDirector._plotPrefabDict[plot.type]);
                    handlingPacket = false;
                    
                    var lp = model.gameObj.GetComponentInChildren<LandPlot>();
                    lp.ApplyUpgrades(ConvertToIEnumerable(plot.upgrades), false);
                    
                    var silos = model.gameObj.GetComponentsInChildren<SiloStorage>();
                    foreach (var silo in silos)
                    {
                        silo.RegisterAmmoPointer();

                        foreach (var ammo in plot.siloData[silo.AmmoSetReference.name].ammo)
                        {
                            try
                            {
                                if (!(ammo.count == 0 || ammo.id == 9))
                                {
                                    silo.LocalAmmo.Slots[ammo.slot]._count = ammo.count;
                                    silo.LocalAmmo.Slots[ammo.slot]._id = identifiableTypes[ammo.id];
                                }
                                else
                                {
                                    silo.LocalAmmo.Slots[ammo.slot]._count = 0;
                                    silo.LocalAmmo.Slots[ammo.slot]._id = null;
                                }
                            }
                            catch
                            {
                            }
                        }
                    }

                    if (plot.type != Il2Cpp.LandPlot.Id.GARDEN) continue;
                    
                    GardenCatcher gc = lp.transform.GetComponentInChildren<GardenCatcher>(true);

                    var cropObj = UnityEngine.Object.Instantiate(
                        lp.HasUpgrade(Il2Cpp.LandPlot.Upgrade.DELUXE_GARDEN)
                            ? gc._deluxeDict[identifiableTypes[plot.cropIdent]]
                            : gc._plantableDict[identifiableTypes[plot.cropIdent]], lp.transform.position,
                        lp.transform.rotation);

                    model.resourceGrowerDefinition =
                        gameContext.AutoSaveDirector.resourceGrowers.items._items.FirstOrDefault(x =>
                            x._primaryResourceType == identifiableTypes[plot.cropIdent]);
                    
                    handlingPacket = true;
                    if (gc.CanAccept(identifiableTypes[plot.cropIdent]))
                        lp.Attach(cropObj, true, true);
                    handlingPacket = false;
                }
                catch (Exception e)
                {
                    SRMP.Log($"Error in world load for plot({plot.id}).\n{e}");
                }
            }
        }
        public static IEnumerator OnSaveLoaded()
        {
            if (ClientActive() && !ServerActive())
            {
                if (sceneContext.GetComponent<NetworkTimeDirector>())
                    sceneContext.gameObject.RemoveComponent<NetworkTimeDirector>();

                LoadMessage save = latestSaveJoined;

                sceneContext.player.GetComponent<SRCharacterController>().Position = save.localPlayerSave.pos;
                sceneContext.player.GetComponent<SRCharacterController>().Rotation =
                    Quaternion.Euler(save.localPlayerSave.rot);

                sceneContext.TimeDirector._worldModel.worldTime = save.time;
                sceneContext.TimeDirector._timeFactor = 0;
                
                actors.Clear();
                sceneContext.GameModel.identifiables.Clear();

                bool destroyedExistingActors = false;

                if (!destroyedExistingActors)
                {
                    
                    foreach (var a in Resources.FindObjectsOfTypeAll<Identifiable>())
                    {
                        if (string.IsNullOrEmpty(a.gameObject.scene.name)) continue;

                        try
                        {
                            if (!a.identType.IsSceneObject && !a.identType.IsPlayer)
                            {
                                handlingPacket = true;
                                sceneContext.GameModel.DestroyIdentifiableModel(sceneContext.GameModel.identifiables[a.GetActorId()]);
                                UnityEngine.Object.Destroy(a.gameObject);
                                handlingPacket = false;
                            }
                        }
                        catch
                        {
                        }
                    }
                    destroyedExistingActors = true;
                    yield return null;
                }

                int actorYieldCounter = 0;
                int actorTotalCounter = 0;
                
                while (actorTotalCounter < save.initActors.Count)
                {
                    try
                    {
                        InitActorData newActor = save.initActors[actorTotalCounter];
                        bool gotIdent = identifiableTypes.TryGetValue(newActor.ident, out var ident);
                        if (!gotIdent)
                        {
                            actorTotalCounter++;
                            continue;
                        }

                        if (!ident.IsSceneObject && !ident.IsPlayer)
                        {
                            SRMP.Debug($"Save data loading actor from save.initActors[{actorTotalCounter}]");
                            handlingPacket = true;
                            var obj2 = RegisterActor(new ActorId(newActor.id), ident, newActor.pos, Quaternion.identity, sceneGroups[newActor.scene]);
                            
                            obj2.AddComponent<NetworkActor>();
                            obj2.AddComponent<TransformSmoother>();
                            obj2.AddComponent<NetworkActorOwnerToggle>();

                            obj2.transform.position = newActor.pos;
                            obj2.GetComponent<TransformSmoother>().nextPos = newActor.pos;
                            obj2.GetComponent<NetworkActor>().IsOwned = false;

                            if (!actors.TryAdd(newActor.id, obj2.GetComponent<NetworkActor>()))
                                UnityEngine.Object.Destroy(obj2);

                            handlingPacket = false;
                        }
                    }
                    catch (Exception e)
                    {
                        SRMP.Error(e.ToString());
                    }
                    
                    actorTotalCounter++;
                    
                    actorYieldCounter++;
                    if (actorYieldCounter == 50)
                    {
                        actorYieldCounter = 0;
                        yield return null;
                    }
                }

                bool completedPlayers = false;

                if (!completedPlayers)
                {
                    foreach (var player in save.initPlayers)
                    {
                        try
                        {
                            var playerobj = UnityEngine.Object.Instantiate(MultiplayerManager.Instance.onlinePlayerPrefab);
                            playerobj.name = $"Player{player.id}";
                            var netPlayer = playerobj.GetComponent<NetworkPlayer>();
                            players.Add(new Globals.PlayerState()
                            {
                                playerID = (ushort)player.id,
                                epicID = null,
                                gameObject = netPlayer,
                            });
                            netPlayer.id = player.id;
                            playerobj.SetActive(true);
                            UnityEngine.Object.DontDestroyOnLoad(playerobj);
                            
                            netPlayer.usernamePanel = playerobj.transform.GetComponentInChildren<TextMesh>();
                            netPlayer.usernamePanel.text = player.username;
                            netPlayer.usernamePanel.characterSize = 0.2f;
                            netPlayer.usernamePanel.anchor = TextAnchor.MiddleCenter;
                            netPlayer.usernamePanel.fontSize = 24;
                        }
                        catch
                        {
                        }
                    }

                    completedPlayers = true;
                
                    yield return null;
                }

                bool completedGordos = false;
                if (!completedGordos)
                {
                    foreach (var gordo in save.initGordos)
                    {
                        try
                        {
                            if (!sceneContext.GameModel.gordos.TryGetValue(gordo.id, out var gm))
                                sceneContext.GameModel.gordos.Add(gordo.id, new GordoModel()
                                {
                                    fashions = new Il2CppSystem.Collections.Generic.List<IdentifiableType>(),
                                    gordoEatCount = gordo.eaten,
                                    gordoSeen = true,
                                    identifiableType = identifiableTypes[gordo.ident],
                                    gameObj = null,
                                    GordoEatenCount = gordo.eaten,
                                    targetCount = gameContext.LookupDirector._gordoDict[identifiableTypes[gordo.ident]].GetComponent<GordoEat>().TargetCount,
                                });

                            if (gordo.eaten <= -1 || gordo.eaten >= gm.targetCount)
                            {
                                gm.gameObj.SetActive(false);
                            }

                            gm.gordoEatCount = gordo.eaten;
                        }
                        catch (Exception e)
                        {
                            SRMP.Error($"Error while loading gordo from save data!\n{e}");
                        }
                    }

                    completedGordos = true;

                    yield return null;
                }


                sceneContext.PlayerState._model.currency = save.money;

                bool completedPedia = false;
                if (!completedPedia)
                {
                    var pediaEntries = new Il2CppSystem.Collections.Generic.HashSet<PediaEntry>();
                    foreach (var pediaEntry in save.initPedias)
                    {
                        pediaEntries.Add(Globals.pediaEntries[pediaEntry]);
                    }

                    sceneContext.PediaDirector._pediaModel.unlocked = pediaEntries;

                    completedPedia = true;
                    
                    yield return null;
                }
                


                var np = sceneContext.player.AddComponent<NetworkPlayer>();
                np.id = save.playerID;

                bool completedAccessDoors = false;

                if (!completedAccessDoors)
                {
                    foreach (var access in save.initAccess)
                    {
                        GameModel gm = sceneContext.GameModel;
                        AccessDoorModel adm = gm.doors[access.id];
                        if (access.open)
                        {
                            adm.state = AccessDoor.State.OPEN;
                        }
                        else
                        {
                            adm.state = AccessDoor.State.LOCKED;
                        } // Couldnt figure out the thingy, i tried: access.open ? AccessDoor.State.OPEN : AccessDoor.State.LOCKED
                    }
                    
                    completedAccessDoors = true;
                    
                    yield return null;
                }
                

                int marketPriceCount = 0;
                foreach (var price in sceneContext.EconomyDirector._currValueMap)
                {
                    try
                    {
                        price.Value.CurrValue = save.marketPrices[marketPriceCount];
                        marketPriceCount++;
                    } catch { }
                }
                marketUI?.EconUpdate();
                
                bool completedMaps = false;

                if (!completedMaps)
                {
                    var eventDirModel = sceneContext.eventDirector._model;
                    foreach (var map in save.initMaps)
                    {
                        if (!eventDirModel.table.TryGetValue("fogRevealed", out var table))
                        {
                            eventDirModel.table.Add("fogRevealed", new Il2CppSystem.Collections.Generic.Dictionary<string, EventRecordModel.Entry>());
                            table = eventDirModel.table["fogRevealed"];
                        }
                        table.Add(map, new EventRecordModel.Entry
                        {
                            count = 1,
                            createdRealTime = 0,
                            createdGameTime = 0,
                            dataKey = map,
                            eventKey = "fogRevealed",
                            updatedRealTime = 0,
                            updatedGameTime = 0,
                        });
                        
                        var activator = Resources.FindObjectsOfTypeAll<MapNodeActivator>().FirstOrDefault(x => x._fogRevealEvent._dataKey == map);
                        if (activator)
                            activator.StartCoroutine(activator.ActivateHologramAnimation());
                    }
                }
                bool completedSwitches = false;

                if (!completedSwitches)
                {
                    foreach (var sw in save.initSwitches)
                    {
                        if (sceneContext.GameModel.switches.TryGetValue(sw.id, out var model))
                        {
                            model.state = (SwitchHandler.State)sw.state;
                            if (model.gameObj)
                            {
                                handlingPacket = true;
                    
                                if (model.gameObj.TryGetComponent<WorldStatePrimarySwitch>(out var primary))
                                    primary.SetStateForAll((SwitchHandler.State)sw.state, true);
                    
                                if (model.gameObj.TryGetComponent<WorldStateSecondarySwitch>(out var secondary))
                                    secondary.SetState((SwitchHandler.State)sw.state, true);
                    
                                if (model.gameObj.TryGetComponent<WorldStateInvisibleSwitch>(out var invisible))
                                    invisible.SetStateForAll((SwitchHandler.State)sw.state, true);
                    
                                handlingPacket = false;
                            }
                            else
                            {
                                model = new WorldSwitchModel
                                {
                                    gameObj = null,
                                    state = (SwitchHandler.State)sw.state,
                                };
                                sceneContext.GameModel.switches.Add(sw.id, model);
                            }
                        }
                        else
                        {
                            model = new WorldSwitchModel
                            {
                                gameObj = null,
                                state = (SwitchHandler.State)sw.state,
                            };
                            sceneContext.GameModel.switches.Add(sw.id, model);
                        }
                    }
                    
                    completedSwitches = true;
                    yield return null;
                }
                
                bool completedRefinery = false;

                if (!completedRefinery)
                {
                    Il2CppSystem.Collections.Generic.Dictionary<IdentifiableType, int> refineryItems = new();
                    foreach (var item in save.refineryItems)
                    {
                        var gotIdent = identifiableTypes.TryGetValue(item.Key, out var ident);
                        if (gotIdent)
                        {
                            SRMP.Debug($"Refinery item: {ident.name} - Count: {item.Value}");
                            refineryItems.Add(ident, item.Value);
                        }
                    }
                    sceneContext.GadgetDirector._model._itemCounts = refineryItems;
                    
                    completedRefinery = true;
                    
                    yield return null;
                }
                
                bool completedUpgrades = false;

                if (!completedUpgrades)
                {
                    sceneContext.PlayerState._model.upgradeModel.upgradeLevels = new Il2CppSystem.Collections.Generic.Dictionary<int, int>();

                    foreach (var upgrade in save.upgrades)
                    {
                        sceneContext.PlayerState._model.upgradeModel.upgradeLevels.Add(upgrade.Key, -1);
                        
                        var def = sceneContext.PlayerState._model.upgradeModel.upgradeDefinitions.items._items
                            .FirstOrDefault(x => x._uniqueId == upgrade.Key);

                        sceneContext.PlayerState._model.upgradeModel.SetUpgradeLevel(def, upgrade.Value);
                    }

                    completedUpgrades = true;
                    
                    yield return null;
                }

                handlingPacket = false;
                clientLoading = false;
                
                var ammo = sceneContext.PlayerState.Ammo;

                ammo.RegisterAmmoPointer($"player_{data.Player}");

                try
                {
                    int i = 0;
                    //ammo._ammoModel.slots =
                    //    new Il2CppReferenceArray<Ammo.Slot>(MultiplayerAmmoDataToSlots(save.localPlayerSave.ammo,
                    //        ammo.Slots.Count));
                    foreach (var slot in save.localPlayerSave.ammo)
                    {
                        ammo.MaybeAddToSpecificSlot(identifiableTypes[slot.id], null, i, slot.count, true);
                        i++;
                    }
                }
                catch
                {
                }

                sceneContext.PlayerState.Ammo = ammo;

                yield return null;

                foreach (var pod in save.initPods)
                {
                    var id = ExtendInteger(pod.Key);
                    if (sceneContext.GameModel.pods.TryGetValue($"pod{id}", out var podModel))
                    {
                        podModel.state = pod.Value;
                        podModel.gameObj?.GetComponent<TreasurePod>().UpdateImmediate(pod.Value);
                    }
                    else
                    {
                        sceneContext.GameModel.pods.Add($"pod{id}", new TreasurePodModel
                        {
                            state = pod.Value,
                            gameObj = null,
                            spawnQueue = new Il2CppSystem.Collections.Generic.Queue<IdentifiableType>(),
                        });
                    }
                }
            }

            yield return null;
        }

        public override void OnSceneWasLoaded(int buildIndex, string sceneName)
        {
            switch (sceneName)
            {
                case "SystemCore":
                    Initialize();
                    break;
                case "MainMenuEnvironment":
                    SRMP.Log(SR2ELanguageManger.translation("ui.discord.intro"));
                    SRMP.Log(SR2ELanguageManger.translation("ui.discord.invite"));
                    SRMP.Log("https://discord.gg/a7wfBw5feU", 175);
                    
                    EpicApplication.Instance.Authentication.Login("Player");
                    
                    MultiplayerManager.Instance.GeneratePlayerModel();
                    break;
                case "PlayerCore":        
                    MultiplayerManager.Instance.SetupPlayerAnimations();
                    break;
                case "UICore":
                    break;
                #if SERVER
                case "GameCore":
                    gameContext.AutoSaveDirector.Load()
                #endif
            }
        }


        public static void Initialize()
        {
            Globals.Version = int.Parse(modInstance.Info.Version);
            //ui = InitializeAssetBundle("ui");
            
            var obj = new GameObject();
            obj.name = "MultiplayerContext";
            obj.AddComponent<MultiplayerManager>();
            UnityEngine.Object.DontDestroyOnLoad(obj);
            
            //UnityEngine.Object.Instantiate(ui.LoadAsset("LobbyUI")).Cast<GameObject>().transform.SetParent(obj.transform);
            
            SRMP.Log("Multiplayer Initialized!");
        }
        
        /// <summary>
        /// Multplayer User Data
        /// </summary>
        public class UserData
        {
            /// <summary>
            /// If the player has set their username
            /// </summary>
            public bool HasSavedUsername;
            /// <summary>
            /// Player Username
            /// </summary>
            public string Username;
            /// <summary>
            /// Used for player saving.
            /// </summary>
            public Guid Player;
            public Guid Debug_Player2;

            public string LastIP = "127.0.0.1";
            public string LastPort = "7777";
            public string LastPortHosted = "7777";
            
            /// <summary>
            /// Should use steam to play or not.
            /// </summary>
            public bool UseSteam = false;
        }
        
    }
}
