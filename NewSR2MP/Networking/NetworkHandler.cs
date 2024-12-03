using HarmonyLib;
using Mirror;
using Il2CppMonomiPark.SlimeRancher.DataModel;
using NewSR2MP.Command;
using NewSR2MP.Networking.Component;
using NewSR2MP.Networking.Packet;
using NewSR2MP.Networking.Patches;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Unity.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.SocialPlatforms;




namespace NewSR2MP.Networking
{
    public class NetworkHandler
    {
        public class Server
        {
            #region SERVER
            internal static void Start()
            {
                NetworkServer.RegisterHandler(new Action<NetworkConnectionToClient, TestLogMessage>(HandleTestLog));
                NetworkServer.RegisterHandler(new Action<NetworkConnectionToClient, SetMoneyMessage>(HandleMoneyChange));
                NetworkServer.RegisterHandler(new Action<NetworkConnectionToClient, SetKeysMessage>(HandleKeysChange));
                NetworkServer.RegisterHandler(new Action<NetworkConnectionToClient, PlayerJoinMessage>(HandlePlayerJoin));
                NetworkServer.RegisterHandler(new Action<NetworkConnectionToClient, PlayerUpdateMessage>(HandlePlayer));
                NetworkServer.RegisterHandler(new Action<NetworkConnectionToClient, SleepMessage>(HandleClientSleep));
                NetworkServer.RegisterHandler(new Action<NetworkConnectionToClient, ActorSpawnClientMessage>(HandleClientActorSpawn));
                NetworkServer.RegisterHandler(new Action<NetworkConnectionToClient, ActorUpdateClientMessage>(HandleClientActor));
                NetworkServer.RegisterHandler(new Action<NetworkConnectionToClient, ActorDestroyGlobalMessage>(HandleDestroyActor));
                NetworkServer.RegisterHandler(new Action<NetworkConnectionToClient, ActorUpdateOwnerMessage>(HandleActorOwner));
                NetworkServer.RegisterHandler(new Action<NetworkConnectionToClient, LandPlotMessage>(HandleLandPlot));
                NetworkServer.RegisterHandler(new Action<NetworkConnectionToClient, GordoEatMessage>(HandleGordoEat));
                NetworkServer.RegisterHandler(new Action<NetworkConnectionToClient, GordoBurstMessage>(HandleGordoBurst));
                NetworkServer.RegisterHandler(new Action<NetworkConnectionToClient, PediaMessage>(HandlePedia));
                NetworkServer.RegisterHandler(new Action<NetworkConnectionToClient, AmmoAddMessage>(HandleAmmo));
                NetworkServer.RegisterHandler(new Action<NetworkConnectionToClient, AmmoEditSlotMessage>(HandleAmmoSlot));
                NetworkServer.RegisterHandler(new Action<NetworkConnectionToClient, AmmoRemoveMessage>(HandleAmmoReverse));
                NetworkServer.RegisterHandler(new Action<NetworkConnectionToClient, MapUnlockMessage>(HandleMap));
                NetworkServer.RegisterHandler(new Action<NetworkConnectionToClient, DoorOpenMessage>(HandleDoor));
                NetworkServer.RegisterHandler(new Action<NetworkConnectionToClient, GardenPlantMessage>(HandleGarden));
                NetworkServer.RegisterHandler(new Action<NetworkConnectionToClient, ActorChangeHeldOwnerMessage>(HandleActorHold));
                NetworkServer.RegisterHandler(new Action<NetworkConnectionToClient, PlayerLeaveMessage>(HandlePlayerLeave));
                NetworkServer.RegisterHandler(new Action<NetworkConnectionToClient, ClientUserMessage>(HandleClientJoin));
                NetworkServer.RegisterHandler(new Action<NetworkConnectionToClient, ResourceStateMessage>(HandleResourceState));
            }

            public static void HandleResourceState(NetworkConnectionToClient nctc, ResourceStateMessage packet)
            {
                try
                {
                    var res = SRNetworkManager.actors[packet.id].GetComponent<ResourceCycle>();
                    Rigidbody rigidbody = res.body;

                    switch (packet.state)
                    {
                        case ResourceCycle.State.ROTTEN:
                            if (res.model.state == ResourceCycle.State.ROTTEN) break;
                            res.Rot();
                            res.SetRotten(true);
                            break;
                        case ResourceCycle.State.RIPE:
                            if (res.model.state == ResourceCycle.State.RIPE) break;
                            res.Ripen();
                            if (res.vacuumableWhenRipe)
                            {
                                res.vacuumable.enabled = true;
                            }

                            if (res.gameObject.transform.localScale.x < res.defaultScale.x * 0.33f)
                            {
                                res.gameObject.transform.localScale = res.defaultScale * 0.33f;
                            }

                            TweenUtil.ScaleTo(res.gameObject, res.defaultScale, 4f);
                            break;
                        case ResourceCycle.State.UNRIPE:
                            if (res.model.state == ResourceCycle.State.UNRIPE) break;
                            res.model.state = ResourceCycle.State.UNRIPE;
                            res.transform.localScale = res.defaultScale * 0.33f;
                            break;
                        case ResourceCycle.State.EDIBLE:
                            if (res.model.state == ResourceCycle.State.EDIBLE) break;
                            res.MakeEdible();
                            res.additionalRipenessDelegate = null;
                            rigidbody.isKinematic = false;
                            if (res.preparingToRelease)
                            {
                                res.preparingToRelease = false;
                                res.releaseAt = 0f;
                                res.toShake.localPosition = res.toShakeDefaultPos;
                                if (res.releaseCue != null)
                                {
                                    SECTR_PointSource component = res.GetComponent<SECTR_PointSource>();
                                    component.Cue = res.releaseCue;
                                    component.Play();
                                }
                            }
                            break;
                    }
                }
                catch (Exception e)
                {
                    if (SRMLConfig.SHOW_SRMP_ERRORS)
                        SRMP.Log($"Exception in handling state for resource({packet.id})! Stack Trace:\n{e}");
                }

                // Notify others
                foreach (var conn in NetworkServer.connections.Values)
                {
                    if (conn.connectionId != nctc.connectionId)
                    {

                        NetworkServer.SRMPSend(packet, conn);
                    }
                }
            }
            public static void HandleClientJoin(NetworkConnectionToClient client, ClientUserMessage joinInfo)
            {
                MultiplayerManager.PlayerJoin(client, joinInfo.guid, joinInfo.name);
            }


            public static void HandleTestLog(NetworkConnectionToClient nctc, TestLogMessage packet)
            {
                SRMP.Log(packet.MessageToLog);
            }
            public static void HandleDoor(NetworkConnectionToClient nctc, DoorOpenMessage packet)
            {
                SceneContext.Instance.GameModel.doors[packet.id].gameObj.GetComponent<AccessDoor>().CurrState = AccessDoor.State.OPEN;


                // Notify others
                foreach (var conn in NetworkServer.connections.Values)
                {
                    if (conn.connectionId != nctc.connectionId)
                    {

                        NetworkServer.SRMPSend(packet, conn);
                    }
                }
            }
            public static void HandleMoneyChange(NetworkConnectionToClient nctc, SetMoneyMessage packet)
            {
                if (!SRNetworkManager.savedGame.sharedMoney)
                {
                    SRNetworkManager.savedGame.savedPlayers.playerList[SRNetworkManager.clientToGuid[nctc.connectionId]].money = packet.newMoney;
                    return;
                }
                SceneContext.Instance.PlayerState.model.currency = packet.newMoney;

                // Notify others
                foreach (var conn in NetworkServer.connections.Values)
                {
                    if (conn.connectionId != nctc.connectionId)
                    {

                        NetworkServer.SRMPSend(packet, conn);
                    }
                }
            }
            public static void HandleKeysChange(NetworkConnectionToClient nctc, SetKeysMessage packet)
            {

                if (!SRNetworkManager.savedGame.sharedKeys)
                {
                    SRNetworkManager.savedGame.savedPlayers.playerList[SRNetworkManager.clientToGuid[nctc.connectionId]].keys = packet.newMoney;
                    return;
                }

                SceneContext.Instance.PlayerState.model.keys = packet.newMoney;

                // Notify others
                foreach (var conn in NetworkServer.connections.Values)
                {
                    if (conn.connectionId != nctc.connectionId)
                    {

                        NetworkServer.SRMPSend(packet, conn);
                    }
                }
            }
            public static void HandlePlayerJoin(NetworkConnectionToClient nctc, PlayerJoinMessage packet)
            {
                // Do nothing, everything is already handled anyways.
            }
            public static void HandlePlayerLeave(NetworkConnectionToClient nctc, PlayerLeaveMessage packet)
            {
                // Packet should only be S2C
                SRMP.Log("Bug Alert!!! Packet should only be Server To Client, but it was sent from Client To Server.");
            }
            public static void HandleClientSleep(NetworkConnectionToClient nctc, SleepMessage packet)
            {
                SceneContext.Instance.TimeDirector.FastForwardTo(packet.time);
            }
            public static void HandleClientActorSpawn(NetworkConnectionToClient nctc, ActorSpawnClientMessage packet)
            {
                try
                {
                    SRMP.Log($"Actor spawned with velocity {packet.velocity}.");
                    Quaternion quat = Quaternion.Euler(packet.rotation.x, packet.rotation.y, packet.rotation.z);
                    var identObj = GameContext.Instance.LookupDirector.identifiablePrefabDict[packet.ident];
                    if (identObj.GetComponent<NetworkActor>() == null)
                        identObj.AddComponent<NetworkActor>();
                    if (identObj.GetComponent<NetworkActorOwnerToggle>() == null)
                        identObj.AddComponent<NetworkActorOwnerToggle>();
                    if (identObj.GetComponent<TransformSmoother>() == null)
                        identObj.AddComponent<TransformSmoother>();
                    var obj = SRBehaviour.InstantiateActor(identObj, packet.region, packet.position, quat, false);
                    identObj.RemoveComponent<NetworkActor>();
                    identObj.RemoveComponent<NetworkActorOwnerToggle>();
                    identObj.RemoveComponent<TransformSmoother>();
                    obj.AddComponent<NetworkResource>();
                    if (!SRNetworkManager.actors.ContainsKey(obj.GetComponent<Identifiable>().GetActorId())) // Most useless if statement ever.
                    {
                        obj.GetComponent<TransformSmoother>().enabled = false;
                        SRNetworkManager.actors.Add(obj.GetComponent<Identifiable>().GetActorId().Value, obj.GetComponent<NetworkActor>());
                        obj.GetComponent<Rigidbody>().velocity = packet.velocity;
                        obj.GetComponent<NetworkActor>().startingVel = packet.velocity;
                        obj.GetComponent<TransformSmoother>().interpolPeriod = .15f;
                        obj.GetComponent<Vacuumable>().launched = true;
                    }
                    else
                    {
                        obj.GetComponent<TransformSmoother>().enabled = false;
                        obj.GetComponent<Rigidbody>().velocity = packet.velocity;
                        obj.GetComponent<NetworkActor>().startingVel = packet.velocity;
                        obj.GetComponent<TransformSmoother>().interpolPeriod = .15f;
                        obj.GetComponent<Vacuumable>().launched = true;
                    }
                }
                catch (Exception e)
                {
                    if (SRMLConfig.SHOW_SRMP_ERRORS)
                        SRMP.Log($"Exception in spawning actor(no id)! Stack Trace:\n{e}");
                }
            }
            public static void HandleClientActor(NetworkConnectionToClient nctc, ActorUpdateClientMessage packet)
            {
                try
                {
                    var actor = SRNetworkManager.actors[packet.id];
                    var t = actor.GetComponent<TransformSmoother>();
                    t.nextPos = packet.position;
                    t.nextRot = packet.rotation;
                }
                catch (Exception e)
                {
                    if (SRMLConfig.SHOW_SRMP_ERRORS)
                        SRMP.Log($"Exception in handling actor({packet.id})! Stack Trace:\n{e}");
                }
                ActorUpdateMessage packetS2C = new ActorUpdateMessage()
                {
                    id = packet.id,
                    position = packet.position,
                    rotation = packet.rotation,
                };

                foreach (var conn in NetworkServer.connections.Values)
                {
                    if (conn.connectionId != nctc.connectionId)
                    {

                        NetworkServer.SRMPSend(packetS2C, conn);
                    }
                }
            }

            public static void HandleActorOwner(NetworkConnectionToClient nctc, ActorUpdateOwnerMessage packet)
            {
                try
                {
                    var actor = SRNetworkManager.actors[packet.id];

                    actor.GetComponent<NetworkActor>().IsOwned = false;
                    actor.GetComponent<TransformSmoother>().enabled = true;
                    actor.GetComponent<NetworkActor>().enabled = false;

                    actor.GetComponent<NetworkActorOwnerToggle>().LoseGrip();
                }
                catch (Exception e)
                {
                    if (SRMLConfig.SHOW_SRMP_ERRORS)
                        SRMP.Log($"Exception in transfering actor({packet.id})! Stack Trace:\n{e}");
                }

                foreach (var conn in NetworkServer.connections.Values)
                {
                    if (conn.connectionId != nctc.connectionId)
                    {

                        NetworkServer.SRMPSend(packet, conn);
                    }
                }
            }
            public static void HandleDestroyActor(NetworkConnectionToClient nctc, ActorDestroyGlobalMessage packet)
            {
                try
                {
                    UnityEngine.Object.Destroy(SRNetworkManager.actors[packet.id].gameObject);
                    SRNetworkManager.actors.Remove(packet.id);
                }
                catch (Exception e)
                {
                    if (SRMLConfig.SHOW_SRMP_ERRORS)
                        SRMP.Log($"Exception in handling actor({packet.id})! Stack Trace:\n{e}");
                }

                foreach (var conn in NetworkServer.connections.Values)
                {
                    if (conn.connectionId != nctc.connectionId)
                    {

                        NetworkServer.SRMPSend(packet, conn);
                    }
                }
            }
            public static void HandlePlayer(NetworkConnectionToClient nctc, PlayerUpdateMessage packet)
            {
                try
                {
                    var player = SRNetworkManager.players[packet.id];

                    player.GetComponent<TransformSmoother>().nextPos = packet.pos;
                    player.GetComponent<TransformSmoother>().nextRot = packet.rot.eulerAngles;
                }
                catch { }

                foreach (var conn in NetworkServer.connections.Values)
                {
                    if (conn.connectionId != nctc.connectionId)
                    {

                        NetworkServer.SRMPSend(packet, conn);
                    }
                }
            }
            public static void HandleLandPlot(NetworkConnectionToClient nctc, LandPlotMessage packet)
            {
                try
                {
                    var plot = SceneContext.Instance.GameModel.landPlots[packet.id].gameObj;

                    if (packet.messageType == LandplotUpdateType.SET)
                    {
                        plot.AddComponent<HandledDummy>();

                        plot.GetComponent<LandPlotLocation>().Replace(plot.transform.GetChild(0).GetComponent<LandPlot>(), GameContext.Instance.LookupDirector.plotPrefabDict[packet.type]);

                        UnityEngine.Object.Destroy(plot.GetComponent<HandledDummy>());
                    }
                    else
                    {

                        var lp = plot.transform.GetChild(0).GetComponent<LandPlot>();
                        lp.gameObject.AddComponent<HandledDummy>();

                        lp.AddUpgrade(packet.upgrade);

                        UnityEngine.Object.Destroy(lp.GetComponent<HandledDummy>());

                    }
                }
                catch (Exception e)
                {
                    if (SRMLConfig.SHOW_SRMP_ERRORS)
                        SRMP.Log($"Exception in handling landplot({packet.id})! Stack Trace:\n{e}");
                }
                foreach (var conn in NetworkServer.connections.Values)
                {
                    if (conn.connectionId != nctc.connectionId)
                    {

                        NetworkServer.SRMPSend(packet, conn);
                    }
                }
            }
            public static void HandleGarden(NetworkConnectionToClient nctc, GardenPlantMessage packet)
            {
                try
                {
                    // get plot from id.
                    var plot = SceneContext.Instance.GameModel.landPlots[packet.id].gameObj;

                    // Get required components
                    var lp = plot.transform.GetChild(0).GetComponent<LandPlot>();
                    var g = plot.transform.GetComponentInChildren<GardenCatcher>();

                    // Check if is destroy (planting NONE)
                    if (packet.ident != Identifiable.Id.NONE)
                    {
                        // Add handled component.
                        lp.gameObject.AddComponent<HandledDummy>();
                        
                        // Plant
                        if (g.CanAccept(packet.ident))
                            g.Plant(packet.ident, false);

                        // Remove handled component.
                        lp.gameObject.RemoveComponent<HandledDummy>();
                    }
                    else
                    {
                        // Add handled component.

                        lp.gameObject.AddComponent<HandledDummy>();

                        // UnPlant.
                        lp.DestroyAttached();

                        // Remove handled component.
                        lp.gameObject.RemoveComponent<HandledDummy>();


                    }
                }
                catch (Exception e)
                {
                    if (SRMLConfig.SHOW_SRMP_ERRORS)
                        SRMP.Log($"Exception in handling garden({packet.id})! Stack Trace:\n{e}");
                }
                foreach (var conn in NetworkServer.connections.Values)
                {
                    if (conn.connectionId != nctc.connectionId)
                    {

                        NetworkServer.SRMPSend(packet, conn);
                    }
                }
            }

            public static void HandleGordoEat(NetworkConnectionToClient nctc, GordoEatMessage packet)
            {
                try
                {
                    SceneContext.Instance.GameModel.gordos[packet.id].gordoEatenCount = packet.count;
                }
                catch (Exception e)
                {
                    if (SRMLConfig.SHOW_SRMP_ERRORS)
                        SRMP.Log($"Exception in feeding gordo({packet.id})! Stack Trace:\n{e}");
                }
                foreach (var conn in NetworkServer.connections.Values)
                {
                    if (conn.connectionId != nctc.connectionId)
                    {

                        NetworkServer.SRMPSend(packet, conn);
                    }
                }
            }
            public static void HandlePedia(NetworkConnectionToClient nctc, PediaMessage packet)
            {
                SceneContext.Instance.gameObject.AddComponent<HandledDummy>();
                SceneContext.Instance.PediaDirector.MaybeShowPopup(packet.id);
                UnityEngine.Object.Destroy(SceneContext.Instance.gameObject.GetComponent<HandledDummy>());

                foreach (var conn in NetworkServer.connections.Values)
                {
                    if (conn.connectionId != nctc.connectionId)
                    {

                        NetworkServer.SRMPSend(packet, conn);
                    }
                }
            }
            public static void HandleGordoBurst(NetworkConnectionToClient nctc, GordoBurstMessage packet)
            {
                try
                {
                    var gordo = SceneContext.Instance.GameModel.gordos[packet.id].gameObj;
                    gordo.AddComponent<HandledDummy>();
                    gordo.GetComponent<GordoEat>().ImmediateReachedTarget();
                    UnityEngine.Object.Destroy(gordo.GetComponent<HandledDummy>());
                }
                catch (Exception e)
                {
                    if (SRMLConfig.SHOW_SRMP_ERRORS)
                        SRMP.Log($"Exception in feeding gordo({packet.id})! Stack Trace:\n{e}");
                }

                foreach (var conn in NetworkServer.connections.Values)
                {
                    if (conn.connectionId != nctc.connectionId)
                    {

                        NetworkServer.SRMPSend(packet, conn);
                    }
                }
            }
            public static void HandleAmmoSlot(NetworkConnectionToClient nctc, AmmoEditSlotMessage packet)
            {
                try
                {
                    var ammo = SRNetworkManager.ammos[packet.id];
                    if (ammo.Slots[packet.slot] != null)
                    {
                        ammo.Slots[packet.slot].count += packet.count;
                    }
                    else
                    {
                        ammo.Slots[packet.slot] = new Ammo.Slot(packet.ident, packet.count);
                    }
                }
                catch { }

                foreach (var conn in NetworkServer.connections.Values)
                {
                    if (conn.connectionId != nctc.connectionId)
                    {

                        NetworkServer.SRMPSend(packet, conn);
                    }
                }
            }

            public static void HandleAmmo(NetworkConnectionToClient nctc, AmmoAddMessage packet)
            {
                try
                {
                    var ammo = SRNetworkManager.ammos[packet.id];
                    int slot = -1;
                    for (int i = 0; i < ammo.ammoModel.usableSlots; i++)
                    {
                        if (ammo.Slots[i].count + 1 <= ammo.ammoModel.GetSlotMaxCount(packet.ident, i))
                        {
                            slot = i;
                            continue;
                        }
                    }
                    if (ammo.Slots[slot] != null)
                    {
                        ammo.Slots[slot].count++;
                    }
                    else
                    {
                        ammo.Slots[slot] = new Ammo.Slot(packet.ident, 1);
                    }
                }
                catch { }

                foreach (var conn in NetworkServer.connections.Values)
                {
                    if (conn.connectionId != nctc.connectionId)
                    {

                        NetworkServer.SRMPSend(packet, conn);
                    }
                }
            }
            public static void HandleAmmoReverse(NetworkConnectionToClient nctc, AmmoRemoveMessage packet)
            {
                try
                {
                    Ammo ammo = NetworkAmmo.all[packet.id];
                    if (ammo.Slots[packet.index] != null)
                    {
                        if (ammo.Slots[packet.index].count <= packet.count)
                        {
                            ammo.Slots[packet.index] = null;
                        }
                        else
                            ammo.Slots[packet.index].count -= packet.count;
                    }
                }
                catch { }

                foreach (var conn in NetworkServer.connections.Values)
                {
                    if (conn.connectionId != nctc.connectionId)
                    {

                        NetworkServer.SRMPSend(packet, conn);
                    }
                }
            }
            public static void HandleMap(NetworkConnectionToClient nctc, MapUnlockMessage packet)
            {
                SceneContext.Instance.PlayerState.model.unlockedZoneMaps.Add(packet.id);



                foreach (var conn in NetworkServer.connections.Values)
                {
                    if (conn.connectionId != nctc.connectionId)
                    {

                        NetworkServer.SRMPSend(packet, conn);
                    }
                }
            }
            public static void HandleActorHold(NetworkConnectionToClient nctc, ActorChangeHeldOwnerMessage packet)
            {
                try
                {
                    var actor = SRNetworkManager.actors[packet.id];
                    SceneContext.Instance.player.GetComponentInChildren<WeaponVacuum>().ClearVac();
                }
                catch (Exception e)
                {
                    if (SRMLConfig.SHOW_SRMP_ERRORS)
                        SRMP.Log($"Exception in handling held actor({packet.id})! Stack Trace:\n{e}");
                }


                foreach (var conn in NetworkServer.connections.Values)
                {
                    if (conn.connectionId != nctc.connectionId)
                    {

                        NetworkServer.SRMPSend(packet, conn);
                    }
                }
            }
            #endregion
        }
        public class Client
        {
            #region CLIENT
            internal static void Start(bool host)
            {
                NetworkClient.RegisterHandler(new Action<SetMoneyMessage>(HandleMoneyChange));
                NetworkClient.RegisterHandler(new Action<PlayerJoinMessage>(HandlePlayerJoin));
                NetworkClient.RegisterHandler(new Action<PlayerUpdateMessage>(HandlePlayer));
                NetworkClient.RegisterHandler(new Action<TimeSyncMessage>(HandleTime));
                NetworkClient.RegisterHandler(new Action<ActorSpawnMessage>(HandleActorSpawn));
                NetworkClient.RegisterHandler(new Action<ActorUpdateMessage>(HandleActor));
                NetworkClient.RegisterHandler(new Action<ActorDestroyGlobalMessage>(HandleDestroyActor));
                NetworkClient.RegisterHandler(new Action<ActorUpdateOwnerMessage>(HandleActorOwner));
                NetworkClient.RegisterHandler(new Action<LandPlotMessage>(HandleLandPlot));
                NetworkClient.RegisterHandler(new Action<GordoBurstMessage>(HandleGordoBurst));
                NetworkClient.RegisterHandler(new Action<GordoEatMessage>(HandleGordoEat));
                NetworkClient.RegisterHandler(new Action<PediaMessage>(HandlePedia));
                NetworkClient.RegisterHandler(new Action<LoadMessage>(HandleSave));
                NetworkClient.RegisterHandler(new Action<AmmoAddMessage>(HandleAmmo));
                NetworkClient.RegisterHandler(new Action<AmmoEditSlotMessage>(HandleAmmoSlot));
                NetworkClient.RegisterHandler(new Action<AmmoRemoveMessage>(HandleAmmoReverse));
                NetworkClient.RegisterHandler(new Action<MapUnlockMessage>(HandleMap));
                NetworkClient.RegisterHandler(new Action<DoorOpenMessage>(HandleDoor));
                NetworkClient.RegisterHandler(new Action<SetKeysMessage>(HandleKeysChange));
                NetworkClient.RegisterHandler(new Action<ResourceStateMessage>(HandleResourceState));
                NetworkClient.RegisterHandler(new Action<GardenPlantMessage>(HandleGarden));
                NetworkClient.RegisterHandler(new Action<ActorChangeHeldOwnerMessage>(HandleActorHold));
                NetworkClient.RegisterHandler(new Action<PlayerLeaveMessage>(HandlePlayerLeave));
            }
            public static void HandleMoneyChange(SetMoneyMessage packet)
            {
                SceneContext.Instance.PlayerState.model.currency = packet.newMoney;
            }
            public static void HandleKeysChange(SetKeysMessage packet)
            {
                SceneContext.Instance.PlayerState.model.keys = packet.newMoney;
            }
            public static void HandleSave(LoadMessage save)
            {
                SRNetworkManager.latestSaveJoined = save;
                SceneManager.LoadScene("worldGenerated");
            }

            public static void HandleGarden(GardenPlantMessage packet)
            {
                try
                {
                    var plot = SceneContext.Instance.GameModel.landPlots[packet.id].gameObj;

                    var lp = plot.transform.GetChild(0).GetComponent<LandPlot>();
                    var g = plot.transform.GetComponentInChildren<GardenCatcher>();

                    if (packet.ident != Identifiable.Id.NONE)
                    {
                        lp.gameObject.AddComponent<HandledDummy>();
                        
                        if (g.CanAccept(packet.ident))
                            g.Plant(packet.ident, false);

                        lp.gameObject.RemoveComponent<HandledDummy>();
                    }
                    else
                    {

                        lp.gameObject.AddComponent<HandledDummy>();

                        lp.DestroyAttached();

                        UnityEngine.Object.Destroy(lp.GetComponent<HandledDummy>());

                    }
                }
                catch (Exception e)
                {
                    if (SRMLConfig.SHOW_SRMP_ERRORS)
                        SRMP.Log($"Exception in handling garden({packet.id})! Stack Trace:\n{e}");
                }
            }

            public static void HandleResourceState(ResourceStateMessage packet)
            {
                try
                {
                    var res = SRNetworkManager.actors[packet.id].GetComponent<ResourceCycle>();
                    Rigidbody rigidbody = res.body;

                    switch (packet.state)
                    {
                        case ResourceCycle.State.ROTTEN:
                            if (res.model.state == ResourceCycle.State.ROTTEN) break;
                            res.Rot();
                            res.SetRotten(true);
                            break;
                        case ResourceCycle.State.RIPE:
                            if (res.model.state == ResourceCycle.State.RIPE) break;
                            res.Ripen();
                            if (res.vacuumableWhenRipe)
                            {
                                res.vacuumable.enabled = true;
                            }

                            if (res.gameObject.transform.localScale.x < res.defaultScale.x * 0.33f)
                            {
                                res.gameObject.transform.localScale = res.defaultScale * 0.33f;
                            }

                            TweenUtil.ScaleTo(res.gameObject, res.defaultScale, 4f);
                            break;
                        case ResourceCycle.State.UNRIPE:
                            if (res.model.state == ResourceCycle.State.UNRIPE) break;
                            res.model.state = ResourceCycle.State.UNRIPE;
                            res.transform.localScale = res.defaultScale * 0.33f;
                            break;
                        case ResourceCycle.State.EDIBLE:
                            if (res.model.state == ResourceCycle.State.EDIBLE) break;
                            res.MakeEdible();
                            res.additionalRipenessDelegate = null;
                            rigidbody.isKinematic = false;
                            if (res.preparingToRelease)
                            {
                                res.preparingToRelease = false;
                                res.releaseAt = 0f;
                                res.toShake.localPosition = res.toShakeDefaultPos;
                                if (res.releaseCue != null)
                                {
                                    SECTR_PointSource component = res.GetComponent<SECTR_PointSource>();
                                    component.Cue = res.releaseCue;
                                    component.Play();
                                }
                            }
                            break;
                    }

                    res.model.progressTime = double.MaxValue;

                }
                catch (Exception e)
                {
                    if (SRMLConfig.SHOW_SRMP_ERRORS)
                        SRMP.Log($"Exception in handling state for resource({packet.id})! Stack Trace:\n{e}");
                }
            }
            public static void HandlePlayerJoin(PlayerJoinMessage packet)
            {
                try
                {
                    if (packet.local)
                    {
                        var localPlayer = SceneContext.Instance.player.AddComponent<NetworkPlayer>();
                        localPlayer.id = packet.id;
                        SRNetworkManager.playerID = localPlayer.id;
                    }
                    else
                    {
                        var player = UnityEngine.Object.Instantiate(MultiplayerManager.Instance.onlinePlayerPrefab);
                        player.name = $"Player{packet.id}";
                        var netPlayer = player.GetComponent<NetworkPlayer>();
                        SRNetworkManager.players.Add(packet.id, netPlayer);
                        netPlayer.id = packet.id;
                        player.SetActive(true);
                        UnityEngine.Object.DontDestroyOnLoad(player);
                        var marker = UnityEngine.Object.Instantiate(Map.Instance.mapUI.transform.GetComponentInChildren<PlayerMapMarker>().gameObject);
                        SRNetworkManager.playerToMarkerDict.Add(netPlayer, marker.GetComponent<PlayerMapMarker>());
                        TeleportCommand.playerLookup.Add(TeleportCommand.playerLookup.Count, packet.id);
                    }
                }
                catch { } // Some reason it does happen.
            }
            public static void HandlePlayerLeave(PlayerLeaveMessage packet)
            {
                SRNetworkManager.players[packet.id].gameObject.AddComponent<HandledDummy>();
                SRNetworkManager.players[packet.id].gameObject.Destroy();
                SRNetworkManager.players.Remove(packet.id);
            }
            public static void HandlePlayer(PlayerUpdateMessage packet)
            {
                try
                {
                    var player = SRNetworkManager.players[packet.id];

                    player.GetComponent<TransformSmoother>().nextPos = packet.pos;
                    player.GetComponent<TransformSmoother>().nextRot = packet.rot.eulerAngles;
                }
                catch (Exception e)
                {
                    if (SRMLConfig.SHOW_SRMP_ERRORS)
                        SRMP.Log($"Exception handling player({packet.id})! Stack trace: \n{e}");
                }

            }
            public static void HandleTime(TimeSyncMessage packet)
            {
                try
                {
                    SceneContext.Instance.TimeDirector.worldModel.worldTime = packet.time;
                }
                catch { }
            }
            public static void HandleDestroyActor(ActorDestroyGlobalMessage packet)
            {
                try
                {
                    UnityEngine.Object.Destroy(SRNetworkManager.actors[packet.id].gameObject);
                    SRNetworkManager.actors.Remove(packet.id);
                }
                catch (Exception e)
                {
                    if (SRMLConfig.SHOW_SRMP_ERRORS)
                        SRMP.Log($"Exception in handling actor({packet.id})! Stack Trace:\n{e}");
                }
            }
            public static void HandleActorSpawn(ActorSpawnMessage packet)
            {
                try
                {
                    Quaternion quat = Quaternion.Euler(packet.rotation.x, packet.rotation.y, packet.rotation.z);
                    var identObj = GameContext.Instance.LookupDirector.identifiablePrefabDict[packet.ident]; 
                    if (identObj.GetComponent<NetworkActor>() == null)
                        identObj.AddComponent<NetworkActor>();
                    if (identObj.GetComponent<NetworkActorOwnerToggle>() == null)
                        identObj.AddComponent<NetworkActorOwnerToggle>();
                    if (identObj.GetComponent<TransformSmoother>() == null)
                        identObj.AddComponent<TransformSmoother>();
                    var obj = SceneContext.Instance.GameModel.InstantiateActor(packet.id, identObj, packet.region, packet.position, quat, false, false);
                    obj.GetComponent<NetworkActor>().enabled = false;
                    UnityEngine.Object.Destroy(identObj.GetComponent<TransformSmoother>());
                    UnityEngine.Object.Destroy(identObj.GetComponent<NetworkActor>());
                    UnityEngine.Object.Destroy(identObj.GetComponent<NetworkActorOwnerToggle>());
                    SRNetworkManager.actors.Add(packet.id, obj.GetComponent<NetworkActor>());
                    obj.GetComponent<NetworkActor>().trueID = packet.id;
                    obj.GetComponent<NetworkActor>().IsOwned = false;
                    obj.GetComponent<TransformSmoother>().interpolPeriod = .15f;
                }
                catch (Exception e)
                {
                    if (SRMLConfig.SHOW_SRMP_ERRORS)
                        SRMP.Log($"Exception spawning actor({packet.id})! Stack trace: \n{e}");
                }
            }
            public static void HandleActor(ActorUpdateMessage packet)
            {
                try
                {
                    var actor = SRNetworkManager.actors[packet.id];
                    var t = actor.GetComponent<TransformSmoother>();
                    t.nextPos = packet.position;
                    t.nextRot = packet.rotation;
                }
                catch (Exception e)
                {
                    if (SRMLConfig.SHOW_SRMP_ERRORS)
                        SRMP.Log($"Exception in handling actor({packet.id})! Stack Trace:\n{e}");
                }
            }
            public static void HandleActorOwner(ActorUpdateOwnerMessage packet)
            {
                try
                {
                    var actor = SRNetworkManager.actors[packet.id];
                    actor.GetComponent<TransformSmoother>().enabled = true;
                    actor.GetComponent<NetworkActor>().enabled = false;
                    actor.GetComponent<NetworkActor>().IsOwned = false;

                    actor.GetComponent<NetworkActorOwnerToggle>().LoseGrip();

                }
                catch (Exception e)
                {
                    if (SRMLConfig.SHOW_SRMP_ERRORS)
                        SRMP.Log($"Exception in transfering actor({packet.id})! Stack Trace:\n{e}");
                }
            }
            public static void HandleLandPlot(LandPlotMessage packet)
            {
                try
                {
                    var plot = SceneContext.Instance.GameModel.landPlots[packet.id].gameObj;

                    if (packet.messageType == LandplotUpdateType.SET)
                    {
                        plot.AddComponent<HandledDummy>();

                        plot.GetComponent<LandPlotLocation>().Replace(plot.transform.GetChild(0).GetComponent<LandPlot>(), GameContext.Instance.LookupDirector.plotPrefabDict[packet.type]);

                        UnityEngine.Object.Destroy(plot.GetComponent<HandledDummy>());

                        foreach (var silo in plot.GetComponentsInChildren<SiloStorage>())
                        {
                            silo.InitAmmo();
                        }
                    }
                    else
                    {

                        var lp = plot.transform.GetChild(0).GetComponent<LandPlot>();
                        lp.gameObject.AddComponent<HandledDummy>();

                        lp.AddUpgrade(packet.upgrade);

                        UnityEngine.Object.Destroy(lp.GetComponent<HandledDummy>());
                    }
                }
                catch (Exception e)
                {
                    if (SRMLConfig.SHOW_SRMP_ERRORS)
                        SRMP.Log($"Exception in handling landplot({packet.id})! Stack Trace:\n{e}");
                }
            }
            public static void HandleGordoEat(GordoEatMessage packet)
            {
                try
                {
                    SceneContext.Instance.GameModel.gordos[packet.id].gordoEatenCount = packet.count;
                }
                catch (Exception e)
                {
                    if (SRMLConfig.SHOW_SRMP_ERRORS)
                        SRMP.Log($"Exception in feeding gordo({packet.id})! Stack Trace:\n{e}");
                }
            }
            public static void HandlePedia(PediaMessage packet)
            {
                SceneContext.Instance.gameObject.AddComponent<HandledDummy>();
                SceneContext.Instance.PediaDirector.MaybeShowPopup(packet.id);
                UnityEngine.Object.Destroy(SceneContext.Instance.gameObject.GetComponent<HandledDummy>());

            }
            public static void HandleGordoBurst(GordoBurstMessage packet)
            {
                try
                {
                    var gordo = SceneContext.Instance.GameModel.gordos[packet.id].gameObj;
                    gordo.AddComponent<HandledDummy>();
                    gordo.GetComponent<GordoEat>().ImmediateReachedTarget();
                    UnityEngine.Object.Destroy(gordo.GetComponent<HandledDummy>());
                }
                catch (Exception e)
                {
                    if (SRMLConfig.SHOW_SRMP_ERRORS)
                        SRMP.Log($"Exception in feeding gordo({packet.id})! Stack Trace:\n{e}");
                }
            }
            public static void HandleAmmoSlot(AmmoEditSlotMessage packet)
            {
                try
                {
                    Ammo ammo = NetworkAmmo.all[packet.id];
                    if (ammo.Slots[packet.slot] != null)
                    {
                        ammo.Slots[packet.slot].count += packet.count;
                    }
                    else
                    {
                        ammo.Slots[packet.slot] = new Ammo.Slot(packet.ident, packet.count);
                    }
                }
                catch
                {
                    if (SRMLConfig.SHOW_SRMP_ERRORS)
                        SRMP.Log($"Error adding to ammo slot({packet.id}_{packet.slot})\n{StackTraceUtility.ExtractStackTrace()}");
                }
            }
            public static void HandleAmmo(AmmoAddMessage packet)
            {
                try
                {
                    Ammo ammo = NetworkAmmo.all[packet.id];
                    int slot = -1;
                    for (int i = 0; i < ammo.ammoModel.usableSlots; i++)
                    {
                        if (ammo.Slots[i].count + 1 <= ammo.ammoModel.GetSlotMaxCount(packet.ident, i))
                        {
                            slot = i;
                            continue;
                        }
                    }
                    if (ammo.Slots[slot] != null)
                    {
                        ammo.Slots[slot].count++;
                    }
                    else
                    {
                        ammo.Slots[slot] = new Ammo.Slot(packet.ident, 1);
                    }
                }
                catch
                {
                    if (SRMLConfig.SHOW_SRMP_ERRORS)
                        SRMP.Log($"Error adding to ammo slot({packet.id})\n{StackTraceUtility.ExtractStackTrace()}");
                }
            }

            public static void HandleAmmoReverse(AmmoRemoveMessage packet)
            {
                SRMP.Log("recieve");

                try
                {
                    Ammo ammo = NetworkAmmo.all[packet.id];
                    if (ammo.Slots[packet.index] != null)
                    {
                        if (ammo.Slots[packet.index].count <= packet.count)
                        {
                            ammo.Slots[packet.index] = null;
                        }
                        else
                            ammo.Slots[packet.index].count -= packet.count;
                    }
                }
                catch
                {
                    if (SRMLConfig.SHOW_SRMP_ERRORS)
                        SRMP.Log($"Error taking from ammo slot({packet.id}_{packet.index})\n{StackTraceUtility.ExtractStackTrace()}");
                }
            }
            public static void HandleMap(MapUnlockMessage packet)
            {
                SceneContext.Instance.PlayerState.model.unlockedZoneMaps.Add(packet.id);
            }


            public static void HandleDoor(DoorOpenMessage packet)
            {
                SceneContext.Instance.GameModel.doors[packet.id].gameObj.GetComponent<AccessDoor>().CurrState = AccessDoor.State.OPEN;
            }


            public static void HandleActorHold(ActorChangeHeldOwnerMessage packet)
            {
                try
                {
                    var actor = SRNetworkManager.actors[packet.id];
                    SceneContext.Instance.player.GetComponentInChildren<WeaponVacuum>().ClearVac();
                }
                catch (Exception e)
                {
                    if (SRMLConfig.SHOW_SRMP_ERRORS)
                        SRMP.Log($"Exception in handling held actor({packet.id})! Stack Trace:\n{e}");
                }
            }
            #endregion
        }
    }
}
