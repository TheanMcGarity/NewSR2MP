using HarmonyLib;
using Mirror;
using NewSR2MP.Networking;
using NewSR2MP.Networking.Component;
using NewSR2MP.Networking.Packet;

namespace NewSR2MP.Networking.Patches
{
    [HarmonyPatch(typeof(PediaDirector), nameof(PediaDirector.MaybeShowPopup), typeof(PediaDirector.Id))]
    internal class PediaDirectorMaybeShowPopup
    {
        public static void Postfix(PediaDirector __instance, PediaDirector.Id id)
        {
            if ((NetworkClient.active || NetworkServer.active) && !__instance.GetComponent<HandledDummy>())
            {
                PediaMessage message = new PediaMessage()
                {
                    id = id
                };
                SRNetworkManager.NetworkSend(message);
            }
        }
    }
}
