using Mirror;
using Il2CppMonomiPark.SlimeRancher.Regions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace NewSR2MP.Networking.Packet
{
    public struct PediaMessage : NetworkMessage
    {
        public PediaDirector.Id id;
    }
}
