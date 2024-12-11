using Mirror;
using System.Collections.Generic;
using System.Drawing;
using MelonLoader;

namespace NewSR2MP
{
    public class SRMP
    {
        // Not used, but i would like if it wasnt removed, as it helps with debugging.
        internal static ushort MessageId<M>() where M: struct, NetworkMessage => NetworkMessageId<M>.Id;

        private static MelonLogger.Instance logger;

        static SRMP()
        {
            logger = new MelonLogger.Instance("New SR2MP");
        }
        
        public static void Log(string message)
        {
            logger.Msg(message);
        }
        
        public static void Error(string message)
        {
            logger.Error(message);
        }
        
        public static void Warn(string message)
        {
            logger.Warning(message);
        }
        public static void Debug(string message)
        {
            logger.Msg(Color.Aqua, message);
        }
    }
}
