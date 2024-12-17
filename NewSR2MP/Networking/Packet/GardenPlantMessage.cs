
using Il2CppMonomiPark.SlimeRancher.Regions;
using Riptide;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace NewSR2MP.Networking.Packet
{
    public class GardenPlantMessage : ICustomMessage
    {
        public string id;
        public int ident;
        public bool replace;
        
        public Message Serialize()
        {
            Message msg = Message.Create(MessageSendMode.Unreliable, PacketType.GardenPlant);
            msg.AddInt(ident);
            msg.AddBool(replace);
            msg.AddString(id);

            return msg;
        }
    }
}
