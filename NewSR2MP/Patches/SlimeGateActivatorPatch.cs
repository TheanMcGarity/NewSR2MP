using HarmonyLib;
using NewSR2MP.Networking.Packet;

namespace NewSR2MP.Networking.Patches
{
    [HarmonyPatch(typeof(SlimeGateActivator), nameof(SlimeGateActivator.Activate))]
    internal class SlimeGateActivatorActivate
    {
        public static void Postfix(SlimeGateActivator __instance)
        {
            var message = new DoorOpenMessage()
            {
                id = __instance.GateDoor._id
            };
            MultiplayerManager.NetworkSend(message);
        }
    }
}
