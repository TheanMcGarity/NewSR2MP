using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;

using Il2CppMonomiPark.SlimeRancher.DataModel;
using Il2CppMonomiPark.SlimeRancher.Regions;
using NewSR2MP.Networking.Component;
using NewSR2MP.Networking.Packet;
using UnityEngine;
using UnityEngine.UIElements;

namespace NewSR2MP.Networking.Patches
{
    [HarmonyPatch(typeof(RegionMember), nameof(RegionMember.Unhibernate))]
    public class RegionMemberUnhibernate
    {
        public static void Postfix(RegionMember __instance)
        {
            if (__instance.TryGetComponent<NetworkActorOwnerToggle>(out var netActorOwnerToggle))
            {
                netActorOwnerToggle.OwnActor(NetworkActorOwnerToggle.OwnershipTransferCause.REGION);
            }
        }
    }
}
