using Il2CppMonomiPark.SlimeRancher.Analytics.Event;
using Il2CppMonomiPark.SlimeRancher.Player.PlayerItems;
using Il2CppMonomiPark.SlimeRancher.UI.Map;
using Il2CppMonomiPark.World;
using UnityEngine;
using UnityEngine.SceneManagement;


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
                    var res = actors[packet.id].GetComponent<ResourceCycle>();
                    Rigidbody rigidbody = res._body;

                    switch (packet.state)
                    {
                        case ResourceCycle.State.ROTTEN:
                            if (res._model.state == ResourceCycle.State.ROTTEN) break;
                            res.Rot();
                            res.SetRotten(true);
                            break;
                        case ResourceCycle.State.RIPE:
                            if (res._model.state == ResourceCycle.State.RIPE) break;
                            res.Ripen();
                            if (res.VacuumableWhenRipe)
                            {
                                res._vacuumable.enabled = true;
                            }

                            if (res.gameObject.transform.localScale.x < res._defaultScale.x * 0.33f)
                            {
                                res.gameObject.transform.localScale = res._defaultScale * 0.33f;
                            }

                            TweenUtil.ScaleTo(res.gameObject, res._defaultScale, 4f);
                            break;
                        case ResourceCycle.State.UNRIPE:
                            if (res._model.state == ResourceCycle.State.UNRIPE) break;
                            res._model.state = ResourceCycle.State.UNRIPE;
                            res.transform.localScale = res._defaultScale * 0.33f;
                            break;
                        case ResourceCycle.State.EDIBLE:
                            if (res._model.state == ResourceCycle.State.EDIBLE) break;
                            res.MakeEdible();
                            res._additionalRipenessDelegate = null;
                            rigidbody.isKinematic = false;
                            if (res._preparingToRelease)
                            {
                                res._preparingToRelease = false;
                                res._releaseAt = 0f;
                                res.ToShake.localPosition = res._toShakeDefaultPos;
                                if (res.ReleaseCue != null)
                                {
                                    SECTR_PointSource component = res.GetComponent<SECTR_PointSource>();
                                    component.Cue = res.ReleaseCue;
                                    component.Play();
                                }
                            }
                            break;
                    }
                }
                catch (Exception e)
                {
                    if (SHOW_ERRORS)
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
                if (!savedGame.sharedMoney)
                {
                    savedGame.savedPlayers.playerList[clientToGuid[nctc.connectionId]].money = packet.newMoney;
                    return;
                }
                SceneContext.Instance.PlayerState._model.currency = packet.newMoney;

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
            }
            public static void HandlePlayerJoin(NetworkConnectionToClient nctc, PlayerJoinMessage packet)
            {
                // Do nothing, everything is already handled anyways.
            }
            public static void HandlePlayerLeave(NetworkConnectionToClient nctc, PlayerLeaveMessage packet)
            {
                // Packet should only be S2C
                SRMP.Error("Bug Alert!!! Packet should only be Server To Client, but it was sent from Client To Server.");
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
                    var identObj = identifiableTypes[packet.ident].prefab;
                    if (identObj.GetComponent<NetworkActor>() == null)
                        identObj.AddComponent<NetworkActor>();
                    if (identObj.GetComponent<NetworkActorOwnerToggle>() == null)
                        identObj.AddComponent<NetworkActorOwnerToggle>();
                    if (identObj.GetComponent<TransformSmoother>() == null)
                        identObj.AddComponent<TransformSmoother>();
                    var obj = InstantiateActor(identObj, SystemContext.Instance.SceneLoader._currentSceneGroup, packet.position, quat, false);
                    identObj.RemoveComponent<NetworkActor>();
                    identObj.RemoveComponent<NetworkActorOwnerToggle>();
                    identObj.RemoveComponent<TransformSmoother>();
                    obj.AddComponent<NetworkResource>();
                    if (!actors.ContainsKey(obj.GetComponent<IdentifiableActor>().GetActorId().Value)) // Most useless if statement ever.
                    {
                        obj.GetComponent<TransformSmoother>().enabled = false;
                        actors.Add(obj.GetComponent<Identifiable>().GetActorId().Value, obj.GetComponent<NetworkActor>());
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
                    if (SHOW_ERRORS)
                        SRMP.Log($"Exception in spawning actor(no id)! Stack Trace:\n{e}");
                }
            }
            public static void HandleClientActor(NetworkConnectionToClient nctc, ActorUpdateClientMessage packet)
            {
                try
                {
                    var actor = actors[packet.id];
                    var t = actor.GetComponent<TransformSmoother>();
                    t.nextPos = packet.position;
                    t.nextRot = packet.rotation;
                }
                catch (Exception e)
                {
                    if (SHOW_ERRORS)
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
                    var actor = actors[packet.id];

                    actor.GetComponent<NetworkActor>().IsOwned = false;
                    actor.GetComponent<TransformSmoother>().enabled = true;
                    actor.GetComponent<NetworkActor>().enabled = false;

                    actor.GetComponent<NetworkActorOwnerToggle>().LoseGrip();
                }
                catch (Exception e)
                {
                    if (SHOW_ERRORS)
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
                    UnityEngine.Object.Destroy(actors[packet.id].gameObject);
                    actors.Remove(packet.id);
                }
                catch (Exception e)
                {
                    if (SHOW_ERRORS)
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
                    var player = players[packet.id];

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

                        plot.GetComponent<LandPlotLocation>().Replace(plot.transform.GetChild(0).GetComponent<LandPlot>(), GameContext.Instance.LookupDirector._plotPrefabDict[packet.type]);

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
                    if (SHOW_ERRORS)
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
                    if (string.IsNullOrEmpty(packet.ident))
                    {
                        // Add handled component.
                        lp.gameObject.AddComponent<HandledDummy>();
                        
                        // Plant
                        if (g.CanAccept(identifiableTypes[packet.ident]))
                            g.Plant(identifiableTypes[packet.ident], false);

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
                    if (SHOW_ERRORS)
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
                    SceneContext.Instance.GameModel.gordos[packet.id].gordoEatCount = packet.count;
                }
                catch (Exception e)
                {
                    if (SHOW_ERRORS)
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
                SceneContext.Instance.PediaDirector.ShowPopupIfUnlocked(pediaEntries[packet.id]);
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
                    if (SHOW_ERRORS)
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
                    var ammo = ammos[packet.id];

                    if (!ammo.Slots[packet.slot]._id.name.Equals(packet.id))
                    {
                        ammo.Slots[packet.slot]._id = identifiableTypes[packet.ident];
                    }
                    ammo.Slots[packet.slot]._count += packet.count;

                    
                    
                }
                catch { }

                if (packet.id.ToLower().Contains("player")) return;
                
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
                    var ammo = ammos[packet.id];
                    int slot = -1;
                    for (int i = 0; i < ammo._ammoModel.slots.Count; i++)
                    {
                        if (ammo.Slots[i]._count + 1 <= ammo._ammoModel.GetSlotMaxCount(identifiableTypes[packet.ident], i))
                        {
                            slot = i;
                            continue;
                        }
                    }
                    
                    if (!ammo.Slots[slot]._id.name.Equals(packet.id))
                    {
                        ammo.Slots[slot]._id = identifiableTypes[packet.ident];
                    }
                    ammo.Slots[slot]._count++;
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
                    Ammo ammo = ammos[packet.id];
                    if (ammo.Slots[packet.index]._id != null)
                    {
                        if (ammo.Slots[packet.index]._count <= packet.count)
                        {
                            ammo.Slots[packet.index]._id = null;
                        }
                        else
                            ammo.Slots[packet.index]._count -= packet.count;
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
            //
            // TODO: Add map handling. look into disabling the map fog game objects.
            //
            public static void HandleMap(NetworkConnectionToClient nctc, MapUnlockMessage packet)
            {
                // SceneContext.Instance.PlayerState._model.unlockedZoneMaps.Add(packet.id);



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
                    var actor = actors[packet.id];
                    SceneContext.Instance.player.GetComponentInChildren<VacuumItem>().ClearVac();
                }
                catch (Exception e)
                {
                    if (SHOW_ERRORS)
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
                NetworkClient.RegisterHandler(new Action<ResourceStateMessage>(HandleResourceState));
                NetworkClient.RegisterHandler(new Action<GardenPlantMessage>(HandleGarden));
                NetworkClient.RegisterHandler(new Action<ActorChangeHeldOwnerMessage>(HandleActorHold));
                NetworkClient.RegisterHandler(new Action<PlayerLeaveMessage>(HandlePlayerLeave));
            }
            public static void HandleMoneyChange(SetMoneyMessage packet)
            {
                SceneContext.Instance.PlayerState._model.currency = packet.newMoney;
            }
           
            public static void HandleSave(LoadMessage save)
            {
                latestSaveJoined = save;
                SceneManager.LoadScene("worldGenerated");
            }

            public static void HandleGarden(GardenPlantMessage packet)
            {
                try
                {
                    var plot = SceneContext.Instance.GameModel.landPlots[packet.id].gameObj;

                    var lp = plot.transform.GetChild(0).GetComponent<LandPlot>();
                    var g = plot.transform.GetComponentInChildren<GardenCatcher>();

                    if (packet.ident != "")
                    {
                        lp.gameObject.AddComponent<HandledDummy>();
                        
                        if (g.CanAccept(identifiableTypes[packet.ident]))
                            g.Plant(identifiableTypes[packet.ident], false);

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
                    if (SHOW_ERRORS)
                        SRMP.Log($"Exception in handling garden({packet.id})! Stack Trace:\n{e}");
                }
            }

            public static void HandleResourceState(ResourceStateMessage packet)
            {
                try
                {
                    var res = actors[packet.id].GetComponent<ResourceCycle>();
                    Rigidbody rigidbody = res._body;

                    switch (packet.state)
                    {
                        case ResourceCycle.State.ROTTEN:
                            if (res._model.state == ResourceCycle.State.ROTTEN) break;
                            res.Rot();
                            res.SetRotten(true);
                            break;
                        case ResourceCycle.State.RIPE:
                            if (res._model.state == ResourceCycle.State.RIPE) break;
                            res.Ripen();
                            if (res.VacuumableWhenRipe)
                            {
                                res._vacuumable.enabled = true;
                            }

                            if (res.gameObject.transform.localScale.x < res._defaultScale.x * 0.33f)
                            {
                                res.gameObject.transform.localScale = res._defaultScale * 0.33f;
                            }

                            TweenUtil.ScaleTo(res.gameObject, res._defaultScale, 4f);
                            break;
                        case ResourceCycle.State.UNRIPE:
                            if (res._model.state == ResourceCycle.State.UNRIPE) break;
                            res._model.state = ResourceCycle.State.UNRIPE;
                            res.transform.localScale = res._defaultScale * 0.33f;
                            break;
                        case ResourceCycle.State.EDIBLE:
                            if (res._model.state == ResourceCycle.State.EDIBLE) break;
                            res.MakeEdible();
                            res._additionalRipenessDelegate = null;
                            rigidbody.isKinematic = false;
                            if (res._preparingToRelease)
                            {
                                res._preparingToRelease = false;
                                res._releaseAt = 0f;
                                res.ToShake.localPosition = res._toShakeDefaultPos;
                                if (res.ReleaseCue != null)
                                {
                                    SECTR_PointSource component = res.GetComponent<SECTR_PointSource>();
                                    component.Cue = res.ReleaseCue;
                                    component.Play();
                                }
                            }
                            break;
                    }

                    res._model.progressTime = double.MaxValue;

                }
                catch (Exception e)
                {
                    if (SHOW_ERRORS)
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
                        currentPlayerID = localPlayer.id;
                    }
                    else
                    {
                        var player = UnityEngine.Object.Instantiate(MultiplayerManager.Instance.onlinePlayerPrefab);
                        player.name = $"Player{packet.id}";
                        var netPlayer = player.GetComponent<NetworkPlayer>();
                        players.Add(packet.id, netPlayer);
                        netPlayer.id = packet.id;
                        player.SetActive(true);
                        UnityEngine.Object.DontDestroyOnLoad(player);
                    }
                }
                catch { } // Some reason it does happen.
            }
            public static void HandlePlayerLeave(PlayerLeaveMessage packet)
            {
                players[packet.id].gameObject.AddComponent<HandledDummy>();
                players[packet.id].gameObject.RemoveComponent<HandledDummy>();
                players.Remove(packet.id);
            }
            public static void HandlePlayer(PlayerUpdateMessage packet)
            {
                try
                {
                    var player = players[packet.id];

                    player.GetComponent<TransformSmoother>().nextPos = packet.pos;
                    player.GetComponent<TransformSmoother>().nextRot = packet.rot.eulerAngles;
                }
                catch (Exception e)
                {
                    if (SHOW_ERRORS)
                        SRMP.Log($"Exception handling player({packet.id})! Stack trace: \n{e}");
                }

            }
            public static void HandleTime(TimeSyncMessage packet)
            {
                try
                {
                    SceneContext.Instance.TimeDirector._worldModel.worldTime = packet.time;
                }
                catch { }
            }
            public static void HandleDestroyActor(ActorDestroyGlobalMessage packet)
            {
                try
                {
                    UnityEngine.Object.Destroy(actors[packet.id].gameObject);
                    actors.Remove(packet.id);
                }
                catch (Exception e)
                {
                    if (SHOW_ERRORS)
                        SRMP.Log($"Exception in handling actor({packet.id})! Stack Trace:\n{e}");
                }
            }
            public static void HandleActorSpawn(ActorSpawnMessage packet)
            {
                try
                {
                    Quaternion quat = Quaternion.Euler(packet.rotation.x, packet.rotation.y, packet.rotation.z);
                    var identObj = identifiableTypes[packet.ident].prefab; 
                    if (identObj.GetComponent<NetworkActor>() == null)
                        identObj.AddComponent<NetworkActor>();
                    if (identObj.GetComponent<NetworkActorOwnerToggle>() == null)
                        identObj.AddComponent<NetworkActorOwnerToggle>();
                    if (identObj.GetComponent<TransformSmoother>() == null)
                        identObj.AddComponent<TransformSmoother>();
                    var obj = InstantiateActor(identObj, SystemContext.Instance.SceneLoader._currentSceneGroup, packet.position, quat, false);
                    obj.GetComponent<NetworkActor>().enabled = false;
                    UnityEngine.Object.Destroy(identObj.GetComponent<TransformSmoother>());
                    UnityEngine.Object.Destroy(identObj.GetComponent<NetworkActor>());
                    UnityEngine.Object.Destroy(identObj.GetComponent<NetworkActorOwnerToggle>());
                    actors.Add(packet.id, obj.GetComponent<NetworkActor>());
                    obj.GetComponent<NetworkActor>().trueID = packet.id;
                    obj.GetComponent<NetworkActor>().IsOwned = false;
                    obj.GetComponent<TransformSmoother>().interpolPeriod = .15f;
                }
                catch (Exception e)
                {
                    if (SHOW_ERRORS)
                        SRMP.Log($"Exception spawning actor({packet.id})! Stack trace: \n{e}");
                }
            }
            public static void HandleActor(ActorUpdateMessage packet)
            {
                try
                {
                    var actor = actors[packet.id];
                    var t = actor.GetComponent<TransformSmoother>();
                    t.nextPos = packet.position;
                    t.nextRot = packet.rotation;
                }
                catch (Exception e)
                {
                    if (SHOW_ERRORS)
                        SRMP.Log($"Exception in handling actor({packet.id})! Stack Trace:\n{e}");
                }
            }
            public static void HandleActorOwner(ActorUpdateOwnerMessage packet)
            {
                try
                {
                    var actor = actors[packet.id];
                    actor.GetComponent<TransformSmoother>().enabled = true;
                    actor.GetComponent<NetworkActor>().enabled = false;
                    actor.GetComponent<NetworkActor>().IsOwned = false;

                    actor.GetComponent<NetworkActorOwnerToggle>().LoseGrip();

                }
                catch (Exception e)
                {
                    if (SHOW_ERRORS)
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

                        plot.GetComponent<LandPlotLocation>().Replace(plot.transform.GetChild(0).GetComponent<LandPlot>(), GameContext.Instance.LookupDirector._plotPrefabDict[packet.type]);

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
                    if (SHOW_ERRORS)
                        SRMP.Log($"Exception in handling landplot({packet.id})! Stack Trace:\n{e}");
                }
            }
            public static void HandleGordoEat(GordoEatMessage packet)
            {
                try
                {
                    SceneContext.Instance.GameModel.gordos[packet.id].gordoEatCount = packet.count;
                }
                catch (Exception e)
                {
                    if (SHOW_ERRORS)
                        SRMP.Log($"Exception in feeding gordo({packet.id})! Stack Trace:\n{e}");
                }
            }
            public static void HandlePedia(PediaMessage packet)
            {
                SceneContext.Instance.gameObject.AddComponent<HandledDummy>();
                SceneContext.Instance.PediaDirector.ShowPedia(pediaEntries[packet.id], PediaOpenedAnalyticsEvent.Opener.HOTKEY_WITH_POPUP);
                SceneContext.Instance.gameObject.RemoveComponent<HandledDummy>();
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
                    if (SHOW_ERRORS)
                        SRMP.Log($"Exception in feeding gordo({packet.id})! Stack Trace:\n{e}");
                }
            }
            public static void HandleAmmoSlot(AmmoEditSlotMessage packet)
            {
                try
                {
                    var ammo = ammos[packet.id];

                    if (!ammo.Slots[packet.slot]._id.name.Equals(packet.id))
                    {
                        ammo.Slots[packet.slot]._id = identifiableTypes[packet.ident];
                    }
                    ammo.Slots[packet.slot]._count += packet.count;
                    
                }
                catch
                {
                    if (SHOW_ERRORS)
                        SRMP.Log($"Error adding to ammo slot({packet.id}_{packet.slot})\n{StackTraceUtility.ExtractStackTrace()}");
                }
            }
            public static void HandleAmmo(AmmoAddMessage packet)
            {
                try
                {
                    Ammo ammo = NetworkAmmo.all[packet.id];
                    int slot = -1;
                    for (int i = 0; i < ammo._ammoModel.slots.Count; i++)
                    {
                        if (ammo.Slots[i]._count + 1 <= ammo._ammoModel.GetSlotMaxCount(identifiableTypes[packet.ident], i))
                        {
                            slot = i;
                            continue;
                        }
                    }
                    if (!ammo.Slots[slot]._id.name.Equals(packet.id))
                    {
                        ammo.Slots[slot]._id = identifiableTypes[packet.ident];
                    }
                    ammo.Slots[slot]._count++;


                }
                catch
                {
                    if (SHOW_ERRORS)
                        SRMP.Log($"Error adding to ammo slot({packet.id})\n{StackTraceUtility.ExtractStackTrace()}");
                }
            }

            public static void HandleAmmoReverse(AmmoRemoveMessage packet)
            {
                try
                {
                    Ammo ammo = NetworkAmmo.all[packet.id];
                    if (ammo.Slots[packet.index] != null)
                    {
                        if (ammo.Slots[packet.index]._count <= packet.count)
                        {
                            ammo.Slots[packet.index]._id = null;
                            ammo.Slots[packet.index]._count = 0;
                            return;
                        }
                        ammo.Slots[packet.index]._count -= packet.count;


                    }
                }
                catch
                {
                    if (SHOW_ERRORS)
                        SRMP.Log($"Error taking from ammo slot({packet.id}_{packet.index})\n{StackTraceUtility.ExtractStackTrace()}");
                }
            }
            public static void HandleMap(MapUnlockMessage packet)
            {
                // SceneContext.Instance.eventDirector.RaiseEvent();
            }


            public static void HandleDoor(DoorOpenMessage packet)
            {
                SceneContext.Instance.GameModel.doors[packet.id].gameObj.GetComponent<AccessDoor>().CurrState = AccessDoor.State.OPEN;
            }


            public static void HandleActorHold(ActorChangeHeldOwnerMessage packet)
            {
                try
                {
                    var actor = actors[packet.id];
                    SceneContext.Instance.PlayerState.VacuumItem.ClearVac();
                }
                catch (Exception e)
                {
                    if (SHOW_ERRORS)
                        SRMP.Log($"Exception in handling held actor({packet.id})! Stack Trace:\n{e}");
                }
            }
            #endregion
        }
    }
}
