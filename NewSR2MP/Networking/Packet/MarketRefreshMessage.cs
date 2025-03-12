

namespace NewSR2MP.Networking.Packet
{
    public class MarketRefreshMessage : ICustomMessage
    {
        public List<float> prices = new();
    
        public Message Serialize()
        {
            Message msg = Message.Create(MessageSendMode.Reliable, PacketType.MarketRefresh);
            
            msg.AddInt(prices.Count);
            
            foreach (var price in prices)
                msg.AddFloat(price);

            return msg;
        }
        
    }
}
