using HarmonyLib;


namespace NewSR2MP.Networking.Patches
{
    [HarmonyPatch(typeof(SpawnResource), nameof(SpawnResource.Awake))]
    internal class SpawnResourceAwake
    {
        public static void Postfix(SpawnResource __instance)
        {
            if (ClientActive() && !ServerActive())
                __instance._model.nextSpawnTime = double.MaxValue;
        }
    }
}
