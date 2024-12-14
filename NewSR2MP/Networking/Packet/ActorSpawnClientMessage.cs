
using Il2CppMonomiPark.SlimeRancher.Regions;
using UnityEngine;

namespace NewSR2MP.Networking.Packet
{
    public class ActorSpawnClientMessage : ICustomMessage
    {
        public Vector3 position;
        public Vector3 rotation;
        public Vector3 velocity;
        public string ident;    
        public int player;
        
        public Message Serialize()
        {
            Message msg = Message.Create(MessageSendMode.Unreliable, PacketType.TempClientActorSpawn);
            
            msg.AddString(ident);
            msg.AddVector3(position);
            msg.AddVector3(rotation);
            msg.AddVector3(velocity);
            msg.AddInt(player);
            
            return msg;
        }
    }
}
