using Il2CppMonomiPark.SlimeRancher.DataModel;

namespace NewSR2MP.Networking.Patches;

[HarmonyPatch(typeof(UpgradeModel), nameof(UpgradeModel.IncrementUpgradeLevel))]
internal class UpgradeModelIncrementUpgradeLevel
{
    public static void Postfix(UpgradeModel __instance, UpgradeDefinition definition)
    {
        if (ClientActive() || ServerActive())
        {
            byte idx = (byte)__instance.upgradeDefinitions.IndexOf(definition);
            MultiplayerManager.NetworkSend(new PlayerUpgradeMessage{ index = idx });
        }
    }
}