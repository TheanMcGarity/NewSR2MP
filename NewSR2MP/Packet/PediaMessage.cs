
using Il2CppMonomiPark.SlimeRancher.Regions;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace NewSR2MP.Packet
{
    public class PediaMessage : IPacket
    {
        public PacketReliability Reliability => PacketReliability.UnreliableUnordered;

        public PacketType Type => PediaUnlock;

        public string id;
    
        public void Serialize(OutgoingMessage msg)
        {
            
            msg.Write(id);

            
        }

        public void Deserialize(IncomingMessage msg)
        {
            id = msg.ReadString();
        }
    }
}
