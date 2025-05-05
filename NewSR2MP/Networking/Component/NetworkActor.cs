
using NewSR2MP.Networking.Packet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Il2CppMonomiPark.SlimeRancher.Slime;
using Il2CppMonomiPark.SlimeRancher.World;
using MelonLoader;
using UnityEngine;

namespace NewSR2MP.Networking.Component
{
    [RegisterTypeInIl2Cpp(false)]
    public class NetworkActor : MonoBehaviour
    {


        private bool isOwned = true;
        
        /// <summary>
        /// Is currently owned by the client. Recommended to use ownership system for this.
        /// </summary>
        public bool IsOwned
        {
            get
            {
                return isOwned; 
            }
            internal set
            {
                isOwned = value;
            }
        }

        private Identifiable identComp;
        private Rigidbody rigidbody;
        void Awake()
        {
            try
            {
                identComp = GetComponent<Identifiable>();
                rigidbody = GetComponent<Rigidbody>();
            }
            catch { }
        }

        private float transformTimer = 0;
        public float vacTimer = 0;

        internal int startingOwner = 0;

        public long trueID = -1;

        void Start()
        {
            if (GetComponent<ResourceCycle>() != null)
            {
                gameObject.AddComponent<NetworkResource>();
            }
            
            
            if (ClientActive() && !ServerActive())
                isOwned = false;
        }
        uint frame;
        bool appliedLaunch;
        bool appliedCollider;
        public void Update()
        {
            if (gameObject.TryGetComponent(out Gadget gadget))
            {
                gameObject.RemoveComponent<TransformSmoother>();
                Destroy(this);
            }
            try
            {
                if (frame > 3 && !appliedLaunch)
                {
                    GetComponent<Vacuumable>().SetLaunched(true);
                    appliedLaunch = true;
                }
            }
            catch { }


            if (!IsOwned)
            {
                GetComponent<TransformSmoother>().enabled = true;
                enabled = false;
                return;
            }
            transformTimer -= Time.unscaledDeltaTime;
            if (transformTimer <= 0)
            {
                transformTimer = .215f;

                if (MultiplayerManager.server == null && MultiplayerManager.client != null)
                {
                    var packet = new ActorUpdateClientMessage()
                    {
                        id = identComp.GetActorId().Value,
                        position = transform.position,
                        rotation = transform.eulerAngles,
                        velocity = rigidbody.velocity,
                    };
                    
                    if (TryGetComponent<SlimeEmotions>(out var emotions))
                    {
                        packet.slimeEmotions = new NetworkEmotions(
                            emotions._emotions.x,
                            emotions._emotions.y,
                            emotions._emotions.z,
                            emotions._emotions.w);
                    }
                    
                    MultiplayerManager.NetworkSend(packet);
                }
                else if (MultiplayerManager.server != null)
                {

                    var packet = new ActorUpdateMessage()
                    {
                        id = identComp.GetActorId().Value,
                        position = transform.position,
                        rotation = transform.eulerAngles,
                        velocity = rigidbody.velocity,
                    };
                    
                    if (TryGetComponent<SlimeEmotions>(out var emotions))
                    {
                        packet.slimeEmotions = new NetworkEmotions(
                            emotions._emotions.x,
                            emotions._emotions.y,
                            emotions._emotions.z,
                            emotions._emotions.w);
                    }
                    
                    MultiplayerManager.NetworkSend(packet);
                }


            }
            frame++;
        }
        public void OnDisable()
        {
            GetComponent<TransformSmoother>().enabled = true;

        }
        void OnDestroy()
        {
            actors.Remove(identComp.GetActorId().Value);
        }
    }
}
