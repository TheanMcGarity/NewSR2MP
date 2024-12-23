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

        public List<string> initPedias;
        public List<string> initMaps;

        public HashSet<InitGordoData> initGordos;

        public LocalPlayerData localPlayerSave;
        public int playerID;
        
        public int money;
        public Il2CppSystem.Collections.Generic.Dictionary<int, int> upgrades; // Needs to be Il2Cpp so it can be moved right into the player upgrades model.
        public double time;
    
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
            foreach (var upg in upgrades)
            {
                msg.AddInt(upg.key);
                msg.AddInt(upg.value);
            }

            msg.AddDouble(time);

            return msg;
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
    }
    public class LocalPlayerData
    {
        public Vector3 pos;
        public Vector3 rot;

        public int sceneGroup;
        
        public List<AmmoData> ammo;
    }
}
