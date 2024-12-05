using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Il2CppMonomiPark.SlimeRancher.Pedia;
using Il2CppMonomiPark.SlimeRancher.SceneManagement;
using NewSR2MP.Networking.SaveModels;
using UnityEngine;

namespace NewSR2MP
{
    public static class Globals
    {
        internal static EosApiKey GetAPIKey()
        {
            var key = ScriptableObject.CreateInstance<EosApiKey>();

            key.epicClientId = "xyza7891vEIt18NTG5woeNE6E7eKG7Yr";
            key.epicClientSecret = "AaAXPKUogRkC6C0J4N5512Ye81vzr+jJ2zV7nytjptU";
            key.epicProductId = "5cabbf45e03042e9b93f40449849c50d";
            key.epicProductName = "SRMP";

            return key;
        }
        
        public static string GetStringFromPersistentID_IdentifiableType(int persistentID)
        {
            return GameContext.Instance.AutoSaveDirector._savedGame.persistenceIdToIdentifiableType._referenceIdProviderLookup[GameContext.Instance.AutoSaveDirector._savedGame.persistenceIdToIdentifiableType._indexTable[persistentID]].name;
        }
        
        public const bool SHOW_ERRORS = false; 
        
        public static int Version;
        
        public static Dictionary<string, IdentifiableType> identifiableTypes = new Dictionary<string, IdentifiableType>();
        
        public static Dictionary<string, PediaEntry> pediaEntries = new Dictionary<string, PediaEntry>();
        
        public static Dictionary<string, UpgradeDefinition> playerUpgrades = new Dictionary<string, UpgradeDefinition>();
        
        public static Dictionary<int, SceneGroup> sceneGroups = new Dictionary<int, SceneGroup>();
        
        public static Dictionary<int, NetworkPlayer> players = new Dictionary<int, NetworkPlayer>();
        
        public static Dictionary<int, Guid> clientToGuid = new Dictionary<int, Guid>();
        
        public static Dictionary<long, NetworkActor> actors = new Dictionary<long, NetworkActor>();

        public static NetworkV01 savedGame;
        public static string savedGamePath;
        
        public static Dictionary<int, Vector3> playerRegionCheckValues = new Dictionary<int, Vector3>();

        public static Dictionary<string, Ammo> ammos => NetworkAmmo.all;

        public static LoadMessage latestSaveJoined;

        public static int currentPlayerID;
    }
}
