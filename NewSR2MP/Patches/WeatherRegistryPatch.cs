using Il2CppMonomiPark.SlimeRancher.Weather;
namespace NewSR2MP.Patches
{
    [HarmonyPatch(typeof(WeatherRegistry), nameof(WeatherRegistry.Awake))]
    public class WeatherRegistryAwake
    {
        public static void Postfix(WeatherRegistry __instance)
        {
            CreateWeatherPatternLookup(__instance);
        }
    }
}
