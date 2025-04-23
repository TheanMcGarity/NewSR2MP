using UnityEngine;

namespace NewSR2MP.Networking.Packet
{
    public class KillAllCommandMessage : ICustomMessage
    {
        public int sceneGroup;
        public int actorType = -1;
        public Message Serialize()
        {
            var msg = Message.Create(MessageSendMode.Unreliable, PacketType.KillAllCommand);
            
            msg.AddInt(sceneGroup);
            msg.AddInt(actorType);
            
            return msg;
        }

        public void Deserialize(Message msg)
        {
            sceneGroup = msg.GetInt();
            actorType = msg.GetInt();
        }
    }
}