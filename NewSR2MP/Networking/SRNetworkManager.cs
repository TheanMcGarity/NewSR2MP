using Mirror;
using Il2CppMonomiPark.SlimeRancher.Persist;
using NewSR2MP.Networking.Component;
using NewSR2MP.Networking.Packet;
using NewSR2MP.Networking.Patches;
using NewSR2MP.Networking.SaveModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;



namespace NewSR2MP.Networking
{
    public struct NetGameInitialSettings
    {
        public NetGameInitialSettings(bool defaultValueForAll = true) // Would not use paramater here but this version of c# is ehh...
        {
            shareMoney = defaultValueForAll;
            shareKeys = defaultValueForAll;
            shareUpgrades = defaultValueForAll;
        }

        public bool shareMoney;
        public bool shareKeys;
        public bool shareUpgrades;
    }
    public class SRNetworkManager : NetworkManager
    {
        public static NetGameInitialSettings initialWorldSettings = new NetGameInitialSettings();

        internal static void CheckForMPSavePath()
        {
            if (!Directory.Exists(Path.Combine(GameContext.Instance.AutoSaveDirector.StorageProvider.Cast<FileStorageProvider>().savePath, "MultiplayerSaves")))
            {
                Directory.CreateDirectory(Path.Combine(Path.Combine(GameContext.Instance.AutoSaveDirector.StorageProvider.Cast<FileStorageProvider>().savePath, "MultiplayerSaves"));
            }
        }


        public static Dictionary<long, NetworkActor> actors = new Dictionary<long, NetworkActor>();
        public static Dictionary<long, long> actorIDLocals = new Dictionary<long, long>();

        public static Dictionary<int, NetworkRegion> regions = new Dictionary<int, NetworkRegion>();

        public override void OnStartClient()
        {
            NetworkHandler.Client.Start(false);
        }
        public override void OnStartHost()
        {
            NetworkHandler.Client.Start(true);


            var localPlayer = SceneContext.Instance.player.AddComponent<NetworkPlayer>();
            localPlayer.id = 0;

            foreach (var a in Resources.FindObjectsOfTypeAll<Identifiable>())
            {
                try
                {
                    if (a.gameObject.scene.name == "worldGenerated")
                    {
                        var actor = a.gameObject;
                        actor.AddComponent<NetworkActor>();
                        actor.AddComponent<NetworkActorOwnerToggle>();
                        actor.AddComponent<TransformSmoother>();
                        actor.AddComponent<NetworkResource>();
                        var ts = actor.GetComponent<TransformSmoother>();
                        ts.interpolPeriod = 0.15f;
                        ts.enabled = false;
                        actors.Add(a.GetActorId().Value, a.GetComponent<NetworkActor>());
                    }
                }
                catch { }
            }
            SceneContext.Instance.gameObject.AddComponent<TimeSyncer>();

        }
        public override void OnStopHost()
        {
            NetworkAmmo.all.Clear();
            MultiplayerManager.Instance.isHosting = false;

        }
        public override void OnStartServer()
        {
            NetworkHandler.Server.Start();
        }

        public override void OnServerDisconnect(NetworkConnectionToClient conn)
        {
            DoNetworkSave();

            try
            {
                players[conn.connectionId].enabled = true;
                Destroy(players[conn.connectionId].gameObject);
                players.Remove(conn.connectionId);
                clientToGuid.Remove(conn.connectionId);
            }
            catch { }

        }
        public override void OnClientDisconnect()
        {
            NetworkAmmo.all.Clear();
            try
            {
                SystemContext.Instance.SceneLoader.LoadMainMenuSceneGroup();
            }
            catch { }
        }
        public override void OnStopClient()
        {
            SystemContext.Instance.SceneLoader.LoadMainMenuSceneGroup();
        }
        public override void OnServerConnect(NetworkConnectionToClient conn)
        {
        }

        public override void OnClientConnect()
        {
            var joinMsg = new ClientUserMessage()
            {
                guid = Main.data.Player,
                name = Main.data.Username,
            };
            NetworkClient.SRMPSend(joinMsg);
        }

        /// <summary>
        /// The send function common to both server and client. By default uses 'SRMPSendToAll' for server and 'SRMPSend' for client.
        /// </summary>
        /// <typeparam name="M">Message struct type. Ex: 'PlayerJoinMessage'</typeparam>
        /// <param name="message">The actual message itself. Should automatically set the M type paramater.</param>
        public static void NetworkSend<M>(M message) where M : struct, NetworkMessage
        {
            if (NetworkServer.activeHost)
            {
                NetworkServer.SRMPSendToAll(message);
            }
            else if (NetworkClient.active)
            {
                NetworkClient.SRMPSend(message);
            }
        }

        internal static (bool, ArraySegment<byte>) SRDataTransport(ArraySegment<byte> buffer)
        {
            using (NetworkReaderPooled reader = NetworkReaderPool.Get(buffer))
            {
                if (reader.ReadBool())
                    return (true, reader.ReadBytesSegment(reader.Remaining));
                else
                    return (false, reader.ReadBytesSegment(reader.Remaining));
            }
        }

        /// <summary>
        /// Erases sync values.
        /// </summary>
        public static void EraseValues()
        {
            foreach (var actor in actors.Values)
            {
                Destroyer.DestroyActor(actor.gameObject, "SRMP.EraseValues");
            }
            actors = new Dictionary<long, NetworkActor>();

            foreach (var player in players.Values)
            {
                Destroy(player.gameObject);
            }
            players = new Dictionary<int, NetworkPlayer>();
            playerRegionCheckValues = new Dictionary<int, Vector3>();

            clientToGuid = new Dictionary<int, Guid>();

            NetworkAmmo.all = new Dictionary<string, Ammo>();

            latestSaveJoined = new LoadMessage();
            savedGame = new NetworkV01();
            savedGamePath = String.Empty;
        }


        public static void DoNetworkSave()
        {

            foreach (var player in players)
            {
                Guid playerID = clientToGuid[player.Key];
                NetworkAmmo normalAmmo = (NetworkAmmo)ammos[$"player_{playerID}_normal"];
                NetworkAmmo nimbleAmmo = (NetworkAmmo)ammos[$"player_{playerID}_nimble"];
                Dictionary<AmmoMode, Il2CppSystem.Collections.Generic.List<AmmoDataV02>> ammoData = new Dictionary<AmmoMode, Il2CppSystem.Collections.Generic.List<AmmoDataV02>>();
                ammoData.Add(AmmoMode.DEFAULT, GameContext.Instance.AutoSaveDirector.SavedGame.AmmoDataFromSlots(normalAmmo.Slots));
                ammoData.Add(AmmoMode.NIMBLE_VALLEY, GameContext.Instance.AutoSaveDirector.SavedGame.AmmoDataFromSlots(nimbleAmmo.Slots));
                savedGame.savedPlayers.playerList[playerID].ammo = ammoData;
                var playerPos = new Vector3V02();
                playerPos.value = player.Value.transform.position;
                var playerRot = new Vector3V02();
                playerRot.value = player.Value.transform.eulerAngles;
                savedGame.savedPlayers.playerList[playerID].position = playerPos;
                savedGame.savedPlayers.playerList[playerID].rotation = playerRot;
            }
            using (FileStream fs = File.Open(savedGamePath, FileMode.Create))
            {
                savedGame.Write(fs);
            }
        }
    }

    /// <summary>
    /// Server send type for NetworkSend
    /// </summary>
    public enum ServerSendType
    {
        ALL,
        TO_CONNECTION,
        ALL_EXCEPT_CONNECTION,
        TO_MULTIPLE_CONNECTIONS,
    }
}
