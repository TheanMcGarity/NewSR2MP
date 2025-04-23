
using Riptide;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace NewSR2MP.Networking.Packet
{
    public class SleepMessage : ICustomMessage
    {
        public double targetTime;
        
        public Message Serialize()
        {
            Message msg = Message.Create(MessageSendMode.Unreliable, PacketType.FastForward);
            
            msg.AddDouble(targetTime);

            return msg;
        }

        public void Deserialize(Message msg)
        {
            targetTime = msg.GetDouble();
        }
    }
}
