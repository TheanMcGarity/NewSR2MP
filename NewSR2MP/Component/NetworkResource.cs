
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
    public class NetworkResource : MonoBehaviour
    {
        void Awake()
        {
            try
            {
                networkActor = GetComponent<NetworkActor>();
                identComp = GetComponent<Identifiable>();
                resource = GetComponent<ResourceCycle>();
            }
            catch { }
        }
        void Start()
        {
            if (resource == null)
            {
                Destroy(this);
            }
        }
        private Identifiable identComp;

        private float updateTimer = 0;

        private NetworkActor networkActor;

        public ResourceCycle resource;

        public void Update()
        {
            updateTimer -= Time.unscaledDeltaTime;
            if (updateTimer <= 0)
            {
                if (resource != null)
                {
                    if (networkActor.IsOwned)
                    {
                        var message = new ResourceStateMessage()
                        {
                            state = resource._model.state,
                            id = identComp.GetActorId().Value
                        };
                        MultiplayerManager.NetworkSend(message);
                    }
                }
                updateTimer = ActorTimer;
            }

            if (ClientActive())
            {
                resource._model.progressTime = double.MaxValue;
            }
        }
    }
}
