namespace NewSR2MP.Packet
{
    public class DoorOpenMessage : IPacket
    {
        public PacketReliability Reliability => PacketReliability.UnreliableUnordered;

        public PacketType Type => OpenDoor;

        public string id;
        
        public void Serialize(OutgoingMessage msg)
        {
            
            msg.Write(id);

            
        }

        public void Deserialize(IncomingMessage msg)
        {
            id = msg.ReadString();
        }
    }
}
