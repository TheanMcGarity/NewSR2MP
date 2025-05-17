using HarmonyLib;
using Il2CppMonomiPark.SlimeRancher.UI.AccessDoor;
using NewSR2MP.Networking.Packet;

namespace NewSR2MP.Networking.Patches
{
    [HarmonyPatch(typeof(TreasurePod), nameof(Il2Cpp.TreasurePod.Activate))]
    internal class TreasurePodActivate
    {
        public static void Postfix(TreasurePod __instance)
        {
            if (handlingPacket)
                return;
            
            var message = new TreasurePodMessage()
            {
                id = int.Parse(__instance._id.Replace("pod",""))
            };
            
            MultiplayerManager.NetworkSend(message);
        }
    }
}
