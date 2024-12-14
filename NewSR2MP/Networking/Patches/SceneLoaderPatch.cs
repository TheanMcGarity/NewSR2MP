using HarmonyLib;
using Il2CppMonomiPark.SlimeRancher.SceneManagement;

using NewSR2MP.Networking;
using NewSR2MP.Networking.Component;
using NewSR2MP.Networking.Packet;
using UnityEngine.AddressableAssets;

namespace NewSR2MP.Networking.Patches
{
    [HarmonyPatch(typeof(SceneLoader), nameof(SceneLoader.LoadSceneGroupAsync))]
    internal class SceneLoaderLoadSceneGroupAsync
    {
        private static bool isLoadedAlready = false;
        public static void Postfix(SceneLoader __instance,SceneGroup sceneGroup, AssetReference loadingScene, SceneLoadingParameters parameters)
        {
            if (!ServerActive() && ClientActive())
            {
                if (__instance._defaultGameplaySceneGroup == sceneGroup)
                {
                    Main.OnRanchSceneGroupLoaded(SceneContext.Instance);
                }
                
                if (sceneGroup._isGameplay && !isLoadedAlready)
                {
                    isLoadedAlready = true;
                    Main.OnSceneContextLoaded(SceneContext.Instance);

                    
                }
                else if (!sceneGroup._isGameplay)
                {
                    isLoadedAlready = false;
                }
            }
        }
    }
}
