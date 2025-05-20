using HarmonyLib;
using Il2CppMonomiPark.SlimeRancher.UI.AccessDoor;
using NewSR2MP.Packet;

namespace NewSR2MP.Patches
{
    [HarmonyPatch(typeof(KillOnTrigger), nameof(KillOnTrigger.OnTriggerEnter))]
    internal class KillOnTriggerOnTriggerEnter
    {
        public static bool Prefix(AccessDoorUIRoot __instance) => !clientLoading;
    }
}
