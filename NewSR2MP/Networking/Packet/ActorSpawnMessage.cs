
using Il2CppMonomiPark.SlimeRancher.Regions;
using Riptide;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace NewSR2MP.Networking.Packet
{
    public class ActorSpawnMessage : ICustomMessage
    {
        public long id;
        public Vector3 position;
        public Vector3 rotation;
        public int ident;
        public int scene;
        public int player;
        
        public Message Serialize()
        {
            Message msg = Message.Create(MessageSendMode.Reliable, PacketType.ActorSpawn);
            
            msg.AddLong(id);
            msg.AddInt(ident);
            msg.AddVector3(position);
            msg.AddVector3(rotation);
            msg.AddInt(scene);
            msg.AddInt(player);

            return msg;
        }

        public void Deserialize(Message msg)
        {
            id = msg.GetLong();
            ident = msg.GetInt();
            position = msg.GetVector3();
            rotation = msg.GetVector3();
            scene = msg.GetInt();
            player = msg.GetInt();
        }
    }
}
