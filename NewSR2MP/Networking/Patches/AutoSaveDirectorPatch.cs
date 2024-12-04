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
        public static void Postfix(AutoSaveDirector __instance, string gameName, string saveName, bool promptDLCPurgedException, Action onError)
        {
            if (NetworkClient.active) return;
            SRNetworkManager.CheckForMPSavePath();
            var path = Path.Combine(((FileStorageProvider)GameContext.Instance.AutoSaveDirector.StorageProvider).SavePath(), "MultiplayerSaves", $"{gameName}.srmp");
            var networkGame = new NetworkV01();
            try
            {
                using (FileStream fs = File.Open(path, FileMode.OpenOrCreate))
                {
                    networkGame.Load(fs);
                }
            }
            catch { }

            savedGame = networkGame;
            savedGamePath = path;
        }
    }
    [HarmonyPatch(typeof(AutoSaveDirector), nameof(AutoSaveDirector.LoadNewGame))]
    public class AutoSaveDirectorLoadNewGame
    {
        public static void Postfix(AutoSaveDirector __instance, string displayName, Identifiable.Id gameIconId, GameMode gameMode, Action onError)
        {
            SRNetworkManager.CheckForMPSavePath();
            var path = Path.Combine(GameContext.Instance.AutoSaveDirector.StorageProvider.TryCast<FileStorageProvider>().savePath, "MultiplayerSaves", $"{displayName}.srmp");
            var networkGame = new NetworkV01();
            networkGame.sharedKeys = initialWorldSettings.shareKeys;
            networkGame.sharedUpgrades = initialWorldSettings.shareUpgrades;
            networkGame.sharedMoney
                = initialWorldSettings.shareMoney;
            
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
