/*using HarmonyLib;
using Mirror;
using NewSR2MP.Networking.Component;
using NewSR2MP.Networking.Packet;

namespace NewSR2MP.Networking.Patches
{
    [HarmonyPatch(typeof(ResourceCycle), nameof(ResourceCycle.Ripen))]
    internal class ResourceCycleRipen
    {
        public static void Postfix(ResourceCycle __instance)
        {
            if (!NetworkServer.active) return;
            if (__instance.IsHandling()) return;
            var message = new ResourceStateMessage()
            {
                state = ResourceCycle.State.RIPE,
                id = __instance.ident.GetActorId()
            };
            SRNetworkManager.NetworkSend(message);
        }
    }
    [HarmonyPatch(typeof(ResourceCycle), nameof(ResourceCycle.SetInitState))]
    internal class ResourceCycleSetInitState
    {
        public static void Postfix(ResourceCycle __instance, ResourceCycle.State state, double progressTime)
        {
            if (!NetworkServer.active) return;
            var message = new ResourceStateMessage()
            {
                state = state,
                id = __instance.ident.GetActorId()
            };
            SRNetworkManager.NetworkSend(message);
        }
    }
    [HarmonyPatch(typeof(ResourceCycle), nameof(ResourceCycle.Rot))]
    internal class ResourceCycleRot
    {
        public static void Postfix(ResourceCycle __instance)
        {

            if (!NetworkServer.active) return;
            if (__instance.IsHandling()) return;
            var message = new ResourceStateMessage()
            {
                state = ResourceCycle.State.ROTTEN,
                id = __instance.ident.GetActorId()
            };
            SRNetworkManager.NetworkSend(message);

        }
    }
    [HarmonyPatch(typeof(ResourceCycle), nameof(ResourceCycle.MakeEdible))]
    internal class ResourceCycleMakeEdible
    {
        public static void Postfix(ResourceCycle __instance)
        {

            if (!NetworkServer.active) return;
            if (__instance.IsHandling()) return;
            var message = new ResourceStateMessage()
            {
                state = ResourceCycle.State.EDIBLE,
                id = __instance.ident.GetActorId()
            };
            SRNetworkManager.NetworkSend(message);

        }
    }
}
*/