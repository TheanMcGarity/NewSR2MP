
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
        public string username;
    
        public Message Serialize()
        {
            Message msg = Message.Create(MessageSendMode.Reliable, PacketType.PlayerJoin);
            msg.AddInt(id);
            msg.AddBool(local);
            msg.AddString(username);

            return msg;
        }

        public void Deserialize(Message msg)
        {
            id = msg.GetInt();
            local = msg.GetBool();
            username = msg.GetString();
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

        public void Deserialize(Message msg)
        {
            guid = msg.GetGuid();
            name = msg.GetString();
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

        public void Deserialize(Message msg)
        {
            id = msg.GetInt();
        }
    }
}
