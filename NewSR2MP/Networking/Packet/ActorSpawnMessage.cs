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
    public struct ActorSpawnMessage : NetworkMessage
    {
        public long id;
        public Vector3 position;
        public Vector3 rotation;
        public string ident;
        public int scene;
        public int player;
    }
}
