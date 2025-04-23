namespace NewSR2MP.Networking.Packet
{
    public class RefineryItemMessage : ICustomMessage
    {
        public ushort id;
        public ushort count;
        
        public Message Serialize()
        {
            Message msg = Message.Create(MessageSendMode.Unreliable, PacketType.RefineryItem);
            msg.AddUShort(id);
            msg.AddUShort(count);

            return msg;
        }

        public void Deserialize(Message msg)
        {
            id = msg.GetUShort();
            count = msg.GetUShort();
        }
    }
}
