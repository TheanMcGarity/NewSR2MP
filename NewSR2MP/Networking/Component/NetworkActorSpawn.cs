using Mirror;
using NewSR2MP.Networking.Packet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;


namespace NewSR2MP.Networking.Component
{
    public class NetworkActorSpawn : MonoBehaviour
    {
        private byte frame = 0;

        public void Update()
        {

            if (frame > 1) // On frame 2
            {
                Identifiable ident = GetComponent<Identifiable>();
                var packet = new ActorSpawnClientMessage()
                {
                    ident = ident.id,
                    position = transform.position,
                    rotation = transform.eulerAngles,
                    velocity = GetComponent<Rigidbody>().velocity,
                    player = SRNetworkManager.playerID
                };
                NetworkClient.SRMPSend(packet);
                Destroyer.DestroyActor(gameObject, "SRMP.CancelActor");
            }
            frame++;
        }
    }
}
