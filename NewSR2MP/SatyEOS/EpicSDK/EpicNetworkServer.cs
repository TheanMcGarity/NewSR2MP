using Epic.OnlineServices;
using Epic.OnlineServices.P2P;
using NewSR2MP.Networking.Data;
using NewSR2MP.Networking;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SRMP.Enums;

namespace NewSR2MP.EpicSDK
{
    public class EpicNetworkServer : EpicP2P
    {
        private byte nextPlayerId = 0;
        private Queue<byte> freeIds = new Queue<byte>();

        public EpicNetworkServer(P2PInterface p2PInterface) : base(p2PInterface, true)
        {
            
        }
        
        public void StartListen()
        {
            SetupP2P();

            EpicApplication.Instance.Metrics.BeginSession();

            MultiplayerManager.Instance.StartHosting();
        }

        public void SendPacket(ProductUserId targetUserId, IPacket packet,
            PacketReliability packetReliability = PacketReliability.ReliableOrdered)//, byte channel = 0)
        {
            OutgoingMessage om = new OutgoingMessage();
            om.Data = Array.Empty<byte>();
            
            packet.Serialize(om);
            
            int fragCount = (om.m_data.Length + 999) / 1000;
            var payload = om.m_data;
            int len = payload.Length;
            
            byte[] packetTypeAsBytes = BitConverter.GetBytes((ushort)packet.Type);
            
            for (byte i = 0; i < fragCount; i++)
            {               
                //SRMP.Debug($"Sending fragment {i} for packet type \"{packet.Type}\"");
                int offset = i * 1000;
                int chunkSize = Math.Min(1000, len - offset);
                
                byte[] buffer = new byte[4 + chunkSize];
                buffer[0] = packetTypeAsBytes[0];
                buffer[1] = packetTypeAsBytes[1];
                buffer[2] = i;
                buffer[3] = (byte)fragCount;

                Buffer.BlockCopy(payload, offset, buffer, 4, chunkSize);

                SendDataInternal(targetUserId, buffer, packetReliability);
            }
        }

        public void SendPacketToAll(IPacket packet, ProductUserId except = null, PacketReliability packetReliability = PacketReliability.ReliableOrdered)//, byte channel = 0)
        {
            foreach (var player in players)
            {
                if (except == player.epicID || player.playerID == ushort.MaxValue) continue;

                SendPacket(player.epicID, packet, packetReliability);
            }
        }

        public override void OnMessageReceived(ProductUserId senderUserId, byte channel, ref IncomingMessage im)
        {
            
            if (TryGetPlayer(senderUserId, out var player))
            {
                try
                {
                    PacketType packetType = (PacketType)im.ReadUInt16();
                    byte fragmentIndex = im.ReadByte();
                    byte totalFragments = im.ReadByte();

                    byte[] payload = im.Data.Skip(4).ToArray();

                    if (!incompletePackets.TryGetValue(packetType, out var msg))
                    {
                        msg = new IncompletePacket
                        {
                            fragments = new byte[totalFragments][],
                            fragTotal = totalFragments,
                            fragIndex = 0,
                            initTime = Time.unscaledTime,
                        };
                        incompletePackets[packetType] = msg;
                    }

                    if (msg.fragments[fragmentIndex] == null)
                    {
                        msg.fragments[fragmentIndex] = payload;
                        msg.fragIndex++;
                    }

                    if (msg.fragIndex >= msg.fragTotal)
                    {
                        List<byte> completeData = new List<byte>();
                        int debugLogIndex = 0;
                        foreach (var frag in msg.fragments)
                        {
                            //SRMP.Debug($"Adding fragment {debugLogIndex} for message type \"{packetType}\"");              
                            debugLogIndex++;
                            completeData.AddRange(frag);
                        }
                        

                        incompletePackets.Remove(packetType);
                        im = new IncomingMessage
                        {
                            m_data = completeData.ToArray(),
                            LengthBytes = completeData.Count
                        };
                    }
                    else
                        return;
                    
                    
                    if(player.connectionState == NetworkPlayerConnectionState.Authenticating)
                    {
                        if(packetType != Auth)
                        {
                            SRMP.Error($"Packet({packetType}) was sent during authentication!");
                            //CloseConnection(senderUserId);
                            return;
                        }

                        HandleAuthentication(player, im);
                    }
                    else if(player.connectionState != NetworkPlayerConnectionState.Connected)
                    {
                    }
                    else
                    {
                        NetworkHandler.HandleServerPacket(player, channel, packetType, im);
                    }
                }
                catch (Exception ex)
                {
                    SRMP.Error($"Error in network server! {ex}");
                    //CloseConnection(senderUserId);
                }
            }
            else SRMP.Error($"Unknown player {senderUserId} tried to send a packet!");
        }

        private void HandleAuthentication(Globals.PlayerState player, IncomingMessage im)
        {
            var username = im.ReadString();
            var guid = im.ReadString();


            MultiplayerManager.PlayerJoin(player.playerID, Guid.Parse(guid), username);
            player.gameObject = MultiplayerManager.Instance.OnPlayerJoined(username, player.playerID, player.epicID);
            player.gameObject.Intialize(player.epicID);
            player.gameObject.SetUsername(username);
            player.connectionState = NetworkPlayerConnectionState.Connected;
        }

        public override void OnConnected(ProductUserId remoteUserId, NetworkConnectionType networkType, ConnectionEstablishedType connectionType)
        {
            byte nextId;
            if (freeIds.Count > 0)
            {
                nextId = freeIds.Dequeue();
            }
            else
            {
                nextId = nextPlayerId++;
            }

            players.Add(new Globals.PlayerState
            {
                epicID = remoteUserId,
                playerID = nextId,
            });
        }

        public override void OnDisconnected(ProductUserId remoteUserId, ConnectionClosedReason reason)
        {
            if (TryGetPlayer(remoteUserId, out var player))
            {
                MultiplayerManager.Instance.OnPlayerLeft(player.playerID);
            }
        }

        public override void OnConnectionRequest(ProductUserId remoteUserId)
        {
            if (systemContext.SceneLoader.IsSceneLoadInProgress)
            {
                CloseConnection(remoteUserId);
                return;
            }

            if (EpicApplication.Instance.Lobby.ContainsUserId(remoteUserId))
            {
                AcceptConnection(remoteUserId);
            }
            else
            {
                CloseConnection(remoteUserId);
            }
        }
    }
}
