namespace NewSR2MP.Networking.Packet
{
    public class PlayerUpgradeMessage : IPacket
    {
        public PacketReliability Reliability => PacketReliability.UnreliableUnordered;

        public PacketType Type => PlayerUpgrade;

        public byte id;
        
        public void Serialize(OutgoingMessage msg)
        {
            
            msg.Write(id);

            
        }

        public void Deserialize(IncomingMessage msg)
        {
            id = msg.ReadByte();
        }
    }
}
