using HarmonyLib;
using Il2CppMonomiPark.SlimeRancher.UI.Map;

[HarmonyPatch(typeof(OptionsDirector), nameof(OptionsDirector.OnApplicationFocus))]
internal class OptionsDirectorOnApplicationFocus
{
    public static bool Prefix(OptionsDirector __instance, bool focus) => false;
}