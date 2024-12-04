using Mirror;
using NewSR2MP.Networking.Packet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Il2CppMonomiPark.SlimeRancher.Player.PlayerItems;
using UnityEngine;

namespace NewSR2MP.Networking.Component
{
    // Just a toggle thing
    [RegisterTypeInIl2Cpp(false)]
    public class NetworkActorOwnerToggle : MonoBehaviour
    {

        /// <summary>
        /// Use this to drop the current largo held for this client.
        /// </summary>
        public void LoseGrip()
        {
            VacuumItem vac = SceneContext.Instance.PlayerState.VacuumItem;

            if (vac._held == gameObject)
            {
                // SLIGHTLY MODIFIED SR CODE
                Vacuumable vacuumable = vac._held.GetComponent<Vacuumable>();

                if (vacuumable != null)
                {
                    vacuumable.release();
                }

                vac.LockJoint.connectedBody = null;
                Identifiable ident = vac._held.GetComponent<Identifiable>();
                vac._held = null;
                vac.SetHeldRad(0f);
            }
        }
        /// <summary>
        /// This is for transfering actor ownership to another player. Recommended for when you want a client to control a feature on the actor. 
        /// </summary>
        public void OwnActor()
        {
            
            // Owner change
            GetComponent<NetworkActor>().enabled = true;
            GetComponent<NetworkActor>().IsOwned = true;
            GetComponent<TransformSmoother>().enabled = false;

            // Inform server of owner change.
            var packet = new ActorUpdateOwnerMessage()
            {
                id = GetComponent<IdentifiableActor>().GetActorId().Value,
                player = currentPlayerID
            };
            SRNetworkManager.NetworkSend(packet);
        }
    }
}
