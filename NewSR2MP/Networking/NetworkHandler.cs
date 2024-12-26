using Il2CppMonomiPark.SlimeRancher.Analytics.Event;
using Il2CppMonomiPark.SlimeRancher.DataModel;
using Il2CppMonomiPark.SlimeRancher.Map;
using Il2CppMonomiPark.SlimeRancher.Player.PlayerItems;
using Il2CppMonomiPark.SlimeRancher.UI.Map;
using Il2CppMonomiPark.SlimeRancher.Weather;
using Il2CppMonomiPark.SlimeRancher.World;
using Il2CppMonomiPark.World;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;


namespace NewSR2MP.Networking
{
    public class NetworkHandler
    {
        public class Deserializer
        {
            public static WeatherSyncMessage ReadWeatherMessage(Message msg)
            {
                return new WeatherSyncMessage(msg);
            }
            public static LandPlotMessage ReadLandPlotMessage(Message msg)
            {
                LandplotUpdateType mode = (LandplotUpdateType)msg.GetByte();
                string id = msg.GetString();
                LandPlotMessage message = new LandPlotMessage()
                {
                    messageType = mode,
                    id = id,
                };
                if (mode == LandplotUpdateType.SET)
                    message.type = (LandPlot.Id)msg.GetByte();
                else
                    message.upgrade = (LandPlot.Upgrade)msg.GetByte();

                return message;
            }
            public static PlayerJoinMessage ReadPlayerJoinMessage(Message msg)
            {

                return new PlayerJoinMessage()
                {
                    id = msg.GetInt(),
                    local = msg.GetBool()
                };
            }

            public static SetMoneyMessage ReadCurrencyMessage(Message msg)
            {

                return new SetMoneyMessage()
                {
                    newMoney = msg.GetInt(),
                };
            }

            public static ClientUserMessage ReadClientUserMessage(Message msg)
            {
                return new ClientUserMessage()
                {
                    guid = msg.GetGuid(),
                    name = msg.GetString(),
                };
            }

            public static PlayerLeaveMessage ReadPlayerLeaveMessage(Message msg)
            {
                return new PlayerLeaveMessage()
                {
                    id = msg.GetInt(),
                };
            }
            public static GordoEatMessage ReadGordoEatMessage(Message msg)
            {
                return new GordoEatMessage()
                {
                    id = msg.GetString(),
                    count = msg.GetInt()
                };
            }
            public static GordoBurstMessage ReadGordoBurstMessage(Message msg)
            {
                return new GordoBurstMessage()
                {
                    id = msg.GetString()
                };
            }
            public static PediaMessage ReadPediaMessage(Message msg)
            {
                return new PediaMessage()
                {
                    id = msg.GetString()
                };
            }
            public static AmmoAddMessage ReadAmmoAddMessage(Message msg)
            {
                return new AmmoAddMessage()
                {
                    ident = msg.GetInt(),
                    id = msg.GetString()
                };
            }

            public static AmmoRemoveMessage ReadAmmoRemoveMessage(Message msg)
            {
                return new AmmoRemoveMessage()
                {
                    index = msg.GetInt(),
                    id = msg.GetString(),
                    count = msg.GetInt()
                };
            }
            public static MapUnlockMessage ReadMapUnlockMessage(Message msg)
            {
                return new MapUnlockMessage()
                {
                    id = msg.GetString()
                };
            }
            public static AmmoEditSlotMessage ReadAmmoAddToSlotMessage(Message msg)
            {
                return new AmmoEditSlotMessage()
                {
                    ident = msg.GetInt(),
                    slot = msg.GetInt(),
                    count = msg.GetInt(),
                    id = msg.GetString()
                };
            }
            public static GardenPlantMessage ReadGardenPlantMessage(Message msg)
            {
                return new GardenPlantMessage()
                {
                    ident = msg.GetInt(),
                    replace = msg.GetBool(),
                    id = msg.GetString(),
                };
            }

            public static AmmoData ReadAmmoData(Message msg)
            {
                AmmoData data = new AmmoData()
                {
                    count = msg.GetInt(),
                    slot = msg.GetInt(),
                    id = msg.GetInt(),
                };
                return data;
            }


            public static LoadMessage ReadLoadMessage(Message msg)
            {

                int length = msg.GetInt();

                List<InitActorData> actors = new List<InitActorData>();
                for (int i = 0; i < length; i++)
                {
                    long id = msg.GetLong();
                    int ident = msg.GetInt();
                    int sg = msg.GetInt();
                    Vector3 actorPos = msg.GetVector3();
                    actors.Add(new InitActorData()
                    {
                        id = id,
                        ident = ident,
                        scene = sg,
                        pos = actorPos
                    });
                }

                int length2 = msg.GetInt();
                List<InitPlayerData> players = new List<InitPlayerData>();
                for (int i = 0; i < length2; i++)
                {
                    int id = msg.GetInt();
                    players.Add(new InitPlayerData()
                    {
                        id = id
                    });
                }

                int length3 = msg.GetInt();
                List<InitPlotData> plots = new List<InitPlotData>();
                for (int i = 0; i < length3; i++)
                {
                    string id = msg.GetString();
                    LandPlot.Id type = (LandPlot.Id)msg.GetInt();
                    int upgLength = msg.GetInt();
                    Il2CppSystem.Collections.Generic.HashSet<LandPlot.Upgrade> upgrades = new Il2CppSystem.Collections.Generic.HashSet<LandPlot.Upgrade>();
                    for (int i2 = 0; i2 < upgLength; i2++)
                    {
                        upgrades.Add((LandPlot.Upgrade)msg.GetInt());
                    }

                    InitSiloData siloData;
                    int slots = msg.GetInt();
                    int ammLength = msg.GetInt();
                    HashSet<AmmoData> ammoDatas = new HashSet<AmmoData>();
                    for (int i2 = 0; i2 < ammLength; i2++)
                    {
                        var data = msg.GetAmmoData();
                        ammoDatas.Add(data);
                    }

                    siloData = new InitSiloData()
                    {
                        slots = slots,
                        ammo = ammoDatas
                    };
                    var crop = msg.GetInt();
                    plots.Add(new InitPlotData()
                    {
                        type = type,
                        id = id,
                        upgrades = upgrades,
                        siloData = siloData,
                        cropIdent = crop
                    });
                }

                int length4 = msg.GetInt();
                HashSet<InitGordoData> gordos = new HashSet<InitGordoData>();
                for (int i = 0; i < length4; i++)
                {
                    string id = msg.GetString();
                    int eaten = msg.GetInt();
                    gordos.Add(new InitGordoData()
                    {
                        id = id,
                        eaten = eaten,
                    });
                }

                int pedLength = msg.GetInt();
                List<string> pedias = new List<string>();
                for (int i = 0; i < pedLength; i++)
                {
                    pedias.Add(msg.GetString());
                }

                int mapLength = msg.GetInt();
                List<string> maps = new List<string>();
                for (int i = 0; i < mapLength; i++)
                {
                    maps.Add(msg.GetString());
                }

                int accLength = msg.GetInt();
                List<InitAccessData> access = new List<InitAccessData>();
                for (int i = 0; i < accLength; i++)
                {
                    string id = msg.GetString();
                    bool open = msg.GetBool();
                    InitAccessData accessData = new InitAccessData()
                    {
                        id = id,
                        open = open,
                    };
                    access.Add(accessData);
                }

                var pid = msg.GetInt();
                var pos = msg.GetVector3();
                var rot = msg.GetVector3();

                var localAmmoCount = msg.GetInt();

                List<AmmoData> localAmmo = new List<AmmoData>();
                for (int i = 0; i < localAmmoCount; i++)
                {
                    localAmmo.Add(msg.GetAmmoData());
                }

                int scene = msg.GetInt();

                var player = new LocalPlayerData()
                {
                    pos = pos,
                    rot = rot,
                    ammo = localAmmo,
                    sceneGroup = scene
                };

                var money = msg.GetInt();

                var pUpgradesCount = msg.GetInt();
                Il2CppSystem.Collections.Generic.Dictionary<int, int> pUpgrades = new Il2CppSystem.Collections.Generic.Dictionary<int, int>();

                for (int i = 0; i < pUpgradesCount; i++)
                {
                    var key = msg.GetInt();
                    var val = msg.GetInt();

                    pUpgrades.TryAdd(key, val);
                }

                var time = msg.GetDouble();

                //var sm = msg.GetBool();
                //var sk = msg.GetBool();
                //var su = msg.GetBool();

                return new LoadMessage()
                {
                    initActors = actors,
                    initPlayers = players,
                    initPlots = plots,
                    initGordos = gordos,
                    initPedias = pedias,
                    initAccess = access,
                    initMaps = maps,
                    localPlayerSave = player,
                    playerID = pid,
                    money = money,
                    upgrades = pUpgrades,
                    time = time,
                };
            }
            public static TimeSyncMessage ReadTimeMessage(Message msg)
            {
                return new TimeSyncMessage()
                {
                    time = msg.GetDouble()
                };
            }


            public static ActorSpawnClientMessage ReadActorSpawnClientMessage(Message msg)
            {
                var ident = msg.GetInt();
                var pos = msg.GetVector3();
                var rot = msg.GetVector3();
                var vel = msg.GetVector3();
                var scene = msg.GetInt();
                var p = msg.GetInt();
                return new ActorSpawnClientMessage()
                {
                    ident = ident,
                    position = pos,
                    rotation = rot,
                    velocity = vel,
                    scene = scene,
                    player = p
                };
            }
            public static ActorDestroyGlobalMessage ReadActorDestroyMessage(Message msg)
            {
                return new ActorDestroyGlobalMessage()
                {
                    id = msg.GetLong()
                };
            }
            public static ResourceStateMessage ReadResourceStateMessage(Message msg)
            {
                return new ResourceStateMessage()
                {
                    state = (ResourceCycle.State)msg.GetByte(),
                    id = msg.GetLong(),
                };
            }
            public static ActorUpdateOwnerMessage ReadActorUpdateOwnerMessage(Message msg)
            {
                return new ActorUpdateOwnerMessage()
                {
                    id = msg.GetLong(),
                    player = msg.GetInt(),
                };
            }
            public static ActorSetOwnerMessage ReadActorSetOwnerMessage(Message msg)
            {
                return new ActorSetOwnerMessage()
                {
                    id = msg.GetLong(),
                    velocity = msg.GetVector3(),
                };
            }
            public static ActorUpdateClientMessage ReadActorClientMessage(Message msg)
            {
                var id = msg.GetLong();
                var pos = msg.GetVector3();
                var rot = msg.GetVector3();
                return new ActorUpdateClientMessage()
                {
                    id = id,
                    position = pos,
                    rotation = rot
                };
            }
            public static ActorUpdateMessage ReadActorMessage(Message msg)
            {
                var id = msg.GetLong();
                var pos = msg.GetVector3();
                var rot = msg.GetVector3();
                return new ActorUpdateMessage()
                {
                    id = id,
                    position = pos,
                    rotation = rot
                };
            }
            public static PlayerUpdateMessage ReadPlayerMessage(Message msg)
            {
                var id = msg.GetInt(); 
                
                var scene = msg.GetByte();
                var pos = msg.GetVector3();
                var rot = msg.GetQuaternion();
                
                var airborneState = msg.GetInt();
                var moving = msg.GetBool();
                var horizontalSpeed = msg.GetFloat();
                var forwardSpeed = msg.GetFloat();
                var horizontalMovement = msg.GetFloat();
                var forwardMovement = msg.GetFloat();
                var yaw = msg.GetFloat();

                var returnval = new PlayerUpdateMessage()
                {
                    id = id,
                    scene = scene,
                    pos = pos,
                    rot = rot,
                    airborneState = airborneState,
                    moving = moving,
                    yaw = yaw,
                    horizontalSpeed = horizontalSpeed,
                    forwardSpeed = forwardSpeed,
                    horizontalMovement = horizontalMovement,
                    forwardMovement = forwardMovement,
                };
                return returnval;
            }
            public static ActorSpawnMessage ReadActorSpawnMessage(Message msg)
            {
                var id = msg.GetLong();
                var ident = msg.GetInt();
                var pos = msg.GetVector3();
                var rot = msg.GetVector3();
                var scene = msg.GetInt();
                var player = msg.GetInt();
                return new ActorSpawnMessage()
                {
                    ident = ident,
                    position = pos,
                    rotation = rot,
                    id = id,
                    player = player,
                    scene = scene
                };
            }
            public static DoorOpenMessage ReadDoorOpenMessage(Message msg)
            {
                return new DoorOpenMessage()
                {
                    id = msg.GetString()
                };
            }
            public static SleepMessage ReadSleepMessage(Message msg)
            {
                return new SleepMessage()
                {
                    time = msg.GetDouble()
                };
            }
            public static ActorChangeHeldOwnerMessage ReadActorChangeHeldOwnerMessage(Message msg)
            {
                return new ActorChangeHeldOwnerMessage()
                {
                    id = msg.GetLong()
                };
            }

            public static PlaceNavMarkerNessage ReadPlaceNavMarkerNessage(Message msg)
            {
                var map = msg.GetByte();
                var pos = msg.GetVector3();

                return new PlaceNavMarkerNessage()
                {
                    map = (MapType)map,
                    position = pos
                };
            }

        }











        [MessageHandler((ushort)PacketType.ResourceState)]
            public static void HandleResourceState(Message msg)
            {
                var packet = Deserializer.ReadResourceStateMessage(msg);
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
                var packet = Deserializer.ReadResourceStateMessage(msg);
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
                var packet = Deserializer.ReadDoorOpenMessage(msg);
                SceneContext.Instance.GameModel.doors[packet.id].gameObj.GetComponent<AccessDoor>().CurrState = AccessDoor.State.OPEN;
            }
            [MessageHandler((ushort)PacketType.OpenDoor)]
            public static void HandleDoor(ushort client, Message msg)
            {
                var packet = Deserializer.ReadDoorOpenMessage(msg);
                SceneContext.Instance.GameModel.doors[packet.id].gameObj.GetComponent<AccessDoor>().CurrState = AccessDoor.State.OPEN;


                
            }
            [MessageHandler((ushort)PacketType.SetCurrency)]
            public static void HandleMoneyChange(Message msg)
            {
                var packet = Deserializer.ReadCurrencyMessage(msg);
                SceneContext.Instance.PlayerState._model.currency = packet.newMoney;

                
            }
            [MessageHandler((ushort)PacketType.SetCurrency)]
            public static void HandleMoneyChange(ushort client, Message msg)
            {
                var packet = Deserializer.ReadCurrencyMessage(msg);
                SceneContext.Instance.PlayerState._model.currency = packet.newMoney;

                
            }
            [MessageHandler((ushort)PacketType.ActorSpawn)]

            public static void HandleActorSpawn(Message msg)
            {
                var packet = Deserializer.ReadActorSpawnMessage(msg);
                try
                {
                    if (actors.TryGetValue(packet.id, out var actor))
                        actors.Remove(packet.id);
                    
                    Quaternion quat = Quaternion.Euler(packet.rotation.x, packet.rotation.y, packet.rotation.z);
                    var identObj = identifiableTypes[packet.ident].prefab;
                    
                    if (identObj.GetComponent<NetworkActor>() == null)
                        identObj.AddComponent<NetworkActor>();
                    if (identObj.GetComponent<NetworkActorOwnerToggle>() == null)
                        identObj.AddComponent<NetworkActorOwnerToggle>();
                    if (identObj.GetComponent<TransformSmoother>() == null)
                        identObj.AddComponent<TransformSmoother>();

                    identObj.GetComponent<NetworkActor>().enabled = false;
                    identObj.GetComponent<TransformSmoother>().enabled = true;
                    
                    SRMP.Debug($"[{SystemContext.Instance._SceneLoader_k__BackingField.CurrentSceneGroup.name} | {sceneGroups[packet.scene].name}]");
                    
                    var obj = InstantiateActor(identObj, sceneGroups[packet.scene], packet.position, quat, false);
                    
                    identObj.RemoveComponent<NetworkActor>();
                    identObj.RemoveComponent<NetworkActorOwnerToggle>();
                    identObj.RemoveComponent<TransformSmoother>();
                    
                    obj.AddComponent<NetworkResource>(); // Try add resource network component. Will remove if its not a resource so please do not change
                    
                    if (!actors.ContainsKey(obj.GetComponent<IdentifiableActor>().GetActorId().Value))
                    {
                        actors.Add(obj.GetComponent<Identifiable>().GetActorId().Value, obj.GetComponent<NetworkActor>());
                        obj.GetComponent<TransformSmoother>().interpolPeriod = .15f;
                        obj.GetComponent<Vacuumable>()._launched = true;
                    }
                    else
                    {
                        obj.GetComponent<TransformSmoother>().enabled = false;
                        obj.GetComponent<TransformSmoother>().interpolPeriod = .15f;
                        obj.GetComponent<Vacuumable>()._launched = true;
                    }

                    obj.GetComponent<NetworkActor>().IsOwned = false;
                    obj.GetComponent<TransformSmoother>().nextPos = packet.position;
                    
                    obj.GetComponent<IdentifiableActor>()._model.actorId = new ActorId(packet.id);
                    SceneContext.Instance.GameModel.identifiables.Remove(obj.GetComponent<IdentifiableActor>()._model.actorId);
                    SceneContext.Instance.GameModel.identifiables.Add(obj.GetComponent<IdentifiableActor>()._model.actorId, obj.GetComponent<IdentifiableActor>()._model);
                    actors.Add(packet.id, obj.GetComponent<NetworkActor>());
                    
                    SceneContext.Instance.GameModel.RegisterActor( obj.GetComponent<IdentifiableActor>().GetActorId(), obj.GetComponent<IdentifiableActor>().identType,packet.position,Quaternion.identity,sceneGroups[packet.scene]);
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
                var packet = Deserializer.ReadPlayerJoinMessage(msg);

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
                catch {}
            }
            
            [MessageHandler((ushort)PacketType.PlayerLeave)]
            public static void HandlePlayerLeave(Message msg)
            {
                var packet = Deserializer.ReadPlayerLeaveMessage(msg);
                
                var player = players[packet.id];
                players.Remove(packet.id);
                Object.Destroy(player.gameObject);
            }
            [MessageHandler((ushort)PacketType.TimeUpdate)]
            public static void HandleTime(Message msg)
            {
                var packet = Deserializer.ReadTimeMessage(msg);
                SceneContext.Instance.GameModel.world.worldTime = packet.time;
            }
            [MessageHandler((ushort)PacketType.FastForward)]
            public static void HandleClientSleep(ushort client, Message msg)
            {
                var packet = Deserializer.ReadSleepMessage(msg);
                SceneContext.Instance.TimeDirector.FastForwardTo(packet.time);
                
                ForwardMessage(packet, client);
            }
            [MessageHandler((ushort)PacketType.TempClientActorSpawn)]

            public static void HandleClientActorSpawn(ushort client, Message msg)
            {
                var packet = Deserializer.ReadActorSpawnClientMessage(msg);
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
                    var obj = InstantiateActor(identObj, sceneGroups[packet.scene], packet.position, quat, false);
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
                        obj.GetComponent<Vacuumable>()._launched = true;
                    }
                    else
                    {
                        obj.GetComponent<TransformSmoother>().enabled = false;
                        obj.GetComponent<Rigidbody>().velocity = packet.velocity;
                        obj.GetComponent<NetworkActor>().startingVel = packet.velocity;
                        obj.GetComponent<TransformSmoother>().interpolPeriod = .15f;
                        obj.GetComponent<Vacuumable>()._launched = true;
                    }
                    
                    SceneContext.Instance.GameModel.RegisterActor( obj.GetComponent<IdentifiableActor>().GetActorId(), obj.GetComponent<IdentifiableActor>().identType,packet.position,Quaternion.identity,sceneGroups[packet.scene]);
                    
                    var packetR = new ActorSpawnMessage()
                    {
                        id = obj.GetComponent<IdentifiableActor>().GetActorId().Value,
                        ident = packet.ident,
                        position = packet.position,
                        rotation = packet.rotation,
                        scene = packet.scene,
                    };

                    var ownPacket = new ActorSetOwnerMessage()
                    {
                        id = obj.GetComponent<IdentifiableActor>()._model.actorId.Value,
                        velocity = packet.velocity
                    };
                    MultiplayerManager.NetworkSend(ownPacket, MultiplayerManager.ServerSendOptions.SendToPlayer(client));
                    MultiplayerManager.NetworkSend(packetR);
                }
                catch (Exception e)
                {
                    if (ShowErrors)
                        SRMP.Log($"Exception in spawning actor(no id)! Stack Trace:\n{e}");
                }
                
            }
            [MessageHandler((ushort)PacketType.TempClientActorUpdate)]

            public static void HandleClientActor(ushort client, Message msg)
            {
                var packet = Deserializer.ReadActorClientMessage(msg);
                try
                {
                    if (!actors.TryGetValue(packet.id, out var actor)) return;
                    
                    var t = actor.GetComponent<TransformSmoother>();
                    t.nextPos = packet.position;
                    t.nextRot = packet.rotation;
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
                var packet = Deserializer.ReadActorUpdateOwnerMessage(msg);
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
                var packet = Deserializer.ReadActorDestroyMessage(msg);
                try
                {
                    if (!actors.TryGetValue(packet.id, out var actor)) return;

                    UnityEngine.Object.Destroy(actor.gameObject);
                    actors.Remove(packet.id);
                }
                catch (Exception e)
                {
                    SRMP.Error($"Exception in destroying actor({packet.id})! Stack Trace:\n{e}");
                }
            }          
            
            [MessageHandler((ushort)PacketType.ActorBecomeOwner)]
            public static void HandleActorOwner(ushort client, Message msg)
            {
                var packet = Deserializer.ReadActorUpdateOwnerMessage(msg);
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
                ForwardMessage(packet, client);
            }            
            
            [MessageHandler((ushort)PacketType.ActorDestroy)]
            public static void HandleDestroyActor(ushort client, Message msg)
            {var packet = Deserializer.ReadActorDestroyMessage(msg);
                try
                {
                    if (!actors.TryGetValue(packet.id, out var actor)) return;
                    UnityEngine.Object.Destroy(actor.gameObject);
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
                var packet = Deserializer.ReadActorSetOwnerMessage(msg);
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
                var packet = Deserializer.ReadPlayerMessage(msg);

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
                catch { }
            }                  
            [MessageHandler((ushort)PacketType.PlayerUpdate)]

            public static void HandlePlayer(ushort client, Message msg)
            {             
                var packet = Deserializer.ReadPlayerMessage(msg);

                if (packet.id == ushort.MaxValue)
                {
                    return; // 3 player lobby bug - host model would get teleported to different clients.
                }
                
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
                catch { }
                
                ForwardMessage(packet, client);
            }          
            [MessageHandler((ushort)PacketType.LandPlot)]
            public static void HandleLandPlot(Message msg)
            {
                var packet = Deserializer.ReadLandPlotMessage(msg);

                try
                {
                    var plot = SceneContext.Instance.GameModel.landPlots[packet.id].gameObj;

                    if (packet.messageType == LandplotUpdateType.SET)
                    {
                        plot.AddComponent<HandledDummy>();

                        plot.GetComponent<LandPlotLocation>().Replace(plot.GetComponentInChildren<LandPlot>(), GameContext.Instance.LookupDirector._plotPrefabDict[packet.type]);

                        UnityEngine.Object.Destroy(plot.GetComponent<HandledDummy>());
                    }
                    else
                    {

                        var lp = plot.GetComponentInChildren<LandPlot>();
                        
                        lp.gameObject.AddComponent<HandledDummy>();

                        lp.AddUpgrade(packet.upgrade);

                        lp.gameObject.RemoveComponent<HandledDummy>();

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
                var packet = Deserializer.ReadGardenPlantMessage(msg);

                try
                {
                    // get plot from id.
                    var plot = SceneContext.Instance.GameModel.landPlots[packet.id].gameObj;

                    // Get required components
                    var lp = plot.transform.GetChild(0).GetComponent<LandPlot>();
                    var g = plot.transform.GetComponentInChildren<GardenCatcher>();

                    // Check if is destroy (planting NONE)
                    if (packet.ident != 9)
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
                    if (ShowErrors)
                        SRMP.Log($"Exception in handling garden({packet.id})! Stack Trace:\n{e}");
                }
            }
            [MessageHandler((ushort)PacketType.LandPlot)]
            public static void HandleLandPlot(ushort client, Message msg)
            {
                var packet = Deserializer.ReadLandPlotMessage(msg);

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
                    if (ShowErrors)
                        SRMP.Log($"Exception in handling landplot({packet.id})! Stack Trace:\n{e}");
                }
                
                ForwardMessage(packet, client);
            }     
            [MessageHandler((ushort)PacketType.GardenPlant)]

            public static void HandleGarden(ushort client, Message msg)
            {
                var packet = Deserializer.ReadGardenPlantMessage(msg);

                try
                {
                    // get plot from id.
                    var plot = SceneContext.Instance.GameModel.landPlots[packet.id].gameObj;

                    // Get required components
                    var lp = plot.transform.GetChild(0).GetComponent<LandPlot>();
                    var g = plot.transform.GetComponentInChildren<GardenCatcher>();

                    // Check if is destroy (planting NONE)
                    if (packet.ident != 9)
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
                    if (ShowErrors)
                        SRMP.Log($"Exception in handling garden({packet.id})! Stack Trace:\n{e}");
                }           
                
                ForwardMessage(packet, client);
            }

            [MessageHandler((ushort)PacketType.GordoFeed)]
            public static void HandleGordoEat(Message msg)
            {
                var packet = Deserializer.ReadGordoEatMessage(msg);

                try
                {
                    SceneContext.Instance.GameModel.gordos[packet.id].gordoEatCount = packet.count;
                }
                catch (Exception e)
                {
                    if (ShowErrors)
                        SRMP.Log($"Exception in feeding gordo({packet.id})! Stack Trace:\n{e}");
                }
            }
            [MessageHandler((ushort)PacketType.PediaUnlock)]
            public static void HandlePedia(Message msg)
            {
                var packet = Deserializer.ReadPediaMessage(msg);

                SceneContext.Instance.gameObject.AddComponent<HandledDummy>();
                SceneContext.Instance.PediaDirector.Unlock(pediaEntries[packet.id]);
                UnityEngine.Object.Destroy(SceneContext.Instance.gameObject.GetComponent<HandledDummy>());
            }
            [MessageHandler((ushort)PacketType.GordoExplode)]
            public static void HandleGordoBurst(Message msg)
            {
                var packet = Deserializer.ReadGordoBurstMessage(msg);

                try
                {
                    var gordo = SceneContext.Instance.GameModel.gordos[packet.id].gameObj;
                    gordo.AddComponent<HandledDummy>();
                    gordo.GetComponent<GordoEat>().ImmediateReachedTarget();
                    UnityEngine.Object.Destroy(gordo.GetComponent<HandledDummy>());
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
                var packet = Deserializer.ReadGordoEatMessage(msg);

                try
                {
                    SceneContext.Instance.GameModel.gordos[packet.id].gordoEatCount = packet.count;
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
                var packet = Deserializer.ReadPediaMessage(msg);

                SceneContext.Instance.gameObject.AddComponent<HandledDummy>();
                SceneContext.Instance.PediaDirector.ShowPopupIfUnlocked(pediaEntries[packet.id]);
                UnityEngine.Object.Destroy(SceneContext.Instance.gameObject.GetComponent<HandledDummy>());
                
                ForwardMessage(packet, client);
            }
            [MessageHandler((ushort)PacketType.GordoExplode)]
            public static void HandleGordoBurst(ushort client, Message msg)
            {
                var packet = Deserializer.ReadGordoBurstMessage(msg);

                try
                {
                    var gordo = SceneContext.Instance.GameModel.gordos[packet.id].gameObj;
                    gordo.AddComponent<HandledDummy>();
                    gordo.GetComponent<GordoEat>().ImmediateReachedTarget();
                    UnityEngine.Object.Destroy(gordo.GetComponent<HandledDummy>());
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
                var packet = Deserializer.ReadAmmoAddToSlotMessage(msg);

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

            }

            [MessageHandler((ushort)PacketType.AmmoAdd)]
            public static void HandleAmmo(Message msg)
            {
                var packet = Deserializer.ReadAmmoAddMessage(msg);

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
            }
            
            [MessageHandler((ushort)PacketType.AmmoRemove)]
            public static void HandleAmmoReverse(Message msg)
            {
                var packet = Deserializer.ReadAmmoRemoveMessage(msg);

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
            }
            //
            // TODO: Add map handling. look into disabling the map fog game objects.
            //
            public static void HandleMap(Message msg)
            {
                // SceneContext.Instance.PlayerState._model.unlockedZoneMaps.Add(packet.id);
            }
            
            [MessageHandler((ushort)PacketType.ActorUpdate)]
            public static void HandleActor(Message msg)
            {
                var packet = Deserializer.ReadActorMessage(msg);
                
                try
                {
                    if (!actors.TryGetValue(packet.id, out var actor)) return;
                    var t = actor.GetComponent<TransformSmoother>();
                    t.nextPos = packet.position;
                    t.nextRot = packet.rotation;
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
                var packet = Deserializer.ReadLoadMessage(msg);

                latestSaveJoined = packet;
            }


            [MessageHandler((ushort)PacketType.RequestJoin)]
            public static void HandleClientJoin(ushort client, Message joinInfo)
            {
                var packet = Deserializer.ReadClientUserMessage(joinInfo);
                MultiplayerManager.server.TryGetClient(client, out var con);
                MultiplayerManager.PlayerJoin(con, packet.guid, packet.name);
            }
            
            [MessageHandler((ushort)PacketType.NavigationMarkerPlace)]
            public static void HandleNavPlace(ushort client, Message joinInfo)
            {
                var packet = Deserializer.ReadPlaceNavMarkerNessage(joinInfo);


                MapDefinition map = null;
                switch (packet.map)
                {
                    case MapType.RainbowIsland:
                        map = SceneContext.Instance.MapDirector._mapList._maps[0];
                        break;
                    case MapType.Labyrinth:
                        map = SceneContext.Instance.MapDirector._mapList._maps[1];
                        break;
                }
                
                SceneContext.Instance.gameObject.AddComponent<HandledDummy>();
                SceneContext.Instance.MapDirector.SetPlayerNavigationMarker(packet.position, map, 0);
                SceneContext.Instance.gameObject.RemoveComponent<HandledDummy>();
                
                ForwardMessage(packet, client);
            }
            [MessageHandler((ushort)PacketType.NavigationMarkerRemove)]
            public static void HandleNavRemove(ushort client, Message joinInfo)
            {
                SceneContext.Instance.gameObject.AddComponent<HandledDummy>();
                SceneContext.Instance.MapDirector.ClearPlayerNavigationMarker();
                SceneContext.Instance.gameObject.RemoveComponent<HandledDummy>();
                
                ForwardMessage(new RemoveNavMarkerNessage(), client);
            }
            [MessageHandler((ushort)PacketType.NavigationMarkerPlace)]
            public static void HandleNavPlace(Message joinInfo)
            {
                var packet = Deserializer.ReadPlaceNavMarkerNessage(joinInfo);


                MapDefinition map = null;
                switch (packet.map)
                {
                    case MapType.RainbowIsland:
                        map = SceneContext.Instance.MapDirector._mapList._maps[0];
                        break;
                    case MapType.Labyrinth:
                        map = SceneContext.Instance.MapDirector._mapList._maps[1];
                        break;
                }
                
                SceneContext.Instance.gameObject.AddComponent<HandledDummy>();
                SceneContext.Instance.MapDirector.SetPlayerNavigationMarker(packet.position, map, 0);
                SceneContext.Instance.gameObject.RemoveComponent<HandledDummy>();
            }
            [MessageHandler((ushort)PacketType.NavigationMarkerRemove)]
            public static void HandleNavRemove(Message joinInfo)
            {
                SceneContext.Instance.gameObject.AddComponent<HandledDummy>();
                SceneContext.Instance.MapDirector.ClearPlayerNavigationMarker();
                SceneContext.Instance.gameObject.RemoveComponent<HandledDummy>();
            }


            
            [MessageHandler((ushort)PacketType.WeatherUpdate)]
            public static void HandleWeather(Message joinInfo)
            {
                
                var dir2 = Resources.FindObjectsOfTypeAll<WeatherDirector>().First();
                var packet = Deserializer.ReadWeatherMessage(joinInfo);

                var dir = SceneContext.Instance.WeatherRegistry;

                var zones = new Dictionary<byte, ZoneDefinition>();
                byte b = 0;
                foreach (var zone in dir._model._zoneDatas)
                {
                    zones.Add(b, zone.key);
                    b++;
                }
                
                var zoneDatas = new Il2CppSystem.Collections.Generic.Dictionary<ZoneDefinition, WeatherModel.ZoneData>();
                var zoneDatas2 = new Il2CppSystem.Collections.Generic.Dictionary<ZoneDefinition, WeatherRegistry.ZoneWeatherData>();

                foreach (var zone in packet.sync.zones)
                {
                    if (!zones.ContainsKey(zone.Key)) continue;

                    var forcastRunCheck = new List<string>();
                    
                    var forecast = new Il2CppSystem.Collections.Generic.List<WeatherModel.ForecastEntry>();
                    foreach (var f in zone.Value.forcast)
                    {
                        var forcastEntry = new WeatherModel.ForecastEntry()
                        {
                            StartTime = 0.0,
                            EndTime = double.MaxValue,
                            State = f.state.Cast<IWeatherState>(),
                            Pattern = weatherPatternsFromStateNames[f.state.name],
                            Started = true
                        };
                        forecast.Add(forcastEntry);
                        forcastRunCheck.Add(f.state.GetName());

                        // TODO: make it so it wont run if its already running
                        dir.RunPatternState(zones[zone.Key], weatherPatternsFromStateNames[f.state.name].CreatePattern(), f.state.Cast<IWeatherState>(), true);
                    }
                    var runningStates = dir2._runningStates; // There is a bug where the foreach loop implodes on the collection being modified. it is my fault for not adding this variable.

                    foreach (var running in dir2._runningStates)
                    {
                        if (!forcastRunCheck.Contains(running.GetName()))
                            dir.StopPatternState(zones[zone.Key], weatherPatternsFromStateNames[running.Cast<WeatherStateDefinition>().name].CreatePattern(), running);
                    }
                    WeatherModel.ZoneData data = new WeatherModel.ZoneData()
                    {
                        Forecast = forecast,
                        Parameters = new WeatherModel.ZoneWeatherParameters()
                        {
                            WindDirection = zone.Value.windSpeed
                        }
                    };
                    WeatherRegistry.ZoneWeatherData data2 = new WeatherRegistry.ZoneWeatherData(dir.ZoneConfigList._items[zone.Key], data);
                    zoneDatas.Add(zones[zone.Key], data);
                    zoneDatas2.Add(zones[zone.Key], data2);
                }
                dir._zones = zoneDatas2;
                dir._model = new WeatherModel()
                {
                    _participant = SceneContext.Instance.WeatherRegistry.Cast<WeatherModel.Participant>(),
                    _zoneDatas = zoneDatas,
                };

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
}
