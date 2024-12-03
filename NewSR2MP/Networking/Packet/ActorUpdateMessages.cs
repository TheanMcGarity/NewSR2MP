using Mirror;
using Il2CppMonomiPark.SlimeRancher.Regions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace NewSR2MP.Networking.Packet
{
    public struct ActorUpdateMessage : NetworkMessage
    {
        public long id;
        public Vector3 position;
        public Vector3 rotation;
    }
    public struct ActorUpdateClientMessage : NetworkMessage // Client Message is just a copy, but it has a different handler.
    {
        public long id;
        public Vector3 position;
        public Vector3 rotation;
    }
    public struct ActorUpdateOwnerMessage : NetworkMessage // Owner update message.
    {
        public long id;
        public int player;
    }
    public struct ActorChangeHeldOwnerMessage : NetworkMessage // Largo holder change message.
    {
        public long id;
    }
    public struct ActorDestroyGlobalMessage : NetworkMessage // Destroy message. Runs on both client and server (Global)
    {
        public long id;
    }
}
