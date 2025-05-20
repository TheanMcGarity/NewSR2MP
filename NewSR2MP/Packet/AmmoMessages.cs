
using Il2CppMonomiPark.SlimeRancher.Regions;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace NewSR2MP.Packet
{
    public class AmmoEditSlotMessage : IPacket
    {
        public PacketReliability Reliability => PacketReliability.UnreliableUnordered;

        public PacketType Type => AmmoEdit;

        public int ident;
        public int slot;
        public int count;
        public string id;
        
        public void Serialize(OutgoingMessage msg)
        {
            
            msg.Write(ident);
            msg.Write(slot);
            msg.Write(count);
            msg.Write(id);

            
        }

        public void Deserialize(IncomingMessage msg)
        {
            ident = msg.ReadInt32();
            slot = msg.ReadInt32();
            count = msg.ReadInt32();
            id = msg.ReadString();
        }
    }
    public class AmmoAddMessage : IPacket
    {
        public PacketReliability Reliability => PacketReliability.UnreliableUnordered;

        public PacketType Type => AmmoAdd;

        public int ident;
        public string id;
        
        public void Serialize(OutgoingMessage msg)
        {
            
            msg.Write(ident);
            msg.Write(id);

            
        }

        public void Deserialize(IncomingMessage msg)
        {
            ident = msg.ReadInt32();
            id = msg.ReadString();
        }
    }
    public class AmmoRemoveMessage : IPacket
    {
        public PacketReliability Reliability => PacketReliability.UnreliableUnordered;

        public PacketType Type => AmmoRemove;

        public int index;
        public int count;
        public string id;
        
        public void Serialize(OutgoingMessage msg)
        {
            
            msg.Write(index);
            msg.Write(id);
            msg.Write(count);

            
        }

        public void Deserialize(IncomingMessage msg)
        {
            index = msg.ReadInt32();
            id = msg.ReadString();
            count = msg.ReadInt32();
        }
    }
    public class AmmoSelectMessage : IPacket
    {
        public PacketReliability Reliability => PacketReliability.UnreliableUnordered;

        public PacketType Type => AmmoSelect;

        public int index;
        public string id;
        
        public void Serialize(OutgoingMessage msg)
        {
            
            msg.Write(index);
            msg.Write(id);

            
        }

        public void Deserialize(IncomingMessage msg)
        {
            index = msg.ReadInt32();
            id = msg.ReadString();
        }
    }
}
