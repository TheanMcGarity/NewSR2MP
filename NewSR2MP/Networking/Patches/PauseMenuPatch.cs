using HarmonyLib;
using Il2CppMonomiPark.SlimeRancher.UI;
using Il2CppMonomiPark.SlimeRancher.UI.Pause;
using Mirror;
using NewSR2MP.Networking;
using NewSR2MP.Networking.Packet;

namespace NewSR2MP.Networking.Patches
{
    [HarmonyPatch(typeof(QuitPauseItemModel), nameof(QuitPauseItemModel.InvokeBehavior))]
    internal class PauseMenuQuit
    {
        public static void Postfix(QuitPauseItemModel __instance)
        {
            if (NetworkServer.active || NetworkClient.active)
            {
                NetworkServer.Shutdown();
                NetworkClient.Shutdown();

                SRNetworkManager.EraseValues();
            }
        }
    }
    [HarmonyPatch(typeof(PauseMenuRoot), nameof(PauseMenuRoot.Awake))]
    internal class PauseMenuStart
    {
        public static void Postfix(PauseMenuRoot __instance)
        {
        }
    }
}
