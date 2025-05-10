using HarmonyLib;
using Il2CppMonomiPark.SlimeRancher.UI.AccessDoor;
using NewSR2MP.Networking.Packet;

namespace NewSR2MP.Networking.Patches
{
    [HarmonyPatch(typeof(KillOnTrigger), nameof(KillOnTrigger.OnTriggerEnter))]
    internal class KillOnTriggerOnTriggerEnter
    {
        public static bool Prefix(AccessDoorUIRoot __instance) => !clientLoading;
    }
}
