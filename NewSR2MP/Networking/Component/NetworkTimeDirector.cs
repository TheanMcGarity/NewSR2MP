
using NewSR2MP.Networking.Packet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace NewSR2MP.Networking.Component
{
    [RegisterTypeInIl2Cpp(false)]
    public class NetworkTimeDirector : MonoBehaviour
    {
        TimeDirector dir;
        void Start()
        {
            dir = GetComponent<TimeDirector>();
        }

        public float timer = 0;

        void Update()
        {
            timer += Time.unscaledDeltaTime;

            if (timer > .08)
            {
                var msg = new TimeSyncMessage()
                {
                    time = dir._worldModel.worldTime
                };
                MultiplayerManager.NetworkSend(msg);
            }
        }
    }
}
