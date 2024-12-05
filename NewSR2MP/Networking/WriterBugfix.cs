using Mirror;
using Mirror.Discovery;
using NewSR2MP.Networking.Packet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewSR2MP.Networking
{
    public class WriterBugfix
    {
        public static void FixWriters()
        {
            Writer<ServerRequest>.write = new Action<NetworkWriter, ServerRequest>((w, v) => NetworkWriterExtensions.Write(w, v));
            Writer<ServerResponse>.write = new Action<NetworkWriter, ServerResponse>((w, v) => NetworkWriterExtensions.Write(w, v));
            Writer<TestLogMessage>.write = new Action<NetworkWriter, TestLogMessage>((w, v) => NetworkWriterExtensions.Write(w, v));
            Writer<NetworkPingMessage>.write = new Action<NetworkWriter, NetworkPingMessage>((w, v) => NetworkWriterExtensions.Write(w, v));
            Writer<SceneMessage>.write = new Action<NetworkWriter, SceneMessage>((w, v) => NetworkWriterExtensions.Write(w, v));
            Writer<AddPlayerMessage>.write = new Action<NetworkWriter, AddPlayerMessage>((w, v) => NetworkWriterExtensions.Write(w, v));
            Writer<ReadyMessage>.write = new Action<NetworkWriter, ReadyMessage>((w, v) => NetworkWriterExtensions.Write(w, v));
            Writer<NotReadyMessage>.write = new Action<NetworkWriter, NotReadyMessage>((w, v) => NetworkWriterExtensions.Write(w, v));
            Writer<TimeSnapshotMessage>.write = new Action<NetworkWriter, TimeSnapshotMessage>((w, v) => NetworkWriterExtensions.Write(w, v));
            Writer<SetMoneyMessage>.write = new Action<NetworkWriter, SetMoneyMessage>((w, v) => NetworkWriterExtensions.Write(w, v));
            Writer<PlayerJoinMessage>.write = new Action<NetworkWriter, PlayerJoinMessage>((w, v) => NetworkWriterExtensions.Write(w, v));
            Writer<ClientUserMessage>.write = new Action<NetworkWriter, ClientUserMessage>((w, v) => NetworkWriterExtensions.Write(w, v));
            Writer<PlayerLeaveMessage>.write = new Action<NetworkWriter, PlayerLeaveMessage>((w, v) => NetworkWriterExtensions.Write(w, v));
            Writer<PlayerUpdateMessage>.write = new Action<NetworkWriter, PlayerUpdateMessage>((w, v) => NetworkWriterExtensions.Write(w, v));
            Writer<TimeSyncMessage>.write = new Action<NetworkWriter, TimeSyncMessage>((w, v) => NetworkWriterExtensions.Write(w, v));
            Writer<SleepMessage>.write = new Action<NetworkWriter, SleepMessage>((w, v) => NetworkWriterExtensions.Write(w, v));
            Writer<ActorSpawnMessage>.write = new Action<NetworkWriter, ActorSpawnMessage>((w, v) => NetworkWriterExtensions.Write(w, v));
            Writer<ActorSpawnClientMessage>.write = new Action<NetworkWriter, ActorSpawnClientMessage>((w, v) => NetworkWriterExtensions.Write(w, v));
            Writer<ActorUpdateClientMessage>.write = new Action<NetworkWriter, ActorUpdateClientMessage>((w, v) => NetworkWriterExtensions.Write(w, v));
            Writer<ActorUpdateMessage>.write = new Action<NetworkWriter, ActorUpdateMessage>((w, v) => NetworkWriterExtensions.Write(w, v));
            Writer<ActorDestroyGlobalMessage>.write = new Action<NetworkWriter, ActorDestroyGlobalMessage>((w, v) => NetworkWriterExtensions.Write(w, v));
            Writer<ActorUpdateOwnerMessage>.write = new Action<NetworkWriter, ActorUpdateOwnerMessage>((w, v) => NetworkWriterExtensions.Write(w, v));
            Writer<LandPlotMessage>.write = new Action<NetworkWriter, LandPlotMessage>((w, v) => NetworkWriterExtensions.Write(w, v));
            Writer<GordoEatMessage>.write = new Action<NetworkWriter, GordoEatMessage>((w, v) => NetworkWriterExtensions.Write(w, v));
            Writer<GordoBurstMessage>.write = new Action<NetworkWriter, GordoBurstMessage>((w, v) => NetworkWriterExtensions.Write(w, v));
            Writer<PediaMessage>.write = new Action<NetworkWriter, PediaMessage>((w, v) => NetworkWriterExtensions.Write(w, v));
            Writer<LoadMessage>.write = new Action<NetworkWriter, LoadMessage>((w, v) => NetworkWriterExtensions.Write(w, v));
            Writer<AmmoAddMessage>.write = new Action<NetworkWriter, AmmoAddMessage>((w, v) => NetworkWriterExtensions.Write(w, v));
            Writer<AmmoEditSlotMessage>.write = new Action<NetworkWriter, AmmoEditSlotMessage>((w, v) => NetworkWriterExtensions.Write(w, v));
            Writer<AmmoRemoveMessage>.write = new Action<NetworkWriter, AmmoRemoveMessage>((w, v) => NetworkWriterExtensions.Write(w, v));
            Writer<MapUnlockMessage>.write = new Action<NetworkWriter, MapUnlockMessage>((w, v) => NetworkWriterExtensions.Write(w, v));
            Writer<DoorOpenMessage>.write = new Action<NetworkWriter, DoorOpenMessage>((w, v) => NetworkWriterExtensions.Write(w, v));
            Writer<ResourceStateMessage>.write = new Action<NetworkWriter, ResourceStateMessage>((w, v) => NetworkWriterExtensions.Write(w, v));
            Writer<GardenPlantMessage>.write = new Action<NetworkWriter, GardenPlantMessage>((w, v) => NetworkWriterExtensions.Write(w, v));
            Writer<ActorChangeHeldOwnerMessage>.write = new Action<NetworkWriter, ActorChangeHeldOwnerMessage>((w, v) => NetworkWriterExtensions.Write(w, v));
        }
    }
}
