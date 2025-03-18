
using Il2CppMonomiPark.SlimeRancher.Regions;
using UnityEngine;

namespace NewSR2MP.Networking.Packet
{
    public class SwitchModifyMessage : ICustomMessage
    {
        public string id;
        public byte state;
        
        public Message Serialize()
        {
            Message msg = Message.Create(MessageSendMode.Unreliable, PacketType.SwitchModify);
            
            msg.AddString(id);
            msg.AddByte(state);
            
            return msg;
        }
    }
}
