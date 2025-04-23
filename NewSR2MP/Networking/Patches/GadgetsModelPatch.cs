using Il2CppMonomiPark.SlimeRancher.DataModel;

namespace NewSR2MP.Networking.Patches;

[HarmonyPatch(typeof(GadgetsModel), nameof(GadgetsModel.SetCount))]
public class GadgetsModelSetCount
{
    static void Postfix(GadgetsModel __instance, IdentifiableType type, int newCount)
    {
        if (handlingPacket)
            return;
        
        MultiplayerManager.NetworkSend(new RefineryItemMessage
        {
            count = (ushort)newCount,
            id = (ushort)GetIdentID(type),
        });
    }
}