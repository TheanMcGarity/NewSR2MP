

namespace NewSR2MP.Networking.Packet
{
    public class MapUnlockMessage : ICustomMessage
    {
        public string id;
    
        public Message Serialize()
        {
            Message msg = Message.Create(MessageSendMode.Unreliable, PacketType.MapUnlock);
            msg.AddString(id);

            return msg;
        }
        
    }
}
