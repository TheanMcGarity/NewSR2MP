using UnityEngine;

namespace NewSR2MP.Packet
{
    public class KillAllCommandMessage : IPacket
    {
        public PacketReliability Reliability => PacketReliability.UnreliableUnordered;

        public PacketType Type => KillAllCommand;

        public int sceneGroup;
        public int actorType = -1;
        public void Serialize(OutgoingMessage msg)
        {
            msg.Write(sceneGroup);
            msg.Write(actorType);
            
            
        }

        public void Deserialize(IncomingMessage msg)
        {
            sceneGroup = msg.ReadInt32();
            actorType = msg.ReadInt32();
        }
    }
}