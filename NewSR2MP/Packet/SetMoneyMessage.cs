
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewSR2MP.Networking.Packet
{
    public class SetMoneyMessage : IPacket
    {
        public PacketReliability Reliability => PacketReliability.ReliableOrdered;

        public PacketType Type => SetCurrency;

        public int newMoney;
        // public PlayerState.CoinsType type;
        
        public void Serialize(OutgoingMessage msg)
        {
            
            msg.Write(newMoney);

            
        }

        public void Deserialize(IncomingMessage msg)
        {
            newMoney = msg.ReadInt32();
        }
    }
}
