using Il2CppMonomiPark.SlimeRancher.DataModel;

namespace NewSR2MP.Networking.Patches;

[HarmonyPatch(typeof(UpgradeModel), nameof(UpgradeModel.IncrementUpgradeLevel))]
internal class UpgradeModelIncrementUpgradeLevel
{
    public static void Postfix(UpgradeModel __instance, UpgradeDefinition definition)
    {
        if (handlingPacket)
            return;
        
        if (ClientActive() || ServerActive())
            MultiplayerManager.NetworkSend(new PlayerUpgradeMessage{ id = (byte)definition._uniqueId });
    }
}