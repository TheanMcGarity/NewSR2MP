
using Riptide;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Riptide;
using UnityEngine;

namespace NewSR2MP.Networking.Packet
{
    public class PlayerJoinMessage : ICustomMessage
    {
        public int id;
        public bool local;
    
        public Message Serialize()
        {
            Message msg = Message.Create(MessageSendMode.Reliable, PacketType.PlayerJoin);
            msg.AddInt(id);
            msg.AddBool(local);

            return msg;
        }
        
    }
    public class ClientUserMessage : ICustomMessage
    {
        public Guid guid;
        public string name;
    
        public Message Serialize()
        {
            Message msg = Message.Create(MessageSendMode.Reliable, PacketType.RequestJoin);
            msg.AddGuid(guid);
            msg.AddString(name);

            return msg;
        }
        
    }
    public class PlayerLeaveMessage : ICustomMessage
    {
        public int id;
    
        public Message Serialize()
        {
            Message msg = Message.Create(MessageSendMode.Reliable, PacketType.PlayerLeave);
            msg.AddInt(id);

            return msg;
        }
        
    }
}
