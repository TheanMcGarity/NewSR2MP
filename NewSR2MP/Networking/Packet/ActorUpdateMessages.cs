
using Il2CppMonomiPark.SlimeRancher.Regions;
using Riptide;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace NewSR2MP.Networking.Packet
{
    public class ActorUpdateMessage : ICustomMessage
    {
        public long id;
        public Vector3 position;
        public Vector3 rotation;
        
        public Message Serialize()
        {
            Message msg = Message.Create(MessageSendMode.Unreliable, PacketType.ActorUpdate);

            return msg;

            return msg;
        }
    }
    public class ActorUpdateClientMessage : ICustomMessage // Client Message is just a copy, but it has a different handler.
    {
        public long id;
        public Vector3 position;
        public Vector3 rotation;
        
        public Message Serialize()
        {
            Message msg = Message.Create(MessageSendMode.Unreliable, PacketType.TempClientActorUpdate);
            msg.AddLong(id);
            msg.AddVector3(position);
            msg.AddVector3(rotation);

            return msg;
        }
    }
    public class ActorUpdateOwnerMessage : ICustomMessage // Owner update message.
    {
        public long id;
        public int player;
        
        public Message Serialize()
        {
            Message msg = Message.Create(MessageSendMode.Reliable, PacketType.ActorOwner);
            msg.AddLong(id);
            msg.AddInt(player);

            return msg;
        }
    }
    public class ActorChangeHeldOwnerMessage : ICustomMessage // Largo holder change message.
    {
        public long id;
        
        public Message Serialize()
        {
            Message msg = Message.Create(MessageSendMode.Unreliable, PacketType.ActorHeldOwner);
            return msg;

            return msg;
        }
    }
    public class ActorDestroyGlobalMessage : ICustomMessage // Destroy message. Runs on both client and server (Global)
    {
        public long id;
        
        public Message Serialize()
        {
            Message msg = Message.Create(MessageSendMode.Reliable, PacketType.ActorDestroy);
            
            msg.AddLong(id);

            return msg;
        }
    }
}
