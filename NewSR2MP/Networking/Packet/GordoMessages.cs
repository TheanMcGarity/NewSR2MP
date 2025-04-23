
using Il2CppMonomiPark.SlimeRancher.Regions;
using UnityEngine;

namespace NewSR2MP.Networking.Packet
{
    public class GordoEatMessage : ICustomMessage
    {
        public string id;
        public int count;
        public int ident;
        
        
        public Message Serialize()
        {
            Message msg = Message.Create(MessageSendMode.Unreliable, PacketType.GordoFeed);
            msg.AddString(id);
            msg.AddInt(count);
            msg.AddInt(ident);

            return msg;
        }

        public void Deserialize(Message msg)
        {
            id = msg.GetString();
            count = msg.GetInt();
            ident = msg.GetInt();
        }
    }
    public class GordoBurstMessage : ICustomMessage
    {
        public string id;
        public int ident;

        public Message Serialize()
        {
            Message msg = Message.Create(MessageSendMode.Unreliable, PacketType.GordoExplode);
            msg.AddString(id);      
            msg.AddInt(ident);

            return msg;
        }

        public void Deserialize(Message msg)
        {
            id = msg.GetString();
            ident = msg.GetInt();
        }
    }
}
