using Il2CppMonomiPark.SlimeRancher.Persist;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewSR2MP.Networking.SaveModels
{
    
    [RegisterTypeInIl2Cpp(false)]
    public class PlayerListV01 : NetworkPersistedDataSet
    {

        public override string Identifier => "MPLI";
        public override uint Version => 1;

        public Dictionary<Guid,NetPlayerV01> playerList = new Dictionary<Guid, NetPlayerV01>();

        public override void LoadData(GameBinaryReader reader)
        {
            var count = reader.ReadInt32();
            var set = new Dictionary<Guid, NetPlayerV01>();

            for (int i = 0; i < count; i++)
            {
                var guid = Guid.Parse(reader.ReadString());
                var player = NetPlayerV01.Load(reader);

                set.Add(guid,player);
            }
        }

        public override void WriteData(GameBinaryWriter writer)
        {
            writer.Write(playerList.Count);
            foreach (var player in playerList)
            {
                writer.Write(player.Key.ToString());
                player.Value.WriteData(writer);
            }
        }


        public static PlayerListV01 Load(GameBinaryReader reader)
        {
            var list = new PlayerListV01();
            list.Load(reader.BaseStream);
            return list;
        }
    }
}
