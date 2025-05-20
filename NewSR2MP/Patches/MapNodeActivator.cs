using HarmonyLib;

using NewSR2MP;
using NewSR2MP.Networking;
using NewSR2MP.Networking.Component;
using NewSR2MP.Networking.Packet;
using System;
using System.Collections.Generic;
using Il2CppMonomiPark.SlimeRancher.UI.Map;


namespace NewSR2MP.Networking.Patches
{
    [HarmonyPatch(typeof(MapNodeActivator), nameof(MapNodeActivator.Activate))]
    public class MapNodeActivatorActivate
    {
        public static void Postfix(MapNodeActivator __instance)
        {
            MultiplayerManager.NetworkSend(new MapUnlockMessage
            {
                id = __instance._fogRevealEvent._dataKey
            });
        }
    }
}
