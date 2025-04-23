
using System.Collections.Generic;
using System.Drawing;
using MelonLoader;
using SR2E;
using SR2E.Managers;
using SR2E.Menus;
using Color = UnityEngine.Color;

namespace NewSR2MP
{
    public class SRMP
    {

        private static MelonLogger.Instance logger;

        static SRMP()
        {
            logger = new MelonLogger.Instance("New SR2MP");
        }
        
        public static void Log(string message)
        {
            Log(message, 100);
        }
        public static void Log(string message, int size)
        {
            SR2ELogManager.SendMessage($"<size={size}%>{message}</size>");
            logger.Msg(message);
        }
        
        public static void Error(string message)
        {
            SR2ELogManager.SendError(message);
            logger.Error(message);
        }
        
        public static void Warn(string message)
        {
            SR2ELogManager.SendWarning(message);
            logger.Warning(message);
        }

        public static void Debug(string message)
        {
            if (!DEBUG_MODE)
                return;
            
            GM<SR2EConsole>().Send(message, new Color(0, 127, 255));
            logger.Msg(System.Drawing.Color.Aqua, message);
        }
    }
}
