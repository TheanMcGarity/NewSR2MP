using HarmonyLib;
using Mirror;

using Il2CppMonomiPark.SlimeRancher.Persist;
using NewSR2MP.Networking.Component;
using NewSR2MP.Networking.Packet;
using NewSR2MP.Networking.SaveModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices.ComTypes;


namespace NewSR2MP.Networking.Patches
{
    [HarmonyPatch(typeof(AutoSaveDirector), nameof(AutoSaveDirector.LoadSave))]
    public class AutoSaveDirectorLoadSave
    {
        public static void Postfix(AutoSaveDirector __instance, string gameName, string saveName, Il2CppSystem.Action onError, bool reloadAllCoreScenes = false)
        {
            if (NetworkClient.active) return;
            SRNetworkManager.CheckForMPSavePath();
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
            SRNetworkManager.CheckForMPSavePath();
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
        public static void Postfix(AutoSaveDirector __instance)
        {
            if (NetworkClient.active && !NetworkServer.activeHost) return;
            SRNetworkManager.DoNetworkSave();
        }
    }
}
