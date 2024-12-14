
using Riptide;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewSR2MP.Networking.Packet
{
    public class SetMoneyMessage : ICustomMessage
    {
        public int newMoney;
        // public PlayerState.CoinsType type;
        
        public Message Serialize()
        {
            Message msg = Message.Create(MessageSendMode.Unreliable, PacketType.SetCurrency);
            msg.AddInt(newMoney);

            return msg;
        }
    }
}
