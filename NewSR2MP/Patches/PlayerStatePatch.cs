using HarmonyLib;

using NewSR2MP;
using NewSR2MP;
using NewSR2MP.Component;
using NewSR2MP.Packet;
using System;
using Il2CppMonomiPark.SlimeRancher.DataModel;
using Il2CppMonomiPark.SlimeRancher.UI.Map;

namespace NewSR2MP.Patches
{
    [HarmonyPatch(typeof(Il2Cpp.PlayerState), nameof(Il2Cpp.PlayerState.AddCurrency))]
    internal class PlayerStateAddCurrency
    {
        public static void Postfix(Il2Cpp.PlayerState __instance, int adjust, Il2Cpp.PlayerState.CoinsType coinsType)
        {

            if (ClientActive() || ServerActive())
            {
                SetMoneyMessage message = new SetMoneyMessage()
                {
                    newMoney = __instance.GetCurrency()
                };
                MultiplayerManager.NetworkSend(message);
            }
        }
    }
    [HarmonyPatch(typeof(Il2Cpp.PlayerState), nameof(Il2Cpp.PlayerState.SpendCurrency))]
    internal class PlayerStateSpendCurrency
    {
        public static void Postfix(Il2Cpp.PlayerState __instance, int adjust)
        {

            if (ClientActive() || ServerActive())
            {
                SetMoneyMessage message = new SetMoneyMessage()
                {
                    newMoney = __instance.GetCurrency()
                };
                MultiplayerManager.NetworkSend(message);
            }
        }
    }
    
}
