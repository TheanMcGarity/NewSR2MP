using Mirror;
using Il2CppMonomiPark.SlimeRancher.Regions;
using UnityEngine;

namespace NewSR2MP.Networking.Packet
{
    public struct ActorSpawnClientMessage : NetworkMessage
    {
        public Vector3 position;
        public Vector3 rotation;
        public Vector3 velocity;
        public Identifiable.Id ident;
        public RegionRegistry.RegionSetId region;
        public int player;
    }
}
