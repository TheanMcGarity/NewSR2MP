using HarmonyLib;

using NewSR2MP.Component;
using NewSR2MP.Packet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewSR2MP.Patches
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
                    targetTime = fastForwardUntil
                };
                MultiplayerManager.NetworkSend(packet);
                return false;
            }
            return true;
        }
    }
}
