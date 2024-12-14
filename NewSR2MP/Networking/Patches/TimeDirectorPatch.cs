using HarmonyLib;

using NewSR2MP.Networking.Component;
using NewSR2MP.Networking.Packet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewSR2MP.Networking.Patches
{
    [HarmonyPatch(typeof(TimeDirector), nameof(TimeDirector.FastForwardTo))]
    internal class TimeDirectorFastForwardTo
    {
        public static bool Prefix (TimeDirector __instance, double fastForwardUntil)
        {
            if (ClientActive() && !ServerActive())
            {
                var packet = new SleepMessage()
                {
                    time = fastForwardUntil
                };
                MultiplayerManager.NetworkSend(packet);
                return false;
            }
            return true;
        }
    }
}
