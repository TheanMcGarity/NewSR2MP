using UnityEngine;

namespace NewSR2MP.Packet
{
    public enum MapType : byte
    {
        RainbowIsland,
        Labyrinth,
    }
    
    public class PlaceNavMarkerNessage : IPacket
    {
        public PacketReliability Reliability => PacketReliability.UnreliableUnordered;

        public PacketType Type => NavigationMarkerPlace;

        public MapType map;
        public Vector3 position;
        
        public void Serialize(OutgoingMessage msg)
        {
            
            
            msg.Write((byte)map);
            msg.Write(position);
            
            
        }

        public void Deserialize(IncomingMessage msg)
        {
            map = (MapType)msg.ReadByte();
            position = msg.ReadVector3();
        }
    }
    
    public class RemoveNavMarkerNessage : IPacket
    {
        public PacketReliability Reliability => PacketReliability.UnreliableUnordered;

        public PacketType Type => NavigationMarkerRemove;

        // Empty packet. Just used to inform of removal of navigation marker.
        
        public void Serialize(OutgoingMessage msg)
        {
            
            
            
        }

        public void Deserialize(IncomingMessage msg)
        {
            throw new NotImplementedException();
        }
    }
}
