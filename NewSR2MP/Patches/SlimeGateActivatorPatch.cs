using HarmonyLib;
using NewSR2MP.Packet;

namespace NewSR2MP.Patches
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
