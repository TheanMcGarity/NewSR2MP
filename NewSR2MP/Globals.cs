using Il2CppMonomiPark.ScriptedValue;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Il2CppAssets.Script.Util.Extensions;
using Il2CppMonomiPark.SlimeRancher.DataModel;
using Il2CppMonomiPark.SlimeRancher.Event;
using Il2CppMonomiPark.SlimeRancher.Pedia;
using Il2CppMonomiPark.SlimeRancher.Persist;
using Il2CppMonomiPark.SlimeRancher.SceneManagement;
using Il2CppMonomiPark.SlimeRancher.Slime;
using Il2CppMonomiPark.SlimeRancher.UI;
using Il2CppMonomiPark.SlimeRancher.UI.MainMenu;
using Il2CppMonomiPark.SlimeRancher.Weather;
using Il2CppMonomiPark.SlimeRancher.World;
using Il2CppMonomiPark.UnitPropertySystem;
using Il2CppSystem.Net.WebSockets;
using NewSR2MP.Networking.SaveModels;
using SR2E;
using SR2E.Managers;
using Unity.Mathematics;
using UnityEngine;
using Object = UnityEngine.Object;

namespace NewSR2MP
{
    public static class MessageExtensions
    {
        public static void AddVector3(this Message msg, Vector3 vector)
        {
            msg.AddFloat(vector.x);
            msg.AddFloat(vector.y);
            msg.AddFloat(vector.z);
        }

        public static Vector3 GetVector3(this Message msg)
        {
            var x = msg.GetFloat();
            var y = msg.GetFloat();
            var z = msg.GetFloat();

            return new Vector3(x, y, z);
        }

        public static void AddQuaternion(this Message msg, Quaternion quaternion)
        {
            msg.AddFloat(quaternion.x);
            msg.AddFloat(quaternion.y);
            msg.AddFloat(quaternion.z);
            msg.AddFloat(quaternion.w);
        }

        public static Quaternion GetQuaternion(this Message msg)
        {
            var x = msg.GetFloat();
            var y = msg.GetFloat();
            var z = msg.GetFloat();
            var w = msg.GetFloat();

            return new Quaternion(x, y, z, w);
        }

        public static void AddGuid(this Message msg, Guid guid)
        {
            msg.AddString(guid.ToString());
        }

        public static Guid GetGuid(this Message msg)
        {
            var str = msg.GetString();

            return new Guid(str);
        }

        public static void AddAmmoData(this Message msg, AmmoData data)
        {
            msg.AddInt(data.count);
            msg.AddInt(data.slot);
            msg.AddInt(data.id);
        }

        public static AmmoData GetAmmoData(this Message msg)
        {
            var count = msg.GetInt();
            var slot = msg.GetInt();
            var id = msg.GetInt();

            return new AmmoData()
            {
                count = count,
                slot = slot,
                id = id
            };
        }
    }

    public static class Globals
    {
        /// <summary>
        /// Auto host port in options. can be 0/off, 7777, 16500
        /// </summary>
        public static int autoHostPort => scriptedAutoHostPort ? scriptedAutoHostPort.Value : 0;

        /// <summary>
        /// Do not manually edit this.
        /// </summary>
        internal static ScriptedInt? scriptedAutoHostPort;


        /// <summary>
        /// Built in packet IDs, use a custom packet enum or an ushort to make custom packets.
        /// </summary>
        public enum PacketType : ushort
        {
            PlayerUpdate,
            PlayerJoin,
            PlayerLeave,
            TempClientActorUpdate,
            TempClientActorSpawn,
            ActorUpdate,
            ActorSpawn,
            ActorDestroy,
            ActorHeldOwner,
            ActorBecomeOwner,
            ActorVelocitySet,
            ActorSetOwner,
            GordoExplode,
            GordoFeed,
            PediaUnlock,
            MapUnlock,
            ResourceState,
            FastForward,
            TimeUpdate,
            SetCurrency,
            OpenDoor,
            AmmoAdd,
            AmmoEdit,
            AmmoRemove,
            JoinSave,
            RequestJoin,
            LandPlot,
            GardenPlant,
            NavigationMarkerPlace,
            NavigationMarkerRemove,
            WeatherUpdate,
            MarketRefresh,
            KillAllCommand,
            SwitchModify,
            RefineryItem,
            PlayerUpgrade,
        }

        public static GameSettingsModel CreateEmptyGameSettingsModel()
        {
            var ui = Resources.FindObjectsOfTypeAll<NewGameRootUI>().First();
            var optionsList = ui._optionsItemDefinitionsProvider.defaultAsset;

            return new GameSettingsModel(optionsList.GameBasedDefinitions);
        }

        public static MarketUI? marketUI;

        private static bool TryParseFloat(string input, out float value, float min, bool inclusive)
        {
            value = 0;
            try
            {
                value = float.Parse(input);
            }
            catch
            {
                return false;
            }

            if (inclusive)
            {
                if (value < min) return false;
            }
            else if (value <= min) return false;

            return true;
        }
        internal static IdentifiableType getIdentByName(string name)
        {
            if (String.IsNullOrWhiteSpace(name)) return null;
            if (name.ToLower() == "none" || name.ToLower() == "player") return null;
            foreach (var type in identifiableTypes)
                if (type.Value.name.ToUpper() == name.ToUpper()) return type.Value;
            foreach (var type in identifiableTypes) 
                try { if (type.Value.LocalizedName.GetLocalizedString().ToUpper().Replace(" ","").Replace("_","") == name.Replace("_","").ToUpper()) return type.Value; }catch {}
            return null;
        }
        public static void InitializeCommandExtensions()
        {
            SR2ECommandManager.RegisterCommandAddon("killall", args =>
            {
                int actorType = -1;
                
                if (args != null)
                    if (args.Length == 1)
                        actorType = GetIdentID(getIdentByName(args[0]));
                
                MultiplayerManager.NetworkSend(new KillAllCommandMessage
                {
                    actorType = actorType,
                    sceneGroup = sceneGroupsReverse[systemContext.SceneLoader._currentSceneGroup.name]
                });
            });
        }

        public static bool isJoiningAsClient = false;

        public static bool ServerActive() => MultiplayerManager.server != null;
        public static bool ClientActive() => MultiplayerManager.client != null;


        public static void InitEmbeddedDLL(string name)
        {
            Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream($"NewSR2MP.{name}");
            using (MemoryStream ms = new MemoryStream())
            {
                stream.CopyTo(ms);
                Assembly.Load(ms.ToArray());
            }
        }

        /// <summary>
        /// Generates a 7 digit random string containing capital letters (A - Z), and numbers (0 - 9). This string is used for an easily sharable server code.
        /// </summary>
        /// <returns>The 7 digit server code</returns>
        public static string GenerateServerCode()
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            System.Random random = new System.Random();
            char[] result = new char[7];

            for (int i = 0; i < 7; i++)
            {
                result[i] = chars[random.Next(chars.Length)];
            }

            return new string(result);
        }

        /// <summary>
        /// Set this to true to enable error logging. It should be set to true by a console command however.
        /// </summary>
        public static bool ShowErrors = false;

        public static int Version;

        /// <summary>
        /// Shortcut for getting persistent ID for identifiable types. 
        /// </summary>
        /// <param name="ident"></param>
        /// <returns></returns>
        public static int GetIdentID(IdentifiableType ident)
        {
            return gameContext.AutoSaveDirector.SavedGame.identifiableTypeToPersistenceId.GetPersistenceId(ident);
        }

        /// <summary>
        /// Identifiable type persistence ID lookup table. Use id 9 for identifiable type "None"
        /// </summary>
        public static Dictionary<int, IdentifiableType> identifiableTypes = new Dictionary<int, IdentifiableType>();

        /// <summary>
        /// Pedia name table
        /// </summary>
        public static Dictionary<string, PediaEntry> pediaEntries = new Dictionary<string, PediaEntry>();

        /// <summary>
        /// Scene Group persistence ID lookup table. Use id 1 for the ranch's scene group.
        /// </summary>
        public static Dictionary<int, SceneGroup> sceneGroups = new Dictionary<int, SceneGroup>();

        /// <summary>
        /// Scene Group reverse persistence ID lookup table. Use SceneGroup.name for the keys.
        /// </summary>
        public static Dictionary<string, int> sceneGroupsReverse = new Dictionary<string, int>();

        /// <summary>
        /// Weather persistence ID lookup table. Use hash codes for the keys. not to be confused with SR2EUtils.weatherStates
        /// </summary>
        public static Dictionary<int, WeatherStateDefinition> mpWeatherStates;

        /// <summary>
        /// State name to weather pattern object table.
        /// </summary>
        public static Dictionary<string, WeatherPatternDefinition> weatherPatternsFromStateNames;

        /// <summary>
        /// Reverse of the weather persistence ID lookup table. Use weather state names for the keys.
        /// </summary>
        public static Dictionary<string, int> weatherStatesReverseLookup;

        public static Dictionary<int, NetworkPlayer> players = new Dictionary<int, NetworkPlayer>();

        public static Dictionary<int, Guid> clientToGuid = new Dictionary<int, Guid>();

        public static Dictionary<long, NetworkActor> actors = new Dictionary<long, NetworkActor>();
        public static Dictionary<long, Gadget> gadgets = new Dictionary<long, Gadget>();

        internal static List<NetworkActorOwnerToggle> activeActors = new List<NetworkActorOwnerToggle>();

        public static List<NetworkActorOwnerToggle> GetActorsInBounds(Bounds bounds)
        {
            List<NetworkActorOwnerToggle> found = new List<NetworkActorOwnerToggle>();

            List<NetworkActorOwnerToggle> toRemove = new List<NetworkActorOwnerToggle>();

            foreach (var actor in activeActors)
            {
                if (!actor)
                {
                    toRemove.Add(actor);
                    continue;
                }

                if (bounds.Contains(actor.transform.position))
                    found.Add(actor);
            }

            foreach (var remove in toRemove)
                activeActors.Remove(remove);

            return found;
        }

        internal static List<NetworkActorOwnerToggle> unownedActors; 
        
        public static IEnumerator GetUnownedActors()
        {
            // Null check
            if (sceneContext == null || sceneContext.player == null) yield break;
            
            // Local Functions
            void AddList(List<NetworkActorOwnerToggle> toAdd, List<NetworkActorOwnerToggle> original)
            {
                foreach (var add in toAdd)
                    original.Add(add);
            }

            List<NetworkActorOwnerToggle> DifferenceOf(List<NetworkActorOwnerToggle> a, List<NetworkActorOwnerToggle> b)
            {
                List<NetworkActorOwnerToggle> result = new List<NetworkActorOwnerToggle>();

                foreach (var val1 in a)
                    if (!b.Contains(val1))
                        result.Add(val1);

                return result;
            }

            // Main Function
            List<NetworkActorOwnerToggle> found = new List<NetworkActorOwnerToggle>();
            List<NetworkActorOwnerToggle> owned = new List<NetworkActorOwnerToggle>();

            Dictionary<int, List<NetworkActorOwnerToggle>> playersActors =
                new Dictionary<int, List<NetworkActorOwnerToggle>>();
            
            Vector3 size = new Vector3(150, 600, 150);

            foreach (var player in players)
            {            
                playersActors.Add(player.Key, GetActorsInBounds(new Bounds(player.Value.transform.position, size)));
                yield return null;
            }

            found = GetActorsInBounds(new Bounds(sceneContext.player.transform.position, size));
            yield return null;

            foreach (var player in playersActors)
            {
                if (player.Key < currentPlayerID)
                {
                    AddList(player.Value, owned);
                    yield return null;
                }
            }
            
            unownedActors = DifferenceOf(found, owned);
        }

        public static NetworkV01 savedGame;
        public static string savedGamePath;

        public static Dictionary<string, Ammo> ammoByPlotID = new Dictionary<string, Ammo>();

        public static Ammo GetNetworkAmmo(string name)
        {
            ammoByPlotID.TryGetValue(name, out Ammo ammo);

            return ammo;
        }

        public static bool IsUnlocked(this PediaEntry entry)
        {
            try
            {
                sceneContext.PediaDirector._pediaModel.unlocked._slots.First(x => x.value.name == entry.name);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static LoadMessage? latestSaveJoined;

        public static int currentPlayerID;

        public static void CreateWeatherLookup(SavedGame savedGame)
        {
            var states = new Dictionary<int, WeatherStateDefinition>();
            var states2 = new Dictionary<string, int>();
            int i = 0;
            foreach (var state in savedGame._weatherStateTranslation.RawLookupDictionary)
            {
                states.Add(i, state.Value.Cast<WeatherStateDefinition>());
                states2.Add(state.value.Cast<WeatherStateDefinition>().name, i);
                i++;
            }

            mpWeatherStates = states;
            weatherStatesReverseLookup = states2;
        }


        public static void CreateWeatherPatternLookup(WeatherRegistry dir)
        {
            var list = new Dictionary<string, WeatherPatternDefinition>();

            foreach (var config in dir.ZoneConfigList)
            foreach (var pattern in config.Patterns)
            {
                pattern.GetAllStates();
                foreach (var state in pattern._stateList)
                    if (state)
                        list.TryAdd(state.name, pattern);
            }

            weatherPatternsFromStateNames = list;
        }

        public static GameObject? RegisterActor(
            ActorId id,
            IdentifiableType ident,
            Vector3 position,
            Quaternion rotation,
            SceneGroup sceneGroup)
        {
            if (ClientActive())
                sceneContext.GameModel._actorIdProvider._nextActorId = long.MinValue;
            
            SRMP.Debug("Spawning actor -" +
                       $" ID:{id.Value}" +
                       $" TYPE:{ident.name}" +
                       $" POSITION:({position})" +
                       $" ROTATION:({rotation.ToEuler()})" +
                       $" SCENEGROUP:{sceneGroup.name}");
            
            IdentifiableModel model;
            GameObject actor = null;
            var gadget = ident.TryCast<GadgetDefinition>() != null;
            if (gadget)
            {
                var modelGadget = sceneContext.GameModel.CreateGadgetModel(ident.Cast<GadgetDefinition>(), id, sceneGroup, position);
                
                modelGadget.eulerRotation = rotation.ToEuler();
                
                if (modelGadget != null)
                {           
                    handlingPacket = true;
                    actor = SpawnGadgetFromModel(modelGadget);
                    handlingPacket = false;
                    if (actor)
                    {
                        gadgets.Add(id.Value, actor.GetComponent<Gadget>());
                    }
                }
                else
                {
                    SRMP.Error("Null Gadget Model!");
                }
                model = modelGadget;
            }
            else
            {

                model = sceneContext.GameModel.CreateActorModel(id, ident, sceneGroup, position, rotation).TryCast<ActorModel>();

                if (model != null)
                {
                    actor = InstantiateActorFromModel(model.Cast<ActorModel>());
                    if (actor)
                        actor.transform.position = position;
                }
                
            }

            if (model != null)
            {
                if (!sceneContext.GameModel.identifiables.TryGetValue(id, out _))
                {
                    sceneContext.GameModel.identifiables.Add(id, model);
                    if (!sceneContext.GameModel.identifiablesByIdent.TryGetValue(ident, out var types))
                    {
                        sceneContext.GameModel.identifiablesByIdent.Add(ident,
                            new Il2CppSystem.Collections.Generic.List<IdentifiableModel>(0));
                        types = sceneContext.GameModel.identifiablesByIdent[ident];
                    }
                    
                    types.Add(model);
                }


                debugRegisteredActors.TryAdd(id.Value, model);

                var savedGame2 = gameContext.AutoSaveDirector.SavedGame;

                var data = CreateActorDataFromModel(
                    model,
                    gameContext.AutoSaveDirector.SavedGame._statusEffectTranslation,
                    savedGame2.identifiableTypeToPersistenceId);

                savedGame2.gameState.Actors.Add(data);

                

                multiplayerSpawnedActorsIDs.Add(id.Value);

            }

            if (gadget)
            {
                actor.TryRemoveComponent<NetworkActor>();
                actor.TryRemoveComponent<TransformSmoother>();
                actor.TryRemoveComponent<NetworkActorOwnerToggle>();
            }

            sceneContext.GameModel._actorIdProvider._nextActorId++;
            

            return actor;
        }

        public static GameObject SpawnGadgetFromModel(GadgetModel gadgetModel)
        {
            /*
            var obj = Object.Instantiate(gadgetModel.ident.prefab);
            
            gadgetModel.Init(obj);
            gadgetModel.NotifyParticipants(obj);
            
            DynamicObjectContainer.Instance.RegisterDynamicObject(obj);
            
            obj.transform.position = gadgetModel.lastPosition;
            obj.transform.eulerAngles = gadgetModel.eulerRotation;
            
            return obj;*/
            return GadgetDirector.InstantiateGadgetFromModel(gadgetModel);
                        
        }
        
        public static void DeregisterActor(ActorId id)
        {
            if (sceneContext.GameModel.identifiables.TryGetValue(id, out _))
                sceneContext.GameModel.DestroyIdentifiableModel(sceneContext.GameModel.identifiables[id]);

            int idx = 0;
            bool found = false;
            
            foreach (var actor in gameContext.AutoSaveDirector.SavedGame.gameState.Actors)
            {
                if (actor.ActorId == id.Value)
                {
                    found = true;
                    break;
                }

                idx++;
            }

            if (found)
                gameContext.AutoSaveDirector.SavedGame.gameState.Actors.RemoveAt(idx);
        }

        public static ActorModel? CreateActorModel(
            ActorId id,
            IdentifiableType ident,
            Vector3 position,
            Quaternion rotation,
            SceneGroup sceneGroup)
            => sceneContext.GameModel.CreateActorModel(id, ident, sceneGroup, position, rotation);

        private static Dictionary<long, IdentifiableModel> debugRegisteredActors = new Dictionary<long, IdentifiableModel>();

        public static ActorDataV02 CreateActorDataFromModel(
            IdentifiableModel model,
            SavedGame.PersistenceIDTranslation<StatusEffectDefinition> statusEffectToPersistenceIdTranslation,
            IdentifiableTypePersistenceIdLookupTable identTable)
        {
            var actorModel = model.TryCast<ActorModel>();
            var gadgetModel = model.TryCast<GadgetModel>();
            
            var statusEffectToPersistenceId = statusEffectToPersistenceIdTranslation.InstanceLookupTable;
            ActorDataV02 actorDataV = new ActorDataV02();

            IdentifiableType ident = model.ident;
            actorDataV.TypeId = identTable.GetPersistenceId(ident);

            actorDataV.ActorId = model.actorId.Value;

            Vector3V01 position = new Vector3V01();
            position.Value = model.lastPosition;
            actorDataV.Pos = position;

            Vector3V01 rotation = new Vector3V01();
            if (actorModel != null)
                rotation.Value = actorModel.lastRotation.eulerAngles;
            else if (gadgetModel != null)
                rotation.Value = gadgetModel.eulerRotation;
            else
                rotation.Value = Vector3.zero;
            actorDataV.Rot = rotation;

            actorDataV.SceneGroup = sceneGroupsReverse[model.SceneGroup.name];

            var emotions = new SlimeEmotionDataV01();
            emotions.EmotionData = new Il2CppSystem.Collections.Generic.Dictionary<SlimeEmotions.Emotion, float>();
            
            emotions.EmotionData.Add(SlimeEmotions.Emotion.FEAR,
                model.TryCast<SlimeModel>() != null ? model.Cast<SlimeModel>().Emotions.x : 0f);
            
            emotions.EmotionData.Add(SlimeEmotions.Emotion.HUNGER,
                model.TryCast<SlimeModel>() != null ? model.Cast<SlimeModel>().Emotions.y : 0f);
            
            emotions.EmotionData.Add(SlimeEmotions.Emotion.AGITATION,
                model.TryCast<SlimeModel>() != null ? model.Cast<SlimeModel>().Emotions.z : 0f);
            
            emotions.EmotionData.Add(SlimeEmotions.Emotion.SLEEPINESS,
                model.TryCast<SlimeModel>() != null ? model.Cast<SlimeModel>().Emotions.w : 0f);
            
            actorDataV.Emotions = emotions;


            Il2CppSystem.Collections.Generic.List<StatusEffectV01> statusEffects =
                new Il2CppSystem.Collections.Generic.List<StatusEffectV01>();
            if (actorModel != null)
            {
                foreach (var effect in actorModel.statusEffects)
                {
                    var effectV01 = new StatusEffectV01()
                    {
                        ExpirationTime = effect.value.ExpirationTime,
                        ID = statusEffectToPersistenceId.GetPersistenceId(effect.key)
                    };
                    statusEffects.Add(effectV01);
                }
            }
            

            actorDataV.StatusEffects = statusEffects;

            actorDataV.CycleData = new ResourceCycleDataV01();

            if (model.TryCast<SlimeModel>() != null)
            {
                model.TryCast<SlimeModel>().Pull(ref actorDataV, identTable);
            }
            else if (model.TryCast<AnimalModel>() != null)
            {
                model.TryCast<AnimalModel>().Pull(ref actorDataV, identTable);
            }
            else if (model.TryCast<ProduceModel>() != null)
            {
                model.TryCast<ProduceModel>().Pull(out var state, out var time);
                actorDataV.CycleData.State = state;
                actorDataV.CycleData.ProgressTime = time;
            }
            else if (model is StatueFormModel statueFormModel)
            {
                actorDataV.IsStatue = true;
            }

            return actorDataV;
        }


        public static void SetFromNetwork(this SlimeEmotions emotions, NetworkEmotions networkEmotions)
        {
            emotions.SetAll(new float4(networkEmotions.x, networkEmotions.y, networkEmotions.z, networkEmotions.w));
        }


        // POINTERS OH NO
        /// <summary>
        /// Not recommended for direct use! Please just use the extension methods.
        /// </summary>
        public static Dictionary<IntPtr, string> ammoPointersToPlotIDs = new Dictionary<IntPtr, string>();

        /// <summary>
        /// This will get the ID of the plot that uses this ammo.
        /// </summary>
        /// <returns>The ID of the plot that uses this.</returns>
        public static string GetPlotID(this Ammo ammo)
        {
            ammoPointersToPlotIDs.TryGetValue(ammo.Pointer, out var plotID);
            return plotID;
        }

        /// <summary>
        /// This will register the ammo pointer into the lookup.
        /// </summary>
        public static void RegisterAmmoPointer(this SiloStorage storage)
        {
            ammoPointersToPlotIDs.TryAdd(storage.LocalAmmo.Pointer,
                storage.GetComponentInParent<LandPlotLocation>()._id);

            if (!ammoByPlotID.TryAdd(storage.GetComponentInParent<LandPlotLocation>()._id, storage.LocalAmmo))
                ammoByPlotID[storage.GetComponentInParent<LandPlotLocation>()._id] = storage.LocalAmmo;
        }

        /// <summary>
        /// This will register the ammo pointer into the lookup.
        /// </summary>
        public static void RegisterAmmoPointer(this Ammo ammo, string id)
        {
            ammoPointersToPlotIDs.Add(ammo.Pointer, id);

            if (!ammoByPlotID.TryAdd(id, ammo))
                ammoByPlotID[id] = ammo;
        }

        public static Ammo.Slot[] AmmoDataToSlotsSRMP(Il2CppSystem.Collections.Generic.List<AmmoDataV01> ammo)
        {
            Ammo.Slot[] array = new Ammo.Slot[ammo.Count];
            for (int i = 0; i < ammo.Count; i++)
            {
                var slot = new Ammo.Slot();

                slot.Count = ammo._items[i].Count;
                slot._id = identifiableTypes[ammo._items[i].ID];
                slot.Emotions = new float4(0, 0, 0, 0);
            }

            return array;
        }

        public static Ammo.Slot[] MultiplayerAmmoDataToSlots(List<AmmoData> ammo, int slotCount)
        {
            Ammo.Slot[] array = new Ammo.Slot[ammo.Count];
            for (int i = 0; i < slotCount; i++)
            {
                var slot = new Ammo.Slot();

                slot.Count = ammo[i].count;
                slot._id = identifiableTypes[ammo[i].id];
                slot.Emotions = new float4(0, 0, 0, 0);
            }

            return array;
        }

        public static int GetSlotIDX(this Ammo ammo, IdentifiableType id)
        {
            bool isSlotNull = false;
            // bool IsIdentAllowedForAmmo = false;
            bool isSlotEmptyOrSameType = false;
            bool isSlotFull = false;
            for (int j = 0; j < ammo._ammoModel.slots.Count; j++)
            {
                isSlotNull = ammo.Slots[j] == null;

                isSlotEmptyOrSameType = ammo.Slots[j]._count == 0 || ammo.Slots[j]._id == id;

                // IsIdentAllowedForAmmo = slotPreds[j](id) && potentialAmmo.Contains(id);

                if (!isSlotNull)
                    isSlotFull = ammo.Slots[j].Count >= ammo._ammoModel.GetSlotMaxCount(id, j);
                else
                    isSlotFull = false;

                if (isSlotEmptyOrSameType && isSlotFull) break;

                if (isSlotEmptyOrSameType) // && IsIdentAllowedForAmmo)
                {
                    return j;
                }
            }

            return -1;
        }

        public static void RegisterAllSilos()
        {
            foreach (var plot in sceneContext.GameModel.landPlots)
            {
                var silo = plot.Value.gameObj.GetComponentInChildren<SiloStorage>();
                if (silo != null)
                {
                    try
                    {
                        silo.LocalAmmo.RegisterAmmoPointer($"plot{plot.key}");
                    }
                    catch (Exception e)
                    {
                        SRMP.Error($"Error registering ammo pointer!\n{e}");
                    }
                }
            }
        }

        public static List<long> multiplayerSpawnedActorsIDs = new List<long>();

        public static Ammo CreateNewPlayerAmmo()
        {
            var newAmmo = new Ammo(sceneContext.PlayerState._ammoSlotDefinitions);
            newAmmo._ammoModel = new AmmoModel(sceneContext.PlayerState.Ammo._ammoModel.unitPropertyTracker);
            return newAmmo;
        }

        /// <summary>
        /// The instance of the currently loaded weather director.
        /// </summary>
        public static WeatherDirector? weatherDirectorInstance;

        public static IEnumerator WeatherHandlingCoroutine(WeatherSyncMessage packet)
        {
            if (sceneContext == null || weatherDirectorInstance == null)
                yield break;
            
            var reg = sceneContext.WeatherRegistry;
            var dir = weatherDirectorInstance;

            var zones = new Dictionary<byte, ZoneDefinition>();
            bool completedZonesDict = false;
            if (!completedZonesDict)
            {
                byte b = 0;
                foreach (var zone in reg._model._zoneDatas)
                {
                    zones.Add(b, zone.key);
                    b++;
                }

                completedZonesDict = true;
                yield return null;
            }

            var zoneDatas = new Il2CppSystem.Collections.Generic.Dictionary<ZoneDefinition, WeatherModel.ZoneData>();
            var zoneDatas2 =
                new Il2CppSystem.Collections.Generic.Dictionary<ZoneDefinition, WeatherRegistry.ZoneWeatherData>();

            bool completedZoneDataDict = false;
            if (!completedZoneDataDict)
            {
                foreach (var zone in packet.sync.zones)
                {
                    if (!zones.ContainsKey(zone.Key))
                    {
                        continue;
                    }

                    var forcastRunCheck = new List<string>();

                    var forecast = new Il2CppSystem.Collections.Generic.List<WeatherModel.ForecastEntry>();
                    for (var forecastIDX = 0; forecastIDX < zone.Value.forcast.Count; forecastIDX++)
                    {
                        var f = zone.Value.forcast[forecastIDX];
                        var forcastEntry = new WeatherModel.ForecastEntry()
                        {
                            StartTime = 0.0,
                            EndTime = double.MaxValue,
                            State = f.state.Cast<IWeatherState>(),
                            Pattern = weatherPatternsFromStateNames[f.state.name],
                            Started = true
                        };
                        forecast.Add(forcastEntry);

                        try
                        {
                            
                            reg._model._zoneDatas[zones[zone.Key]].Forecast._items
                                .First(x => x.Pattern.name == f.state.name);
                            
                            reg.RunPatternState(zones[zone.Key],
                                weatherPatternsFromStateNames[f.state.name].CreatePattern(),
                                f.state.Cast<IWeatherState>(),
                                true);
                        } catch { }

                        yield return null;
                    }
                    
                    foreach (var running in dir._runningStates)
                    {
                        if (!forcastRunCheck.Contains(running.GetName()))
                            reg.StopPatternState(zones[zone.Key],
                                weatherPatternsFromStateNames[running.Cast<WeatherStateDefinition>().name]
                                    .CreatePattern(),
                                running);
                    }

                    WeatherModel.ZoneData data = new WeatherModel.ZoneData()
                    {
                        Forecast = forecast,
                        Parameters = new WeatherModel.ZoneWeatherParameters()
                        {
                            WindDirection = zone.Value.windSpeed
                        }
                    };
                    WeatherRegistry.ZoneWeatherData data2 =
                        new WeatherRegistry.ZoneWeatherData(reg.ZoneConfigList._items[zone.Key], data);
                    zoneDatas.Add(zones[zone.Key], data);
                    zoneDatas2.Add(zones[zone.Key], data2);
                    
                    yield return null;
                }
            }


            reg._zones = zoneDatas2;
            reg._model = new WeatherModel()
            {
                _participant = sceneContext.WeatherRegistry.Cast<WeatherModel.Participant>(),
                _zoneDatas = zoneDatas,
            };
        }
        
        public static bool handlingPacket = false;

        public static bool handlingNavPacket = false;
        
        public static StaticGameEvent GetGameEvent(string dataKey) => Resources.FindObjectsOfTypeAll<StaticGameEvent>().FirstOrDefault(x => x._dataKey == dataKey);
        
        public const bool DEBUG_MODE = true;

        public static long NextMultiplayerActorID => ++sceneContext.GameModel._actorIdProvider._nextActorId;
    }
}
