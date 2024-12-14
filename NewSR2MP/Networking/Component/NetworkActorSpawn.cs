
using NewSR2MP.Networking.Packet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;


namespace NewSR2MP.Networking.Component
{
    [RegisterTypeInIl2Cpp(false)]
    public class NetworkActorSpawn : MonoBehaviour
    {
        private byte frame = 0;

        public void Update()
        {

            if (frame > 1) // On frame 2
            {
                Identifiable ident = GetComponent<IdentifiableActor>();
                var packet = new ActorSpawnClientMessage()
                {
                    ident = ident.identType.name,
                    position = transform.position,
                    rotation = transform.eulerAngles,
                    velocity = GetComponent<Rigidbody>().velocity,
                    player = currentPlayerID
                };
                MultiplayerManager.NetworkSend(packet);
                Destroyer.DestroyActor(gameObject, "SRMP.CancelActor");
            }
            frame++;
        }
    }
}
