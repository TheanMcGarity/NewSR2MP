
using NewSR2MP.Networking.Packet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Il2CppMonomiPark.SlimeRancher.World;
using UnityEngine;


namespace NewSR2MP.Networking.Component
{
    [RegisterTypeInIl2Cpp(false)]
    public class NetworkActorSpawn : MonoBehaviour
    {
        void Awake()
        {
            if (handlingPacket)
                Destroy(this);
        }
        byte frame = 0;

        void Update()
        {
            if (frame > 1) // On frame 2
            {
                

            }
            frame++;
        }
    }
}
