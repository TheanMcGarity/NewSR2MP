using HarmonyLib;
using Il2CppMonomiPark.SlimeRancher.UI;
using Il2CppMonomiPark.SlimeRancher.UI.Pause;

using NewSR2MP;
using NewSR2MP.Packet;

namespace NewSR2MP.Patches
{
    [HarmonyPatch(typeof(QuitPauseItemModel), nameof(QuitPauseItemModel.InvokeBehavior))]
    internal class PauseMenuQuit
    {
        public static void Postfix(QuitPauseItemModel __instance)
        {
            if (ServerActive() || ClientActive())
            {
                MultiplayerManager.Shutdown();
                

                MultiplayerManager.EraseValues();
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
