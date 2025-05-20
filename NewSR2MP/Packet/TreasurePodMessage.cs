using UnityEngine;

namespace NewSR2MP.Networking.Packet
{
    public class TreasurePodMessage : IPacket
    {   
        public PacketReliability Reliability => PacketReliability.UnreliableUnordered;

        public PacketType Type => PacketType.TreasurePod;

        public int id;
        public void Serialize(OutgoingMessage msg)
        {
            msg.Write(id);
        }

        public void Deserialize(IncomingMessage msg)
        {
            id = msg.ReadInt32();
        }
    }
}