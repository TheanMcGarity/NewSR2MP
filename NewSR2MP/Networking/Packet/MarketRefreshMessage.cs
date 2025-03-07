

namespace NewSR2MP.Networking.Packet
{
    public class MarketRefreshMessage : ICustomMessage
    {
        public List<float> prices = new List<float>();
    
        public Message Serialize()
        {
            Message msg = Message.Create(MessageSendMode.Unreliable, PacketType.MarketRefresh);
            msg.AddInt(prices.Count);
            
            foreach (var price in prices)
                msg.AddFloat(price);

            return msg;
        }
        
    }
}
