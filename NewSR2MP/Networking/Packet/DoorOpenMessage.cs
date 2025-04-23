namespace NewSR2MP.Networking.Packet
{
    public class DoorOpenMessage : ICustomMessage
    {
        public string id;
        
        public Message Serialize()
        {
            Message msg = Message.Create(MessageSendMode.Unreliable, PacketType.OpenDoor);
            msg.AddString(id);

            return msg;
        }

        public void Deserialize(Message msg)
        {
            id = msg.GetString();
        }
    }
}
