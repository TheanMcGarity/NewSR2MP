using UnityEngine;

namespace NewSR2MP.Networking.Packet
{
    public class LoadMessage : ICustomMessage
    {
        public List<InitActorData> initActors;
        public List<InitPlayerData> initPlayers;
        public List<InitPlotData> initPlots;
        //public List<InitGadgetData> initGadgets;
        public List<InitAccessData> initAccess;
        public List<InitSwitchData> initSwitches;
        //public List<InitResourceNodeData> initResourceNodes;

        public List<string> initPedias;
        public List<string> initMaps;

        public HashSet<InitGordoData> initGordos;

        public LocalPlayerData localPlayerSave;
        public int playerID;
        
        public int money;
        public Dictionary<byte, byte> upgrades;
        public double time;
        
        public List<float> marketPrices = new();
        public Dictionary<int, int> refineryItems = new();
        
        public Message Serialize()
        {
            Message msg = Message.Create(MessageSendMode.Reliable, PacketType.JoinSave);
            
            
            msg.AddInt(initActors.Count);
            foreach (var actor in initActors)
            {
                msg.AddLong(actor.id);
                msg.AddInt(actor.ident);
                msg.AddInt(actor.scene);
                msg.AddVector3(actor.pos);
            }
            msg.AddInt(initPlayers.Count);
            foreach (var player in initPlayers)
            {
                msg.AddInt(player.id);
                msg.AddString(player.username);
            }
            msg.AddInt(initPlots.Count);
            foreach (var plot in initPlots)
            {
                msg.AddString(plot.id);
                msg.AddInt((int)plot.type); 
                msg.AddInt(plot.upgrades.Count);

                foreach (var upg in plot.upgrades)
                {
                    msg.AddInt((int)upg);
                }
                msg.AddInt(plot.siloData.slots);

                msg.AddInt(plot.siloData.ammo.Count);
                foreach (var ammo in plot.siloData.ammo)
                {
                    msg.AddAmmoData(ammo);
                }
                msg.AddInt(plot.cropIdent);
            }
            msg.AddInt(initGordos.Count);
            foreach (var gordo in initGordos)
            {
                msg.AddString(gordo.id);
                msg.AddInt(gordo.eaten);
                msg.AddInt(gordo.ident);
            }
            msg.AddInt(initPedias.Count);
            foreach (var pedia in initPedias)
            {
                msg.AddString(pedia);
            }
            msg.AddInt(initMaps.Count);
            foreach (var map in initMaps)
            {
                msg.AddString(map);
            }
            msg.AddInt(initAccess.Count);
            foreach (var access in initAccess)
            {
                msg.AddString(access.id);
                msg.AddBool(access.open);
            }

            msg.AddInt(playerID);
            msg.AddVector3(localPlayerSave.pos);
            msg.AddVector3(localPlayerSave.rot);
            msg.AddInt(localPlayerSave.ammo.Count);

            foreach (var amm in localPlayerSave.ammo)
            {
                msg.AddAmmoData(amm);
            }
            msg.AddInt(localPlayerSave.sceneGroup);
            
            msg.AddInt(money);

            msg.AddInt(upgrades.Count);
            foreach (var upgrade in upgrades)
            {
                msg.AddByte(upgrade.Key);
                msg.AddByte(upgrade.Value);
            }
            

            msg.AddDouble(time);

            msg.AddInt(marketPrices.Count);
            foreach (var price in marketPrices)
                msg.AddFloat(price);
            
            msg.AddInt(refineryItems.Count);
            foreach (var item in refineryItems)
            {
                msg.AddInt(item.Key);
                msg.AddInt(item.Value);
            }

            msg.AddInt(initSwitches.Count);
            foreach (var _switch in initSwitches)
            {
                msg.AddString(_switch.id);
                msg.AddByte(_switch.state);
            }
            
            return msg;
        }

        public void Deserialize(Message msg)
        {
            int lengthActor = msg.GetInt();

            initActors = new List<InitActorData>();
            for (int i = 0; i < lengthActor; i++)
            {
                long id = msg.GetLong();
                int ident = msg.GetInt();
                int sg = msg.GetInt();
                Vector3 actorPos = msg.GetVector3();
                initActors.Add(new InitActorData
                {
                    id = id,
                    ident = ident,
                    scene = sg,
                    pos = actorPos
                });
            }

            int lengthPlayer = msg.GetInt();
            initPlayers = new List<InitPlayerData>();
            for (int i = 0; i < lengthPlayer; i++)
            {
                int id = msg.GetInt();
                string username = msg.GetString();
                initPlayers.Add(new InitPlayerData()
                {
                    id = id
                });
            }

            int lengthPlot = msg.GetInt();
            initPlots = new List<InitPlotData>();
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
                initPlots.Add(new InitPlotData()
                {
                    type = type,
                    id = id,
                    upgrades = upgrades,
                    siloData = siloData,
                    cropIdent = crop
                });
            }

            int lengthGordo = msg.GetInt();
            initGordos = new HashSet<InitGordoData>();
            for (int i = 0; i < lengthGordo; i++)
            {
                string id = msg.GetString();
                int eaten = msg.GetInt();
                int ident = msg.GetInt();
                initGordos.Add(new InitGordoData()
                {
                    id = id,
                    eaten = eaten,
                    ident = ident,
                });
            }

            int pedLength = msg.GetInt();
            initPedias = new List<string>();
            for (int i = 0; i < pedLength; i++)
            {
                initPedias.Add(msg.GetString());
            }

            int mapLength = msg.GetInt();
            initMaps = new List<string>();
            for (int i = 0; i < mapLength; i++)
            {
                initMaps.Add(msg.GetString());
            }

            int accLength = msg.GetInt();
            initAccess = new List<InitAccessData>();
            for (int i = 0; i < accLength; i++)
            {
                string id = msg.GetString();
                bool open = msg.GetBool();
                InitAccessData accessData = new InitAccessData()
                {
                    id = id,
                    open = open,
                };
                initAccess.Add(accessData);
            }

            playerID = msg.GetInt();
            var pos = msg.GetVector3();
            var rot = msg.GetVector3();

            var localAmmoCount = msg.GetInt();

            List<AmmoData> localAmmo = new List<AmmoData>();
            for (int i = 0; i < localAmmoCount; i++)
            {
                localAmmo.Add(msg.GetAmmoData());
            }

            int scene = msg.GetInt();

            localPlayerSave = new LocalPlayerData()
            {
                pos = pos,
                rot = rot,
                ammo = localAmmo,
                sceneGroup = scene
            };


            money = msg.GetInt();

            var pUpgradesCount = msg.GetInt();
            upgrades = new(pUpgradesCount);

            for (int i = 0; i < pUpgradesCount; i++)
            {
                var key = msg.GetByte();
                var val = msg.GetByte();

                upgrades.TryAdd(key, val);
            }

            time = msg.GetDouble();

            var marketCount = msg.GetInt();
            marketPrices = new List<float>(marketCount);

            for (int i = 0; i < marketCount; i++)
                marketPrices.Add(msg.GetFloat());

            var refineryCount = msg.GetInt();
            refineryItems = new Dictionary<int, int>(refineryCount);

            for (int i = 0; i < refineryCount; i++)
                refineryItems.Add(msg.GetInt(), msg.GetInt());


            initSwitches = new List<InitSwitchData>();
            var switchCount = msg.GetInt();
            for (int i = 0; i < switchCount; i++)
                initSwitches.Add(new InitSwitchData
                {
                    id = msg.GetString(),
                    state = msg.GetByte()
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

        public InitSiloData siloData;
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
