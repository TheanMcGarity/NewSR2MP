using HarmonyLib;
using Il2CppMonomiPark.SlimeRancher.Analytics.Event;
using Il2CppMonomiPark.SlimeRancher.Pedia;
using Mirror;
using NewSR2MP.Networking;
using NewSR2MP.Networking.Component;
using NewSR2MP.Networking.Packet;

namespace NewSR2MP.Networking.Patches
{
    [HarmonyPatch(typeof(PediaDirector), nameof(PediaDirector.ShowPedia), typeof(PediaEntry), typeof(PediaOpenedAnalyticsEvent.Opener), typeof(Il2CppSystem.Action))]
    internal class PediaDirectorMaybeShowPopup
    {
        public static void Postfix(PediaDirector __instance,  PediaEntry entry, PediaOpenedAnalyticsEvent.Opener opener, Il2CppSystem.Action onClosed)
        {
            if ((NetworkClient.active || NetworkServer.active) && !__instance.GetComponent<HandledDummy>())
            {
                PediaMessage message = new PediaMessage()
                {
                    id = entry.name
                };
                SRNetworkManager.NetworkSend(message);
            }
        }
    }
}
