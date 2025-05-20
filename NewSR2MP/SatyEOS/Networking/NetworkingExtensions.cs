using Epic.OnlineServices.P2P;
using NewSR2MP.EpicSDK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewSR2MP
{
    public static class NetworkingExtensions
    {
        public static void SendPacket(this IPacket packet, PacketReliability packetReliability = PacketReliability.ReliableOrdered, byte channel = 0, Globals.PlayerState except = null)
        {
            if(ClientActive())
            {
                EpicApplication.Instance.Lobby.NetworkClient.SendPacket(packet, packet.Reliability);
            }
            if(ServerActive())
            {
                EpicApplication.Instance.Lobby.NetworkServer.SendPacketToAll(packet, except?.epicID, packet.Reliability);
            }
        }
    }
}
