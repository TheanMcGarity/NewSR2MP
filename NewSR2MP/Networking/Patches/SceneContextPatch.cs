using HarmonyLib;
using Mirror;
using NewSR2MP.Networking;
using NewSR2MP.Networking.Component;
using NewSR2MP.Networking.Packet;

namespace NewSR2MP.Networking.Patches
{
    [HarmonyPatch(typeof(SceneContext), nameof(SceneContext.Start))]
    internal class SceneContextStart
    {
        public static void Postfix(SceneContext __instance)
        {
            if (!NetworkServer.active && NetworkClient.active)
            {
                Main.OnClientSaveLoaded(__instance);
            }
        }
    }
}
