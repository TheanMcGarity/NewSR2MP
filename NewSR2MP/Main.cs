using Il2CppMonomiPark.ScriptedValue;
using Main = NewSR2MP.Main;



using Il2CppMonomiPark.SlimeRancher.DataModel;
using Il2CppMonomiPark.SlimeRancher.Options;
using Il2CppMonomiPark.SlimeRancher.Pedia;
using Il2CppMonomiPark.SlimeRancher.Player.CharacterController;
using Il2CppMonomiPark.SlimeRancher.Util.Extensions;
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
                        var model = SceneContext.Instance.GameModel.landPlots[plot.id];
                        model.gameObj.AddComponent<HandledDummy>();
                        model.gameObj.GetComponent<LandPlotLocation>().Replace(model.gameObj.GetComponentInChildren<LandPlot>(), GameContext.Instance.LookupDirector._plotPrefabDict[plot.type]);
                        model.gameObj.RemoveComponent<HandledDummy>();
                        var lp = model.gameObj.GetComponentInChildren<LandPlot>();
                        lp.ApplyUpgrades(ConvertToIEnumerable(plot.upgrades), false);
                        var silo = model.gameObj.GetComponentInChildren<SiloStorage>();
                        foreach (var ammo in plot.siloData.ammo)
                        {
                            try
                            {
                                if (!(ammo.count == 0 || ammo.id == 9))
                                {
                                    silo.Ammo.Slots[ammo.slot]._count = ammo.count;
                                    silo.Ammo.Slots[ammo.slot]._id = identifiableTypes[ammo.id];
                                }
                                else
                                {
                                    silo.Ammo.Slots[ammo.slot]._count = 0;
                                    silo.Ammo.Slots[ammo.slot]._id = null;
                                }
                            }
                            catch { }
                        }

                        if (plot.type == LandPlot.Id.GARDEN)
                        {
                            GardenCatcher gc = lp.transform.GetComponentInChildren<GardenCatcher>(true);

                            if (gc != null)
                            {
                                GameObject cropObj = UnityEngine.Object.Instantiate(lp.HasUpgrade(LandPlot.Upgrade.DELUXE_GARDEN) ? gc._deluxeDict[identifiableTypes[plot.cropIdent]] : gc._plantableDict[identifiableTypes[plot.cropIdent]], lp.transform.position, lp.transform.rotation);

                                gc.gameObject.AddComponent<HandledDummy>();
                                if (gc.CanAccept(identifiableTypes[plot.cropIdent]))
                                    lp.Attach(cropObj, true, true);
                                gc.gameObject.RemoveComponent<HandledDummy>();
                            }
                            else
                            {
                                SRMP.Log("'GardenCatcher' is null on a garden! i need to fix this rftijegiostgjio");
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        SRMP.Log($"Error in world load for plot({plot.id}).\n{e}");
                    }
                }
        }

        public static void OnSaveLoaded(SceneContext s)
        {
            if (ClientActive() && !ServerActive())
            {
                if (s.gameObject.GetComponent<TimeSyncer>())
                    s.gameObject.RemoveComponent<TimeSyncer>();
                    
                LoadMessage save = latestSaveJoined;
                
                SceneContext.Instance.player.GetComponent<SRCharacterController>().Position = save.localPlayerSave.pos;
                SceneContext.Instance.player.GetComponent<SRCharacterController>().Rotation = Quaternion.Euler(save.localPlayerSave.rot);
                
                SceneContext.Instance.TimeDirector._worldModel.worldTime = save.time;

                actors.Clear();                            
                SceneContext.Instance.GameModel.identifiables.Clear();

                foreach (var a in Resources.FindObjectsOfTypeAll<IdentifiableActor>())
                {
                    if (a.gameObject.hideFlags != HideFlags.HideAndDontSave && a.gameObject.name.Contains("(Clone)"))
                    {
                        try
                        {
                            if (!a.identType.IsSceneObject && !a.identType.IsPlayer)
                            {
                                a.gameObject.AddComponent<HandledDummy>();
                                SceneContext.Instance.GameModel.identifiables.Remove(a.GetActorId());
                                UnityEngine.Object.Destroy(a.gameObject);
                            }
                        }
                        catch { }
                    }
                }
                
                for (int i = 0; i < save.initActors.Count; i++)
                {
                    try
                    {
                        InitActorData newActor = save.initActors[i];
                        bool gotIdent = Globals.identifiableTypes.TryGetValue(newActor.ident, out var ident);
                        if (!gotIdent) continue;
                        if (!ident.IsSceneObject && !ident.IsPlayer)
                        {
                            var obj = ident.prefab;
                            if (obj.GetComponent<NetworkActor>() == null)
                                obj.AddComponent<NetworkActor>();
                            if (obj.GetComponent<TransformSmoother>() == null)
                                obj.AddComponent<TransformSmoother>();
                            var obj2 = InstantiateActor(obj, sceneGroups[newActor.scene], newActor.pos, Quaternion.identity);
                            var obj2ID = obj2.GetComponent<IdentifiableActor>()._model.actorId;
                            obj2.GetComponent<IdentifiableActor>()._model.actorId = new ActorId(newActor.id);
                            SceneContext.Instance.GameModel.identifiables.Remove(obj2ID);
                            SceneContext.Instance.GameModel._actorIdProvider._nextActorId = obj2.GetComponent<IdentifiableActor>()._model.actorId.Value + 1;
                            SceneContext.Instance.GameModel.identifiables.TryAdd(obj2.GetComponent<IdentifiableActor>()._model.actorId, obj2.GetComponent<IdentifiableActor>()._model);
                            UnityEngine.Object.Destroy(obj.GetComponent<NetworkActor>());
                            UnityEngine.Object.Destroy(obj.GetComponent<TransformSmoother>());

                            obj2.transform.position = newActor.pos;
                            obj2.GetComponent<TransformSmoother>().nextPos = newActor.pos;
                            obj2.GetComponent<NetworkActor>().IsOwned = false;

                            if (!actors.TryAdd(newActor.id, obj2.GetComponent<NetworkActor>()))
                                UnityEngine.Object.Destroy(obj2);
                        }
                    }
                    catch (Exception e)
                    {
                        SRMP.Error(e.ToString());
                    }
                }
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
                    catch { } // Some reason it does happen. // Note found out why, the marker code is completely broken, i forgot that i didnt remove it here so i was wondering why it errored.
                }              

                foreach (var gordo in save.initGordos)
                {
                    try
                    {
                        GordoModel gm = SceneContext.Instance.GameModel.gordos[gordo.id];

                        if (gordo.eaten <= -1 || gordo.eaten >= gm.targetCount)
                        {
                            gm.gameObj.SetActive(false);
                        }
                        gm.gordoEatCount = gordo.eaten;
                    }
                    catch
                    {
                    }
                }


                SceneContext.Instance.PlayerState._model.currency = save.money;

                var pediaEntries = new Il2CppSystem.Collections.Generic.HashSet<PediaEntry>();
                foreach (var pediaEntry in save.initPedias)
                {
                    pediaEntries.Add(Globals.pediaEntries[pediaEntry]);
                }
                                
                SceneContext.Instance.PediaDirector._pediaModel.unlocked = pediaEntries;


                var np = SceneContext.Instance.player.AddComponent<NetworkPlayer>();
                np.id = save.playerID;

                foreach (var access in save.initAccess)
                {
                    GameModel gm = SceneContext.Instance.GameModel;
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

                
                // Player ammo loading and saving... Will remake later.
                /*var ps = SceneContext.Instance.PlayerState;
                var defaultEmotions = new SlimeEmotionDataV02()
                {
                    emotionData = new Dictionary<SlimeEmotions.Emotion, float>()
                        {
                            {SlimeEmotions.Emotion.AGITATION,0},
                            {SlimeEmotions.Emotion.FEAR,0},
                            {SlimeEmotions.Emotion.HUNGER,0},
                        }
                };
                Ammo currentAmmoNormal = SceneContext.Instance.PlayerState.GetAmmo(AmmoMode.DEFAULT);
                NetworkAmmo normalNetAmmo = new NetworkAmmo($"player_{data.Player}_normal", currentAmmoNormal.potentialAmmo, currentAmmoNormal.numSlots, currentAmmoNormal.ammoModel.usableSlots, currentAmmoNormal.slotPreds, currentAmmoNormal.ammoModel.slotMaxCountFunction);
                Il2CppSystem.Collections.Generic.List<AmmoDataV02> ammoDataNormal = new Il2CppSystem.Collections.Generic.List<AmmoDataV02>();
                foreach (var ammo in save.localPlayerSave.ammo[AmmoMode.DEFAULT])
                {
                    ammoDataNormal.Add(new AmmoDataV02()
                    {
                        count = ammo.count,
                        id = ammo.id,
                        emotionData = defaultEmotions
                    });
                }
                normalNetAmmo.ammoModel.slots = NetworkAmmo.SRMPAmmoDataToSlots(ammoDataNormal);
                Ammo currentAmmoNimble = SceneContext.Instance.PlayerState.GetAmmo(AmmoMode.NIMBLE_VALLEY);

                NetworkAmmo nimbleNetAmmo = new NetworkAmmo($"player_{data.Player}_nimble", currentAmmoNimble.potentialAmmo, currentAmmoNimble.numSlots, currentAmmoNimble.ammoModel.usableSlots, currentAmmoNimble.slotPreds, currentAmmoNimble.ammoModel.slotMaxCountFunction);
                Il2CppSystem.Collections.Generic.List<AmmoDataV02> ammoDataNimble = new Il2CppSystem.Collections.Generic.List<AmmoDataV02>();
                foreach (var ammo in save.localPlayerSave.ammo[AmmoMode.NIMBLE_VALLEY])
                {
                    ammoDataNimble.Add(new AmmoDataV02()
                    {
                        count = ammo.count,
                        id = ammo.id,
                        emotionData = defaultEmotions
                    });
                }
                nimbleNetAmmo.ammoModel.slots = NetworkAmmo.SRMPAmmoDataToSlots(ammoDataNimble);

                ps.ammoDict = new Dictionary<AmmoMode, Ammo>()
                    {
                        {AmmoMode.DEFAULT, normalNetAmmo},
                        {AmmoMode.NIMBLE_VALLEY, nimbleNetAmmo},
                    };
                ps.model.ammoDict = new Dictionary<AmmoMode, AmmoModel>()
                    {
                        {AmmoMode.DEFAULT,normalNetAmmo.ammoModel},
                        {AmmoMode.NIMBLE_VALLEY,nimbleNetAmmo.ammoModel},
                    };*/
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
