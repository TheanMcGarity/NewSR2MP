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
                return new GordoEatMessage
                {
                    id = msg.GetString(),
                    count = msg.GetInt(),
                    ident = msg.GetInt(),
                };
            }

            public static GordoBurstMessage ReadGordoBurstMessage(Message msg)
            {
                return new GordoBurstMessage
                {
                    id = msg.GetString(),
                    ident = msg.GetInt(),
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

                int lengthActor = msg.GetInt();

                List<InitActorData> actors = new List<InitActorData>();
                for (int i = 0; i < lengthActor; i++)
                {
                    long id = msg.GetLong();
                    int ident = msg.GetInt();
                    int sg = msg.GetInt();
                    Vector3 actorPos = msg.GetVector3();
                    actors.Add(new InitActorData
                    {
                        id = id,
                        ident = ident,
                        scene = sg,
                        pos = actorPos
                    });
                }

                int lengthPlayer = msg.GetInt();
                List<InitPlayerData> players = new List<InitPlayerData>();
                for (int i = 0; i < lengthPlayer; i++)
                {
                    int id = msg.GetInt();
                    players.Add(new InitPlayerData()
                    {
                        id = id
                    });
                }

                int lengthPlot = msg.GetInt();
                List<InitPlotData> plots = new List<InitPlotData>();
                for (int i = 0; i < lengthPlot; i++)
                {
                    string id = msg.GetString();
                    LandPlot.Id type = (LandPlot.Id)msg.GetInt();
                    int upgLength = msg.GetInt();
                    Il2CppSystem.Collections.Generic.HashSet<LandPlot.Upgrade> upgrades =
                        new Il2CppSystem.Collections.Generic.HashSet<LandPlot.Upgrade>();
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

                int lengthGordo = msg.GetInt();
                HashSet<InitGordoData> gordos = new HashSet<InitGordoData>();
                for (int i = 0; i < lengthGordo; i++)
                {
                    string id = msg.GetString();
                    int eaten = msg.GetInt();
                    int ident = msg.GetInt();
                    gordos.Add(new InitGordoData()
                    {
                        id = id,
                        eaten = eaten,
                        ident = ident,
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
                Il2CppSystem.Collections.Generic.Dictionary<int, int> pUpgrades =
                    new Il2CppSystem.Collections.Generic.Dictionary<int, int>();

                for (int i = 0; i < pUpgradesCount; i++)
                {
                    var key = msg.GetInt();
                    var val = msg.GetInt();

                    pUpgrades.TryAdd(key, val);
                }

                var time = msg.GetDouble();

                var marketCount = msg.GetInt();
                var marketData = new List<float>(marketCount);

                for (int i = 0; i < marketCount; i++)
                    marketData.Add(msg.GetFloat());

                List<InitSwitchData> switches = new List<InitSwitchData>();
                var switchCount = msg.GetInt();
                for (int i = 0; i < switchCount; i++)
                    switches.Add(new InitSwitchData
                    {
                        id = msg.GetString(),
                        state = msg.GetByte()
                    });
                
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
                    marketPrices = marketData,
                    initSwitches = switches,
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
                var vel = msg.GetVector3();

                var emotions = NetworkEmotions.Deserialize(msg);

                return new ActorUpdateClientMessage
                {
                    id = id,
                    position = pos,
                    rotation = rot,
                    velocity = vel,
                    slimeEmotions = emotions,
                };
            }

            public static ActorUpdateMessage ReadActorMessage(Message msg)
            {
                var id = msg.GetLong();
                var pos = msg.GetVector3();
                var rot = msg.GetVector3();
                var vel = msg.GetVector3();

                var emotions = NetworkEmotions.Deserialize(msg);

                return new ActorUpdateMessage()
                {
                    id = id,
                    position = pos,
                    rotation = rot,
                    velocity = vel,
                    slimeEmotions = emotions,
                };
            }
            public static KillAllCommand ReadKillAllCommandMessage(Message msg)
            {
                var sg = msg.GetInt();
                var type = msg.GetInt();
                
                return new KillAllCommand
                {
                    sceneGroup = sg,
                    actorType = type
                };
            }
            
            public static SwitchModifyMessage ReadSwitchModifyMessage(Message msg)
            {
                var id = msg.GetString();
                var state = msg.GetByte();
                
                return new SwitchModifyMessage
                {
                    id = id,
                    state = state
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
                return new ActorSpawnMessage
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
            public static MarketRefreshMessage ReadMarketRefreshMessage(Message msg)
            {
                var c = msg.GetInt();
                var prices = new List<float>(c);
                
                for (int i = 0; i < c; i++)
                    prices.Add(msg.GetFloat());

                return new MarketRefreshMessage()
                {
                    prices = prices
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
            sceneContext.GameModel.doors[packet.id].gameObj.GetComponent<AccessDoor>().CurrState =
                AccessDoor.State.OPEN;
        }

        [MessageHandler((ushort)PacketType.OpenDoor)]
        public static void HandleDoor(ushort client, Message msg)
        {
            var packet = Deserializer.ReadDoorOpenMessage(msg);
            sceneContext.GameModel.doors[packet.id].gameObj.GetComponent<AccessDoor>().CurrState =
                AccessDoor.State.OPEN;



        }

        [MessageHandler((ushort)PacketType.SetCurrency)]
        public static void HandleMoneyChange(Message msg)
        {
            var packet = Deserializer.ReadCurrencyMessage(msg);
            sceneContext.PlayerState._model.currency = packet.newMoney;


        }

        [MessageHandler((ushort)PacketType.SetCurrency)]
        public static void HandleMoneyChange(ushort client, Message msg)
        {
            var packet = Deserializer.ReadCurrencyMessage(msg);
            sceneContext.PlayerState._model.currency = packet.newMoney;


        }

        [MessageHandler((ushort)PacketType.ActorSpawn)]

        public static void HandleActorSpawn(Message msg)
        {
            var packet = Deserializer.ReadActorSpawnMessage(msg);
            try
            {
                var sg = sceneGroups[packet.scene];

                if (actors.TryGetValue(packet.id, out var actor))
                    actors.Remove(packet.id);

                Quaternion quat = Quaternion.Euler(packet.rotation.x, packet.rotation.y, packet.rotation.z);
                var ident = identifiableTypes[packet.ident];
                var identObj = ident.prefab;

                if (identObj.GetComponent<NetworkActor>() == null)
                    identObj.AddComponent<NetworkActor>();
                if (identObj.GetComponent<NetworkActorOwnerToggle>() == null)
                    identObj.AddComponent<NetworkActorOwnerToggle>();
                if (identObj.GetComponent<TransformSmoother>() == null)
                    identObj.AddComponent<TransformSmoother>();

                identObj.GetComponent<NetworkActor>().enabled = false;
                identObj.GetComponent<TransformSmoother>().enabled = true;

                SRMP.Debug($"[{systemContext._SceneLoader_k__BackingField.CurrentSceneGroup.name} | {sg.name}]");



                var obj = RegisterActor(new ActorId(packet.id), ident, packet.position, Quaternion.identity, sg);

                identObj.RemoveComponent<NetworkActor>();
                identObj.RemoveComponent<NetworkActorOwnerToggle>();
                identObj.RemoveComponent<TransformSmoother>();

                if (obj)
                {

                    obj.AddComponent<NetworkResource>(); // Try add resource network component. Will remove if its not a resource so please do not change

                    if (!actors.ContainsKey(obj.GetComponent<IdentifiableActor>().GetActorId().Value))
                    {
                        actors.Add(obj.GetComponent<IdentifiableActor>().GetActorId().Value,
                            obj.GetComponent<NetworkActor>());
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
            var packet = Deserializer.ReadPlayerJoinMessage(msg);

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
            var packet = Deserializer.ReadPlayerLeaveMessage(msg);

            var player = players[packet.id];
            players.Remove(packet.id);
            Object.Destroy(player.gameObject);
        }

        [MessageHandler((ushort)PacketType.TimeUpdate)]
        public static void HandleTime(Message msg)
        {
            try
            {
                var packet = Deserializer.ReadTimeMessage(msg);
                sceneContext.GameModel.world.worldTime = packet.time;
            } catch { }
        }

        [MessageHandler((ushort)PacketType.FastForward)]
        public static void HandleClientSleep(ushort client, Message msg)
        {
            var packet = Deserializer.ReadSleepMessage(msg);
            sceneContext.TimeDirector.FastForwardTo(packet.time);

            ForwardMessage(packet, client);
        }

        [MessageHandler((ushort)PacketType.TempClientActorSpawn)]

        public static void HandleClientActorSpawn(ushort client, Message msg)
        {
            var packet = Deserializer.ReadActorSpawnClientMessage(msg);
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
                var nextID = sceneContext.GameModel._actorIdProvider._nextActorId;
                var obj = RegisterActor(new ActorId(nextID), ident, packet.position, rot, sg);
                identObj.RemoveComponent<NetworkActor>();
                identObj.RemoveComponent<NetworkActorOwnerToggle>();
                identObj.RemoveComponent<TransformSmoother>();
                if (obj)
                {
                    obj.AddComponent<NetworkResource>();
                    obj.GetComponent<TransformSmoother>().enabled = false;
                    obj.GetComponent<Rigidbody>().velocity = packet.velocity;
                    obj.GetComponent<NetworkActor>().startingVel = packet.velocity;
                    obj.GetComponent<TransformSmoother>().interpolPeriod = .15f;
                    obj.GetComponent<Vacuumable>()._launched = true;
                    actors.TryAdd(nextID, obj.GetComponent<NetworkActor>());
                }


                var packetR = new ActorSpawnMessage()
                {
                    id = nextID,
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
                //if (ShowErrors)
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

                DeregisterActor(new ActorId(packet.id));
                
                Object.Destroy(actor.gameObject);
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
        {
            var packet = Deserializer.ReadActorDestroyMessage(msg);
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
            catch
            {
            }
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
            catch
            {
            }

            ForwardMessage(packet, client);
        }

        [MessageHandler((ushort)PacketType.LandPlot)]
        public static void HandleLandPlot(Message msg)
        {
            var packet = Deserializer.ReadLandPlotMessage(msg);

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
            var packet = Deserializer.ReadGardenPlantMessage(msg);

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
            var packet = Deserializer.ReadLandPlotMessage(msg);

            try
            {
                var plot = sceneContext.GameModel.landPlots[packet.id].gameObj;

                if (packet.messageType == LandplotUpdateType.SET)
                {
                    handlingPacket = true;

                    plot.GetComponent<LandPlotLocation>().Replace(plot.transform.GetChild(0).GetComponent<LandPlot>(),
                        GameContext.Instance.LookupDirector._plotPrefabDict[packet.type]);

                    handlingPacket = false;
                }
                else
                {

                    var lp = plot.transform.GetChild(0).GetComponent<LandPlot>();
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
            var packet = Deserializer.ReadGardenPlantMessage(msg);

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
            var packet = Deserializer.ReadGordoEatMessage(msg);

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
            var packet = Deserializer.ReadPediaMessage(msg);

            handlingPacket = true;
            sceneContext.PediaDirector.Unlock(pediaEntries[packet.id]);
            handlingPacket = false;
        }

        [MessageHandler((ushort)PacketType.GordoExplode)]
        public static void HandleGordoBurst(Message msg)
        {
            var packet = Deserializer.ReadGordoBurstMessage(msg);

            try
            {
                var target = gameContext.LookupDirector._gordoDict[identifiableTypes[packet.ident]].GetComponent<GordoEat>().TargetCount;
                if (!sceneContext.GameModel.gordos.TryGetValue(packet.id, out var gordo))
                    sceneContext.GameModel.gordos.Add(packet.id, new GordoModel()
                    {
                        fashions = new Il2CppSystem.Collections.Generic.List<IdentifiableType>(),
                        gordoEatCount = target,
                        gordoSeen = true,
                        identifiableType = identifiableTypes[packet.ident],
                        gameObj = null,
                        GordoEatenCount = target,
                        targetCount = gameContext.LookupDirector._gordoDict[identifiableTypes[packet.ident]].GetComponent<GordoEat>().TargetCount,
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
            var packet = Deserializer.ReadGordoEatMessage(msg);

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
            var packet = Deserializer.ReadPediaMessage(msg);

            handlingPacket = true;
            sceneContext.PediaDirector.Unlock(pediaEntries[packet.id]);
            handlingPacket = false;

            ForwardMessage(packet, client);
        }

        [MessageHandler((ushort)PacketType.GordoExplode)]
        public static void HandleGordoBurst(ushort client, Message msg)
        {
            var packet = Deserializer.ReadGordoBurstMessage(msg);

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
            var packet = Deserializer.ReadAmmoAddToSlotMessage(msg);

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
            var packet = Deserializer.ReadAmmoAddMessage(msg);

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
            var packet = Deserializer.ReadAmmoRemoveMessage(msg);

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
            var packet = Deserializer.ReadAmmoAddToSlotMessage(msg);

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
            var packet = Deserializer.ReadAmmoAddMessage(msg);

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
            var packet = Deserializer.ReadAmmoRemoveMessage(msg);

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
            var packet = Deserializer.ReadActorMessage(msg);

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
            var packet = Deserializer.ReadLoadMessage(msg);
            SRMP.Debug("Finished reading save data!");
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
                    map = sceneContext.MapDirector._mapList._maps[0];
                    break;
                case MapType.Labyrinth:
                    map = sceneContext.MapDirector._mapList._maps[1];
                    break;
            }

            handlingPacket = true;
            sceneContext.MapDirector.SetPlayerNavigationMarker(packet.position, map, 0);
            handlingPacket = false;

            ForwardMessage(packet, client);
        }

        [MessageHandler((ushort)PacketType.NavigationMarkerRemove)]
        public static void HandleNavRemove(ushort client, Message joinInfo)
        {
            handlingPacket = true;
            sceneContext.MapDirector.ClearPlayerNavigationMarker();
            handlingPacket = false;

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
                    map = sceneContext.MapDirector._mapList._maps[0];
                    break;
                case MapType.Labyrinth:
                    map = sceneContext.MapDirector._mapList._maps[1];
                    break;
            }

            handlingPacket = true;
            sceneContext.MapDirector.SetPlayerNavigationMarker(packet.position, map, 0);
            handlingPacket = false;
        }

        [MessageHandler((ushort)PacketType.NavigationMarkerRemove)]
        public static void HandleNavRemove(Message joinInfo)
        {
            handlingPacket = true;
            sceneContext.MapDirector.ClearPlayerNavigationMarker();
            handlingPacket = false;
        }



        [MessageHandler((ushort)PacketType.WeatherUpdate)]
        public static void HandleWeather(Message msg)
        {
            MelonCoroutines.Start(WeatherHandlingCoroutine(Deserializer.ReadWeatherMessage(msg)));
        }

        [MessageHandler((ushort)PacketType.MarketRefresh)]
        public static void HandleMarketRefresh(Message msg)
        {
            var packet = Deserializer.ReadMarketRefreshMessage(msg);
            
            int i = 0;
            
            SRMP.Debug($"Recieved Market Price Listing Count: {packet.prices.Count}");

            foreach (var price in sceneContext.EconomyDirector._currValueMap)
            {
                try
                {
                    SRMP.Debug($"Market price listing {i}: {packet.prices[i]}");
                    price.Value.CurrValue = packet.prices[i];
                }
                catch { }
                i++;
            }
            
            marketUI?.EconUpdate();
        }

        [MessageHandler((ushort)PacketType.KillAllCommand)]
        public static void HandleKillAllCommand(Message msg)
        {
            var packet = Deserializer.ReadKillAllCommandMessage(msg);

            if (packet.actorType == -1)
            {
                foreach (var ident in sceneContext.GameModel.identifiables)
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
                
                foreach (var ident in sceneContext.GameModel.identifiables)
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
            var packet = Deserializer.ReadSwitchModifyMessage(msg);

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
            var packet = Deserializer.ReadSwitchModifyMessage(msg);

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
            var packet = Deserializer.ReadMapUnlockMessage(msg);

            sceneContext.MapDirector.NotifyZoneUnlocked(GetGameEvent(packet.id), false, 0);
            
            var eventDirModel = sceneContext.eventDirector._model;
            if (!eventDirModel.table.TryGetValue("fogRevealed", out var table))
            {
                eventDirModel.table.Add("fogRevealed", new Il2CppSystem.Collections.Generic.Dictionary<string, EventRecordModel.Entry>());
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
            var packet = Deserializer.ReadMapUnlockMessage(msg);

            sceneContext.MapDirector.NotifyZoneUnlocked(GetGameEvent(packet.id), false, 0);
            
            var eventDirModel = sceneContext.eventDirector._model;
            if (!eventDirModel.table.TryGetValue("fogRevealed", out var table))
            {
                eventDirModel.table.Add("fogRevealed", new Il2CppSystem.Collections.Generic.Dictionary<string, EventRecordModel.Entry>());
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

