using NewSR2MP.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Epic.OnlineServices.P2P;

namespace NewSR2MP
{
    public interface IPacket
    {
        PacketType Type { get; }
        PacketReliability Reliability { get; }
        
        void Serialize(OutgoingMessage om);
        void Deserialize(IncomingMessage im);
    }
}
