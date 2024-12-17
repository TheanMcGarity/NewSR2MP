﻿
using Riptide;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace NewSR2MP.Networking.Packet
{
    public class PlayerUpdateMessage : ICustomMessage
    {
        public int id;
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
        
        public Message Serialize()
        {
            Message msg = Message.Create(MessageSendMode.Unreliable, PacketType.PlayerUpdate);
            
            msg.AddInt(id);
            msg.AddVector3(pos);
            msg.AddQuaternion(rot);
            
            msg.AddInt(airborneState);
            msg.AddBool(moving);
            msg.AddFloat(horizontalSpeed);
            msg.AddFloat(forwardSpeed);
            msg.AddFloat(horizontalMovement);
            msg.AddFloat(forwardMovement);
            msg.AddFloat(yaw);
            
            return msg;
        }
    }
}
