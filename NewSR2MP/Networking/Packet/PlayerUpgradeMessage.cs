namespace NewSR2MP.Networking.Packet
{
    public class PlayerUpgradeMessage : ICustomMessage
    {
        public byte id;
        
        public Message Serialize()
        {
            Message msg = Message.Create(MessageSendMode.Unreliable, PacketType.PlayerUpgrade);
            msg.AddByte(id);

            return msg;
        }

        public void Deserialize(Message msg)
        {
            id = msg.GetByte();
        }
    }
}
