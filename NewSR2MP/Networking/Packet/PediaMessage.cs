
using Il2CppMonomiPark.SlimeRancher.Regions;
using Riptide;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace NewSR2MP.Networking.Packet
{
    public class PediaMessage : ICustomMessage
    {
        public string id;
    
        public Message Serialize()
        {
            Message msg = Message.Create(MessageSendMode.Unreliable, PacketType.PediaUnlock);
            msg.AddString(id);

            return msg;
        }

        public void Deserialize(Message msg)
        {
            id = msg.GetString();
        }
    }
}
