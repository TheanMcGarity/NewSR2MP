using Mirror;
using NewSR2MP.Networking.Packet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MelonLoader;
using UnityEngine;

namespace NewSR2MP.Networking.Component
{
    [RegisterTypeInIl2Cpp(false)]
    public class NetworkActor : MonoBehaviour
    {


        private bool m_isOwned = true;
        
        /// <summary>
        /// Is currently owned by the client. Recommended to use ownership system for this.
        /// </summary>
        public bool IsOwned
        {
            get
            {
                return m_isOwned; 
            }
            internal set
            {
                m_isOwned = value;
            }
        }

        private Identifiable identComp;
        void Awake()
        {
            try
            {
                identComp = GetComponent<Identifiable>();
            }
            catch { }
        }

        private float transformTimer = 0;
        public float vacTimer = 0;

        internal int startingOwner = 0;

        public long trueID = -1;

        /// <summary>
        /// Spawn velocity. used on server for adding velocity on spawn. Only works on actor spawn.
        /// </summary>
        public Vector3 startingVel = Vector3.zero;

        private bool appliedVel;

    

        void Start()
        {
            if (GetComponent<ResourceCycle>() != null)
            {
                gameObject.AddComponent<NetworkResource>();
            }
            if (startingVel != Vector3.zero)
                GetComponent<Rigidbody>().velocity = startingVel;
            appliedVel = true;
        }
        uint frame;
        bool appliedLaunch;
        bool appliedCollider;
        public void Update()
        {
            try
            {
                if (frame > 3 && !appliedLaunch)
                {
                    GetComponent<Vacuumable>().launched = true;
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
                transformTimer = .15f;

                if (NetworkClient.active && !NetworkServer.activeHost)
                {
                    var packet = new ActorUpdateClientMessage()
                    {
                        id = identComp.GetActorId().Value,
                        position = transform.position,
                        rotation = transform.eulerAngles,
                    };
                    SRNetworkManager.NetworkSend(packet);
                }
                else if (NetworkServer.activeHost)
                {

                    var packet = new ActorUpdateMessage()
                    {
                        id = identComp.GetActorId().Value,
                        position = transform.position,
                        rotation = transform.eulerAngles,
                    };
                    SRNetworkManager.NetworkSend(packet);
                }


            }
            frame++;
        }
        public void OnDisable()
        {
            GetComponent<TransformSmoother>().enabled = true;

        }
    }
}
