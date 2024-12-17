using Il2CppMonomiPark.SlimeRancher.Persist;
using Il2CppSystem.Text;
using Stream = Il2CppSystem.IO.Stream;
namespace NewSR2MP.Networking.SaveModels
{
    public class NetworkPersistedDataSet : PersistedDataSet
    {
        public override void Write(Stream stream)
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
    }
}
