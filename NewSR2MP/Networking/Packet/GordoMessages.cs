
using Epic.OnlineServices.P2P;
using Il2CppMonomiPark.SlimeRancher.Regions;
using UnityEngine;

namespace NewSR2MP.Networking.Packet
{
    public class GordoEatMessage : IPacket
    {
        public PacketReliability Reliability => PacketReliability.UnreliableUnordered;
        public PacketType Type => GordoFeed;

        public string id;
        public int count;
        public int ident;
        
        
        public void Serialize(OutgoingMessage msg)
        {
            
            msg.Write(id);
            msg.Write(count);
            msg.Write(ident);

            
        }

        public void Deserialize(IncomingMessage msg)
        {
            id = msg.ReadString();
            count = msg.ReadInt32();
            ident = msg.ReadInt32();
        }
    }
    public class GordoBurstMessage : IPacket
    {
        public PacketReliability Reliability => PacketReliability.UnreliableUnordered;

        public PacketType Type => GordoExplode;

        public string id;
        public int ident;

        public void Serialize(OutgoingMessage msg)
        {
            
            msg.Write(id);      
            msg.Write(ident);

            
        }

        public void Deserialize(IncomingMessage msg)
        {
            id = msg.ReadString();
            ident = msg.ReadInt32();
        }
    }
}
