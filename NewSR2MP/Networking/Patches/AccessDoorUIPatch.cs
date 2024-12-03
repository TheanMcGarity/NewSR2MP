using HarmonyLib;
using NewSR2MP.Networking.Packet;

namespace NewSR2MP.Networking.Patches
{
    [HarmonyPatch(typeof(AccessDoorUI), nameof(AccessDoorUI.UnlockDoor))]
    internal class AccessDoorUIUnlockDoor
    {
        public static void Postfix(AccessDoorUI __instance)
        {
            var message = new DoorOpenMessage()
            {
                id = __instance.door.id
            };
            SRNetworkManager.NetworkSend(message);
        }
    }
}
