
using Il2CppMonomiPark.SlimeRancher.Regions;
using Riptide;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace NewSR2MP.Networking.Packet
{
    public class AmmoEditSlotMessage : ICustomMessage
    {
        public int ident;
        public int slot;
        public int count;
        public string id;
        
        public Message Serialize()
        {
            Message msg = Message.Create(MessageSendMode.Unreliable, PacketType.AmmoEdit);
            msg.AddInt(ident);
            msg.AddInt(slot);
            msg.AddInt(count);
            msg.AddString(id);

            return msg;
        }

        public void Deserialize(Message msg)
        {
            ident = msg.GetInt();
            slot = msg.GetInt();
            count = msg.GetInt();
            id = msg.GetString();
        }
    }
    public class AmmoAddMessage : ICustomMessage
    {
        public int ident;
        public string id;
        
        public Message Serialize()
        {
            Message msg = Message.Create(MessageSendMode.Unreliable, PacketType.AmmoAdd);
            msg.AddInt(ident);
            msg.AddString(id);

            return msg;
        }

        public void Deserialize(Message msg)
        {
            ident = msg.GetInt();
            id = msg.GetString();
        }
    }
    public class AmmoRemoveMessage : ICustomMessage
    {
        public int index;
        public int count;
        public string id;
        
        public Message Serialize()
        {
            Message msg = Message.Create(MessageSendMode.Unreliable, PacketType.AmmoRemove);
            msg.AddInt(index);
            msg.AddString(id);
            msg.AddInt(count);

            return msg;
        }

        public void Deserialize(Message msg)
        {
            index = msg.GetInt();
            id = msg.GetString();
            count = msg.GetInt();
        }
    }
}
