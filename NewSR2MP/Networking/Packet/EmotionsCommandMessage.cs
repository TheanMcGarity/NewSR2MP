using UnityEngine;

namespace NewSR2MP.Networking.Packet
{
    public class EmotionsCommandMessage : ICustomMessage
    {
        public NetworkEmotions emotions;
        public Message Serialize()
        {
            var msg = Message.Create(MessageSendMode.Unreliable, PacketType.EmotionsCommand);
            
            emotions.Serialize(msg);
            
            return msg;
        }
    }
}