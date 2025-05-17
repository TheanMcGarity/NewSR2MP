using Epic.OnlineServices.P2P;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewSR2MP.Attributes
{
    public class PacketResponseAttribute : Attribute
    {
        public PacketReliability PacketReliability { get; set; }
        public bool ExcludeSender { get; set; }
        public byte? Channel { get; set; }

        public PacketResponseAttribute(bool excludeSender = true, PacketReliability packetReliability = PacketReliability.ReliableOrdered)
        {
            PacketReliability = packetReliability;
            ExcludeSender = excludeSender;
            Channel = null;
        }

        public PacketResponseAttribute(byte channel, bool excludeSender = true, PacketReliability packetReliability = PacketReliability.ReliableOrdered)
        {
            PacketReliability = packetReliability;
            ExcludeSender = excludeSender;
            Channel = channel;
        }
    }
}
