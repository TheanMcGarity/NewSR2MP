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
using Il2CppSystem.Data.Common;
using Il2CppTMPro;
using Il2CppXGamingRuntime.Interop;
using NewSR2MP.Attributes;
using NewSR2MP.Patches;
using SRMP.Enums;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;


namespace NewSR2MP;

public partial class NetworkHandler
{
    [PacketResponse]
    private static void HandleResourceState(NetPlayerState netPlayer, ResourceStateMessage packet, byte channel)
    {
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
    
    private static void HandleActorSpawn(NetPlayerState netPlayer, ActorSpawnMessage packet, byte channel)
    {
        try
        {
            var sg = sceneGroups[packet.scene];

            if (actors.TryGetValue(packet.id, out var actor))
                actors.Remove(packet.id);

            Quaternion quat = Quaternion.Euler(packet.rotation.x, packet.rotation.y, packet.rotation.z);
            var ident = identifiableTypes[packet.ident];
            var identObj = ident.prefab;


            SRMP.Debug($"[{systemContext._SceneLoader_k__BackingField.CurrentSceneGroup.name} | {sg.name}]");


            

            handlingPacket = true;
            var obj = RegisterActor(new ActorId(packet.id), ident, packet.position, Quaternion.identity, sg);
            handlingPacket = false;
            
            obj.AddComponent<NetworkActor>();
            obj.AddComponent<NetworkActorOwnerToggle>();
            obj.AddComponent<TransformSmoother>();
            
            if (obj.TryGetComponent<NetworkActor>(out var netComp))
                if (!actors.TryAdd(packet.id, netComp))
                    actors[packet.id] = netComp;
            
            if (obj && !ident.TryCast<GadgetDefinition>())
            {
                obj.AddComponent<NetworkResource>(); // Try add resource network component. Will remove if it's not a resource so please do not change

                if (!actors.ContainsKey(obj.GetComponent<Identifiable>().GetActorId().Value))
                {
                    actors.Add(obj.GetComponent<Identifiable>().GetActorId().Value,
                        obj.GetComponent<NetworkActor>());
                    obj.GetComponent<TransformSmoother>().interpolPeriod = ActorTimer;
                    if (obj.TryGetComponent<Vacuumable>(out var vac))
                        vac._launched = true;
                }
                else
                {
                    if (!obj.TryGetComponent<Gadget>(out _))
                        obj.GetComponent<TransformSmoother>().enabled = false;
                    obj.GetComponent<TransformSmoother>().interpolPeriod = ActorTimer;
                    if (obj.TryGetComponent<Vacuumable>(out var vac))
                        vac._launched = true;
                }

                obj.GetComponent<NetworkActor>().IsOwned = false;
                obj.GetComponent<TransformSmoother>().nextPos = packet.position;

                if (obj.TryGetComponent<Rigidbody>(out var rb))
                    rb.velocity = packet.velocity;
            }

        }
        catch (Exception e)
        {
            if (ShowErrors)
                SRMP.Log($"Exception in spawning actor(no id)! Stack Trace:\n{e}");
        }
    }

    
    [PacketResponse]
    private static void HandlePlayerJoin(NetPlayerState netPlayer, PlayerJoinMessage packet, byte channel)
    {

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
                var playerObj = Object.Instantiate(MultiplayerManager.Instance.onlinePlayerPrefab);
                playerObj.name = $"Player{packet.id}";
                
                var netPlayerNew = playerObj.GetComponent<NetworkPlayer>();     
                
                netPlayerNew.usernamePanel = netPlayerNew.transform.GetChild(1).GetComponent<TextMesh>();
                netPlayerNew.usernamePanel.text = packet.username;
                netPlayerNew.usernamePanel.characterSize = 0.2f;
                netPlayerNew.usernamePanel.anchor = TextAnchor.MiddleCenter;
                netPlayerNew.usernamePanel.fontSize = 24;
                
                netPlayerNew.id = packet.id;

                playerUsernames.Add(packet.username, packet.id);
                playerUsernamesReverse.Add(packet.id, packet.username);
                players.Add(new NetPlayerState
                {
                    connectionState = NetworkPlayerConnectionState.Connected,
                    epicID = null,
                    gameObject = netPlayerNew,
                    playerID = (ushort)packet.id
                });
                
                playerObj.SetActive(true);
                Object.DontDestroyOnLoad(playerObj);
            }
        }
        catch
        {
        }
    }

    [PacketResponse]
    private static void HandlePlayerLeave(NetPlayerState netPlayer, PlayerLeaveMessage packet, byte channel)
    {

        var playerObj = netPlayer.gameObject;
        players.Remove(netPlayer);
        Object.Destroy(playerObj.gameObject);
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

    
    

    private static void HandleClientActorSpawn(NetPlayerState netPlayer, ActorSpawnClientMessage packet, byte channel)
    {
        try
        {
            var sg = sceneGroups[packet.scene];
            Quaternion rot = Quaternion.Euler(packet.rotation);
            var ident = identifiableTypes[packet.ident];
            var identObj = ident.prefab;


            var nextID = NextMultiplayerActorID;

            var obj = RegisterActor(new ActorId(nextID), ident, packet.position, rot, sg);

            obj.AddComponent<NetworkActor>();
            obj.AddComponent<NetworkActorOwnerToggle>();
            obj.AddComponent<TransformSmoother>();

            if (obj && !ident.TryCast<GadgetDefinition>())
            {
                obj.AddComponent<NetworkResource>();
                obj.GetComponent<TransformSmoother>().enabled = false;
                if (obj.TryGetComponent<Rigidbody>(out var rb))
                    rb.velocity = packet.velocity;
                obj.GetComponent<TransformSmoother>().interpolPeriod = ActorTimer;
                obj.GetComponent<Vacuumable>()._launched = true;
            }

            if (obj.TryGetComponent<NetworkActor>(out var netComp)
               )
                if (!actors.TryAdd(nextID, netComp))
                    actors[nextID] = netComp;

            var forwardPacket = new ActorSpawnMessage()
            {
                id = nextID,
                ident = packet.ident,
                position = packet.position,
                rotation = packet.rotation,
                velocity = packet.velocity,
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
            MultiplayerManager.NetworkSend(ownPacket, MultiplayerManager.ServerSendOptions.SendToPlayer(netPlayer.playerID));
            MultiplayerManager.NetworkSend(forwardPacket);
        }
        catch (Exception e)
        {
            //if (ShowErrors)
            SRMP.Log($"Exception in spawning actor(no id)! Stack Trace:\n{e}");
        }

    }

    
    [PacketResponse]
    private static void HandleActorOwner(NetPlayerState netPlayer, ActorUpdateOwnerMessage packet, byte channel)
    {
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

    
    [PacketResponse]
    private static void HandleDestroyActor(NetPlayerState netPlayer, ActorDestroyGlobalMessage packet, byte channel)
    {
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

    [PacketResponse]
    private static void HandleActorVelocity(NetPlayerState netPlayer, ActorVelocityMessage packet, byte channel)
    {
        try
        {
            if (!actors.TryGetValue(packet.id, out var actor)) return;
            
            actor.GetComponent<Rigidbody>().velocity = packet.velocity;
            
            if (packet.bounce)
                if (!actor.IsOwned)
                    MultiplayerManager.NetworkSend(new ActorVelocityMessage
                    {
                        id = packet.id,
                        bounce = false,
                        velocity = actor.GetComponent<Rigidbody>().velocity
                    });
        }
        catch (Exception e)
        {
            if (ShowErrors)
                SRMP.Log($"Exception in setting actor({packet.id}) velocity! Stack Trace:\n{e}");
        }
    }

    [PacketResponse]
    private static void HandleActorSetOwner(NetPlayerState netPlayer, ActorSetOwnerMessage packet, byte channel)
    {
        try
        {
            if (!actors.TryGetValue(packet.id, out var actor)) return;

            actor.GetComponent<NetworkActorOwnerToggle>().OwnActor();
        }
        catch (Exception e)
        {
            if (ShowErrors)
                SRMP.Log($"Exception in transfering actor({packet.id})! Stack Trace:\n{e}");
        }
    }

    
    [PacketResponse]
    private static void HandlePlayer(NetPlayerState netPlayer, PlayerUpdateMessage packet, byte channel)
    {

        try
        {
            if (!TryGetPlayer((ushort)packet.id, out var state))
                return;
            var playerObj = state.gameObject;
            playerObj.GetComponent<TransformSmoother>().nextPos = packet.pos;
            playerObj.GetComponent<TransformSmoother>().nextRot = packet.rot.eulerAngles;


            var anim = playerObj.GetComponent<Animator>();

            anim.SetFloat("HorizontalMovement", packet.horizontalMovement);
            anim.SetFloat("ForwardMovement", packet.forwardMovement);
            anim.SetFloat("Yaw", packet.yaw);
            anim.SetInteger("AirborneState", packet.airborneState);
            anim.SetBool("Moving", packet.moving);
            anim.SetFloat("HorizontalSpeed", packet.horizontalSpeed);
            anim.SetFloat("ForwardSpeed", packet.forwardSpeed);
            anim.SetBool("Sprinting", packet.sprinting);
        }
        catch
        {
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
            if (ShowErrors)
                SRMP.Log($"Exception in handling landplot({packet.id})! Stack Trace:\n{e}");
        }
    }

    
    [PacketResponse]
    private static void HandleGarden(NetPlayerState netPlayer, GardenPlantMessage packet, byte channel)
    {     
        
        try
        {
            // get plot from id.
            var model = sceneContext.GameModel.landPlots[packet.id];
            var plot = model.gameObj;

            model.resourceGrowerDefinition =
                gameContext.AutoSaveDirector.resourceGrowers.items._items.FirstOrDefault(x =>
                    x._primaryResourceType == identifiableTypes[packet.ident]);

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
    private static void HandlePedia(NetPlayerState netPlayer, PediaMessage packet, byte channel)
    {

        handlingPacket = true;
        sceneContext.PediaDirector.Unlock(pediaEntries[packet.id]);
        handlingPacket = false;
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
    private static void HandleAmmoSlot(NetPlayerState netPlayer, AmmoEditSlotMessage packet, byte channel)
    {

        try
        {
            var ammo = GetNetworkAmmo(packet.id);
            
            handlingPacket = true;
            ammo?.MaybeAddToSpecificSlot(identifiableTypes[packet.ident], null, packet.slot, packet.count);
            handlingPacket = false;
        }
        catch (Exception e)
        {
            SRMP.Error($"Error in handling inventory({packet.id})! Stack Trace:\n{e}");
        }

    }

    [PacketResponse]
    private static void HandleAmmo(NetPlayerState netPlayer, AmmoAddMessage packet, byte channel)
    {

        try
        {
            var ammo = GetNetworkAmmo(packet.id);
            
            handlingPacket = true;
            ammo?.MaybeAddToSlot(identifiableTypes[packet.ident], null, SlimeAppearance.AppearanceSaveSet.NONE);
            handlingPacket = false;
        }
        catch (Exception e)
        {
            SRMP.Error($"Error in handling inventory({packet.id})! Stack Trace:\n{e}");
        }
    }

    
    [PacketResponse]
    private static void HandleAmmoSelect(NetPlayerState netPlayer, AmmoSelectMessage packet, byte channel)
    {

        try
        {
            Ammo ammo = GetNetworkAmmo(packet.id);

            handlingPacket = true;
            ammo?.SetAmmoSlot(packet.index);
            handlingPacket = false;
        }
        catch (Exception e)
        {
            SRMP.Error($"Error in handling inventory({packet.id})! Stack Trace:\n{e}");
        }
    }

    
    [PacketResponse]
    private static void HandleAmmoReverse(NetPlayerState netPlayer, AmmoRemoveMessage packet, byte channel)
    {

        try
        {
            Ammo ammo = GetNetworkAmmo(packet.id);

            handlingPacket = true;
            ammo?.Decrement(packet.index, packet.count);
            handlingPacket = false;
        }
        catch (Exception e)
        {
            SRMP.Error($"Error in handling inventory({packet.id})! Stack Trace:\n{e}");
        }
    }

    [PacketResponse]
    private static void HandleActor(NetPlayerState netPlayer, ActorUpdateMessage packet, byte channel)
    {

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

    private static void HandleSavedPlayers(NetPlayerState netPlayer, LoadMessage packet, byte channel)
    {
        latestSaveJoined = packet;
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
    private static void HandleKillAllCommand(NetPlayerState netPlayer, KillAllCommandMessage packet, byte channel)
    {

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
    private static void HandleRefineryItem(NetPlayerState netPlayer, RefineryItemMessage packet, byte channel)
    {

        handlingPacket = true;
        sceneContext.GadgetDirector._model.SetCount(identifiableTypes[packet.id], packet.count);
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