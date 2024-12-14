
using Il2CppMonomiPark.SlimeRancher.Regions;
using UnityEngine;

namespace NewSR2MP.Networking.Packet
{
    public class GordoEatMessage : ICustomMessage
    {
        public string id;
        public int count;
        
        
        public Message Serialize()
        {
            Message msg = Message.Create(MessageSendMode.Unreliable, PacketType.GordoFeed);
            msg.AddString(id);
            msg.AddInt(count);

            return msg;
        }
    }
    public class GordoBurstMessage : ICustomMessage
    {
        public string id;

        public Message Serialize()
        {
            Message msg = Message.Create(MessageSendMode.Unreliable, PacketType.GordoExplode);
            msg.AddString(id);
            return msg;
        }
    }
}
