
using Il2CppMonomiPark.SlimeRancher.Regions;
using UnityEngine;

namespace NewSR2MP.Networking.Packet
{
    public class ActorSpawnClientMessage : ICustomMessage
    {
        public Vector3 position;
        public Vector3 rotation;
        public Vector3 velocity;
        public int ident;    
        public int scene;
        public int player;
        
        public Message Serialize()
        {
            Message msg = Message.Create(MessageSendMode.Unreliable, PacketType.TempClientActorSpawn);
            
            msg.AddInt(ident);
            msg.AddVector3(position);
            msg.AddVector3(rotation);
            msg.AddVector3(velocity);
            msg.AddInt(scene);
            msg.AddInt(player);
            
            return msg;
        }

        public void Deserialize(Message msg)
        {
            ident = msg.GetInt();
            position = msg.GetVector3();
            rotation = msg.GetVector3();
            velocity = msg.GetVector3();
            scene = msg.GetInt();
            player = msg.GetInt();
        }
    }
}
