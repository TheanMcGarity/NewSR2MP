
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace NewSR2MP.Networking.Packet
{
    public class PlayerJoinMessage : IPacket
    {
        public PacketReliability Reliability => PacketReliability.UnreliableUnordered;

        public PacketType Type => PlayerJoin;

        public int id;
        public bool local;
        public string username;
    
        public void Serialize(OutgoingMessage msg)
        {
            
            msg.Write(id);
            msg.Write(local);
            msg.Write(username);

            
        }

        public void Deserialize(IncomingMessage msg)
        {
            id = msg.ReadInt32();
            local = msg.ReadBoolean();
            username = msg.ReadString();
        }
    }
    public class PlayerLeaveMessage : IPacket
    {
        public PacketReliability Reliability => PacketReliability.UnreliableUnordered;

        public PacketType Type => PlayerLeave;

        public int id;
    
        public void Serialize(OutgoingMessage msg)
        {
            
            msg.Write(id);

            
        }

        public void Deserialize(IncomingMessage msg)
        {
            id = msg.ReadInt32();
        }
    }
}
