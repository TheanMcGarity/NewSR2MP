

namespace NewSR2MP.Packet
{
    public class MarketRefreshMessage : IPacket
    {
        public PacketReliability Reliability => PacketReliability.UnreliableUnordered;

        public PacketType Type => MarketRefresh;

        public List<float> prices = new();
        

        public void Serialize(OutgoingMessage msg)
        {
            
            
            msg.Write(prices.Count);
            
            foreach (var price in prices)
                msg.Write(price);

            
        }

        public void Deserialize(IncomingMessage msg)
        {
            var c = msg.ReadInt32();
            prices = new List<float>(c);

            for (int i = 0; i < c; i++)
                prices.Add(msg.ReadFloat());
        }
    }
}
