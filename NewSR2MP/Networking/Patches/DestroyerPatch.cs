using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using Mirror;
using Il2CppMonomiPark.SlimeRancher.DataModel;
using NewSR2MP.Networking.Component;
using NewSR2MP.Networking.Packet;
using UnityEngine;
namespace NewSR2MP.Networking.Patches
{
    [HarmonyPatch(typeof(Destroyer), nameof(Destroyer.DestroyActor), typeof(GameObject), typeof(string), typeof(bool))]
    public class DestroyerDestroyActor
    {
        public static bool Prefix(GameObject actorObj, string source, bool okIfNonActor)
        {
            if (NetworkServer.active || NetworkClient.active)
            {
                if (source.Equals("ResourceCycle.RegistryUpdate#1"))
                {
                    return false;
                }
            }
            return true;
        }
    }
}
