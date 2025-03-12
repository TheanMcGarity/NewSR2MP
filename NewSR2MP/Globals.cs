using Il2CppMonomiPark.ScriptedValue;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Il2CppAssets.Script.Util.Extensions;
using Il2CppMonomiPark.SlimeRancher.DataModel;
using Il2CppMonomiPark.SlimeRancher.Pedia;
using Il2CppMonomiPark.SlimeRancher.Persist;
using Il2CppMonomiPark.SlimeRancher.SceneManagement;
using Il2CppMonomiPark.SlimeRancher.Slime;
using Il2CppMonomiPark.SlimeRancher.UI;
using Il2CppMonomiPark.SlimeRancher.UI.MainMenu;
using Il2CppMonomiPark.SlimeRancher.Weather;
using Il2CppMonomiPark.UnitPropertySystem;
using Il2CppSystem.Net.WebSockets;
using NewSR2MP.Networking.SaveModels;
using SR2E;
using SR2E.Managers;
using Unity.Mathematics;
using UnityEngine;

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
            try { value = float.Parse(input); }
            catch { return false; }
            if (inclusive)
            {
                if (value < min) return false;
            }
            else if (value <= min) return false;
            return true;
        }
        public static void InitializeCommandExtensions()
        {
            SR2ECommandManager.RegisterCommandAddon("emotions", new(args =>
            {
                SlimeEmotions.Emotion emotion;
                switch (args[0])
                {
                    case "hunger": emotion = SlimeEmotions.Emotion.HUNGER; break;
                    case "agitation": emotion = SlimeEmotions.Emotion.AGITATION; break;
                    case "fear": emotion = SlimeEmotions.Emotion.FEAR; break;
                    case "sleepiness": emotion = SlimeEmotions.Emotion.SLEEPINESS; break;
                    default: return;
                }
                Camera cam = Camera.main; if (cam == null) return;
                if (Physics.Raycast(new Ray(cam.transform.position, cam.transform.forward), out var hit,Mathf.Infinity,defaultMask))
                {
                    var slime = hit.collider.gameObject.GetComponent<SlimeEmotions>();
                    if (slime != null)
                    {
                        if (args.Length == 2)
                        {
                            if (!TryParseFloat(args[1], out float newValue, 0,true)) return;
                            if (newValue > 1) newValue = 1;
                            slime.Set(emotion, newValue);
                        }

                    }
                }
            }));
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
        
        public static Dictionary<string, UpgradeDefinition> playerUpgrades = new Dictionary<string, UpgradeDefinition>();
        
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

        public static List<NetworkActorOwnerToggle> GetUnownedActors()
        {
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
            List<NetworkActorOwnerToggle> unowned = new List<NetworkActorOwnerToggle>();

            Vector3 size = new Vector3(50, 200, 50);
            
            foreach(var player in players.Values)
                AddList(GetActorsInBounds(new Bounds(player.transform.position, size)), owned);

            found = GetActorsInBounds(new Bounds(sceneContext.player.transform.position, size));
            
            unowned = DifferenceOf(found, owned);
            
            return unowned;
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
                            list.TryAdd(state.name,pattern);
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
            sceneContext.GameModel._actorIdProvider._nextActorId++;
            
            var model = CreateActorModel(id, ident, position, rotation, sceneGroup);
            
            sceneContext.GameModel.identifiables.Add(id, model);
            if (!sceneContext.GameModel.identifiablesByIdent.TryGetValue(ident, out var types))
            {  
                sceneContext.GameModel.identifiablesByIdent.Add(ident, new Il2CppSystem.Collections.Generic.List<IdentifiableModel>(0));
                types = sceneContext.GameModel.identifiablesByIdent[ident];
            }
            types.Add(model);
            
            debugRegisteredActors.Add(id.Value, model);
            var actor = InstantiateActorFromModel(model);
            
            if (actor)
                actor.transform.position = position;
            var savedGame2 = gameContext.AutoSaveDirector.SavedGame;

            var data = CreateActorDataFromModel(
                model,
                gameContext.AutoSaveDirector.SavedGame._statusEffectTranslation,
                savedGame2.identifiableTypeToPersistenceId);
            
            savedGame2.gameState.Actors.Add(data);
            
            SRMP.Debug($"Spawned actor - ID:{id.Value} TYPE:{ident.name} POSITION:({position}) ROTATION:({rotation.ToEuler()}) SCENEGROUP:{sceneGroup.name}");

            multiplayerSpawnedActorsIDs.Add(id.Value);
            
            return actor;
        }
        public static void DeregisterActor(ActorId id)
        {
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
        
        private static Dictionary<long, ActorModel> debugRegisteredActors = new Dictionary<long, ActorModel>();

        public static ActorDataV02 CreateActorDataFromModel(
            ActorModel actorModel,
            SavedGame.PersistenceIDTranslation<StatusEffectDefinition> statusEffectToPersistenceIdTranslation,
            IdentifiableTypePersistenceIdLookupTable identTable)
        {
            var statusEffectToPersistenceId = statusEffectToPersistenceIdTranslation.InstanceLookupTable;
            ActorDataV02 actorDataV = new ActorDataV02();

            IdentifiableType ident = actorModel.ident;
            actorDataV.TypeId = identTable.GetPersistenceId(ident);

            actorDataV.ActorId = actorModel.actorId.Value;

            Vector3V01 position = new Vector3V01();
            position.Value = actorModel.lastPosition;
            actorDataV.Pos = position;

            Vector3V01 rotation = new Vector3V01();
            rotation.Value = actorModel.lastRotation.eulerAngles;
            actorDataV.Rot = rotation;

            actorDataV.SceneGroup = sceneGroupsReverse[actorModel.SceneGroup.name];

            var emotions = new SlimeEmotionDataV01();
            emotions.EmotionData = new Il2CppSystem.Collections.Generic.Dictionary<SlimeEmotions.Emotion, float>();
            emotions.EmotionData.Add(SlimeEmotions.Emotion.FEAR,
                actorModel.TryCast<SlimeModel>() != null ? actorModel.Cast<SlimeModel>().Emotions.x : 0f);
            emotions.EmotionData.Add(SlimeEmotions.Emotion.HUNGER,
                actorModel.TryCast<SlimeModel>() != null ? actorModel.Cast<SlimeModel>().Emotions.y : 0f);
            emotions.EmotionData.Add(SlimeEmotions.Emotion.AGITATION,
                actorModel.TryCast<SlimeModel>() != null ? actorModel.Cast<SlimeModel>().Emotions.z : 0f);
            emotions.EmotionData.Add(SlimeEmotions.Emotion.SLEEPINESS,
                actorModel.TryCast<SlimeModel>() != null ? actorModel.Cast<SlimeModel>().Emotions.w : 0f);
            actorDataV.Emotions = emotions;


            Il2CppSystem.Collections.Generic.List<StatusEffectV01> statusEffects =
                new Il2CppSystem.Collections.Generic.List<StatusEffectV01>();
            foreach (var effect in actorModel.statusEffects)
            {
                var effectV01 = new StatusEffectV01()
                {
                    ExpirationTime = effect.value.ExpirationTime,
                    ID = statusEffectToPersistenceId.GetPersistenceId(effect.key)
                };
                statusEffects.Add(effectV01);
            }

            actorDataV.StatusEffects = statusEffects;

            actorDataV.CycleData = new ResourceCycleDataV01();

            if (actorModel is SlimeModel slimeModel)
            {
                slimeModel.Pull(ref actorDataV, identTable);
            }
            else if (actorModel is AnimalModel animalModel)
            {
                animalModel.Pull(ref actorDataV, identTable);
            }
            else if (actorModel is ProduceModel produceModel)
            {
                produceModel.Pull(out var state, out var time);
                actorDataV.CycleData.State = state;
                actorDataV.CycleData.ProgressTime = time;
            }
            else if (actorModel is StatueFormModel statueFormModel)
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
            ammoPointersToPlotIDs.TryAdd(storage.LocalAmmo.Pointer, storage.GetComponentInParent<LandPlotLocation>()._id);
            
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

                if (isSlotEmptyOrSameType)// && IsIdentAllowedForAmmo)
                {
                    return j;
                }
            }
            return -1;
        }

        public static void RegisterAllSilos()
        {
            foreach (var silo in Resources.FindObjectsOfTypeAll<SiloStorage>())
            {
                if (!string.IsNullOrEmpty(silo.gameObject.scene.name))
                {
                    try
                    {
                        silo.RegisterAmmoPointer();
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
    }
}
