using Il2CppMonomiPark.ScriptedValue;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Il2CppMonomiPark.SlimeRancher.Pedia;
using Il2CppMonomiPark.SlimeRancher.SceneManagement;
using Il2CppMonomiPark.SlimeRancher.Weather;
using NewSR2MP.Networking.SaveModels;
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
        }
        
        public static bool isJoiningAsClient = false;
        
        public static bool ServerActive() => MultiplayerManager.server != null;
        public static bool ClientActive() => MultiplayerManager.client != null;
        
        
        public static void InitEmbeddedDLL(string name)
        {
            System.IO.Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream($"NewSR2MP.{name}");
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
            return GameContext.Instance.AutoSaveDirector.SavedGame.identifiableTypeToPersistenceId.GetPersistenceId(ident);
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

            found = GetActorsInBounds(new Bounds(SceneContext.Instance.player.transform.position, size));
            
            unowned = DifferenceOf(found, owned);
            
            return unowned;
        }
        
        public static NetworkV01 savedGame;
        public static string savedGamePath;

        public static Dictionary<string, Ammo> ammos => NetworkAmmo.all;

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
    }
    
}
