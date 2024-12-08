using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Il2CppMonomiPark.SlimeRancher.Pedia;
using Il2CppMonomiPark.SlimeRancher.SceneManagement;
using NewSR2MP.Networking.SaveModels;
using UnityEngine;

namespace NewSR2MP
{
    public static class FinnDevExtentions
    {
        //
        // Copied from SR2E
        // I do technically partially own sr2e as well but these methods were made by Finn (https://github.com/ThatFinnDev)
        //
        public static T getObjRec<T>(this GameObject obj, string name) where T : class
        {
            var transform = obj.transform;

            List<GameObject> totalChildren = getAllChildren(transform);
            for (int i = 0; i < totalChildren.Count; i++)
                if (totalChildren[i].name == name)
                {
                    if (typeof(T) == typeof(GameObject))
                        return totalChildren[i] as T;
                    if (typeof(T) == typeof(Transform))
                        return totalChildren[i].transform as T;
                    if (totalChildren[i].GetComponent<T>() != null)
                        return totalChildren[i].GetComponent<T>();
                }
            return null;
        }

        public static List<Transform> GetChildren(this Transform obj)
        {
            List<Transform> children = new List<Transform>();
            for (int i = 0; i < obj.childCount; i++)
                children.Add(obj.GetChild(i)); 
            return children;
        }
        public static void DestroyAllChildren(this Transform obj)
        {
            for (int i = 0; i < obj.childCount; i++) GameObject.Destroy(obj.GetChild(i).gameObject); 
        }
        public static List<GameObject> getAllChildren(this GameObject obj)
        {
            var container = obj.transform;
            List<GameObject> allChildren = new List<GameObject>();
            for (int i = 0; i < container.childCount; i++)
            {
                var child = container.GetChild(i);
                allChildren.Add(child.gameObject);
                allChildren.AddRange(getAllChildren(child));
            }
            return allChildren;
        }

        public static T[] getAllChildrenOfType<T>(this GameObject obj) where T : Component
        {
            List<T> children = new List<T>();
            foreach (var child in obj.getAllChildren())
            {
                if (child.GetComponent<T>() != null)
                {
                    children.Add(child.GetComponent<T>());
                }
            }
            return children.ToArray();
        }

        public static T[] getAllChildrenOfType<T>(this Transform obj) where T : Component
        {
            List<T> children = new List<T>();
            foreach (var child in obj.getAllChildren())
            {
                if (child.GetComponent<T>() != null)
                {
                    children.Add(child.GetComponent<T>());
                }
            }
            return children.ToArray();
        }

        public static T getObjRec<T>(this Transform transform, string name) where T : class
        {
            List<GameObject> totalChildren = getAllChildren(transform);
            for (int i = 0; i < totalChildren.Count; i++)
                if (totalChildren[i].name == name)
                {
                    if (typeof(T) == typeof(GameObject))
                        return totalChildren[i] as T;
                    if (typeof(T) == typeof(Transform))
                        return totalChildren[i].transform as T;
                    if (totalChildren[i].GetComponent<T>() != null)
                        return totalChildren[i].GetComponent<T>();
                }
            return null;
        } public static List<GameObject> getAllChildren(this Transform container)
        {
            List<GameObject> allChildren = new List<GameObject>();
            for (int i = 0; i < container.childCount; i++)
            {
                var child = container.GetChild(i);
                allChildren.Add(child.gameObject);
                allChildren.AddRange(getAllChildren(child));
            }
            return allChildren;
        }
        
    }
    
    public static class Globals
    {
        
        public static AssetBundle InitializeAssetBundle(string bundleName)
        {
            System.IO.Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream($"NewSR2MP.{bundleName}");
            byte[] buffer = new byte[16 * 1024];
            Il2CppSystem.IO.MemoryStream ms = new Il2CppSystem.IO.MemoryStream();
            int read;
            while ((read = stream.Read(buffer, 0, buffer.Length)) > 0)
                ms.Write(buffer, 0, read);
            return AssetBundle.LoadFromMemory(ms.ToArray());
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
        /// Do not use this in your own programs!
        /// </summary>
        /// <returns>The mod's very own API key. DO NOT USE THIS FOR YOUR OWN PROJECTS</returns>
        /// DO NOT USE THIS FOR YOUR OWN PROJECTS
        /// DO NOT USE THIS FOR YOUR OWN PROJECTS
        /// DO NOT USE THIS FOR YOUR OWN PROJECTS
        /// DO NOT USE THIS FOR YOUR OWN PROJECTS
        /// DO NOT USE THIS FOR YOUR OWN PROJECTS
        /// DO NOT USE THIS FOR YOUR OWN PROJECTS
        internal static EosApiKey GetAPIKey()
        {
            var key = ScriptableObject.CreateInstance<EosApiKey>();

            key.epicDeploymentId = "85657bc0947f45b8976082409f60b3ad";
            key.epicSandboxId = "dfcf7f2faa004223b14b04d7f5aaeac1";
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
