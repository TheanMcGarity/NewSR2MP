using UnityEngine;

namespace NewSR2MP.Networking.Packet
{
    public struct LoadMessage : NetworkMessage
    {
        public List<InitActorData> initActors;
        public List<InitPlayerData> initPlayers;
        public List<InitPlotData> initPlots;
        public HashSet<InitGordoData> initGordos;
        //public List<InitGadgetData> initGadgets;

        public List<string> initPedias;
        public List<string> initMaps;

        public List<InitAccessData> initAccess;

        public LocalPlayerData localPlayerSave;
        public int playerID;
        public int money;
        public int keys;
        public List<string> upgrades;
        public double time;

        public bool sharedMoney;
        public bool sharedKeys;
        public bool sharedUpgrades;
    }

    public struct InitActorData
    {
        public long id;
        public string ident;
        public Vector3 pos;
    }
    public struct InitGordoData
    {
        public string id;
        public int eaten;
    }
    /*public struct InitGadgetData
    {
        public thingy gadgetData;

        public string id;
        public string gadget;
    }*/

    public struct InitAccessData
    {
        public string id;
        public bool open;
    }
    public struct InitPlotData
    {
        public string id;
        public LandPlot.Id type;
        public HashSet<LandPlot.Upgrade> upgrades;
        public string cropIdent;

        public InitSiloData siloData;
    }

    public struct InitSiloData
    {
        public int slots;

        public HashSet<AmmoData> ammo;
    }

    public struct AmmoData
    {
        public string id;
        public int count;
        public int slot;
    }

    public struct InitPlayerData
    {
        public int id;
    }
    public struct LocalPlayerData
    {
        public Vector3 pos;
        public Vector3 rot;

        public List<AmmoData> ammo;
    }
}
