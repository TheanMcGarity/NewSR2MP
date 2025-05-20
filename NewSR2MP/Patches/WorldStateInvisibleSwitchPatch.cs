using HarmonyLib;
using Il2CppMonomiPark.SlimeRancher.World;
using Il2CppXGamingRuntime.Interop;
using NewSR2MP.Networking.Component;
using NewSR2MP.Networking.Packet;

namespace NewSR2MP.Networking.Patches
{
    [HarmonyPatch(typeof(WorldStateInvisibleSwitch), nameof(WorldStateInvisibleSwitch.SetStateForAll))]
    internal class WorldStateInvisibleSwitchSetStateForAll
    {
        public static void Postfix(WorldStateInvisibleSwitch __instance, SwitchHandler.State state, bool immediate)
        {
            if (handlingPacket)
                return;
            MultiplayerManager.NetworkSend(new SwitchModifyMessage
            {
                id = __instance.SwitchDefinition.ID,
                state = (byte)state,
            });
        }
    }
}