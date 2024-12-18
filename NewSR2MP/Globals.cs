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
        public static Il2CppSystem.Type il2cppTypeof(this Type type)
        {
            string typeName = type.AssemblyQualifiedName;

            if (typeName.ToLower().StartsWith("il2cpp"))
            {
                typeName = typeName.Substring("il2cpp".Length);
            }

            Il2CppSystem.Type il2cppType = Il2CppSystem.Type.GetType(typeName);

            return il2cppType;
        }

    }
    
    // Not static, so i can look at it through UE.
    public class Globals
    {
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
            ActorOwner,
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
        }

        public static bool isJoiningAsClient = false;
        
        public static bool ServerActive() => MultiplayerManager.server != null;
        public static bool ClientActive() => MultiplayerManager.client != null;
        
        
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
        
        public static Dictionary<int, NetworkPlayer> players = new Dictionary<int, NetworkPlayer>();
        
        public static Dictionary<int, Guid> clientToGuid = new Dictionary<int, Guid>();
        
        public static Dictionary<long, NetworkActor> actors = new Dictionary<long, NetworkActor>();

        public static NetworkV01 savedGame;
        public static string savedGamePath;

        public static Dictionary<string, Ammo> ammos => NetworkAmmo.all;

        public static LoadMessage? latestSaveJoined;

        public static int currentPlayerID;
    }
}
