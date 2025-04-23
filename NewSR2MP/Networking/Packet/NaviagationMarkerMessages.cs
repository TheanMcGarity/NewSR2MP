using UnityEngine;

namespace NewSR2MP.Networking.Packet
{
    public enum MapType : byte
    {
        RainbowIsland,
        Labyrinth,
    }
    
    public class PlaceNavMarkerNessage : ICustomMessage
    {
        public MapType map;
        public Vector3 position;
        
        public Message Serialize()
        {
            Message msg = Message.Create(MessageSendMode.Unreliable, PacketType.NavigationMarkerPlace);
            
            msg.AddByte((byte)map);
            msg.AddVector3(position);
            
            return msg;
        }

        public void Deserialize(Message msg)
        {
            map = (MapType)msg.GetByte();
            position = msg.GetVector3();
        }
    }
    
    public class RemoveNavMarkerNessage : ICustomMessage
    {
        // Empty packet. Just used to inform of removal of navigation marker.
        
        public Message Serialize()
        {
            Message msg = Message.Create(MessageSendMode.Unreliable, PacketType.NavigationMarkerRemove);
            
            return msg;
        }

        public void Deserialize(Message msg)
        {
            throw new NotImplementedException();
        }
    }
}
