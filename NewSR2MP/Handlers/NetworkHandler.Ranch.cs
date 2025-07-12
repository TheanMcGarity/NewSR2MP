using NewSR2MP.Attributes;

namespace NewSR2MP;

public partial class NetworkHandler
{
    
    [PacketResponse]
    private static void HandleMarketRefresh(NetPlayerState netPlayer, MarketRefreshMessage packet, byte channel)
    {

        int i = 0;

        SRMP.Debug($"Recieved Market Price Listing Count: {packet.prices.Count}");

        foreach (var price in sceneContext.EconomyDirector._currValueMap)
        {
            try
            {
                SRMP.Debug($"Market price listing {i}: {packet.prices[i]}");
                price.Value.CurrValue = packet.prices[i];
            }
            catch
            {
            }

            i++;
        }

        marketUI?.EconUpdate();
    }
    [PacketResponse]
    private static void HandleRefineryItem(NetPlayerState netPlayer, RefineryItemMessage packet, byte channel)
    {

        handlingPacket = true;
        sceneContext.GadgetDirector._model.SetCount(identifiableTypes[packet.id], packet.count);
        handlingPacket = false;
    }
    [PacketResponse]
    private static void HandleGarden(NetPlayerState netPlayer, GardenPlantMessage packet, byte channel)
    {     
        
        try
        {
            var model = sceneContext.GameModel.landPlots[packet.id];
            var plot = model.gameObj;

            model.resourceGrowerDefinition =
                gameContext.AutoSaveDirector.resourceGrowers.items._items.FirstOrDefault(x =>
                    x._primaryResourceType == identifiableTypes[packet.ident]);

            var lp = plot.transform.GetChild(0).GetComponent<LandPlot>();
            var g = plot.transform.GetComponentInChildren<GardenCatcher>();

            if (packet.ident != 9)
            {
                handlingPacket = true;

                if (g.CanAccept(identifiableTypes[packet.ident]))
                    g.Plant(identifiableTypes[packet.ident], false);

                handlingPacket = false;
            }
            else
            {

                handlingPacket = true;

                lp.DestroyAttached();

                handlingPacket = false;


            }
        }
        catch (Exception e)
        {
            SRMP.Log($"Exception in handling garden({packet.id})! Stack Trace:\n{e}");
        }
    }
    [PacketResponse]
    private static void HandleLandPlot(NetPlayerState netPlayer, LandPlotMessage packet, byte channel)
    {
        try
        {
            var model = sceneContext.GameModel.landPlots[packet.id];
            var plot = model.gameObj;

            if (packet.messageType == LandplotUpdateType.SET)
            {
                handlingPacket = true;

                plot.GetComponent<LandPlotLocation>().Replace(plot.GetComponentInChildren<LandPlot>(),
                    GameContext.Instance.LookupDirector._plotPrefabDict[packet.type]);

                model.typeId = packet.type;
                
                handlingPacket = false;
            }
            else
            {

                var lp = plot.GetComponentInChildren<LandPlot>();

                handlingPacket = true;

                lp.AddUpgrade(packet.upgrade);

                model.upgrades.Add(packet.upgrade);
                
                handlingPacket = false;

            }
        }
        catch (Exception e)
        {
            SRMP.Log($"Exception in handling landplot({packet.id})! Stack Trace:\n{e}");
        }
    }
}