using System.Collections;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using Il2CppMonomiPark.ScriptedValue;
using Main = NewSR2MP.Main;



using Il2CppMonomiPark.SlimeRancher.DataModel;
using Il2CppMonomiPark.SlimeRancher.Event;
using Il2CppMonomiPark.SlimeRancher.Options;
using Il2CppMonomiPark.SlimeRancher.Pedia;
using Il2CppMonomiPark.SlimeRancher.Player.CharacterController;
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
            MultiplayerManager.Instance.Host(ushort.Parse(args[0]));
            return true;
        }

        public override string ID => "host";
        public override string Usage => "host <port>";
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
            ushort port = ushort.Parse(args[1]);
            MultiplayerManager.Instance.Connect(args[0], port);
            return true;
        }

        public override string ID => "join";
        public override string Usage => "join <ip> <port>";
    }
    public class SplitScreenDebugCommand : SR2ECommand
    {
        public override bool Execute(string[] args)
        {
            Main.data.Player = Main.data.Debug_Player2;

            ushort port = ushort.Parse(args[1]);
            MultiplayerManager.Instance.Connect(args[0], port);
            return true;
        }

        public override string ID => "debug_ss";
        public override string Usage => "debug_ss <ip> <port>";
    }
    public static class Extentions
    {
        public static void RemoveComponent<T>(this GameObject go) where T : Component => UnityEngine.Object.Destroy(go.GetComponent<T>());

        public static bool TryRemoveComponent<T>(this GameObject go) where T : Component
        {
            try
            {
                go.RemoveComponent<T>();
                return true;
            }
            catch
            {
                return false;
            }
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
            InitEmbeddedDLL("RiptideNetworking.dll");
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
            dat.Debug_Player2 = guid;
            dat.ignoredMods = new List<string>()
            {
                "NewSR2MP"
            };
            data = dat;

            SaveData();
        }
        private void SaveData()
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

        public static void OnRanchSceneGroupLoaded(SceneContext s)
        {
            var save = latestSaveJoined;

            foreach (var plot in save.initPlots)
            {
                try
                {
                    var model = sceneContext.GameModel.landPlots[plot.id];
                    handlingPacket = true;
                    model.gameObj.GetComponent<LandPlotLocation>().Replace(
                        model.gameObj.GetComponentInChildren<LandPlot>(),
                        GameContext.Instance.LookupDirector._plotPrefabDict[plot.type]);
                    handlingPacket = false;
                    var lp = model.gameObj.GetComponentInChildren<LandPlot>();
                    lp.ApplyUpgrades(ConvertToIEnumerable(plot.upgrades), false);
                    var silo = model.gameObj.GetComponentInChildren<SiloStorage>();
                    silo.RegisterAmmoPointer();
                    foreach (var ammo in plot.siloData.ammo)
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

                    if (plot.type == LandPlot.Id.GARDEN)
                    {
                        GardenCatcher gc = lp.transform.GetComponentInChildren<GardenCatcher>(true);

                        GameObject cropObj = UnityEngine.Object.Instantiate(
                            lp.HasUpgrade(LandPlot.Upgrade.DELUXE_GARDEN)
                                ? gc._deluxeDict[identifiableTypes[plot.cropIdent]]
                                : gc._plantableDict[identifiableTypes[plot.cropIdent]], lp.transform.position,
                            lp.transform.rotation);

                        handlingPacket = true;
                        if (gc.CanAccept(identifiableTypes[plot.cropIdent]))
                            lp.Attach(cropObj, true, true);
                        handlingPacket = false;

                    }
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
                if (sceneContext.gameObject.GetComponent<NetworkTimeDirector>())
                    sceneContext.gameObject.RemoveComponent<NetworkTimeDirector>();

                LoadMessage save = latestSaveJoined;

                sceneContext.player.GetComponent<SRCharacterController>().Position = save.localPlayerSave.pos;
                sceneContext.player.GetComponent<SRCharacterController>().Rotation =
                    Quaternion.Euler(save.localPlayerSave.rot);

                sceneContext.TimeDirector._worldModel.worldTime = save.time;

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
                            handlingPacket = true;
                            var obj = ident.prefab;
                            if (!obj.GetComponent<NetworkActor>())
                                obj.AddComponent<NetworkActor>();
                            if (!obj.GetComponent<TransformSmoother>())
                                obj.AddComponent<TransformSmoother>();
                            var obj2 = RegisterActor(new ActorId(newActor.id), ident, newActor.pos, Quaternion.identity, sceneGroups[newActor.scene]);

                            UnityEngine.Object.Destroy(obj.GetComponent<NetworkActor>());
                            UnityEngine.Object.Destroy(obj.GetComponent<TransformSmoother>());

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
                            players.Add(player.id, netPlayer);
                            netPlayer.id = player.id;
                            playerobj.SetActive(true);
                            UnityEngine.Object.DontDestroyOnLoad(playerobj);
                        }
                        catch
                        {
                        } // Some reason it does happen. // Note found out why, the marker code is completely broken, i forgot that i didnt remove it here so i was wondering why it errored.
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
                        }
                        else
                        {
                            model = new WorldSwitchModel()
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
                    var playerUpgrades = new Il2CppSystem.Collections.Generic.Dictionary<int, int>();

                    foreach (var upgrade in save.upgrades)
                        playerUpgrades.Add(upgrade.Key, upgrade.Value);
                
                    sceneContext.PlayerState._model.upgradeModel.upgradeLevels = playerUpgrades;

                    completedUpgrades = true;
                    
                    yield return null;
                }

                handlingPacket = false;
                var ammo = sceneContext.PlayerState.Ammo;

                ammo.RegisterAmmoPointer($"player_{data.Player}");

                try
                {
                    ammo._ammoModel.slots =
                        new Il2CppReferenceArray<Ammo.Slot>(MultiplayerAmmoDataToSlots(save.localPlayerSave.ammo,
                            ammo.Slots.Count));
                }
                catch
                {
                }

                sceneContext.PlayerState.Ammo = ammo;
            }
        }

        public override void OnSceneWasLoaded(int buildIndex, string sceneName)
        {
            switch (sceneName)
            {
                case "SystemCore":
                    Initialize();
                    break;
                case "MainMenuEnvironment":
                    SRMP.Log("Join the discord server for help and updates!");
                    SRMP.Log("Discord server invite:");
                    SRMP.Log("https://discord.gg/a7wfBw5feU", 175);
                    
                    MultiplayerManager.Instance.GeneratePlayerModel();
                    break;
                case "PlayerCore":        
                    MultiplayerManager.Instance.SetupPlayerAnimations();
                    break;
                case "UICore":
                    if (autoHostPort != 0)
                        MultiplayerManager.Instance.Host((ushort)autoHostPort);
                    break;
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
            public string Username;
            /// <summary>
            /// Used for player saving.
            /// </summary>
            public Guid Player;
            public Guid Debug_Player2;
            public List<string> ignoredMods;
        }
        
    }
}
