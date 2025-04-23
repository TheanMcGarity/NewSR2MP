using Il2CppMonomiPark.SlimeRancher.Analytics.Event;
using Il2CppMonomiPark.SlimeRancher.DataModel;
using Il2CppMonomiPark.SlimeRancher.Event;
using Il2CppMonomiPark.SlimeRancher.Map;
using Il2CppMonomiPark.SlimeRancher.Player.PlayerItems;
using Il2CppMonomiPark.SlimeRancher.Slime;
using Il2CppMonomiPark.SlimeRancher.UI.Map;
using Il2CppMonomiPark.SlimeRancher.Weather;
using Il2CppMonomiPark.SlimeRancher.World;
using Il2CppMonomiPark.World;
using Il2CppXGamingRuntime.Interop;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;


namespace NewSR2MP.Networking;

public class NetworkHandler
{
    [MessageHandler((ushort)PacketType.ResourceState)]
    public static void HandleResourceState(Message msg)
    {
        var packet = ICustomMessage.Deserialize<ResourceStateMessage>(msg);
        try
        {
            if (!actors.TryGetValue(packet.id, out var nres)) return;

            var res = nres.GetComponent<ResourceCycle>();
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
            if (ShowErrors)
                SRMP.Log($"Exception in handling state for resource({packet.id})! Stack Trace:\n{e}");
        }


    }

    [MessageHandler((ushort)PacketType.ResourceState)]
    public static void HandleResourceState(ushort client, Message msg)
    {
        var packet = ICustomMessage.Deserialize<ResourceStateMessage>(msg);
        try
        {
            if (!actors.TryGetValue(packet.id, out var nres)) return;

            var res = nres.GetComponent<ResourceCycle>();

            var rigidbody = nres.GetComponent<Rigidbody>();

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
            if (ShowErrors)
                SRMP.Log($"Exception in handling state for resource({packet.id})! Stack Trace:\n{e}");
        }

        ForwardMessage(packet, client);
    }


    [MessageHandler((ushort)PacketType.OpenDoor)]
    public static void HandleDoor(Message msg)
    {
        var packet = ICustomMessage.Deserialize<DoorOpenMessage>(msg);
        sceneContext.GameModel.doors[packet.id].gameObj.GetComponent<AccessDoor>().CurrState =
            AccessDoor.State.OPEN;
    }

    [MessageHandler((ushort)PacketType.OpenDoor)]
    public static void HandleDoor(ushort client, Message msg)
    {
        var packet = ICustomMessage.Deserialize<DoorOpenMessage>(msg);
        sceneContext.GameModel.doors[packet.id].gameObj.GetComponent<AccessDoor>().CurrState =
            AccessDoor.State.OPEN;



    }

    [MessageHandler((ushort)PacketType.SetCurrency)]
    public static void HandleMoneyChange(Message msg)
    {
        var packet = ICustomMessage.Deserialize<SetMoneyMessage>(msg);
        sceneContext.PlayerState._model.currency = packet.newMoney;


    }

    [MessageHandler((ushort)PacketType.SetCurrency)]
    public static void HandleMoneyChange(ushort client, Message msg)
    {
        var packet = ICustomMessage.Deserialize<SetMoneyMessage>(msg);
        sceneContext.PlayerState._model.currency = packet.newMoney;


    }

    [MessageHandler((ushort)PacketType.ActorSpawn)]

    public static void HandleActorSpawn(Message msg)
    {
        var packet = ICustomMessage.Deserialize<ActorSpawnMessage>(msg);
        try
        {
            var sg = sceneGroups[packet.scene];

            if (actors.TryGetValue(packet.id, out var actor))
                actors.Remove(packet.id);

            Quaternion quat = Quaternion.Euler(packet.rotation.x, packet.rotation.y, packet.rotation.z);
            var ident = identifiableTypes[packet.ident];
            var identObj = ident.prefab;


            SRMP.Debug($"[{systemContext._SceneLoader_k__BackingField.CurrentSceneGroup.name} | {sg.name}]");


            identObj.AddComponent<NetworkActor>();
            identObj.AddComponent<NetworkActorOwnerToggle>();
            identObj.AddComponent<TransformSmoother>();

            handlingPacket = true;
            var obj = RegisterActor(new ActorId(packet.id), ident, packet.position, Quaternion.identity, sg);
            handlingPacket = false;


            identObj.RemoveComponent<NetworkActor>();
            identObj.RemoveComponent<NetworkActorOwnerToggle>();
            identObj.RemoveComponent<TransformSmoother>();
            
            if (obj && !ident.TryCast<GadgetDefinition>())
            {
                obj.AddComponent<NetworkResource>(); // Try add resource network component. Will remove if its not a resource so please do not change

                if (!actors.ContainsKey(obj.GetComponent<Identifiable>().GetActorId().Value))
                {
                    actors.Add(obj.GetComponent<Identifiable>().GetActorId().Value,
                        obj.GetComponent<NetworkActor>());
                    obj.GetComponent<TransformSmoother>().interpolPeriod = .15f;
                    if (obj.TryGetComponent<Vacuumable>(out var vac))
                        vac._launched = true;
                }
                else
                {
                    if (!obj.TryGetComponent<Gadget>(out var gadget))
                        obj.GetComponent<TransformSmoother>().enabled = false;
                    obj.GetComponent<TransformSmoother>().interpolPeriod = .15f;
                    if (obj.TryGetComponent<Vacuumable>(out var vac))
                        vac._launched = true;
                }

                obj.GetComponent<NetworkActor>().IsOwned = false;
                obj.GetComponent<TransformSmoother>().nextPos = packet.position;

                actors.TryAdd(packet.id, obj.GetComponent<NetworkActor>());
            }

        }
        catch (Exception e)
        {
            if (ShowErrors)
                SRMP.Log($"Exception in spawning actor(no id)! Stack Trace:\n{e}");
        }
    }

    [MessageHandler((ushort)PacketType.PlayerJoin)]

    public static void HandlePlayerJoin(Message msg)
    {
        var packet = ICustomMessage.Deserialize<PlayerJoinMessage>(msg);

        try
        {
            if (packet.local)
            {
                var localPlayer = sceneContext.player.AddComponent<NetworkPlayer>();
                localPlayer.id = packet.id;
                currentPlayerID = localPlayer.id;
            }
            else
            {
                var player = Object.Instantiate(MultiplayerManager.Instance.onlinePlayerPrefab);
                player.name = $"Player{packet.id}";
                var netPlayer = player.GetComponent<NetworkPlayer>();
                players.Add(packet.id, netPlayer);
                netPlayer.id = packet.id;
                player.SetActive(true);
                Object.DontDestroyOnLoad(player);
            }
        }
        catch
        {
        }
    }

    [MessageHandler((ushort)PacketType.PlayerLeave)]
    public static void HandlePlayerLeave(Message msg)
    {
        var packet = ICustomMessage.Deserialize<PlayerLeaveMessage>(msg);

        var player = players[packet.id];
        players.Remove(packet.id);
        Object.Destroy(player.gameObject);
    }

    [MessageHandler((ushort)PacketType.TimeUpdate)]
    public static void HandleTime(Message msg)
    {
        try
        {
            var packet = ICustomMessage.Deserialize<TimeSyncMessage>(msg);
            sceneContext.GameModel.world.worldTime = packet.time;
        }
        catch
        {
        }
    }

    [MessageHandler((ushort)PacketType.FastForward)]
    public static void HandleClientSleep(ushort client, Message msg)
    {
        var packet = ICustomMessage.Deserialize<SleepMessage>(msg);
        sceneContext.TimeDirector.FastForwardTo(packet.targetTime);

        ForwardMessage(packet, client);
    }

    [MessageHandler((ushort)PacketType.TempClientActorSpawn)]

    public static void HandleClientActorSpawn(ushort client, Message msg)
    {
        var packet = ICustomMessage.Deserialize<ActorSpawnClientMessage>(msg);
        try
        {
            var sg = sceneGroups[packet.scene];
            Quaternion rot = Quaternion.Euler(packet.rotation);
            var ident = identifiableTypes[packet.ident];
            var identObj = ident.prefab;
            if (!identObj.GetComponent<NetworkActor>())
                identObj.AddComponent<NetworkActor>();
            if (!identObj.GetComponent<NetworkActorOwnerToggle>())
                identObj.AddComponent<NetworkActorOwnerToggle>();
            if (!identObj.GetComponent<TransformSmoother>())
                identObj.AddComponent<TransformSmoother>();
            var nextID = NextMultiplayerActorID;
            var obj = RegisterActor(new ActorId(nextID), ident, packet.position, rot, sg);
            identObj.RemoveComponent<NetworkActor>();
            identObj.RemoveComponent<NetworkActorOwnerToggle>();
            identObj.RemoveComponent<TransformSmoother>();
            if (obj && !ident.TryCast<GadgetDefinition>())
            {
                obj.AddComponent<NetworkResource>();
                obj.GetComponent<TransformSmoother>().enabled = false;
                if (obj.TryGetComponent<Rigidbody>(out var rb))
                    rb.velocity = packet.velocity;
                obj.GetComponent<NetworkActor>().startingVel = packet.velocity;
                obj.GetComponent<TransformSmoother>().interpolPeriod = .15f;
                obj.GetComponent<Vacuumable>()._launched = true;
                actors.TryAdd(nextID, obj.GetComponent<NetworkActor>());
            }


            var forwardPacket = new ActorSpawnMessage()
            {
                id = nextID,
                ident = packet.ident,
                position = packet.position,
                rotation = packet.rotation,
                scene = packet.scene,
            };

            long actorID = -1;
            
            if (obj.TryGetComponent<IdentifiableActor>(out var identifiableActor))
                actorID = identifiableActor._model.actorId.Value;
            else if (obj.TryGetComponent<Gadget>(out var gadget))
                actorID = gadget._model.actorId.Value;
            
            var ownPacket = new ActorSetOwnerMessage()
            {
                id = actorID,
                velocity = packet.velocity
            };
            MultiplayerManager.NetworkSend(ownPacket, MultiplayerManager.ServerSendOptions.SendToPlayer(client));
            MultiplayerManager.NetworkSend(forwardPacket);
        }
        catch (Exception e)
        {
            //if (ShowErrors)
            SRMP.Log($"Exception in spawning actor(no id)! Stack Trace:\n{e}");
        }

    }

    [MessageHandler((ushort)PacketType.TempClientActorUpdate)]

    public static void HandleClientActor(ushort client, Message msg)
    {
        var packet = ICustomMessage.Deserialize<ActorUpdateClientMessage>(msg);
        try
        {
            if (!actors.TryGetValue(packet.id, out var actor)) return;

            var t = actor.GetComponent<TransformSmoother>();
            t.nextPos = packet.position;
            t.nextRot = packet.rotation;

            if (actor.TryGetComponent<SlimeEmotions>(out var emotions))
                emotions.SetFromNetwork(packet.slimeEmotions);

            if (actor.TryGetComponent<Rigidbody>(out var rigidbody))
                rigidbody.velocity = packet.velocity;
        }
        catch (Exception e)
        {
            if (ShowErrors)
                SRMP.Log($"Exception in handling actor({packet.id})! Stack Trace:\n{e}");
        }

        ActorUpdateMessage packetS2C = new ActorUpdateMessage()
        {
            id = packet.id,
            position = packet.position,
            rotation = packet.rotation,
        };

        ForwardMessage(packetS2C, client);
    }

    [MessageHandler((ushort)PacketType.ActorBecomeOwner)]

    public static void HandleActorOwner(Message msg)
    {
        var packet = ICustomMessage.Deserialize<ActorUpdateOwnerMessage>(msg);
        try
        {
            if (!actors.TryGetValue(packet.id, out var actor)) return;


            actor.IsOwned = false;
            actor.GetComponent<TransformSmoother>().enabled = true;
            actor.GetComponent<TransformSmoother>().nextPos = actor.transform.position;
            actor.enabled = false;

            actor.GetComponent<NetworkActorOwnerToggle>().LoseGrip();
        }
        catch (Exception e)
        {
            if (ShowErrors)
                SRMP.Log($"Exception in transfering actor({packet.id})! Stack Trace:\n{e}");
        }
    }

    [MessageHandler((ushort)PacketType.ActorDestroy)]

    public static void HandleDestroyActor(Message msg)
    {
        var packet = ICustomMessage.Deserialize<ActorDestroyGlobalMessage>(msg);
        try
        {
            if (actors.TryGetValue(packet.id, out var actor))
            {
                DeregisterActor(new ActorId(packet.id));

                Object.Destroy(actor.gameObject);
                actors.Remove(packet.id);
            }
            else if (gadgets.TryGetValue(packet.id, out var gadget))
            {
                DeregisterActor(new ActorId(packet.id));

                Object.Destroy(gadget.gameObject);
                actors.Remove(packet.id);
            }
            
        }
        catch (Exception e)
        {
            SRMP.Error($"Exception in destroying actor({packet.id})! Stack Trace:\n{e}");
        }
    }

    [MessageHandler((ushort)PacketType.ActorBecomeOwner)]
    public static void HandleActorOwner(ushort client, Message msg)
    {
        var packet = ICustomMessage.Deserialize<ActorUpdateOwnerMessage>(msg);
        try
        {
            if (!actors.TryGetValue(packet.id, out var actor)) return;


            actor.IsOwned = false;
            actor.GetComponent<TransformSmoother>().enabled = true;
            actor.GetComponent<TransformSmoother>().nextPos = actor.transform.position;
            actor.enabled = false;

            actor.GetComponent<NetworkActorOwnerToggle>().LoseGrip();

            MultiplayerManager.NetworkSend(new ActorVelocityMessage
            {
                id = packet.id,
                velocity = actor.GetComponent<Rigidbody>().velocity
            }, MultiplayerManager.ServerSendOptions.SendToPlayer(client));
        }
        catch (Exception e)
        {
            if (ShowErrors)
                SRMP.Log($"Exception in transfering actor({packet.id})! Stack Trace:\n{e}");
        }
        
        ForwardMessage(packet, client);
    }
[MessageHandler((ushort)PacketType.ActorVelocitySet)]
    public static void HandleActorVelocity(ushort client, Message msg)
    {
        var packet = ICustomMessage.Deserialize<ActorVelocityMessage>(msg);
        try
        {
            if (!actors.TryGetValue(packet.id, out var actor)) return;
            
            actor.GetComponent<Rigidbody>().velocity = packet.velocity;
        }
        catch (Exception e)
        {
            if (ShowErrors)
                SRMP.Log($"Exception in setting actor({packet.id}) velocity! Stack Trace:\n{e}");
        }
    }
[MessageHandler((ushort)PacketType.ActorVelocitySet)]
    public static void HandleActorVelocity(Message msg)
    {
        var packet = ICustomMessage.Deserialize<ActorVelocityMessage>(msg);
        try
        {
            if (!actors.TryGetValue(packet.id, out var actor)) return;
            
            actor.GetComponent<Rigidbody>().velocity = packet.velocity;
        }
        catch (Exception e)
        {
            if (ShowErrors)
                SRMP.Log($"Exception in setting actor({packet.id}) velocity! Stack Trace:\n{e}");
        }
    }

    [MessageHandler((ushort)PacketType.ActorDestroy)]
    public static void HandleDestroyActor(ushort client, Message msg)
    {
        var packet = ICustomMessage.Deserialize<ActorDestroyGlobalMessage>(msg);
        try
        {
            if (!actors.TryGetValue(packet.id, out var actor)) return;

            DeregisterActor(new ActorId(packet.id));

            Object.Destroy(actor.gameObject);
            actors.Remove(packet.id);
        }
        catch (Exception e)
        {
            SRMP.Error($"Exception in destroying actor({packet.id})! Stack Trace:\n{e}");
        }

        ForwardMessage(packet, client);
    }

    [MessageHandler((ushort)PacketType.ActorSetOwner)]
    public static void HandleActorSetOwner(Message msg)
    {
        var packet = ICustomMessage.Deserialize<ActorSetOwnerMessage>(msg);
        try
        {
            if (!actors.TryGetValue(packet.id, out var actor)) return;

            actor.GetComponent<NetworkActorOwnerToggle>().OwnActor();
            actor.GetComponent<Rigidbody>().velocity = packet.velocity;
        }
        catch (Exception e)
        {
            if (ShowErrors)
                SRMP.Log($"Exception in transfering actor({packet.id})! Stack Trace:\n{e}");
        }
    }

    [MessageHandler((ushort)PacketType.PlayerUpdate)]

    public static void HandlePlayer(Message msg)
    {
        var packet = ICustomMessage.Deserialize<PlayerUpdateMessage>(msg);

        try
        {
            var player = players[packet.id];

            player.GetComponent<TransformSmoother>().nextPos = packet.pos;
            player.GetComponent<TransformSmoother>().nextRot = packet.rot.eulerAngles;


            var anim = player.GetComponent<Animator>();

            anim.SetFloat("HorizontalMovement", packet.horizontalMovement);
            anim.SetFloat("ForwardMovement", packet.forwardMovement);
            anim.SetFloat("Yaw", packet.yaw);
            anim.SetInteger("AirborneState", packet.airborneState);
            anim.SetBool("Moving", packet.moving);
            anim.SetFloat("HorizontalSpeed", packet.horizontalSpeed);
            anim.SetFloat("ForwardSpeed", packet.forwardSpeed);
        }
        catch
        {
        }
    }

    [MessageHandler((ushort)PacketType.PlayerUpdate)]

    public static void HandlePlayer(ushort client, Message msg)
    {
        var packet = ICustomMessage.Deserialize<PlayerUpdateMessage>(msg);

        try
        {
            var player = players[packet.id];

            savedGame.savedPlayers.playerList[clientToGuid[client]].sceneGroup = packet.scene;

            player.GetComponent<TransformSmoother>().nextPos = packet.pos;
            player.GetComponent<TransformSmoother>().nextRot = packet.rot.eulerAngles;


            var anim = player.GetComponent<Animator>();

            anim.SetFloat("HorizontalMovement", packet.horizontalMovement);
            anim.SetFloat("ForwardMovement", packet.forwardMovement);
            anim.SetFloat("Yaw", packet.yaw);
            anim.SetInteger("AirborneState", packet.airborneState);
            anim.SetBool("Moving", packet.moving);
            anim.SetFloat("HorizontalSpeed", packet.horizontalSpeed);
            anim.SetFloat("ForwardSpeed", packet.forwardSpeed);
        }
        catch
        {
        }

        ForwardMessage(packet, client);
    }

    [MessageHandler((ushort)PacketType.LandPlot)]
    public static void HandleLandPlot(Message msg)
    {
        var packet = ICustomMessage.Deserialize<LandPlotMessage>(msg);


        try
        {
            var plot = sceneContext.GameModel.landPlots[packet.id].gameObj;

            if (packet.messageType == LandplotUpdateType.SET)
            {
                handlingPacket = true;

                plot.GetComponent<LandPlotLocation>().Replace(plot.GetComponentInChildren<LandPlot>(),
                    GameContext.Instance.LookupDirector._plotPrefabDict[packet.type]);

                handlingPacket = false;
            }
            else
            {

                var lp = plot.GetComponentInChildren<LandPlot>();

                handlingPacket = true;

                lp.AddUpgrade(packet.upgrade);

                handlingPacket = false;

            }
        }
        catch (Exception e)
        {
            if (ShowErrors)
                SRMP.Log($"Exception in handling landplot({packet.id})! Stack Trace:\n{e}");
        }
    }

    [MessageHandler((ushort)PacketType.GardenPlant)]

    public static void HandleGarden(Message msg)
    {     
        var packet = ICustomMessage.Deserialize<GardenPlantMessage>(msg);
        
        try
        {
            // get plot from id.
            var plot = sceneContext.GameModel.landPlots[packet.id].gameObj;

            // Get required components
            var lp = plot.transform.GetChild(0).GetComponent<LandPlot>();
            var g = plot.transform.GetComponentInChildren<GardenCatcher>();

            // Check if is destroy (planting NONE)
            if (packet.ident != 9)
            {
                // Add handled component.
                handlingPacket = true;

                // Plant
                if (g.CanAccept(identifiableTypes[packet.ident]))
                    g.Plant(identifiableTypes[packet.ident], false);

                // Remove handled component.
                handlingPacket = false;
            }
            else
            {
                // Add handled component.

                handlingPacket = true;

                // UnPlant.
                lp.DestroyAttached();

                // Remove handled component.
                handlingPacket = false;


            }
        }
        catch (Exception e)
        {
            if (ShowErrors)
                SRMP.Log($"Exception in handling garden({packet.id})! Stack Trace:\n{e}");
        }
    }

    [MessageHandler((ushort)PacketType.LandPlot)]
    public static void HandleLandPlot(ushort client, Message msg)
    {     
        var packet = ICustomMessage.Deserialize<LandPlotMessage>(msg);


        try
        {
            var plot = sceneContext.GameModel.landPlots[packet.id].gameObj;

            if (packet.messageType == LandplotUpdateType.SET)
            {
                handlingPacket = true;

                plot.GetComponent<LandPlotLocation>().Replace(plot.GetComponentInChildren<LandPlot>(),
                    GameContext.Instance.LookupDirector._plotPrefabDict[packet.type]);

                handlingPacket = false;
            }
            else
            {

                var lp = plot.GetComponentInChildren<LandPlot>();
                handlingPacket = true;

                lp.AddUpgrade(packet.upgrade);

                handlingPacket = false;

            }
        }
        catch (Exception e)
        {
            if (ShowErrors)
                SRMP.Log($"Exception in handling landplot({packet.id})! Stack Trace:\n{e}");
        }

        ForwardMessage(packet, client);
    }

    [MessageHandler((ushort)PacketType.GardenPlant)]

    public static void HandleGarden(ushort client, Message msg)
    {
        var packet = ICustomMessage.Deserialize<GardenPlantMessage>(msg);

        try
        {
            // get plot from id.
            var plot = sceneContext.GameModel.landPlots[packet.id].gameObj;

            // Get required components
            var lp = plot.transform.GetChild(0).GetComponent<LandPlot>();
            var g = plot.transform.GetComponentInChildren<GardenCatcher>();

            // Check if is destroy (planting NONE)
            if (packet.ident != 9)
            {
                // Add handled component.
                handlingPacket = true;

                // Plant
                if (g.CanAccept(identifiableTypes[packet.ident]))
                    g.Plant(identifiableTypes[packet.ident], false);

                // Remove handled component.
                handlingPacket = false;
            }
            else
            {
                // Add handled component.

                handlingPacket = true;

                // UnPlant.
                lp.DestroyAttached();

                // Remove handled component.
                handlingPacket = false;


            }
        }
        catch (Exception e)
        {
            if (ShowErrors)
                SRMP.Log($"Exception in handling garden({packet.id})! Stack Trace:\n{e}");
        }

        ForwardMessage(packet, client);
    }

    [MessageHandler((ushort)PacketType.GordoFeed)]
    public static void HandleGordoEat(Message msg)
    {
        var packet = ICustomMessage.Deserialize<GordoEatMessage>(msg);

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

    [MessageHandler((ushort)PacketType.PediaUnlock)]
    public static void HandlePedia(Message msg)
    {
        var packet = ICustomMessage.Deserialize<PediaMessage>(msg);

        handlingPacket = true;
        sceneContext.PediaDirector.Unlock(pediaEntries[packet.id]);
        handlingPacket = false;
    }

    [MessageHandler((ushort)PacketType.GordoExplode)]
    public static void HandleGordoBurst(Message msg)
    {
        var packet = ICustomMessage.Deserialize<GordoBurstMessage>(msg);

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

    [MessageHandler((ushort)PacketType.GordoFeed)]
    public static void HandleGordoEat(ushort client, Message msg)
    {
        var packet = ICustomMessage.Deserialize<GordoEatMessage>(msg);

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
            if (ShowErrors)
                SRMP.Log($"Exception in feeding gordo({packet.id})! Stack Trace:\n{e}");
        }

        ForwardMessage(packet, client);

    }

    [MessageHandler((ushort)PacketType.PediaUnlock)]
    public static void HandlePedia(ushort client, Message msg)
    {
        var packet = ICustomMessage.Deserialize<PediaMessage>(msg);

        handlingPacket = true;
        sceneContext.PediaDirector.Unlock(pediaEntries[packet.id]);
        handlingPacket = false;

        ForwardMessage(packet, client);
    }

    [MessageHandler((ushort)PacketType.GordoExplode)]
    public static void HandleGordoBurst(ushort client, Message msg)
    {
        var packet = ICustomMessage.Deserialize<GordoBurstMessage>(msg);

        try
        {
            var gordo = sceneContext.GameModel.gordos[packet.id].gameObj;
            handlingPacket = true;
            gordo.GetComponent<GordoEat>().ImmediateReachedTarget();
            handlingPacket = false;
        }
        catch (Exception e)
        {
            if (ShowErrors)
                SRMP.Log($"Exception in feeding gordo({packet.id})! Stack Trace:\n{e}");
        }

        ForwardMessage(packet, client);
    }

    [MessageHandler((ushort)PacketType.AmmoEdit)]
    public static void HandleAmmoSlot(Message msg)
    {
        var packet = ICustomMessage.Deserialize<AmmoEditSlotMessage>(msg);

        try
        {
            var ammo = GetNetworkAmmo(packet.id);

            if (!ammo.Slots[packet.slot]._id.name.Equals(identifiableTypes[packet.ident].name))
            {
                ammo.Slots[packet.slot]._id = identifiableTypes[packet.ident];
            }

            ammo.Slots[packet.slot]._count += packet.count;



        }
        catch
        {
        }

    }

    [MessageHandler((ushort)PacketType.AmmoAdd)]
    public static void HandleAmmo(Message msg)
    {
        var packet = ICustomMessage.Deserialize<AmmoAddMessage>(msg);

        try
        {
            var ammo = GetNetworkAmmo(packet.id);
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
        }
    }

    [MessageHandler((ushort)PacketType.AmmoRemove)]
    public static void HandleAmmoReverse(ushort client, Message msg)
    {
        var packet = ICustomMessage.Deserialize<AmmoRemoveMessage>(msg);

        try
        {
            Ammo ammo = GetNetworkAmmo(packet.id);

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
        catch
        {
        }

        ForwardMessage(packet, client);
    }

    [MessageHandler((ushort)PacketType.AmmoEdit)]
    public static void HandleAmmoSlot(ushort client, Message msg)
    {
        var packet = ICustomMessage.Deserialize<AmmoEditSlotMessage>(msg);

        try
        {
            var ammo = GetNetworkAmmo(packet.id);

            if (!ammo.Slots[packet.slot]._id.name.Equals(identifiableTypes[packet.ident].name))
            {
                ammo.Slots[packet.slot]._id = identifiableTypes[packet.ident];
            }

            ammo.Slots[packet.slot]._count += packet.count;



        }
        catch
        {
        }

        ForwardMessage(packet, client);
    }

    [MessageHandler((ushort)PacketType.AmmoAdd)]
    public static void HandleAmmo(ushort client, Message msg)
    {
        var packet = ICustomMessage.Deserialize<AmmoAddMessage>(msg);

        try
        {
            var ammo = GetNetworkAmmo(packet.id);
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
        }

        ForwardMessage(packet, client);
    }

    [MessageHandler((ushort)PacketType.AmmoRemove)]
    public static void HandleAmmoReverse(Message msg)
    {
        var packet = ICustomMessage.Deserialize<AmmoRemoveMessage>(msg);

        try
        {
            Ammo ammo = GetNetworkAmmo(packet.id);

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
        catch
        {
        }
    }

    //
    // TODO: Add map handling. look into disabling the map fog game objects.
    //
    public static void HandleMap(Message msg)
    {
        // sceneContext.PlayerState._model.unlockedZoneMaps.Add(packet.id);
    }

    [MessageHandler((ushort)PacketType.ActorUpdate)]
    public static void HandleActor(Message msg)
    {
        var packet = ICustomMessage.Deserialize<ActorUpdateMessage>(msg);

        try
        {
            if (!actors.TryGetValue(packet.id, out var actor)) return;
            var t = actor.GetComponent<TransformSmoother>();
            t.nextPos = packet.position;
            t.nextRot = packet.rotation;

            if (actor.TryGetComponent<SlimeEmotions>(out var emotions))
                emotions.SetFromNetwork(packet.slimeEmotions);

            if (actor.TryGetComponent<Rigidbody>(out var rigidbody))
                rigidbody.velocity = packet.velocity;
        }
        catch (Exception e)
        {
            if (ShowErrors)
                SRMP.Log($"Exception in handling actor({packet.id})! Stack Trace:\n{e}");
        }


    }

    [MessageHandler((ushort)PacketType.JoinSave)]
    public static void HandleSave(Message msg)
    {
        SRMP.Debug("Starting to read save data!");
        latestSaveJoined = ICustomMessage.Deserialize<LoadMessage>(msg);
        SRMP.Debug("Finished reading save data!");
    }


    [MessageHandler((ushort)PacketType.RequestJoin)]
    public static void HandleClientJoin(ushort client, Message joinInfo)
    {
        var packet = ICustomMessage.Deserialize<ClientUserMessage>(joinInfo);
        MultiplayerManager.server.TryGetClient(client, out var con);
        MultiplayerManager.PlayerJoin(con, packet.guid, packet.name);
    }

    [MessageHandler((ushort)PacketType.NavigationMarkerPlace)]
    public static void HandleNavPlace(ushort client, Message msg)
    {
        var packet = ICustomMessage.Deserialize<PlaceNavMarkerNessage>(msg);


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

        ForwardMessage(packet, client);
    }

    [MessageHandler((ushort)PacketType.NavigationMarkerRemove)]
    public static void HandleNavRemove(ushort client, Message joinInfo)
    {
        handlingNavPacket = true;
        sceneContext.MapDirector.ClearPlayerNavigationMarker();
        handlingNavPacket = false;

        ForwardMessage(new RemoveNavMarkerNessage(), client);
    }

    [MessageHandler((ushort)PacketType.NavigationMarkerPlace)]
    public static void HandleNavPlace(Message msg)
    {
        var packet = ICustomMessage.Deserialize<PlaceNavMarkerNessage>(msg);


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

    [MessageHandler((ushort)PacketType.NavigationMarkerRemove)]
    public static void HandleNavRemove(Message joinInfo)
    {
        handlingNavPacket = true;
        sceneContext.MapDirector.ClearPlayerNavigationMarker();
        handlingNavPacket = false;
    }



    [MessageHandler((ushort)PacketType.WeatherUpdate)]
    public static void HandleWeather(Message msg)
    {
        MelonCoroutines.Start(WeatherHandlingCoroutine(ICustomMessage.Deserialize<WeatherSyncMessage>(msg)));
    }

    [MessageHandler((ushort)PacketType.MarketRefresh)]
    public static void HandleMarketRefresh(Message msg)
    {
        var packet = ICustomMessage.Deserialize<MarketRefreshMessage>(msg);

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

    [MessageHandler((ushort)PacketType.KillAllCommand)]
    public static void HandleKillAllCommand(ushort client, Message msg)
    {
        var packet = ICustomMessage.Deserialize<KillAllCommandMessage>(msg);

        SRMP.Debug("Ran KillAll command!");

        if (packet.actorType == -1)
        {
            foreach (var ident in sceneContext.GameModel.identifiables._entries
                         .Where(x => sceneGroupsReverse[x.value.sceneGroup.name] == packet.sceneGroup).ToList())
            {
                if (ident.value.ident.name != "Player")
                {
                    var model = ident.value.TryCast<ActorModel>();
                    if (model != null)
                    {
                        if (model.transform != null)
                            Object.Destroy(model.transform.gameObject);
                        SceneContext.Instance.GameModel.identifiables.Remove(model.actorId);
                    }
                }
            }
        }
        else
        {

            var type = identifiableTypes[packet.actorType];

            foreach (var ident in sceneContext.GameModel.identifiables._entries
                         .Where(x => sceneGroupsReverse[x.value.sceneGroup.name] == packet.sceneGroup).ToList())
            {
                if (ident.value.ident != type)
                {
                    var model = ident.value.TryCast<ActorModel>();
                    if (model != null)
                    {
                        if (model.transform != null)
                            Object.Destroy(model.transform.gameObject);
                        SceneContext.Instance.GameModel.identifiables.Remove(model.actorId);
                    }
                }
            }

            ForwardMessage(packet, client);
        }
    }

    [MessageHandler((ushort)PacketType.KillAllCommand)]
    public static void HandleKillAllCommand(Message msg)
    {
        var packet = ICustomMessage.Deserialize<KillAllCommandMessage>(msg);

        SRMP.Debug("Ran KillAll command!");

        if (packet.actorType == -1)
        {
            foreach (var ident in sceneContext.GameModel.identifiables._entries
                          .Where(x => sceneGroupsReverse[x.value.sceneGroup.name] == packet.sceneGroup).ToList())
            {
                if (ident.value.ident.name != "Player")
                {
                    var model = ident.value.TryCast<ActorModel>();
                    if (model != null)
                    {
                        if (model.transform != null)
                            Object.Destroy(model.transform.gameObject);
                        SceneContext.Instance.GameModel.identifiables.Remove(model.actorId);
                    }
                }
            }
        }
        else
        {

            var type = identifiableTypes[packet.actorType];

            foreach (var ident in sceneContext.GameModel.identifiables._entries
                         .Where(x => sceneGroupsReverse[x.value.sceneGroup.name] == packet.sceneGroup).ToList())
            {
                if (ident.value.ident != type)
                {
                    var model = ident.value.TryCast<ActorModel>();
                    if (model != null)
                    {
                        if (model.transform != null)
                            Object.Destroy(model.transform.gameObject);
                        SceneContext.Instance.GameModel.identifiables.Remove(model.actorId);
                    }
                }
            }
        }
    }

    [MessageHandler((ushort)PacketType.SwitchModify)]
    public static void HandleSwitchModify(Message msg)
    {
        var packet = ICustomMessage.Deserialize<SwitchModifyMessage>(msg);

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

    [MessageHandler((ushort)PacketType.SwitchModify)]
    public static void HandleSwitchModify(ushort client, Message msg)
    {
        var packet = ICustomMessage.Deserialize<SwitchModifyMessage>(msg);

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

        ForwardMessage(packet, client);
    }

    [MessageHandler((ushort)PacketType.MapUnlock)]
    public static void HandleMapUnlock(Message msg)
    {
        var packet = ICustomMessage.Deserialize<MapUnlockMessage>(msg);

        sceneContext.MapDirector.NotifyZoneUnlocked(GetGameEvent(packet.id), false, 0);

        var eventDirModel = sceneContext.eventDirector._model;
        if (!eventDirModel.table.TryGetValue("fogRevealed", out var table))
        {
            eventDirModel.table.Add("fogRevealed",
                new Il2CppSystem.Collections.Generic.Dictionary<string, EventRecordModel.Entry>());
            table = eventDirModel.table["fogRevealed"];
        }

        table.Add(packet.id, new EventRecordModel.Entry
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

    [MessageHandler((ushort)PacketType.MapUnlock)]
    public static void HandleMapUnlock(ushort client, Message msg)
    {
        var packet = ICustomMessage.Deserialize<MapUnlockMessage>(msg);

        sceneContext.MapDirector.NotifyZoneUnlocked(GetGameEvent(packet.id), false, 0);

        var eventDirModel = sceneContext.eventDirector._model;
        if (!eventDirModel.table.TryGetValue("fogRevealed", out var table))
        {
            eventDirModel.table.Add("fogRevealed",
                new Il2CppSystem.Collections.Generic.Dictionary<string, EventRecordModel.Entry>());
            table = eventDirModel.table["fogRevealed"];
        }

        table.Add(packet.id, new EventRecordModel.Entry
        {
            count = 1,
            createdRealTime = 0,
            createdGameTime = 0,
            dataKey = packet.id,
            eventKey = "fogRevealed",
            updatedRealTime = 0,
            updatedGameTime = 0,
        });

        ForwardMessage(packet, client);
    }
    
    [MessageHandler((ushort)PacketType.RefineryItem)]
    public static void HandleRefineryItem(ushort client, Message msg)
    {
        var packet = ICustomMessage.Deserialize<RefineryItemMessage>(msg);

        handlingPacket = true;
        sceneContext.GadgetDirector._model.SetCount(identifiableTypes[packet.id], packet.count);
        handlingPacket = false;
        
        ForwardMessage(packet, client);
    }
    
    [MessageHandler((ushort)PacketType.RefineryItem)]
    public static void HandleRefineryItem(Message msg)
    {
        var packet = ICustomMessage.Deserialize<RefineryItemMessage>(msg);

        handlingPacket = true;
        sceneContext.GadgetDirector._model.SetCount(identifiableTypes[packet.id], packet.count);
        handlingPacket = false;
    }

    [MessageHandler((ushort)PacketType.PlayerUpgrade)]
    public static void HandlePlayerUpgrade(ushort client, Message msg)
    {
        var packet = ICustomMessage.Deserialize<PlayerUpgradeMessage>(msg);

        sceneContext.PlayerState._model.upgradeModel.upgradeLevels[packet.index]++;
        
        ForwardMessage(packet, client);
    }

    [MessageHandler((ushort)PacketType.PlayerUpgrade)]
    public static void HandlePlayerUpgrade(Message msg)
    {
        var packet = ICustomMessage.Deserialize<PlayerUpgradeMessage>(msg);

        sceneContext.PlayerState._model.upgradeModel.upgradeLevels[packet.index]++;
    }
    
    /// <summary>
    /// Shortcut for forwarding messages.
    /// </summary>
    /// <param name="msg">Message to forward</param>
    /// <param name="from">The client the message came from</param>
    public static void ForwardMessage(ICustomMessage msg, ushort from)
    {
        MultiplayerManager.NetworkSend(msg, MultiplayerManager.ServerSendOptions.SendToAllExcept(from));
    }
}