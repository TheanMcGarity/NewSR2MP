using HarmonyLib;
using Mirror;
using NewSR2MP;
using NewSR2MP.Networking;
using NewSR2MP.Networking.Component;
using NewSR2MP.Networking.Packet;
using System;
using System.Collections.Generic;




namespace NewSR2MP.Networking.Patches
{
    [HarmonyPatch(typeof(MapDataEntry), nameof(MapDataEntry.Start))]
    public class MapDataEntryStart
    {
        public static Il2CppSystem.Collections.Generic.List<MapDataEntry> entries = new Il2CppSystem.Collections.Generic.List<MapDataEntry>();
        public static void Postfix(MapDataEntry __instance)
        {
            entries.Add(__instance);
        }
    }
}
