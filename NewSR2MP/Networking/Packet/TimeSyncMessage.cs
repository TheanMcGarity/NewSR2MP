
using Riptide;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace NewSR2MP.Networking.Packet
{
    public class TimeSyncMessage : ICustomMessage
    {
        public double time;
        
        public Message Serialize()
        {
            Message msg = Message.Create(MessageSendMode.Unreliable, PacketType.TimeUpdate);
            msg.AddDouble(time);

            return msg;
        }
    }
}
