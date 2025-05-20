using HarmonyLib;
using UnityEngine;
namespace NewSR2MP.Networking.Patches;

[HarmonyPatch(typeof(UnityEngine.GUIStateObjects))]
internal static class GUIStateObjectsMultiPatch
{
    private static System.Collections.Generic.Dictionary<int, Il2CppSystem.Object> s_StateCache = new();

    [HarmonyPrefix]
    [HarmonyPatch(nameof(GUIStateObjects.QueryStateObject))]
    internal static bool QueryStateObject(Il2CppSystem.Type t, int controlID, ref Il2CppSystem.Object __result)
    {
        Il2CppSystem.Object o = s_StateCache[controlID];
        __result = (t.IsInstanceOfType(o) ? o : null)!;
        return false;
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(GUIStateObjects.GetStateObject))]
    public static bool GetStateObject(Il2CppSystem.Type t, int controlID, ref Il2CppSystem.Object __result)
    {
        if (!s_StateCache.TryGetValue(controlID, out var instance) || instance.GetIl2CppType() != t)
        {
            instance = Il2CppSystem.Activator.CreateInstance(t);
            s_StateCache[controlID] = instance;
        }

        __result = instance;
        return false;
    }
}