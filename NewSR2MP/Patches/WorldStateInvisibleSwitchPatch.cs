using HarmonyLib;
using Il2CppMonomiPark.SlimeRancher.World;
using Il2CppXGamingRuntime.Interop;
using NewSR2MP.Component;
using NewSR2MP.Packet;

namespace NewSR2MP.Patches
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