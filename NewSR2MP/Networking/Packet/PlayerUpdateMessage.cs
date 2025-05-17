
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace NewSR2MP.Networking.Packet
{
    public class PlayerUpdateMessage : IPacket
    {
        public PacketReliability Reliability => PacketReliability.UnreliableUnordered;

        public PacketType Type => PlayerUpdate;
        
        public int id;
        public byte scene;
        public Vector3 pos;
        public Quaternion rot;
        
        // Amimation stuff
        public int airborneState;
        public bool moving;
        public float yaw;
        public float horizontalMovement;
        public float forwardMovement;
        public float horizontalSpeed;
        public float forwardSpeed;
        public bool sprinting;
        
        public void Serialize(OutgoingMessage msg)
        {
            
            
            msg.Write(id);
            msg.Write(scene);
            msg.Write(pos);//Compressed(pos);
            msg.Write(rot);
            
            msg.Write(airborneState);
            msg.Write(moving);
            msg.Write(horizontalSpeed);
            msg.Write(forwardSpeed);
            msg.Write(horizontalMovement);
            msg.Write(forwardMovement);
            msg.Write(yaw);
            msg.Write(sprinting);
            
            
        }

        public void Deserialize(IncomingMessage msg)
        {
            id = msg.ReadInt32();

            scene = msg.ReadByte();
            pos = msg.ReadVector3();
            rot = msg.ReadQuaternion();

            airborneState = msg.ReadInt32();
            moving = msg.ReadBoolean();
            horizontalSpeed = msg.ReadFloat();
            forwardSpeed = msg.ReadFloat();
            horizontalMovement = msg.ReadFloat();
            forwardMovement = msg.ReadFloat();
            yaw = msg.ReadFloat();
            sprinting = msg.ReadBoolean();
        }
    }
}
