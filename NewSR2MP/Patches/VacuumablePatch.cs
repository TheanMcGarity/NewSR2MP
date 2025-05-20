using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;

using Il2CppMonomiPark.SlimeRancher.DataModel;
using NewSR2MP.Networking.Component;
using NewSR2MP.Networking.Packet;
using UnityEngine;
namespace NewSR2MP.Networking.Patches
{
    [HarmonyPatch(typeof(Vacuumable), nameof(Vacuumable.Capture))]
    public class VacuumableCapture
    {
        public static void Postfix(Vacuumable __instance, Joint toJoint)
        {
            if (ServerActive() || ClientActive())
            {
                var actor = __instance.GetComponent<NetworkActorOwnerToggle>();
                if (actor != null)
                {
                    actor.OwnActor(NetworkActorOwnerToggle.OwnershipTransferCause.VAC);
                }
            }
        }
    }
    
    [HarmonyPatch(typeof(Vacuumable), nameof(Vacuumable.SetHeld))]
    public class VacuumableSetHeld
    {
        public static void Prefix(Vacuumable __instance, bool held)
        {
            if (!held) return;

            if (ServerActive() || ClientActive())
            {
                var actor = __instance.GetComponent<NetworkActorOwnerToggle>();
                if (actor != null)
                {
                    actor.OwnActor(NetworkActorOwnerToggle.OwnershipTransferCause.VAC);
                }
            }
        }
    }
}
