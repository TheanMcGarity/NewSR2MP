
using Il2CppMonomiPark.SlimeRancher.Persist;
using NewSR2MP.Component;
using NewSR2MP.Packet;
using NewSR2MP.Patches;
using NewSR2MP.SaveModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Epic.OnlineServices;
using Il2CppMono.Security.Protocol.Ntlm;
using Il2CppTMPro;
using NewSR2MP.EpicSDK;
using SRMP.Enums;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;


namespace NewSR2MP
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
            foreach (var a in Resources.FindObjectsOfTypeAll<IdentifiableActor>())
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
                        ts.interpolPeriod = ActorTimer;
                        ts.enabled = false;
                        actors.Add(a.GetActorId().Value, a.GetComponent<NetworkActor>());
                    }
                }
                catch { }
            }
            
            sceneContext.gameObject.AddComponent<NetworkTimeDirector>();
            sceneContext.gameObject.AddComponent<NetworkWeatherDirector>();
            
            var hostNetworkPlayer = sceneContext.player.AddComponent<NetworkPlayer>();
            hostNetworkPlayer.id = ushort.MaxValue;
            currentPlayerID = hostNetworkPlayer.id;
            players.Add(new Globals.PlayerState
            {
                epicID = EpicApplication.Instance.Authentication.ProductUserId,
                playerID = ushort.MaxValue,
                gameObject = hostNetworkPlayer,
                connectionState = NetworkPlayerConnectionState.Connected,
            });
        }

        public NetworkPlayer OnPlayerJoined(string username, ushort id, ProductUserId epicID)
        {
            DoNetworkSave();
            
            var player = Instantiate(onlinePlayerPrefab);
            player.name = $"Player{id}";
            var netPlayer = player.GetComponent<NetworkPlayer>();
            
            netPlayer.usernamePanel = netPlayer.transform.GetChild(1).GetComponent<TextMesh>();
            netPlayer.usernamePanel.text = username;
            netPlayer.usernamePanel.characterSize = 0.2f;
            netPlayer.usernamePanel.anchor = TextAnchor.MiddleCenter;
            netPlayer.usernamePanel.fontSize = 24;
            
            playerUsernames.Add(username, id);
            playerUsernamesReverse.Add(id, username);
            
            netPlayer.id = id;
            
            DontDestroyOnLoad(player);
            player.SetActive(true);
            
            var packet = new PlayerJoinMessage()
            {
                id = id,
                local = false,
                username = username,
            };
            var packet2 = new PlayerJoinMessage()
            {
                id = id,
                local = true,
                username = username,
            };
            NetworkSend(packet, ServerSendOptions.SendToAllExcept(id));
            NetworkSend(packet2, ServerSendOptions.SendToPlayer(id));

            return netPlayer;
        }
        public void OnPlayerLeft(ushort id)
        {
            OnServerDisconnect(id);
            
            var packet = new PlayerLeaveMessage
            {
                id = ushort.MaxValue,
            };
            
            NetworkSend(packet);
        }
        
        public void StopHosting()
        {
            ammoByPlotID.Clear();

        }

        public void OnServerDisconnect(ushort player)
        {
            DoNetworkSave();
            
            try
            {
                if (!TryGetPlayer(player, out var state))
                    return;
                
                Destroy(state.gameObject);
                players.Remove(state);
                playerUsernames.Remove(playerUsernamesReverse[state.playerID]);
                playerUsernamesReverse.Remove(player);
                players.Remove(state);
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

        /// <summary>
        /// The send function common to both server and client.
        /// </summary>
        /// <typeparam name="P">Message struct type. Ex: 'PlayerJoinMessage'</typeparam>
        /// <param name="packet">The actual message itself. Should automatically set the M type paramater.</param>
        public static void NetworkSend<P>(P packet, ServerSendOptions serverOptions) where P : IPacket
        {
            
            if (ServerActive())
                if (TryGetPlayer(serverOptions.player, out var state))
                {
                    if (serverOptions.ignoreSpecificPlayer)
                        EpicApplication.Instance.Lobby.NetworkServer.SendPacketToAll(packet, state.epicID, packetReliability:packet.Reliability);
                    else if (serverOptions.onlySendToPlayer)
                        EpicApplication.Instance.Lobby.NetworkServer.SendPacket(state.epicID, packet, packetReliability:packet.Reliability);
                    else
                        EpicApplication.Instance.Lobby.NetworkServer.SendPacketToAll(packet, packetReliability:packet.Reliability);
                }
                else
                    EpicApplication.Instance.Lobby.NetworkServer.SendPacketToAll(packet, packetReliability:packet.Reliability);
            else if (ClientActive())
                EpicApplication.Instance.Lobby.NetworkClient.SendPacket(packet, packetReliability:packet.Reliability);
            
        }

        public static void NetworkSend<P>(P packet) where P : IPacket
        {
            NetworkSend(packet, ServerSendOptions.SendToAllDefault());
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

            foreach (var player in players)
            {
                Destroy(player.gameObject);
            }
            players.Clear();
            playerUsernames.Clear();
            playerUsernamesReverse.Clear();

            clientToGuid.Clear();

            ammoByPlotID.Clear();

            savedGame = new NetworkV01();
            savedGamePath = String.Empty;
        }


        public static void DoNetworkSave()
        {
            return; // its broken rn
            
            foreach (var player in players)
            {
                if (player.playerID == ushort.MaxValue)
                    continue;
                

                if (clientToGuid.TryGetValue(player.playerID, out var playerID))
                {
                    var ammo = GetNetworkAmmo($"player_{playerID}");
                    Il2CppSystem.Collections.Generic.List<AmmoDataV01> ammoData =
                        GameContext.Instance.AutoSaveDirector.SavedGame.AmmoDataFromSlots(ammo.Slots,
                            GameContext.Instance.AutoSaveDirector._savedGame.identifiableTypeToPersistenceId);
                    if (player.gameObject && savedGame.savedPlayers.playerList.TryGetValue(playerID, out var playerFromID) )
                    {
                        playerFromID.ammo = ammoData;
                        var playerPos = new Vector3V01();
                        playerPos.Value = player.gameObject.transform.position;
                        var playerRot = new Vector3V01();
                        playerRot.Value = player.gameObject.transform.eulerAngles;
                        playerFromID.position = playerPos;
                        playerFromID.rotation = playerRot;
                    }
                }
                else
                {
                    SRMP.Error($"Error saving player {player.playerID}, their GUID was not found.");
                }
            }

            GameStream fs = CppFile.Open(savedGamePath, Il2CppSystem.IO.FileMode.Create);
            savedGame.Write(fs);
            fs.Dispose();
        }
    }
}
