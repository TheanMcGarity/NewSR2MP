using Il2CppMonomiPark.SlimeRancher.Persist;
using System.IO;

namespace NewSR2MP.SaveModels
{

    // To future self; make sure you use VersionedPersistedDataSet<NetworkV01> for upgrading the network version
    // Also please take changelogs for it as well.
    
    public class NetworkV01 : NetworkPersistedDataSet
    {
        public override string Identifier => "MPNK";
        public override uint Version => 1;

        public bool sharedMoney = true;

        public bool sharedKeys = true;
        
        public bool sharedUpgrades = true;

        public PlayerListV01 savedPlayers = new PlayerListV01();

        public override void LoadData(GameBinaryReader reader)
        {
            sharedMoney = reader.ReadBoolean();
            sharedKeys = reader.ReadBoolean();
            sharedUpgrades = reader.ReadBoolean();
        
            savedPlayers = PlayerListV01.Load(reader);
        }

        public override void WriteData(GameBinaryWriter writer)
        {
            writer.Write(sharedMoney);
            writer.Write(sharedKeys);
            writer.Write(sharedUpgrades);

            savedPlayers.Write(writer);
        }
    }
}
