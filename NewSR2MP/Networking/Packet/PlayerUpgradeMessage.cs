namespace NewSR2MP.Networking.Packet
{
    public class PlayerUpgradeMessage : ICustomMessage
    {
        public byte index;
        
        public Message Serialize()
        {
            Message msg = Message.Create(MessageSendMode.Unreliable, PacketType.PlayerUpgrade);
            msg.AddByte(index);

            return msg;
        }

        public void Deserialize(Message msg)
        {
            index = msg.GetByte();
        }
    }
}
