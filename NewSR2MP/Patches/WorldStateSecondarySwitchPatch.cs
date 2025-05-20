using HarmonyLib;
using Il2CppXGamingRuntime.Interop;
using NewSR2MP.Component;
using NewSR2MP.Packet;

namespace NewSR2MP.Patches
{
    [HarmonyPatch(typeof(WorldStateSecondarySwitch), nameof(WorldStateSecondarySwitch.SetState))]
    internal class WorldStateSecondarySwitchSetState
    {
        public static void Postfix(WorldStateSecondarySwitch __instance, SwitchHandler.State state, bool immediate)
        {
            if (handlingPacket)
                return;
            MultiplayerManager.NetworkSend(new SwitchModifyMessage
            {
                id = __instance._primary.SwitchDefinition.ID,
                state = (byte)state,
            });
        }
    }
}