
using Il2CppMonomiPark.SlimeRancher.Persist;
using System;
using System.Collections.Generic;
using Il2CppSystem.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;
namespace NewSR2MP.Networking.SaveModels
{
    /// <summary>
    /// TODO: Add health, stamina, and rads.
    /// </summary>
    public class NetPlayerV01 : NetworkPersistedDataSet
    {
        public override string Identifier => "MPPL";
        public override uint Version => 1;

        public byte sceneGroup = 1;
        
        public Vector3V01 position;
        public Vector3V01 rotation;

        public NetPlayerV01()
        {
            position = new Vector3V01();
            position.Value = new Vector3(541.6466f, 18.646f, 349.3299f);
            rotation = new Vector3V01();
            rotation.Value = Vector3.up * 236.8107f;

        }
        public int money = 250;

        public Il2CppSystem.Collections.Generic.List<AmmoDataV01> ammo = new Il2CppSystem.Collections.Generic.List<AmmoDataV01>();

        public List<string> upgrades = new List<string>();

        public Guid playerID;


        public static NetPlayerV01 Load(GameBinaryReader reader)
        {
            var netPlayer = new NetPlayerV01();
            netPlayer.Load(reader.BaseStream);
            return netPlayer;
        }

        public override void LoadData(GameBinaryReader reader)
        {
            sceneGroup = reader.ReadByte();
            
            position = Vector3V01.Load(reader);
            rotation = Vector3V01.Load(reader);

            money = reader.ReadInt32();

            var ammoModeC = reader.ReadInt32();


            var ammoDataC = reader.ReadInt32();

            ammo = new Il2CppSystem.Collections.Generic.List<AmmoDataV01>();

            for (int x = 0; x < ammoDataC; x++)
            {
                var ammoSlot = new AmmoDataV01();
                ammoSlot.LoadData(reader);

                ammo.Add(ammoSlot);
            }

            var upgradeC = reader.ReadInt32();

            upgrades = new List<string>();

            for (int i = 0; i < upgradeC; i++)
            {
                upgrades.Add(reader.ReadString());
            }

            playerID = Guid.Parse(reader.ReadString());
        }

        public override void WriteData(GameBinaryWriter writer)
        {
            writer.Write(sceneGroup);

            position.WriteData(writer);
            rotation.WriteData(writer);

            writer.Write(money);

            writer.Write(ammo.Count);

            writer.Write(ammo.Count);
            foreach (var ammoSlot in ammo)
            {
                ammoSlot.WriteData(writer);
            }

            writer.Write(upgrades.Count);

            foreach (var upgrade in upgrades)
            {
                writer.Write(upgrade);
            }
            
        }
    }
}
