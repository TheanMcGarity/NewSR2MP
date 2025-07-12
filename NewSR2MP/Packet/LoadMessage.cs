using UnityEngine;

namespace NewSR2MP.Packet
{
    public class LoadMessage : IPacket
    {
        public List<InitActorData> initActors;
        public List<InitPlayerData> initPlayers;
        public List<InitPlotData> initPlots;
        //public List<InitGadgetData> initGadgets;
        public List<InitAccessData> initAccess;
        public List<InitSwitchData> initSwitches;
        public Dictionary<int, TreasurePod.State> initPods;
        //public List<InitResourceNodeData> initResourceNodes;

        public List<string> initPedias;
        public List<string> initMaps;

        public HashSet<InitGordoData> initGordos;

        public LocalPlayerData localPlayerSave;
        public int playerID;
        
        public int money;
        public Dictionary<byte, sbyte> upgrades;
        public double time;
        
        public List<float> marketPrices = new();
        public Dictionary<int, int> refineryItems = new();

        public PacketType Type => PacketType.JoinSave;
        public PacketReliability Reliability => PacketReliability.ReliableUnordered;

        public void Serialize(OutgoingMessage msg)
        {
            msg.Write(initActors.Count);
            foreach (var actor in initActors)
            {
                msg.Write(actor.id);
                msg.Write(actor.ident);
                msg.Write(actor.scene);
                msg.Write(actor.pos);
            }
            msg.Write(initPlayers.Count);
            foreach (var player in initPlayers)
            {
                msg.Write(player.id);
                msg.Write(player.username);
            }
            msg.Write(initPlots.Count);
            foreach (var plot in initPlots)
            {
                msg.Write(plot.id);
                msg.Write((int)plot.type); 
                msg.Write(plot.upgrades.Count);

                foreach (var upg in plot.upgrades)
                {
                    msg.Write((int)upg);
                }

                msg.Write(plot.siloData.Count);
                foreach (var silo in plot.siloData)
                {
                    msg.Write(silo.Key);
                    msg.Write(silo.Value.slots);

                    msg.Write(silo.Value.ammo.Count);
                    foreach (var ammo in silo.Value.ammo)
                    {
                        msg.Write(ammo);
                    }
                }
                msg.Write(plot.cropIdent);
            }
            msg.Write(initGordos.Count);
            foreach (var gordo in initGordos)
            {
                msg.Write(gordo.id);
                msg.Write(gordo.eaten);
                msg.Write(gordo.ident);
                msg.Write(gordo.targetCount);
            }
            msg.Write(initPedias.Count);
            foreach (var pedia in initPedias)
            {
                msg.Write(pedia);
            }
            msg.Write(initMaps.Count);
            foreach (var map in initMaps)
            {
                msg.Write(map);
            }
            msg.Write(initAccess.Count);
            foreach (var access in initAccess)
            {
                msg.Write(access.id);
                msg.Write(access.open);
            }

            msg.Write(playerID);
            msg.Write(localPlayerSave.pos);
            msg.Write(localPlayerSave.rot);
            msg.Write(localPlayerSave.ammo.Count);

            foreach (var amm in localPlayerSave.ammo)
            {
                msg.Write(amm);
            }
            msg.Write(localPlayerSave.sceneGroup);
            
            msg.Write(money);

            msg.Write(upgrades.Count);
            foreach (var upgrade in upgrades)
            {
                msg.Write(upgrade.Key);
                msg.Write(upgrade.Value);
            }
            

            msg.Write(time);

            msg.Write(marketPrices.Count);
            foreach (var price in marketPrices)
                msg.Write(price);
            
            msg.Write(refineryItems.Count);
            foreach (var item in refineryItems)
            {
                msg.Write(item.Key);
                msg.Write(item.Value);
            }

            msg.Write(initSwitches.Count);
            foreach (var _switch in initSwitches)
            {
                msg.Write(_switch.id);
                msg.Write(_switch.state);
            }
            
            msg.Write(initPods.Count);
            foreach (var pod in initPods)
            {
                msg.Write(pod.Key);
                msg.Write((byte)pod.Value);
            }
        }

        public void Deserialize(IncomingMessage msg)
        {
            int lengthActor = msg.ReadInt32();

            initActors = new List<InitActorData>();
            for (int i = 0; i < lengthActor; i++)
            {
                long id = msg.ReadInt64();
                int ident = msg.ReadInt32();
                int sg = msg.ReadInt32();
                Vector3 actorPos = msg.ReadVector3();
                initActors.Add(new InitActorData
                {
                    id = id,
                    ident = ident,
                    scene = sg,
                    pos = actorPos
                });
            }

            int lengthPlayer = msg.ReadInt32();
            initPlayers = new List<InitPlayerData>();
            for (int i = 0; i < lengthPlayer; i++)
            {
                int id = msg.ReadInt32();
                string username = msg.ReadString();
                initPlayers.Add(new InitPlayerData()
                {
                    id = id,
                    username = username,
                });
            }

            int lengthPlot = msg.ReadInt32();
            initPlots = new List<InitPlotData>();
            for (int i = 0; i < lengthPlot; i++)
            {
                string id = msg.ReadString();
                LandPlot.Id type = (LandPlot.Id)msg.ReadInt32();
                int upgLength = msg.ReadInt32();
                Il2CppSystem.Collections.Generic.HashSet<LandPlot.Upgrade> upgrades =
                    new Il2CppSystem.Collections.Generic.HashSet<LandPlot.Upgrade>();
                for (int i2 = 0; i2 < upgLength; i2++)
                {
                    upgrades.Add((LandPlot.Upgrade)msg.ReadInt32());
                }

                Dictionary<string, InitSiloData> silos = new();
                int inventories = msg.ReadInt32();
                for (int j = 0; j < inventories; j++)
                {
                    string inventoryID = msg.ReadString();
                    int slots = msg.ReadInt32();
                    int ammLength = msg.ReadInt32();
                    HashSet<AmmoData> ammoDatas = new HashSet<AmmoData>();
                    for (int i2 = 0; i2 < ammLength; i2++)
                    {
                        var data = msg.ReadAmmoData();
                        ammoDatas.Add(data);
                    }

                    silos.Add(inventoryID, new InitSiloData
                    {
                        slots = slots,
                        ammo = ammoDatas
                    });
                }
                var crop = msg.ReadInt32();
                initPlots.Add(new InitPlotData()
                {
                    type = type,
                    id = id,
                    upgrades = upgrades,
                    siloData = silos,
                    cropIdent = crop
                });
            }

            int lengthGordo = msg.ReadInt32();
            initGordos = new HashSet<InitGordoData>();
            for (int i = 0; i < lengthGordo; i++)
            {
                string id = msg.ReadString();
                int eaten = msg.ReadInt32();
                int ident = msg.ReadInt32();
                int target = msg.ReadInt32();
                initGordos.Add(new InitGordoData()
                {
                    id = id,
                    eaten = eaten,
                    ident = ident,
                    targetCount = target,
                });
            }

            int pedLength = msg.ReadInt32();
            initPedias = new List<string>();
            for (int i = 0; i < pedLength; i++)
            {
                initPedias.Add(msg.ReadString());
            }

            int mapLength = msg.ReadInt32();
            initMaps = new List<string>();
            for (int i = 0; i < mapLength; i++)
            {
                initMaps.Add(msg.ReadString());
            }

            int accLength = msg.ReadInt32();
            initAccess = new List<InitAccessData>();
            for (int i = 0; i < accLength; i++)
            {
                string id = msg.ReadString();
                bool open = msg.ReadBoolean();
                InitAccessData accessData = new InitAccessData()
                {
                    id = id,
                    open = open,
                };
                initAccess.Add(accessData);
            }

            playerID = msg.ReadInt32();
            var pos = msg.ReadVector3();
            var rot = msg.ReadVector3();

            var localAmmoCount = msg.ReadInt32();

            List<AmmoData> localAmmo = new List<AmmoData>();
            for (int i = 0; i < localAmmoCount; i++)
            {
                localAmmo.Add(msg.ReadAmmoData());
            }

            int scene = msg.ReadInt32();

            localPlayerSave = new LocalPlayerData()
            {
                pos = pos,
                rot = rot,
                ammo = localAmmo,
                sceneGroup = scene
            };


            money = msg.ReadInt32();

            var pUpgradesCount = msg.ReadInt32();
            upgrades = new(pUpgradesCount);

            for (int i = 0; i < pUpgradesCount; i++)
            {
                var key = msg.ReadByte();
                var val = msg.ReadSByte();

                upgrades.TryAdd(key, val);
            }

            time = msg.ReadDouble();

            var marketCount = msg.ReadInt32();
            marketPrices = new List<float>(marketCount);

            for (int i = 0; i < marketCount; i++)
                marketPrices.Add(msg.ReadFloat());

            var refineryCount = msg.ReadInt32();
            refineryItems = new Dictionary<int, int>(refineryCount);

            for (int i = 0; i < refineryCount; i++)
                refineryItems.Add(msg.ReadInt32(), msg.ReadInt32());


            initSwitches = new List<InitSwitchData>();
            var switchCount = msg.ReadInt32();
            for (int i = 0; i < switchCount; i++)
                initSwitches.Add(new InitSwitchData
                {
                    id = msg.ReadString(),
                    state = msg.ReadByte()
                });

        }
    }

    public class InitActorData
    {
        public long id;
        public int ident;
        public int scene;
        public Vector3 pos;
    }
    public class InitGordoData
    {
        public string id;
        public int eaten;
        public int ident;
        public int targetCount;
    }
    /*public class InitGadgetData
    {
        public thingy gadgetData;

        public string id;
        public string gadget;
    }*/

    public class InitAccessData
    {
        public string id;
        public bool open;
    }
    public class InitPlotData
    {
        public string id;
        public LandPlot.Id type;
        public Il2CppSystem.Collections.Generic.HashSet<LandPlot.Upgrade> upgrades;
        public int cropIdent;

        public Dictionary<string, InitSiloData> siloData;
    }

    public class InitSiloData
    {
        public int slots;

        public HashSet<AmmoData> ammo;
    }

    public class AmmoData
    {
        public int id;
        public int count;
        public int slot;
    }

    public class InitPlayerData
    {
        public int id;
        public string username;
    }
    public class InitSwitchData
    {
        public string id;
        public byte state;
    }
    public class InitResourceNodeData
    {
        public long id;
        public byte definition;
    }
    public class LocalPlayerData
    {
        public Vector3 pos;
        public Vector3 rot;

        public int sceneGroup;
        
        public List<AmmoData> ammo;
    }
}
