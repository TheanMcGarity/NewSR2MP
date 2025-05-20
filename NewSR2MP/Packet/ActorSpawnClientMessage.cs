
using Il2CppMonomiPark.SlimeRancher.Regions;
using UnityEngine;

namespace NewSR2MP.Packet
{
    public class ActorSpawnClientMessage : IPacket
    {
        public PacketReliability Reliability => PacketReliability.ReliableOrdered;

        public PacketType Type => TempClientActorSpawn;

        public Vector3 position;
        public Vector3 rotation;
        public Vector3 velocity;
        public int ident;    
        public int scene;
        public int player;
        
        public void Serialize(OutgoingMessage msg)
        {
            
            
            msg.Write(ident);
            msg.Write(position);
            msg.Write(rotation);
            msg.Write(velocity);
            msg.Write(scene);
            msg.Write(player);
            
            
        }

        public void Deserialize(IncomingMessage msg)
        {
            ident = msg.ReadInt32();
            position = msg.ReadVector3();
            rotation = msg.ReadVector3();
            velocity = msg.ReadVector3();
            scene = msg.ReadInt32();
            player = msg.ReadInt32();
        }
    }
}
