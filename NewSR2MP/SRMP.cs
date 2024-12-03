using Mirror;
using System.Collections.Generic;
using MelonLoader;

namespace NewSR2MP
{
    public class SRMP
    {
        // Not used, but i would like if it wasnt removed, as it helps with debugging.
        internal static ushort MessageId<M>() where M: struct, NetworkMessage => NetworkMessageId<M>.Id;

        public static void Log(string message)
        {
            MelonLogger.Msg($"[NewSR2MP] {message}");
        }
        
        public static void Error(string message)
        {
            MelonLogger.Error($"[NewSR2MP] {message}");
        }
        
        public static void Warn(string message)
        {
            MelonLogger.Warning($"[NewSR2MP] {message}");
        }
    }
}
