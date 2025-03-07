using Il2CppMonomiPark.SlimeRancher.DataModel;
using Il2CppMonomiPark.SlimeRancher.Player.CharacterController;

namespace NewSR2MP.Networking.Packet;

[HarmonyPatch(typeof(EconomyDirector),nameof(EconomyDirector.ResetPrices))]
public class EconomyDirectorResetPrices
{
    static void Postfix(EconomyDirector __instance)
    {
        var prices = new List<float>();
        foreach (var price in __instance._currValueMap)
        {
            prices.Add(price.value.CurrValue);
        }

        var packet = new MarketRefreshMessage
        {
            prices = prices
        };
        MultiplayerManager.NetworkSend(packet);
    }
}