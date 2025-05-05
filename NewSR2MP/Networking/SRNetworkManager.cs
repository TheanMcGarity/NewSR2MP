
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
using Il2CppMono.Security.Protocol.Ntlm;
using Riptide;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;


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
    public partial class MultiplayerManager
    {
        public static Server server;
        
        public static Client client;
        
        public static NetGameInitialSettings initialWorldSettings = new NetGameInitialSettings();

        internal static void CheckForMPSavePath()
        {
            if (!Directory.Exists(Path.Combine(GameContext.Instance.AutoSaveDirector.StorageProvider.Cast<FileStorageProvider>().savePath, "MultiplayerSaves")))
            {
                Directory.CreateDirectory(Path.Combine(Path.Combine(GameContext.Instance.AutoSaveDirector.StorageProvider.Cast<FileStorageProvider>().savePath, "MultiplayerSaves")));
            }
        }

        public void StartHosting()
        {
            foreach (var a in Resources.FindObjectsOfTypeAll<Identifiable>())
            {
                try
                {
                    if (!string.IsNullOrEmpty(a.gameObject.scene.name))
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

            
            
            server.ClientConnected += OnPlayerJoined;
            server.ClientDisconnected += OnPlayerLeft;

            sceneContext.gameObject.AddComponent<NetworkTimeDirector>();
            sceneContext.gameObject.AddComponent<WeatherSyncer>();
            
            var hostNetworkPlayer = sceneContext.player.AddComponent<NetworkPlayer>();
            hostNetworkPlayer.id = ushort.MaxValue;
            currentPlayerID = hostNetworkPlayer.id;
            players.Add(ushort.MaxValue, hostNetworkPlayer);
        }

        public void OnPlayerJoined(object? sender, ServerConnectedEventArgs args)
        {
            DoNetworkSave();
            foreach (var loadedPlayer in players)
            {
                // !TEMP!
                var packet3 = new PlayerJoinMessage()
                {
                    id = loadedPlayer.Key,
                    local = false
                };
                NetworkSend(packet3, ServerSendOptions.SendToPlayer(args.Client.Id));
            }
            var player = Instantiate(onlinePlayerPrefab);
            player.name = $"Player{args.Client.Id}";
            var netPlayer = player.GetComponent<NetworkPlayer>();
            players.Add(args.Client.Id, netPlayer);
            netPlayer.id = args.Client.Id;
            DontDestroyOnLoad(player);
            player.SetActive(true);
            var packet = new PlayerJoinMessage()
            {
                id = args.Client.Id,
                local = false
            };
            var packet2 = new PlayerJoinMessage()
            {
                id = args.Client.Id,
                local = true
            };
            NetworkSend(packet, ServerSendOptions.SendToAllExcept(args.Client.Id));
            NetworkSend(packet2, ServerSendOptions.SendToPlayer(args.Client.Id));

            args.Client.MaxSendAttempts = 750;
        }
        public void OnPlayerLeft(object? sender, ServerDisconnectedEventArgs args)
        {
            DoNetworkSave();

            var player = players[args.Client.Id];
            players.Remove(args.Client.Id);
            Destroy(player.gameObject);
            
            var packet = new PlayerLeaveMessage
            {
                id = args.Client.Id,
            };
            
            NetworkSend(packet);
        }
        
        public void StopHosting()
        {
            ammoByPlotID.Clear();

        }

        public void OnServerDisconnect(ushort player)
        {

            try
            {
                players[player].enabled = true;
                Destroy(players[player].gameObject);
                players.Remove(player);
                clientToGuid.Remove(player);
            }
            catch { }

        }
        public void Leave()
        {
            ammoByPlotID.Clear();
            try
            {
                systemContext.SceneLoader.LoadMainMenuSceneGroup();
            }
            catch { }
        }
        public void ClientDisconnect()
        {
            systemContext.SceneLoader.LoadMainMenuSceneGroup();
        }
        public void InitializeClient()
        {
            var joinMsg = new ClientUserMessage()
            {
                guid = Main.data.Player,
                name = Main.data.Username,
            };
            NetworkSend(joinMsg, ServerSendOptions.SendToAllDefault());
        }

        /// <summary>
        /// The send function common to both server and client. By default uses 'SRMPSendToAll' for server and 'SRMPSend' for client.
        /// </summary>
        /// <typeparam name="M">Message struct type. Ex: 'PlayerJoinMessage'</typeparam>
        /// <param name="message">The actual message itself. Should automatically set the M type paramater.</param>
        
        
        public static void NetworkSend<M>(M msg, ServerSendOptions serverOptions) where M : ICustomMessage
        {
            Message message = msg.Serialize();
            
            if (client != null)
            {
                client.Send(message);
            }
            else if (server != null)
            {
                if (serverOptions.ignoreSpecificPlayer)
                    server.SendToAll(message, serverOptions.player);
                else if (serverOptions.onlySendToPlayer)          
                    server.Send(message, serverOptions.player);
                else
                    server.SendToAll(message);

            }
        }

        public static void NetworkSend<M>(M msg) where M : ICustomMessage
        {
            NetworkSend(msg, ServerSendOptions.SendToAllDefault());
        }

        public struct ServerSendOptions
        {
            public ushort player;
            public bool ignoreSpecificPlayer;
            public bool onlySendToPlayer;

            public static ServerSendOptions SendToAllDefault()
            {
                return new ServerSendOptions()
                {
                    ignoreSpecificPlayer = false,
                    onlySendToPlayer = false,
                    player = UInt16.MinValue
                };
            }
            public static ServerSendOptions SendToPlayer(ushort player)
            {
                return new ServerSendOptions()
                {
                    ignoreSpecificPlayer = false,
                    onlySendToPlayer = true,
                    player = player
                };
            }
            public static ServerSendOptions SendToAllExcept(ushort player)
            {
                return new ServerSendOptions()
                {
                    ignoreSpecificPlayer = true,
                    onlySendToPlayer = false,
                    player = player
                };
            }
        }

        /// <summary>
        /// Erases sync values.
        /// </summary>
        public static void EraseValues()
        {
            foreach (var gadget in gadgets.Values)
            {
                DestroyGadget(gadget.gameObject, "SRMP.EraseValuesGadget");
            }
            foreach (var actor in actors.Values)
            {
                DestroyActor(actor.gameObject, "SRMP.EraseValuesActor");
            }
            actors.Clear();
            gadgets.Clear();

            foreach (var player in players.Values)
            {
                Destroy(player.gameObject);
            }
            players.Clear();

            clientToGuid.Clear();

            ammoByPlotID.Clear();

            savedGame = new NetworkV01();
            savedGamePath = String.Empty;
        }


        public static void DoNetworkSave()
        {

            foreach (var player in players)
            {
                if (player.Key == ushort.MaxValue)
                {
                    continue;
                }
                Guid playerID = clientToGuid[player.Key];
                var ammo = GetNetworkAmmo($"player_{playerID}");
                Il2CppSystem.Collections.Generic.List<AmmoDataV01> ammoData = GameContext.Instance.AutoSaveDirector.SavedGame.AmmoDataFromSlots(ammo.Slots, GameContext.Instance.AutoSaveDirector._savedGame.identifiableTypeToPersistenceId);
                savedGame.savedPlayers.playerList[playerID].ammo = ammoData;
                if (player.Value)
                {
                    var playerPos = new Vector3V01();
                    playerPos.Value = player.Value.transform.position;
                    var playerRot = new Vector3V01();
                    playerRot.Value = player.Value.transform.eulerAngles;
                    savedGame.savedPlayers.playerList[playerID].position = playerPos;
                    savedGame.savedPlayers.playerList[playerID].rotation = playerRot;
                }
            }

            GameStream fs = CppFile.Open(savedGamePath, Il2CppSystem.IO.FileMode.Create);
            savedGame.Write(fs);
            fs.Dispose();
        }
    }
}
