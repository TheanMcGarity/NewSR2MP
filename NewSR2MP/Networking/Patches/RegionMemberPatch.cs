using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using Mirror;
using Il2CppMonomiPark.SlimeRancher.DataModel;
using Il2CppMonomiPark.SlimeRancher.Regions;
using NewSR2MP.Networking.Component;
using NewSR2MP.Networking.Packet;
using UnityEngine;
using UnityEngine.UIElements;

namespace NewSR2MP.Networking.Patches
{
    [HarmonyPatch(typeof(RegionMember), nameof(RegionMember.UpdateRegionMembership))]
    public class UpdateRegionMembership
    {
        public static bool Prefix(RegionMember __instance)
        {
            if (__instance.setId == RegionRegistry.RegionSetId.UNSET)
            {
                __instance.actorModel.currRegionSetId = RegionRegistry.RegionSetId.HOME;
                return false;
            }
            return true;
        }
    }
}
