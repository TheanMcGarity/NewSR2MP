
using Il2CppMonomiPark.SlimeRancher.Regions;
using Riptide;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace NewSR2MP.Networking.Packet
{
    public struct NetworkEmotions
    {
        public float x;
        public float y;
        public float z;
        public float w;

        public NetworkEmotions(float x, float y, float z, float w)
        {
            this.x = x;
            this.y = y;
            this.z = z;
            this.w = w;
        }

        public void Serialize(Message msg)
        {
            msg.AddFloat(x);
            msg.AddFloat(y);
            msg.AddFloat(z);
            msg.AddFloat(w);
        }
        public static NetworkEmotions Deserialize(Message msg)
        {
            var x = msg.GetFloat();
            var y = msg.GetFloat();
            var z = msg.GetFloat();
            var w = msg.GetFloat();
            
            return new NetworkEmotions(x, y, z, w);
        }
    }
    public class ActorUpdateMessage : ICustomMessage
    {
        public long id;
        
        public Vector3 position;
        public Vector3 rotation;

        public Vector3 velocity;
        
        public NetworkEmotions slimeEmotions = new NetworkEmotions();
        
        public Message Serialize()
        {
            Message msg = Message.Create(MessageSendMode.Unreliable, PacketType.ActorUpdate);

            msg.AddLong(id);
            msg.AddVector3(position);
            msg.AddVector3(rotation);
            msg.AddVector3(velocity);

            slimeEmotions.Serialize(msg);
            
            return msg;
        }

        public void Deserialize(Message msg)
        {
            id = msg.GetLong();
            position = msg.GetVector3();
            rotation = msg.GetVector3();
            velocity = msg.GetVector3();
            slimeEmotions = NetworkEmotions.Deserialize(msg);
        }
    }
    public class ActorUpdateClientMessage : ICustomMessage // Remind me to merge this with the main message
    {
        public long id;
        
        public Vector3 position;
        public Vector3 rotation;
        
        public Vector3 velocity;

        public NetworkEmotions slimeEmotions = new NetworkEmotions();

        public Message Serialize()
        {
            Message msg = Message.Create(MessageSendMode.Unreliable, PacketType.TempClientActorUpdate);
            msg.AddLong(id);
            msg.AddVector3(position);
            msg.AddVector3(rotation);
            msg.AddVector3(velocity);
            
            slimeEmotions.Serialize(msg);
            
            return msg;
        }

        public void Deserialize(Message msg)
        {
            id = msg.GetLong();
            position = msg.GetVector3();
            rotation = msg.GetVector3();
            velocity = msg.GetVector3();
            slimeEmotions = NetworkEmotions.Deserialize(msg);
        }
    }
    public class ActorUpdateOwnerMessage : ICustomMessage // Owner update message.
    {
        public long id;
        public int player;
        
        public Message Serialize()
        {
            Message msg = Message.Create(MessageSendMode.Reliable, PacketType.ActorBecomeOwner);
            msg.AddLong(id);
            msg.AddInt(player);

            return msg;
        }

        public void Deserialize(Message msg)
        {
            id = msg.GetLong();
            player = msg.GetInt();
        }
    }
    public class ActorVelocityMessage : ICustomMessage // Set velocity for new actor owner.
    {   
        public Vector3 velocity;
        public bool isOwner;
        public long id;

        
        public Message Serialize()
        {
            Message msg = Message.Create(MessageSendMode.Reliable, PacketType.ActorVelocitySet);
            msg.AddVector3(velocity);
            msg.AddLong(id);
            msg.AddBool(isOwner);

            return msg;
        }

        public void Deserialize(Message msg)
        {
            velocity = msg.GetVector3();
            id = msg.GetLong();
            isOwner = msg.GetBool();
        }
    }
    public class ActorSetOwnerMessage : ICustomMessage // Host informing client to set actor
    {
        public long id;
        public Vector3 velocity;
        
        public Message Serialize()
        {
            Message msg = Message.Create(MessageSendMode.Reliable, PacketType.ActorBecomeOwner);
            msg.AddLong(id);
            msg.AddVector3(velocity);
            
            return msg;
        }

        public void Deserialize(Message msg)
        {
            id = msg.GetLong();
            velocity = msg.GetVector3();
        }
    }
    public class ActorChangeHeldOwnerMessage : ICustomMessage // Largo holder change message.
    {
        public long id;
        
        public Message Serialize()
        {
            Message msg = Message.Create(MessageSendMode.Unreliable, PacketType.ActorHeldOwner);
            msg.AddLong(id);
            return msg;
        }

        public void Deserialize(Message msg)
        {
            // no
        }
    }
    public class ActorDestroyGlobalMessage : ICustomMessage // Destroy message. Runs on both client and server (Global)
    {
        public long id;
        private ICustomMessage _customMessageImplementation;

        public Message Serialize()
        {
            Message msg = Message.Create(MessageSendMode.Reliable, PacketType.ActorDestroy);
            
            msg.AddLong(id);

            return msg;
        }

        public void Deserialize(Message msg)
        {
            id = msg.GetLong();
        }
    }
}
