
using Il2CppMonomiPark.SlimeRancher.Regions;
using UnityEngine;

namespace NewSR2MP.Packet
{
    public class SwitchModifyMessage : IPacket
    {
        public PacketReliability Reliability => PacketReliability.UnreliableUnordered;

        public PacketType Type => SwitchModify;

        public string id;
        public byte state;
        
        public void Serialize(OutgoingMessage msg)
        {
            
            
            msg.Write(id);
            msg.Write(state);
            
            
        }

        public void Deserialize(IncomingMessage msg)
        {
            id = msg.ReadString();
            state = msg.ReadByte();
        }
    }
}
