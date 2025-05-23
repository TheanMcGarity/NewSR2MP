﻿using HarmonyLib;
using Il2CppMonomiPark.SlimeRancher.UI.AccessDoor;
using Il2CppMonomiPark.SlimeRancher.Weather;
using NewSR2MP.Packet;

namespace NewSR2MP.Patches
{
    [HarmonyPatch(typeof(WeatherDirector), nameof(WeatherDirector.Start))]
    internal class WeatherDirectorStart
    {
        public static void Postfix(WeatherDirector __instance)
        {
            weatherDirectorInstance = __instance;
        }
    }
}
