
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace NewSR2MP.Networking.Packet
{
    public class ResourceStateMessage : IPacket
    {
        public PacketReliability Reliability => PacketReliability.UnreliableUnordered;

        public PacketType Type => ResourceState;

        public ResourceCycle.State state;
        public long id;
        
        public void Serialize(OutgoingMessage msg)
        {
            
            msg.Write((byte)state);
            msg.Write(id);
        }

        public void Deserialize(IncomingMessage msg)
        {
            state = (ResourceCycle.State)msg.ReadByte();
            id = msg.ReadInt64();
        }
    }
}
