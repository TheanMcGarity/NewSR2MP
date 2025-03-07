using Il2CppSystem.Text;
using Il2CppSystem.IO;

namespace NewSR2MP.Networking.SaveModels
{
    public class NetworkPersistedDataSet
    {
        public virtual uint Version => 0;
        public virtual string Identifier => "";
        public void Write(Il2CppSystem.IO.Stream stream)
        {
            
            Encoding utf = Encoding.UTF8;
            GameBinaryWriter binaryWriter = new GameBinaryWriter(stream, utf);
            string identifier = Identifier;
            binaryWriter.Write(identifier);
            uint version = Version;
            binaryWriter.Write(version);
            WriteData(binaryWriter);
            binaryWriter.Dispose();
        }

        public virtual void WriteData(GameBinaryWriter binaryWriter)
        {
            SRMP.Debug("Encountered unoverridden WriteData() function!");
        }

        public virtual void LoadData(GameBinaryReader binaryWriter)
        {
            SRMP.Debug("Encountered unoverridden WriteData() function!");
        }
        public void Load(Il2CppSystem.IO.Stream stream)
        {
            Encoding encoding = Encoding.UTF8;
            GameBinaryReader reader = new GameBinaryReader(stream, encoding);

            var identifier = reader.ReadString();
            var ver = reader.ReadUInt32();
            
            if (ver != Version)
                SRMP.Warn("Version mismatch on loading save data!");
            if (identifier != Identifier)
                SRMP.Warn("Save component identifier mismatch on loading save data!");
            
            LoadData(reader);
        }
    }
}
