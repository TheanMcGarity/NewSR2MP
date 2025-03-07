var infoVER = File.Open("../NewSR2MP/asmver.txt", FileMode.Open);

int ver = 0;
using (BinaryReader reader = new BinaryReader(infoVER))
    ver = int.Parse(new string(reader.ReadChars(4)));

ver++;

using (BinaryWriter writer = new BinaryWriter(File.Open("../NewSR2MP/asmver.txt", FileMode.Create)))
    writer.Write(ver.ToString().ToArray());

string infoCS = File.ReadAllText("../NewSR2MP/Properties/AssemblyInfo.cs");
string newVersion = $"0.0.0.{ver}";
string newContent = infoCS
    .Replace($"[assembly: AssemblyVersion(\"0.0.0.{ver - 1}", $"[assembly: AssemblyVersion(\"{newVersion}")
    .Replace($"[assembly: AssemblyFileVersion(\"0.0.0.{ver - 1}", $"[assembly: AssemblyFileVersion(\"{newVersion}")
    .Replace($"\"{ver - 1}\"", $"\"{ver}\"");

File.WriteAllText("../NewSR2MP/Properties/AssemblyInfo.cs", newContent);
