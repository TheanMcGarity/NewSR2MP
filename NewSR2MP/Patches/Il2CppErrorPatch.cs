namespace NewSR2MP.Patches;

public class Il2CppErrorPatch
{
    // Thank you Atmudia - MelonLoader Discord
    [HarmonyPatch("Il2CppInterop.HarmonySupport.Il2CppDetourMethodPatcher", "ReportException")]
    public static class Patch_Il2CppDetourMethodPatcher
    {
        public static bool Prefix(System.Exception ex)
        {
            MelonLogger.Error("During invoking native->managed trampoline", ex);
            return false;                               
        }
    }
}