

namespace NewSR2MP.Packet
{
    public class LandPlotMessage : IPacket
    {
        public PacketReliability Reliability => PacketReliability.ReliableOrdered;

        public PacketType Type => PacketType.LandPlot;

        public string id;
        public LandPlot.Id type;
        public LandPlot.Upgrade upgrade;
        public LandplotUpdateType messageType;
    
        public void Serialize(OutgoingMessage msg)
        {
            
            msg.Write((byte)messageType);
            msg.Write(id);

            if (messageType == LandplotUpdateType.SET)
                msg.Write((byte)type);
            else
                msg.Write((byte)upgrade);

            
        }

        public void Deserialize(IncomingMessage msg)
        {
            messageType = (LandplotUpdateType)msg.ReadByte();
            id = msg.ReadString();
            if (messageType == LandplotUpdateType.SET)
                type = (LandPlot.Id)msg.ReadByte();
            else
                upgrade = (LandPlot.Upgrade)msg.ReadByte();

        }
    }

    public enum LandplotUpdateType : byte
    {
        SET,
        UPGRADE
    }
}
