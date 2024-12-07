using HarmonyLib;
using Mirror;
using NewSR2MP;
using NewSR2MP.Networking;
using NewSR2MP.Networking.Component;
using NewSR2MP.Networking.Packet;
using System;
using System.Collections.Generic;
using Il2CppMonomiPark.SlimeRancher.UI.Map;


namespace NewSR2MP.Networking.Patches
{
    [HarmonyPatch(typeof(MapNodeActivator), nameof(MapNodeActivator.Start))]
    public class MapDataEntryStart
    {
        public static List<MapNodeActivator> entries = new List<MapNodeActivator>();
        public static void Postfix(MapNodeActivator __instance)
        {
            entries.Add(__instance);
        }
    }
}
