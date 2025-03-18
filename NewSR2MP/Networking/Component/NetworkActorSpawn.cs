﻿
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
        byte frame = 0;

        void Update()
        {

            if (frame > 1) // On frame 2
            {
                Identifiable ident = GetComponent<IdentifiableActor>();
                
                var packet = new ActorSpawnClientMessage()
                {
                    ident = GetIdentID(ident.identType),
                    position = transform.position,
                    rotation = transform.eulerAngles,
                    velocity = GetComponent<Rigidbody>().velocity,
                    player = currentPlayerID,
                    scene = sceneGroupsReverse[systemContext.SceneLoader.CurrentSceneGroup.name]
                };
                MultiplayerManager.NetworkSend(packet);
                handlingPacket = true;
                Destroyer.DestroyActor(gameObject, "SR2MP.SpawnActorClient");
            }
            frame++;
        }
    }
}
