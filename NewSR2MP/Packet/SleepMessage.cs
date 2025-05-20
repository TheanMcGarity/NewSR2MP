
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace NewSR2MP.Packet
{
    public class SleepMessage : IPacket
    {
        public PacketReliability Reliability => PacketReliability.UnreliableUnordered;

        public PacketType Type => FastForward;

        public double targetTime;
        
        public void Serialize(OutgoingMessage msg)
        {
            
            
            msg.Write(targetTime);

            
        }

        public void Deserialize(IncomingMessage msg)
        {
            targetTime = msg.ReadDouble();
        }
    }
}
