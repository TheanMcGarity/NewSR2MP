using NewSR2MP.Attributes;

namespace NewSR2MP;

public partial class NetworkHandler
{
    
    [PacketResponse]
    private static void HandlePedia(NetPlayerState netPlayer, PediaMessage packet, byte channel)
    {

        handlingPacket = true;
        sceneContext.PediaDirector.Unlock(pediaEntries[packet.id]);
        handlingPacket = false;
    }
    [PacketResponse]
    private static void HandlePlayerUpgrade(NetPlayerState netPlayer, PlayerUpgradeMessage packet, byte channel)
    {

        handlingPacket = true;
        sceneContext.PlayerState._model.upgradeModel.IncrementUpgradeLevel(sceneContext.PlayerState._model.upgradeModel.upgradeDefinitions.items._items
            .FirstOrDefault(x => x._uniqueId == packet.id));
        handlingPacket = false;

    }
}