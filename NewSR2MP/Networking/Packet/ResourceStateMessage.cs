
using Riptide;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace NewSR2MP.Networking.Packet
{
    public class ResourceStateMessage : ICustomMessage
    {
        public ResourceCycle.State state;
        public long id;
        
        public Message Serialize()
        {
            Message msg = Message.Create(MessageSendMode.Unreliable, PacketType.ResourceState);
            msg.AddByte((byte)state);
            msg.AddLong(id);

            return msg;
        }
    }
}
