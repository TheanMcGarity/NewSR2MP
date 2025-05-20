
using Il2CppMonomiPark.SlimeRancher.Regions;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Epic.OnlineServices.P2P;
using UnityEngine;

namespace NewSR2MP.Networking.Packet
{
    public class GardenPlantMessage : IPacket
    {        public PacketReliability Reliability => PacketReliability.UnreliableUnordered;

        public PacketType Type => GardenPlant;

        public string id;
        public int ident;
        public bool replace;
        
        public void Serialize(OutgoingMessage msg)
        {
            
            msg.Write(ident);
            msg.Write(replace);
            msg.Write(id);

            
        }

        public void Deserialize(IncomingMessage msg)
        {
            ident = msg.ReadInt32();
            replace = msg.ReadBoolean();
            id = msg.ReadString();
        }
    }
}
