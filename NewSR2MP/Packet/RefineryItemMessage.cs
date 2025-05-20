using Il2CppSystem.Xml;

namespace NewSR2MP.Packet
{
    public class RefineryItemMessage : IPacket
    {
        public PacketReliability Reliability => PacketReliability.UnreliableUnordered;

        public PacketType Type => RefineryItem;

        public ushort id;
        public ushort count;
        
        public void Serialize(OutgoingMessage msg)
        {
            
            msg.Write(id);
            msg.Write(count);

            
        }

        public void Deserialize(IncomingMessage msg)
        {
            id = msg.ReadUInt16();
            count = msg.ReadUInt16();
        }
    }
}
