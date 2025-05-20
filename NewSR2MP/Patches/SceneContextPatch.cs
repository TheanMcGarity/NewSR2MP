using HarmonyLib;
using Il2CppMonomiPark.SlimeRancher.SceneManagement;

using NewSR2MP.Networking;
using NewSR2MP.Networking.Component;
using NewSR2MP.Networking.Packet;
using UnityEngine.AddressableAssets;

namespace NewSR2MP.Networking.Patches
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
