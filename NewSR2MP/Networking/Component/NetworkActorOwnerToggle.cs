
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
        void Start()
        {
            if (GetComponent<NetworkActor>() == null)
            {
                Destroy(this);
                return;
            }

            started = true;
        }
        bool started = false;
        void OnEnable()
        {
            activeActors.Add(this);
        }

        void OnDisable()
        {
            activeActors.Remove(this);
        }
        /// <summary>
        /// Use this to drop the current largo held for this client.
        /// </summary>
        public void LoseGrip()
        {
            VacuumItem vac = sceneContext.PlayerState.VacuumItem;

            if (vac._held == gameObject)
            {
                // SLIGHTLY MODIFIED SR CODE
                Vacuumable vacuumable = vac._held.GetComponent<Vacuumable>();

                if (vacuumable != null)
                {
                    vacuumable.Release();
                }

                vac.LockJoint.connectedBody = null;
                Identifiable ident = vac._held.GetComponent<Identifiable>();
                vac._held = null;
                vac.SetHeldRad(0f);
            }
        }

        public enum OwnershipTransferCause { VAC, REGION, UNSPECIFIED }
        
        /// <summary>
        /// This is for transfering actor ownership to another player. Recommended for when you want a client to control a feature on the actor. 
        /// </summary>
        public void OwnActor(OwnershipTransferCause cause = OwnershipTransferCause.UNSPECIFIED)
        {
            if (!started)
                return;

            if (cause == OwnershipTransferCause.REGION)
            {
                Vector3 size = new Vector3(50, 200, 50);
                
                bool intersects = false;
                
                var bounds1 = new Bounds(sceneContext.player.transform.position, size);
                
                foreach (var player in players)
                {
                    if (player.playerID == currentPlayerID)
                        continue;
                    
                    var bounds2 = new Bounds(player.gameObject.transform.position, size);
                    
                    if (bounds2.Intersects(bounds1))
                    {
                        intersects = true;
                        break;
                    }
                }

                if (intersects) return;
            }
            NetworkActor net = GetComponent<NetworkActor>();

            if (net == null)
            {
                Destroy(this);
                return;
            }
            
            if (net.IsOwned) return;
            
            // Owner change
            net.enabled = true;
            net.IsOwned = true;
            GetComponent<TransformSmoother>().enabled = false;

            // Inform server of owner change.
            var packet = new ActorUpdateOwnerMessage()
            {
                id = GetComponent<IdentifiableActor>().GetActorId().Value,
                player = currentPlayerID
            };
            MultiplayerManager.NetworkSend(packet);
        }
    }
}
