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
                    actor.OwnActor();
                }
            }
        }
    }
    [HarmonyPatch(typeof(Vacuumable), nameof(Vacuumable.TryConsume))]
    public class VacuumableTryConsume
    {
        public static bool Prefix(Vacuumable __instance, bool __result)
        {
            if (ServerActive() || ClientActive())
            {
                var ammo = sceneContext.PlayerState.Ammo;
                if (!(ammo is NetworkAmmo)) return true;

                NetworkAmmo netAmmo = (NetworkAmmo)ammo;
                var openSlot = netAmmo.GetSlotIDX(__instance._identifiable.identType);
                if (openSlot == -1)
                {
                    __result = false;
                    return false;
                }
                netAmmo.MaybeAddToSpecificSlot(__instance._identifiable.identType, __instance._identifiable, openSlot);
                Destroyer.Destroy(__instance.gameObject, "SRMP.NetworkVac");
                __result = true;
                return false;
            }
            return true;
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
                    actor.OwnActor();
                }
            }
        }
    }
}
