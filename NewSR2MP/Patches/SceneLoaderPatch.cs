﻿using HarmonyLib;
using Il2CppMonomiPark.SlimeRancher.SceneManagement;

using NewSR2MP;
using NewSR2MP.Component;
using NewSR2MP.Packet;
using UnityEngine.AddressableAssets;

namespace NewSR2MP.Patches
{
    //[HarmonyPatch(typeof(SceneLoader), nameof(SceneLoader.LoadSceneGroupAsync))]
    /*internal class SceneLoaderLoadSceneGroupAsync
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
                    Main.OnSaveLoaded(SceneContext.Instance);

                    
                }
                else if (!sceneGroup._isGameplay)
                {
                    isLoadedAlready = false;
                }
            }
        }
    }*/
}
