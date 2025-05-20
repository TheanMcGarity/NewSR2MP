
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace NewSR2MP.Packet
{
    public class TimeSyncMessage : IPacket
    {
        public PacketReliability Reliability => PacketReliability.ReliableOrdered;

        public PacketType Type => TimeUpdate;

        public double time;
        
        public void Serialize(OutgoingMessage msg)
        {
            
            msg.Write(time);

            
        }

        public void Deserialize(IncomingMessage msg)
        {
            time = msg.ReadDouble();
        }
    }
}
