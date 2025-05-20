using HarmonyLib;
using Il2CppXGamingRuntime.Interop;
using NewSR2MP.Networking.Component;
using NewSR2MP.Networking.Packet;

namespace NewSR2MP.Networking.Patches
{
    [HarmonyPatch(typeof(WorldStatePrimarySwitch), nameof(WorldStatePrimarySwitch.SetStateForAll))]
    internal class WorldStatePrimarySwitchSetStateForAll
    {
        public static void Postfix(WorldStatePrimarySwitch __instance, SwitchHandler.State state, bool immediate)
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