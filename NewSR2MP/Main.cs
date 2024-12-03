using Mirror;
using Il2CppMonomiPark.SlimeRancher.DataModel;
using System.Reflection;
using Il2CppAssets.Script.Util.Extensions;
using Il2CppMonomiPark.SlimeRancher.Pedia;
using Il2CppMonomiPark.World;
using Il2CppSystem;
using Il2CppSystem.Collections;
using Il2CppSystem.Linq;
using MelonLoader;
using Newtonsoft.Json;
using Il2CppTMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using Convert = System.Convert;
using Exception = System.Exception;
using Guid = System.Guid;
using Object = Il2CppSystem.Object;


namespace NewSR2MP
{
    public static class Extentions
    {
        public static void RemoveComponent<T>(this GameObject go) where T : Component => UnityEngine.Object.Destroy(go.GetComponent<T>());
    }
    
    public class Main : MelonMod
    {
        private static GameObject m_GameObject;

        // Called before GameContext.Awake
        // this is where you want to register stuff (like custom enum values or identifiable id's)
        // and patch anything you want to patch with harmony
        public override void OnEarlyInitializeMelon()
        {
            LoadData();
            Application.add_quitting(new System.Action(SaveData));;
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

            dat.Name = $"User{i}";
            dat.Player = guid;
            dat.compareDLC = true;
            dat.ignoredMods = new Il2CppSystem.Collections.Generic.List<string>()
            {
                "newsr2mp"
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

        public static Il2CppSystem.Collections.Generic.IEnumerable<LandPlot.Upgrade> ConvertToIEnumerable(HashSet<LandPlot.Upgrade> upgrades)
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
                        model.gameObj.GetComponent<LandPlotLocation>().Replace(model.gameObj.transform.GetChild(0).GetComponent<LandPlot>(), GameContext.Instance.LookupDirector._plotPrefabDict[plot.type]);
                        model.gameObj.RemoveComponent<HandledDummy>();
                        var lp = model.gameObj.transform.GetChild(0).GetComponent<LandPlot>();
                        lp.ApplyUpgrades(ConvertToIEnumerable(plot.upgrades), false);
                        var silo = model.gameObj.GetComponentInChildren<SiloStorage>();
                        foreach (var ammo in plot.siloData.ammo)
                        {
                            try
                            {
                                if (!(ammo.count == 0 || ammo.id == "None"))
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
                            GardenCatcher gc = lp.transform.GetChild(3).GetChild(1).GetComponent<GardenCatcher>();

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

        public static void OnSceneContextLoaded(SceneContext s)
        {
            if (NetworkClient.active && !NetworkServer.activeHost)
            {

                LoadMessage save = latestSaveJoined;

                SceneContext.Instance.player.transform.position = save.localPlayerSave.pos;
                SceneContext.Instance.player.transform.eulerAngles = save.localPlayerSave.rot;

                SceneContext.Instance.TimeDirector._worldModel.worldTime = save.time;

                foreach (var a in Resources.FindObjectsOfTypeAll<IdentifiableActor>())
                {
                    if (a.gameObject.scene.name == "worldGenerated")
                    {
                        try
                        {

                            if (a.identType.IsSceneObject && a.identType.IsPlayer)
                                Destroyer.DestroyActor(a.gameObject, "SR2MP.LoadWorld", true);
                        }
                        catch { }
                    }
                }

                for (int i = 0; i < save.initActors.Count; i++)
                {
                    try
                    {
                        InitActorData newActor = save.initActors[i];
                        IdentifiableType ident = Globals.identifiableTypes[newActor.ident];
                        if (!ident.IsSceneObject && !ident.IsPlayer)
                        {
                            SRMP.Log(newActor.ident.ToString());
                            var obj = ident.prefab;
                            if (obj.GetComponent<NetworkActor>() == null)
                                obj.AddComponent<NetworkActor>();
                            if (obj.GetComponent<TransformSmoother>() == null)
                                obj.AddComponent<TransformSmoother>();
                            var obj2 = InstantiateActor(obj, SystemContext.Instance.SceneLoader._currentSceneGroup, Vector3.zero, Quaternion.identity);
                            var obj2ID = obj2.GetComponent<IdentifiableActor>().model.actorId;
                            obj2.GetComponent<IdentifiableActor>().model.actorId = new ActorId(newActor.id);
                            SceneContext.Instance.GameModel.identifiables.Remove(obj2ID);
                            SceneContext.Instance.GameModel._actorIdProvider._nextActorId = obj2.GetComponent<IdentifiableActor>().model.actorId.Value + 1;
                            SceneContext.Instance.GameModel.identifiables.Add(obj2.GetComponent<IdentifiableActor>().model.actorId, obj2.GetComponent<IdentifiableActor>().model);
                            UnityEngine.Object.Destroy(obj.GetComponent<NetworkActor>());
                            UnityEngine.Object.Destroy(obj.GetComponent<TransformSmoother>());

                            obj2.transform.position = newActor.pos;

                            SRNetworkManager.actors.Add(newActor.id, obj2.GetComponent<NetworkActor>());
                        }
                    }
                    catch (Exception e)
                    {
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
                SceneContext.Instance.PlayerState._model.keys = save.keys;

                var pediaEntries = new Il2CppSystem.Collections.Generic.HashSet<PediaEntry>();
                foreach (var pediaEntry in save.initPedias)
                {
                    pediaEntries.Add(Globals.pediaEntries[pediaEntry]);
                }
                
                SceneContext.Instance.PediaDirector._pediaModel.unlocked = pediaEntries;

                var mapFogs =
                    SceneContext.Instance.GameModel.mapDirector.DefaultMap.Prefab.transform.FindChild("zone_fog_areas");
                foreach (var fog in save.initMaps)
                {
                    var fogObject = mapFogs.FindChild($"map_fog_{fog}")
                }

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

        // Called right before PostLoad
        // Used to register stuff that needs lookupdirector access
        public override void Load()
        {
            modifiedGameUI = AssetBundle.LoadFromStream(Assembly.GetExecutingAssembly().GetManifestResourceStream("SRMP.modified_sr_ui"));

            SRCallbacks.OnMainMenuLoaded += OverrideSaveMenu;

            OverrideSaveMenu(Resources.FindObjectsOfTypeAll<MainMenuUI>().FirstOrDefault(x => x.gameObject.scene.isLoaded));



            // SRCallbacks.OnSaveGameLoaded += OnClientSaveLoaded;

            if (m_GameObject != null) return;

            SRMP.Log("Loading SRMP SRML Version");


            m_GameObject = new GameObject("SRMP");
            m_GameObject.AddComponent<MultiplayerManager>();
            
            //mark all mod objects and do not destroy
            GameObject.DontDestroyOnLoad(m_GameObject);

            //get current mod version
            Globals.Version = Assembly.GetExecutingAssembly().GetName().Version.Revision;

            //mark the mod as a background task
            Application.runInBackground = true;

            //initialize connect to the harmony patcher
            HarmonyPatcher.GetInstance().PatchAll(Assembly.GetExecutingAssembly());


            SRML.Console.Console.RegisterCommand(new TeleportCommand());
            SRML.Console.Console.RegisterCommand(new PlayerCameraCommand());
        }

        /// <summary>
        /// Multplayer User Data
        /// </summary>
        public class UserData
        {
            public string Name;
            /// <summary>
            /// Used for player saving.
            /// </summary>
            public Guid Player;
            public bool compareDLC;
            public Il2CppSystem.Collections.Generic.List<string> ignoredMods;
        }

        // Called after GameContext.Start
        // stuff like gamecontext.lookupdirector are available in this step, generally for when you want to access
        // ingame prefabs and the such
        public override void PostLoad()
        {

        }
    }
}
