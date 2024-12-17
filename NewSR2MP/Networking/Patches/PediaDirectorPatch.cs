using HarmonyLib;
using Il2CppMonomiPark.SlimeRancher.Analytics.Event;
using Il2CppMonomiPark.SlimeRancher.Pedia;

using NewSR2MP.Networking;
using NewSR2MP.Networking.Component;
using NewSR2MP.Networking.Packet;

namespace NewSR2MP.Networking.Patches
{
    [HarmonyPatch(typeof(PediaDirector), nameof(PediaDirector.Unlock),typeof(PediaEntry),typeof(bool))]
    internal class PediaDirectorUnlock
    {
        public static void Postfix(PediaDirector __instance,  PediaEntry entry, bool showPopup)
        {
            if ((ClientActive() || ServerActive()) && !__instance.GetComponent<HandledDummy>())
            {
                PediaMessage message = new PediaMessage()
                {
                    id = entry.name
                };
                MultiplayerManager.NetworkSend(message);
            }
        }
    }
}
