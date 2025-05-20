using HarmonyLib;
using Il2CppMonomiPark.SlimeRancher.Analytics.Event;
using Il2CppMonomiPark.SlimeRancher.Pedia;

using NewSR2MP;
using NewSR2MP.Component;
using NewSR2MP.Packet;

namespace NewSR2MP.Patches
{
    [HarmonyPatch(typeof(PediaDirector), nameof(PediaDirector.Unlock),typeof(PediaEntry),typeof(bool))]
    internal class PediaDirectorUnlock
    {
        public static void Postfix(PediaDirector __instance,  PediaEntry entry, bool showPopup)
        {
            if (handlingPacket)
                return;
            
            if (ClientActive() || ServerActive())
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
