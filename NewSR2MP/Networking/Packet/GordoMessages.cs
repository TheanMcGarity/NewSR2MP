using Mirror;
using Il2CppMonomiPark.SlimeRancher.Regions;
using UnityEngine;

namespace NewSR2MP.Networking.Packet
{
    public struct GordoEatMessage : NetworkMessage
    {
        public string id;
        public int count;
    }
    public struct GordoBurstMessage : NetworkMessage
    {
        public string id;
    }
}
