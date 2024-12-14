
using Riptide;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace NewSR2MP.Networking.Packet
{
    public class PlayerUpdateMessage : ICustomMessage
    {
        public int id;
        public Vector3 pos;
        public Quaternion rot;
        
        public Message Serialize()
        {
            Message msg = Message.Create(MessageSendMode.Unreliable, PacketType.PlayerUpdate);
            
            msg.AddInt(id);
            msg.AddVector3(pos);
            msg.AddQuaternion(rot);

            return msg;
        }
    }
}
