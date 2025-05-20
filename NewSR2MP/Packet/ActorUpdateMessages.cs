
using Il2CppMonomiPark.SlimeRancher.Regions;
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

        public void Serialize(OutgoingMessage msg)
        {
            msg.WriteCompressed(this);
        }
        public static NetworkEmotions Deserialize(IncomingMessage msg)
        {
            return msg.ReadCompressedSlimeEmotions();
        }
    }
    public class ActorUpdateMessage : IPacket
    {
        public PacketReliability Reliability => PacketReliability.UnreliableUnordered;

        public PacketType Type => ActorUpdate;

        public long id;
        
        public Vector3 position;
        public Vector3 rotation;

        public Vector3 velocity;
        
        public NetworkEmotions slimeEmotions = new NetworkEmotions();
        
        public void Serialize(OutgoingMessage msg)
        {
            msg.Write(id);
            msg.Write(position);
            msg.WriteCompressed(rotation);
            msg.Write(velocity);

            slimeEmotions.Serialize(msg);
        }

        public void Deserialize(IncomingMessage msg)
        {
            id = msg.ReadInt64();
            position = msg.ReadVector3();
            rotation = msg.ReadCompressedVector3();
            velocity = msg.ReadVector3();
            
            slimeEmotions = NetworkEmotions.Deserialize(msg);
        }
    }
    public class ActorUpdateClientMessage : IPacket // Remind me to merge this with the main message
    {
        public PacketReliability Reliability => PacketReliability.UnreliableUnordered;

        public PacketType Type => TempClientActorUpdate;

        public long id;
        
        public Vector3 position;
        public Vector3 rotation;

        public Vector3 velocity;
        
        public NetworkEmotions slimeEmotions = new NetworkEmotions();
        
        public void Serialize(OutgoingMessage msg)
        {
            msg.Write(id);
            msg.Write(position);
            msg.WriteCompressed(rotation);
            msg.Write(velocity);

            slimeEmotions.Serialize(msg);
        }

        public void Deserialize(IncomingMessage msg)
        {
            id = msg.ReadInt64();
            position = msg.ReadVector3();
            rotation = msg.ReadCompressedVector3();
            velocity = msg.ReadVector3();
            
            slimeEmotions = NetworkEmotions.Deserialize(msg);
        }
    }
    public class ActorUpdateOwnerMessage : IPacket // Owner update message.
    {
        public PacketReliability Reliability => PacketReliability.UnreliableUnordered;

        public PacketType Type => ActorBecomeOwner;

        public long id;
        public int player;
        
        public void Serialize(OutgoingMessage msg)
        {
            
            msg.Write(id);
            msg.Write(player);

            
        }

        public void Deserialize(IncomingMessage msg)
        {
            id = msg.ReadInt64();
            player = msg.ReadInt32();
        }
    }
    public class ActorVelocityMessage : IPacket // Set velocity for new actor owner.
    {   
        public PacketReliability Reliability => PacketReliability.UnreliableUnordered;

        public PacketType Type => ActorVelocitySet;

        public Vector3 velocity;
        public bool bounce;
        public long id;

        
        public void Serialize(OutgoingMessage msg)
        {
            
            msg.Write(velocity);
            msg.Write(id);
            msg.Write(bounce);

            
        }

        public void Deserialize(IncomingMessage msg)
        {
            velocity = msg.ReadVector3();
            id = msg.ReadInt64();
            bounce = msg.ReadBoolean();
        }
    }
    public class ActorSetOwnerMessage : IPacket // Host informing client to set actor
    {
        public PacketReliability Reliability => PacketReliability.UnreliableUnordered;

        public PacketType Type => ActorSetOwner;

        public long id;
        public Vector3 velocity;
        
        public void Serialize(OutgoingMessage msg)
        {
            
            msg.Write(id);
            msg.Write(velocity);
            
            
        }

        public void Deserialize(IncomingMessage msg)
        {
            id = msg.ReadInt64();
            velocity = msg.ReadVector3();
        }
    }
    public class ActorChangeHeldOwnerMessage : IPacket // Largo holder change message.
    {
        public PacketReliability Reliability => PacketReliability.UnreliableUnordered;

        public PacketType Type => ActorHeldOwner;

        public long id;
        
        public void Serialize(OutgoingMessage msg)
        {
            
            msg.Write(id);
            
        }

        public void Deserialize(IncomingMessage msg)
        {
            // no
        }
    }
    public class ActorDestroyGlobalMessage : IPacket // Destroy message. Runs on both client and server (Global)
    {
        public PacketReliability Reliability => PacketReliability.UnreliableUnordered;

        public PacketType Type => ActorDestroy;

        public long id;

        public void Serialize(OutgoingMessage msg)
        {
            msg.Write(id);
        }

        public void Deserialize(IncomingMessage msg)
        {
            id = msg.ReadInt64();
        }
    }
}
