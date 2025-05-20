using HarmonyLib;
using Il2CppMonomiPark.SlimeRancher.SceneManagement;

using NewSR2MP;
using NewSR2MP.Component;
using NewSR2MP.Packet;
using UnityEngine.AddressableAssets;

namespace NewSR2MP.Patches
{
    //[HarmonyPatch(typeof(SceneContext), nameof(SceneContext.NoteGameFullyLoaded))]
    internal class SceneContextNoteGameFullyLoaded
    {
        internal static float loadTime = 0f;
        public static void Postfix(SceneLoader __instance)
        {
            loadTime = Time.unscaledTime;
        }
    }
}
