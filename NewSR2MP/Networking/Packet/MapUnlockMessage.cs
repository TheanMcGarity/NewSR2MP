using Mirror;

namespace NewSR2MP.Networking.Packet
{
    public struct MapUnlockMessage : NetworkMessage
    {
        public ZoneDirector.Zone id;
    }
}
