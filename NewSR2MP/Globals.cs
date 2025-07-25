﻿using Il2CppMonomiPark.ScriptedValue;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Epic.OnlineServices;
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
using NewSR2MP.SaveModels;
using SR2E;
using SR2E.Managers;
using SRMP.Enums;
using Unity.Mathematics;
using UnityEngine;

namespace NewSR2MP
{
    public static class Globals
    {
        public static void NetworkCopyTo(this byte[] src, byte[] dst, int offsetDst = 4, int offsetSrc = 0, int size = 1000)
        {
            for (int i = offsetSrc; i < offsetSrc + size; i++)
            {
                if (!(dst.Length <= (i - offsetSrc) + offsetDst))
                    dst[(i - offsetSrc) + offsetDst] = src[i];
                else return;
            }
        }

        public static void WriteCompressed(this NetBuffer net, Vector3 pos)
        {
            int x = (int)pos.x & 0xFFF; 
            int z = (int)pos.z & 0xFFF; 
            int y = (int)pos.y & 0xFF;
            
            net.Write((y << 24) | (z << 12) | x);
        }
        
        public static void WriteCompressed(this NetBuffer net, NetworkEmotions emotions)
        {
            byte x = (byte)Mathf.Clamp(emotions.x * 255f, 0f, 255f);
            byte y = (byte)Mathf.Clamp(emotions.y * 255f, 0f, 255f);
            byte z = (byte)Mathf.Clamp(emotions.z * 255f, 0f, 255f);
            byte w = (byte)Mathf.Clamp(emotions.w * 255f, 0f, 255f);
            net.Write((uint)(x | (y << 8) | (z << 16) | (w << 24)));
        }
        public static NetworkEmotions ReadCompressedSlimeEmotions(this NetBuffer net)
        {
            var compressed = net.ReadInt32();
            
            float x = ((compressed >> 0) & 0xFF) / 255f;
            float y = ((compressed >> 8) & 0xFF) / 255f;
            float z = ((compressed >> 16) & 0xFF) / 255f;
            float w = ((compressed >> 24) & 0xFF) / 255f;
            
            return new NetworkEmotions(x, y, z, w);
        }
        public static Vector3 ReadCompressedVector3(this NetBuffer net)
        {
            var compressed = net.ReadInt32();
            
            int x = compressed & 0xFFF;
            int z = (compressed >> 12) & 0xFFF;
            int y = (compressed >> 24) & 0xFF;

            if ((x & 0x800) != 0) x |= unchecked((int)0xFFFFF000);
            if ((z & 0x800) != 0) z |= unchecked((int)0xFFFFF000);
            if ((y & 0x80) != 0) y |= unchecked((int)0xFFFFFF00);

            return new Vector3(x, y, z);
        }
        public static void Write(this NetBuffer net, AmmoData ammoData)
        {
            net.Write(ammoData.id);
            net.Write(ammoData.count);
            net.Write(ammoData.slot);
        }
        public static AmmoData ReadAmmoData(this NetBuffer net)
        {
            return new AmmoData
            {
                id = net.ReadInt32(),
                count = net.ReadInt32(),
                slot = net.ReadInt32(),
            };
        }
        public static Guid ReadGuid(this NetBuffer net)
        {
            return Guid.Parse(net.ReadString());
        }
        
        public static bool TryGetPlayer(ProductUserId id, out NetPlayerState netPlayer)
        {
            netPlayer = players.FirstOrDefault(x => x.epicID == id);
            return netPlayer != null;
        }
        public static bool TryGetPlayer(ushort id, out NetPlayerState netPlayer)
        {
            netPlayer = players.FirstOrDefault(x => x.playerID == id);
            return netPlayer != null;
        }
        
        /// <summary>
        /// Auto host port in options. can be 0 (off), 7777, 16500
        /// </summary>
        public static int autoHostPort => scriptedAutoHostPort ? scriptedAutoHostPort.Value : 0;

        /// <summary>
        /// Do not manually edit this.
        /// </summary>
        internal static ScriptedInt? scriptedAutoHostPort;

        internal static Texture2D LoadImage(string filename)
        {
            Assembly executingAssembly = Assembly.GetExecutingAssembly();
            Stream manifestResourceStream = executingAssembly.GetManifestResourceStream(executingAssembly.GetName().Name + "." + filename + ".png");
            byte[] array = new byte[manifestResourceStream.Length];
            manifestResourceStream.Read(array, 0, array.Length);
            Texture2D texture2D = new Texture2D(1, 1);
            ImageConversion.LoadImage(texture2D, array);
            texture2D.filterMode = FilterMode.Bilinear;
            return texture2D;
        }
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
            AmmoSelect,
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
            TreasurePod,
            Auth,
        }
        public static byte[] ExtractResource(String filename)
        {
            System.Reflection.Assembly a = System.Reflection.Assembly.GetExecutingAssembly();
            using (Stream resFilestream = a.GetManifestResourceStream(filename))
            {
                if (resFilestream == null) return null;
                byte[] ba = new byte[resFilestream.Length];
                resFilestream.Read(ba, 0, ba.Length);
                return ba;
            }
        }
        
        public static Dictionary<string, int> playerUsernames = new();
        public static Dictionary<int, string> playerUsernamesReverse = new();
        
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

        public static bool ServerActive() => EpicApplication.Instance.Lobby.IsLobbyOwner;
        public static bool ClientActive() => EpicApplication.Instance.Lobby.IsInLobby && !EpicApplication.Instance.Lobby.IsLobbyOwner;


        public static readonly string EOS_SDK_PATH = Path.Combine(Application.dataPath, "..", "UserLibs", "EOSSDK-Win64-Shipping.dll");

        
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

        public static List<NetPlayerState> players = new List<NetPlayerState>();

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

        /*internal static List<NetworkActorOwnerToggle> unownedActors; 
        
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
            

            foreach (var player in players)
            {            
                playersActors.Add(player.Key, GetActorsInBounds(new Bounds(player.Value.transform.position, size)));
                yield return null;
            }

            found = GetActorsInBounds(new Bounds(sceneContext.player.transform.position, size));
            yield return null;

            foreach (var player in playersActors)
            {
                if (player.Key < currentPlayerID && player.Key != 65535)
                {
                    AddList(player.Value, owned);
                    yield return null;
                }
            }
            
            unownedActors = DifferenceOf(found, owned);
        }*/

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

        public static LoadMessage latestSaveJoined;

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
                storage.GetComponentInParent<LandPlotLocation>()._id + storage.AmmoSetReference.name);

            if (!ammoByPlotID.TryAdd(storage.GetComponentInParent<LandPlotLocation>()._id + storage.AmmoSetReference.name, storage.LocalAmmo))
                ammoByPlotID[storage.GetComponentInParent<LandPlotLocation>()._id + storage.AmmoSetReference.name] = storage.LocalAmmo;
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
                array[i] = new Ammo.Slot
                {
                    Count = ammo._items[i].Count,
                    _id = identifiableTypes[ammo._items[i].ID],
                    Emotions = new float4(0, 0, 0, 0)
                };;
            }

            return array;
        }

        public static Ammo.Slot[] MultiplayerAmmoDataToSlots(List<AmmoData> ammo, int slotCount)
        {
            Ammo.Slot[] array = new Ammo.Slot[ammo.Count];
            for (int i = 0; i < slotCount; i++)
            {
                array[i] = new Ammo.Slot
                {
                    Count = ammo[i].count,
                    _id = identifiableTypes[ammo[i].id],
                    Emotions = new float4(0, 0, 0, 0)
                };
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
                var silos = plot.Value.gameObj.GetComponentsInChildren<SiloStorage>();
                foreach (var silo in silos)
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

        /// <summary>
        /// The instance of the currently loaded weather director.
        /// </summary>
        public static WeatherDirector? weatherDirectorInstance;

        internal static WeatherSyncMessage latestWeatherSyncMessage;

        public static IEnumerator WeatherHandlingCoroutine(WeatherSyncMessage packet)
        {
            var run = new List<Action>();
            latestWeatherSyncMessage = packet;
            if (sceneContext == null || weatherDirectorInstance == null)
                yield break;

            var reg = sceneContext.WeatherRegistry;
            var dir = weatherDirectorInstance;

            var zones = new Dictionary<byte, ZoneDefinition>();
            byte b = 0;
            foreach (var zone in reg._model._zoneDatas)
            {
                zones.Add(b, zone.key);
                b++;
            }
            yield return null;
            
            var zoneDatas = new Il2CppSystem.Collections.Generic.Dictionary<ZoneDefinition, WeatherModel.ZoneData>();
            var zoneWeatherDatas =
                new Il2CppSystem.Collections.Generic.Dictionary<ZoneDefinition, WeatherRegistry.ZoneWeatherData>();

            
            
            foreach (var zone in packet.sync.zones)
            {
                
                
                
                if (!zones.ContainsKey(zone.Key))
                {
                    continue;
                }

                var forecast = new Il2CppSystem.Collections.Generic.List<WeatherModel.ForecastEntry>();
                foreach (var f in zone.Value.forcast)
                {
                    var forcastEntry = new WeatherModel.ForecastEntry()
                    {
                        StartTime = 0.0,
                        EndTime = double.MaxValue,
                        State = f.state.Cast<IWeatherState>(),
                        Pattern = weatherPatternsFromStateNames[f.state.name],
                        Started = true
                    };
                    forecast.Add(forcastEntry);

                    
                    var f1 = f;
                    run.Add(() =>
                    {
                        var zoneDefinition = zones[zone.Key];

                        reg.RunPatternState(zoneDefinition,
                            weatherPatternsFromStateNames[f1.state.name].CreatePattern(),
                            f1.state.Cast<IWeatherState>(),
                            true);

                        if (dir.Zone == zoneDefinition)
                            dir.RunState(f1.state.Cast<IWeatherState>(), new WeatherModel.ZoneWeatherParameters());


                    });

                    var stopList = reg._zones[zones[zone.Key]].Forecast;
                    var stopZone = zones[zone.Key];
                    foreach (var stop in stopList)
                    {
                        reg.StopPatternState(stopZone,
                            weatherPatternsFromStateNames[f1.state.name].CreatePattern(),
                            stop.State);

                        if (dir.Zone == stopZone)
                            dir.StopState(stop.State, new WeatherModel.ZoneWeatherParameters());
                    }
                    
                    yield return null;
                }

                var wind = zone.Value.windSpeed;
                WeatherModel.ZoneData data = new WeatherModel.ZoneData
                {
                    Forecast = forecast,
                    Parameters = new WeatherModel.ZoneWeatherParameters
                    {
                        WindDirection = wind
                    }
                };
                WeatherRegistry.ZoneWeatherData data2 =
                    new WeatherRegistry.ZoneWeatherData(reg.ZoneConfigList._items[zone.Key], data);
                zoneDatas.Add(zones[zone.Key], data);
                zoneWeatherDatas.Add(zones[zone.Key], data2);

                yield return null;
            }
            reg._model = new WeatherModel
            {
                _participant = sceneContext.WeatherRegistry.Cast<WeatherModel.Participant>(),
                _zoneDatas = zoneDatas
            };
            reg._zones = zoneWeatherDatas;
            
            foreach (var toRun in run)
                toRun();
        }

        public static bool handlingPacket = false;

        public static bool handlingNavPacket = false;
        
        public static StaticGameEvent GetGameEvent(string dataKey) => Resources.FindObjectsOfTypeAll<StaticGameEvent>().FirstOrDefault(x => x._dataKey == dataKey);
        
        public const bool DEBUG_MODE = 
#if DEBUG
            true;
#else
            false;
#endif        

        public static long NextMultiplayerActorID => ++sceneContext.GameModel._actorIdProvider._nextActorId;
        
        public static bool clientLoading = false;

        public static string ExtendInteger(int value) => new string('0', 10 - value.ToString().Length) + value;

        public class NetPlayerState
        {
            public NetworkPlayerConnectionState connectionState = NetworkPlayerConnectionState.Authenticating;
            public ProductUserId epicID;
            public ushort playerID;
            public NetworkPlayer gameObject;
        }
    }
}
