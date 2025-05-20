using Epic.OnlineServices;
using Epic.OnlineServices.P2P;
using SRMP.Enums;
using NewSR2MP;
using NewSR2MP.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.SceneManagement;
using static NewSR2MP.Globals.PlayerState;

namespace NewSR2MP.EpicSDK
{
    public class EpicNetworkClient : EpicP2P
    {
        private ProductUserId serverUserId;

        public NetworkClientStatus Status { get; private set; }

        public EpicNetworkClient(P2PInterface p2PInterface) : base(p2PInterface, false)
        {
            Status = NetworkClientStatus.None;
        }

        public void Connect(ProductUserId serverUserId)
        {
            this.serverUserId = serverUserId;
            Status = NetworkClientStatus.Connecting;

            SetupP2P();

            AcceptConnection(serverUserId);
        }

        public void SendPacket(IPacket packet, PacketReliability packetReliability = PacketReliability.ReliableOrdered)//, byte channel = 0)
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
                int offset = i * 1000;
                int chunkSize = Math.Min(1000, len - offset);
                
                byte[] buffer = new byte[4 + chunkSize];
                buffer[0] = packetTypeAsBytes[0];
                buffer[1] = packetTypeAsBytes[1];
                buffer[2] = i;
                buffer[3] = (byte)fragCount;

                Buffer.BlockCopy(payload, offset, buffer, 4, chunkSize);

                SendDataInternal(serverUserId, buffer, packetReliability);
            }
        }

        public override void OnMessageReceived(ProductUserId senderUserId, byte channel, ref IncomingMessage im)
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

            NetworkHandler.HandleClientPacket(packetType, channel, im);
        }

        public override void OnShutdown()
        {
            EpicApplication.Instance.Metrics.EndSession();

            CloseConnection(serverUserId);

            if (SceneManager.GetActiveScene().buildIndex == 3)
            {
                SceneManager.LoadScene(2);
            }
        }

        private void SendAuthentication()
        {
            OutgoingMessage om = new OutgoingMessage();
            om.Data = new byte[0];

            om.Write((ushort)Auth);
            om.Write((byte)0);
            om.Write((byte)1);
            om.Write(Main.data.Username);
            om.Write(Main.data.Player.ToString());

            SendDataInternal(serverUserId, om.Data);
        }

        public override void OnConnected(ProductUserId remoteUserId, NetworkConnectionType networkType, ConnectionEstablishedType connectionType)
        {

            Status = NetworkClientStatus.Authenticating;

            EpicApplication.Instance.Metrics.BeginSession();

            SendAuthentication();
        }

        public override void OnDisconnected(ProductUserId remoteUserId, ConnectionClosedReason reason)
        {
            EpicApplication.Instance.Metrics.EndSession();

            if(remoteUserId == EpicApplication.Instance.Authentication.ProductUserId)
            {
                Status = NetworkClientStatus.Disconnected;

                if (SceneManager.GetActiveScene().buildIndex == 3)
                {
                    SceneManager.LoadScene(2);
                }
            }
        }
    }
}
