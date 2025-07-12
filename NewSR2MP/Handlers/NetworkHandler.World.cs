using Il2CppMonomiPark.SlimeRancher.DataModel;
using Il2CppMonomiPark.SlimeRancher.Event;
using Il2CppMonomiPark.SlimeRancher.Map;
using Il2CppMonomiPark.SlimeRancher.UI.Map;
using Il2CppMonomiPark.SlimeRancher.World;
using Il2CppMonomiPark.World;
using NewSR2MP.Attributes;

namespace NewSR2MP;

public partial class NetworkHandler
{
    
    [PacketResponse]
    private static void HandleDoor(NetPlayerState netPlayer, DoorOpenMessage packet, byte channel)
    {
        sceneContext.GameModel.doors[packet.id].gameObj.GetComponent<AccessDoor>().CurrState =
            AccessDoor.State.OPEN;
    }
    [PacketResponse]
    private static void HandleMoneyChange(NetPlayerState netPlayer, SetMoneyMessage packet, byte channel)
    {
        sceneContext.PlayerState._model.currency = packet.newMoney;


    }

    

    [PacketResponse]
    private static void HandleTime(NetPlayerState netPlayer, TimeSyncMessage packet, byte channel)
    {
        try
        {
            sceneContext.GameModel.world.worldTime = packet.time;
        }
        catch
        {
        }
    }
    private static void HandleSleep(NetPlayerState netPlayer, SleepMessage packet, byte channel)
    {

        try
        {
            sceneContext?.TimeDirector.FastForwardTo(packet.targetTime);
        }
        catch (Exception e)
        {
            SRMP.Error($"Exception in sleeping! Stack Trace:\n{e}");
        }
    }

    [PacketResponse]
    private static void HandleNavPlace(NetPlayerState netPlayer, PlaceNavMarkerNessage packet, byte channel)
    {


        MapDefinition map = null;
        switch (packet.map)
        {
            case MapType.RainbowIsland:
                map = sceneContext.MapDirector._mapList._maps[0];
                break;
            case MapType.Labyrinth:
                map = sceneContext.MapDirector._mapList._maps[1];
                break;
        }

        handlingNavPacket = true;
        sceneContext.MapDirector.SetPlayerNavigationMarker(packet.position, map, 0);
        handlingNavPacket = false;
    }

    [PacketResponse]
    private static void HandleNavRemove(NetPlayerState netPlayer, RemoveNavMarkerNessage packet, byte channel)
    {
        handlingNavPacket = true;
        sceneContext.MapDirector.ClearPlayerNavigationMarker();
        handlingNavPacket = false;
    }



    [PacketResponse]
    private static void HandleWeather(NetPlayerState netPlayer, WeatherSyncMessage packet, byte channel)
    {
        MelonCoroutines.Start(WeatherHandlingCoroutine(packet));
    }


    [PacketResponse]
    private static void HandleSwitchModify(NetPlayerState netPlayer, SwitchModifyMessage packet, byte channel)
    {

        if (sceneContext.GameModel.switches.TryGetValue(packet.id, out var model))
        {
            model.state = (SwitchHandler.State)packet.state;
            if (model.gameObj)
            {
                handlingPacket = true;

                if (model.gameObj.TryGetComponent<WorldStatePrimarySwitch>(out var primary))
                    primary.SetStateForAll((SwitchHandler.State)packet.state, false);

                if (model.gameObj.TryGetComponent<WorldStateSecondarySwitch>(out var secondary))
                    secondary.SetState((SwitchHandler.State)packet.state, false);

                if (model.gameObj.TryGetComponent<WorldStateInvisibleSwitch>(out var invisible))
                    invisible.SetStateForAll((SwitchHandler.State)packet.state, false);

                handlingPacket = false;
            }
        }
        else
        {
            model = new WorldSwitchModel()
            {
                gameObj = null,
                state = (SwitchHandler.State)packet.state,
            };
            sceneContext.GameModel.switches.Add(packet.id, model);
        }
    }

    [PacketResponse]
    private static void HandleMapUnlock(NetPlayerState netPlayer, MapUnlockMessage packet, byte channel)
    {

        sceneContext.MapDirector.NotifyZoneUnlocked(GetGameEvent(packet.id), false, 0);

        var activator = Resources.FindObjectsOfTypeAll<MapNodeActivator>().FirstOrDefault(x => x._fogRevealEvent._dataKey == packet.id);

        if (activator)
            activator.StartCoroutine(activator.ActivateHologramAnimation());
        
        
        var eventDirModel = sceneContext.eventDirector._model;
        if (!eventDirModel.table.TryGetValue("fogRevealed", out var table))
        {
            eventDirModel.table.Add("fogRevealed",
                new Il2CppSystem.Collections.Generic.Dictionary<string, EventRecordModel.Entry>());
            table = eventDirModel.table["fogRevealed"];
        }

        table.TryAdd(packet.id, new EventRecordModel.Entry
        {
            count = 1,
            createdRealTime = 0,
            createdGameTime = 0,
            dataKey = packet.id,
            eventKey = "fogRevealed",
            updatedRealTime = 0,
            updatedGameTime = 0,
        });
    }

    


    [PacketResponse]
    private static void HandleTreasurePod(NetPlayerState netPlayer, TreasurePodMessage packet, byte channel)
    {
        
        var identifier = $"pod{ExtendInteger(packet.id)}";
        
        if (sceneContext.GameModel.pods.TryGetValue(identifier, out var model))
        {
            handlingPacket = true;
            model.gameObj?.GetComponent<TreasurePod>().Activate();
            handlingPacket = false;
            
            model.state = Il2Cpp.TreasurePod.State.OPEN;
        }
        else
        {
            sceneContext.GameModel.pods.Add(identifier, new TreasurePodModel
            {
                state = Il2Cpp.TreasurePod.State.OPEN,
                gameObj = null,
                spawnQueue = new Il2CppSystem.Collections.Generic.Queue<IdentifiableType>()
            });
        }
    }
}