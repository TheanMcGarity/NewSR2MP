using HarmonyLib;
using Il2CppMonomiPark.SlimeRancher.UI.Map;

[HarmonyPatch(typeof(MapNodeActivator), nameof(MapNodeActivator.Activate))]
internal class MapNodeActivatorActivate
{
    public static void Postfix(MapNodeActivator __instance)
    {
        if (NetworkClient.active || NetworkServer.active)
        {
            MapUnlockMessage message = new MapUnlockMessage()
            {
                id = __instance._fogRevealEvent._dataKey
            };
            SRNetworkManager.NetworkSend(message);
        }
    }
}