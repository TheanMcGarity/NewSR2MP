using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using Mirror;
using Il2CppMonomiPark.SlimeRancher.DataModel;
using NewSR2MP.Networking;
using NewSR2MP.Networking.Component;
using NewSR2MP.Networking.Packet;
using UnityEngine;

namespace NewSR2MP.Networking.Patches
{
    [HarmonyPatch(typeof(IdentifiableActor),nameof(IdentifiableActor.SetModel))]
    public class IdentifiableSetModel
    {

        public static void Postfix(IdentifiableActor __instance)
        {

            if (NetworkClient.active && !NetworkServer.activeHost && !__instance.identType.IsPlayer && __instance.GetComponent<NetworkActor>() == null)
            {
                if (__instance.GetComponent<NetworkActor>() == null)
                {
                    try
                    {

                        __instance.transform.GetChild(0).gameObject.SetActive(false);
                        __instance.GetComponent<Collider>().isTrigger = true;
                        __instance.gameObject.AddComponent<NetworkActorSpawn>();
                        return;
                    }
                    catch { }
                }
            }
            else if (NetworkServer.activeHost)
            {
                if (!__instance.identType.IsPlayer)
                {
                    var actor = __instance.gameObject;
                    if (actor.GetComponent<NetworkActor>() == null)
                    {
                        actor.AddComponent<NetworkActor>();
                        actor.AddComponent<TransformSmoother>();
                        actor.AddComponent<NetworkActorOwnerToggle>();
                    }
                    var ts = actor.GetComponent<TransformSmoother>();
                    actors.Add(__instance.GetActorId().Value, actor.GetComponent<NetworkActor>());


                    ts.interpolPeriod = 0.15f;
                    ts.enabled = false;
                    var id = __instance.GetActorId().Value;
                    var packet = new ActorSpawnMessage()
                    {
                        id = id,
                        ident = __instance.identType.name,
                        position = __instance.transform.position,
                        rotation = __instance.transform.eulerAngles

                    };
                    SRNetworkManager.NetworkSend(packet);

                }
            }
        }
    }


    [HarmonyPatch(typeof(Identifiable),nameof(Identifiable.OnDestroy))]
    public class IdentifiableDestroy
    {
        public static void Postfix(Identifiable __instance)
        {
            if (NetworkServer.active || NetworkClient.active)
            {
                if (!__instance.identType.IsPlayer)
                {
                    var id = __instance.GetActorId().Value;
                    var packet = new ActorDestroyGlobalMessage()
                    {
                        id = id
                    };
                    SRNetworkManager.NetworkSend(packet);

                    actors.Remove(id);

                }
            }
        }
    }
}
