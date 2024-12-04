using HarmonyLib;
using Mirror;
using NewSR2MP;
using NewSR2MP.Networking;
using NewSR2MP.Networking.Component;
using NewSR2MP.Networking.Packet;
using System;




namespace NewSR2MP.Networking.Patches
{
    [HarmonyPatch(typeof(PlayerState), nameof(PlayerState.AddCurrency))]
    internal class PlayerStateAddCurrency
    {
        public static void Postfix(PlayerState __instance, int adjust, PlayerState.CoinsType coinsType)
        {

            if (NetworkClient.active || NetworkServer.active)
            {
                if (NetworkServer.activeHost && savedGame.sharedMoney)
                {
                    return;
                }
                SetMoneyMessage message = new SetMoneyMessage()
                {
                    newMoney = __instance.GetCurrency()
                };
                SRNetworkManager.NetworkSend(message);
            }
        }
    }
    [HarmonyPatch(typeof(PlayerState), nameof(PlayerState.SpendCurrency))]
    internal class PlayerStateSpendCurrency
    {
        public static void Postfix(PlayerState __instance, int adjust)
        {

            if (NetworkClient.active || NetworkServer.active)
            {
                if (NetworkServer.activeHost && savedGame.sharedMoney)
                {
                    return;
                }
                SetMoneyMessage message = new SetMoneyMessage()
                {
                    newMoney = __instance.GetCurrency()
                };
                SRNetworkManager.NetworkSend(message);
            }
        }
    }
    [HarmonyPatch(typeof(PlayerState), nameof(PlayerState.UnlockMap))]
    internal class PlayerStateUnlockMap
    {
        public static void Postfix(PlayerState __instance, ZoneDirector.Zone zone)
        {
            if (NetworkClient.active || NetworkServer.active)
            {
                MapUnlockMessage message = new MapUnlockMessage()
                {
                    id = zone
                };
                SRNetworkManager.NetworkSend(message);
            }
        }
    }
    [HarmonyPatch(typeof(PlayerState), nameof(PlayerState.AddKey))]
    internal class PlayerStateAddKey
    {
        public static bool Prefix(PlayerState __instance)
        {

            if (NetworkClient.active || NetworkServer.active)
            {
                if (NetworkServer.activeHost && savedGame.sharedKeys)
                {
                    return true;
                }
                if (HandledKey.collected) return false;
                HandledKey.StartTimer();
                SetKeysMessage message = new SetKeysMessage()
                {
                    newMoney = __instance.model.keys + 1
                };
                SRNetworkManager.NetworkSend(message);
            }
            return true;
        }
    }
    [HarmonyPatch(typeof(PlayerState), nameof(PlayerState.SpendKey))]
    internal class PlayerStateSpendKey
    {
        public static void Postfix(PlayerState __instance)
        {

            if (NetworkClient.active || NetworkServer.active)
            {

                if (NetworkServer.activeHost && savedGame.sharedKeys)
                {
                    return;
                }
                SetKeysMessage message = new SetKeysMessage()
                {
                    newMoney = __instance.model.keys
                };
                SRNetworkManager.NetworkSend(message);
            }
        }
    }
}
