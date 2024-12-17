using HarmonyLib;


using Il2CppMonomiPark.SlimeRancher.Persist;
using NewSR2MP.Networking.Component;
using NewSR2MP.Networking.Packet;
using NewSR2MP.Networking.SaveModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices.ComTypes;
using Il2CppMonomiPark.SlimeRancher.Pedia;
using UnityEngine;


namespace NewSR2MP.Networking.Patches
{
    [HarmonyPatch(typeof(AutoSaveDirector), nameof(AutoSaveDirector.Load),typeof(Il2CppSystem.IO.Stream),typeof(string),typeof(string),typeof(bool))]
    public class AutoSaveDirectorLoadSave
    {
        public static void Postfix(AutoSaveDirector __instance, Il2CppSystem.IO.Stream gameData, string gameName, string saveName, bool reloadAllCoreScenes)
        {
            if (ClientActive()) return;
            MultiplayerManager.CheckForMPSavePath();
            var path = Path.Combine(GameContext.Instance.AutoSaveDirector.StorageProvider.Cast<FileStorageProvider>().savePath, "MultiplayerSaves", $"{gameName}.srmp");
            var networkGame = new NetworkV01();

            GameStream fs = CppFile.Open(path, Il2CppSystem.IO.FileMode.OpenOrCreate);
            try { networkGame.Load(fs); } catch { }
            fs.Dispose();
            
            savedGame = networkGame;
            savedGamePath = path;
        }
    }
    [HarmonyPatch(typeof(AutoSaveDirector), nameof(AutoSaveDirector.LoadNewGame))]
    public class AutoSaveDirectorLoadNewGame
    {
        public static void Postfix(AutoSaveDirector __instance, AutoSaveDirector.LoadNewGameMetadata metadata, Il2CppSystem.Action onError)
        {
            MultiplayerManager.CheckForMPSavePath();
            var path = Path.Combine(GameContext.Instance.AutoSaveDirector.StorageProvider.TryCast<FileStorageProvider>().savePath, "MultiplayerSaves", $"{__instance._currentGameName}.srmp");
            var networkGame = new NetworkV01();
            
            GameStream fs = CppFile.Create(path);
            try
            {
                networkGame.Write(fs);
            } catch { }
            fs.Dispose();
            
            savedGame = networkGame;
            savedGamePath = path;
        }
    }
    [HarmonyPatch(typeof(AutoSaveDirector), nameof(AutoSaveDirector.SaveGame))]
    public class AutoSaveDirectorSaveGame
    {
        public static bool isClient = false;
        public static bool Prefix(AutoSaveDirector __instance)
        {
            
            if (isClient)
            {
                if (!ClientActive())
                    isClient = false;
                return false;
            }
            return true;
        }
        public static void Postfix(AutoSaveDirector __instance)
        {
            try
            {
                if (isClient)
                    return;

                MultiplayerManager.DoNetworkSave();
            }
            catch (Exception ex)
            {
                SRMP.Error($"Error occured during saving multiplayer data!\n{ex}");
            }
        }
    }
    
    
    [HarmonyPatch(typeof(AutoSaveDirector), nameof(AutoSaveDirector.Awake))]
    public class AutoSaveDirectorAwake
    {
        public static void Postfix(AutoSaveDirector __instance)
        {
            MultiplayerManager.Instance.GeneratePlayerBean();
            
            foreach (var ident in __instance.identifiableTypes._memberTypes)
            {
                identifiableTypes.Add(GetIdentID(ident), ident);
            }
            foreach (var pedia in Resources.FindObjectsOfTypeAll<PediaEntry>()) // SavedGame's list doesnt include some pedia entries.
            {
                pediaEntries.Add(pedia.name, pedia); 
            }

            foreach (var scene in __instance.SavedGame._sceneGroupTranslation.RawLookupDictionary)
            {
                sceneGroups.Add(__instance._savedGame._sceneGroupTranslation.InstanceLookupTable._reverseIndex[scene.key], scene.value);
            }
        }
    }
}
