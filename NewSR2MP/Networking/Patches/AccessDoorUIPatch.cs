using HarmonyLib;
using Il2CppMonomiPark.SlimeRancher.UI.AccessDoor;
using NewSR2MP.Networking.Packet;

namespace NewSR2MP.Networking.Patches
{
    [HarmonyPatch(typeof(AccessDoorUIRoot), nameof(AccessDoorUIRoot.UnlockDoor))]
    internal class AccessDoorUIUnlockDoor
    {
        public static void Postfix(AccessDoorUIRoot __instance)
        {
            var message = new DoorOpenMessage()
            {
                id = __instance._door._id
            };
            MultiplayerManager.NetworkSend(message);
        }
    }
}