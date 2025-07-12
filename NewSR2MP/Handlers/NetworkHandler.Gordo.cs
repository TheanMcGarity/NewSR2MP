using Il2CppMonomiPark.SlimeRancher.DataModel;
using NewSR2MP.Attributes;

namespace NewSR2MP;

public class NetworkHandler_Gordo
{
    
    [PacketResponse]
    private static void HandleGordoEat(NetPlayerState netPlayer, GordoEatMessage packet, byte channel)
    {

        try
        {
            if (!sceneContext.GameModel.gordos.TryGetValue(packet.id, out var gordo))
                sceneContext.GameModel.gordos.Add(packet.id, new GordoModel()
                {
                    fashions = new Il2CppSystem.Collections.Generic.List<IdentifiableType>(),
                    gordoEatCount = packet.count,
                    gordoSeen = true,
                    identifiableType = identifiableTypes[packet.ident],
                    gameObj = null,
                    GordoEatenCount = packet.count,
                    targetCount = gameContext.LookupDirector._gordoDict[identifiableTypes[packet.ident]]
                        .GetComponent<GordoEat>().TargetCount,
                });
            gordo.gordoEatCount = packet.count;
        }
        catch (Exception e)
        {
            SRMP.Log($"Exception in feeding gordo({packet.id})! Stack Trace:\n{e}");
        }
    }


    [PacketResponse]
    private static void HandleGordoBurst(NetPlayerState netPlayer, GordoBurstMessage packet, byte channel)
    {

        try
        {
            var target = gameContext.LookupDirector._gordoDict[identifiableTypes[packet.ident]]
                .GetComponent<GordoEat>().TargetCount;
            if (!sceneContext.GameModel.gordos.TryGetValue(packet.id, out var gordo))
                sceneContext.GameModel.gordos.Add(packet.id, new GordoModel()
                {
                    fashions = new Il2CppSystem.Collections.Generic.List<IdentifiableType>(),
                    gordoEatCount = target,
                    gordoSeen = true,
                    identifiableType = identifiableTypes[packet.ident],
                    gameObj = null,
                    GordoEatenCount = target,
                    targetCount = gameContext.LookupDirector._gordoDict[identifiableTypes[packet.ident]]
                        .GetComponent<GordoEat>().TargetCount,
                });
            else
            {
                var gordoObj = gordo.gameObj;
                handlingPacket = true;
                gordoObj.GetComponent<GordoEat>().ImmediateReachedTarget();
                handlingPacket = false;
            }
        }
        catch (Exception e)
        {
            if (ShowErrors)
                SRMP.Log($"Exception in popping gordo({packet.id})! Stack Trace:\n{e}");
        }


    }
}