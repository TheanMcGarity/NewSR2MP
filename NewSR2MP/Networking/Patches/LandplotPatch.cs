using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;

using Il2CppMonomiPark.SlimeRancher.DataModel;
using NewSR2MP.Networking.Component;
using NewSR2MP.Networking.Packet;
using UnityEngine;
namespace NewSR2MP.Networking.Patches
{
    [HarmonyPatch(typeof(LandPlot), nameof(LandPlot.AddUpgrade))]
    public class LandPlotApplyUpgrades
    {
        public static void Prefix(LandPlot __instance, LandPlot.Upgrade upgrade)
        {
            try
            {
                if ((ServerActive() || ClientActive()) && !__instance.GetComponent<HandledDummy>())
                {
                    var packet = new LandPlotMessage()
                    {
                        id = __instance._model.gameObj.GetComponent<LandPlotLocation>()._id,
                        upgrade = upgrade,
                        messageType = LandplotUpdateType.UPGRADE
                    };

                    MultiplayerManager.NetworkSend(packet);
                }
            }
            catch { }
        }

    }
    [HarmonyPatch(typeof(LandPlot), nameof(LandPlot.DestroyAttached))]
    public class LandPlotDestroyAttached
    {
        public static void Postfix(LandPlot __instance)
        {
            try
            {
                if ((ServerActive() || ClientActive()) && !__instance.GetComponent<HandledDummy>())
                {
                    var packet = new GardenPlantMessage()
                    {
                        id = __instance._model.gameObj.GetComponent<LandPlotLocation>()._id,
                        ident = 9,
                        replace = true,
                    };

                    MultiplayerManager.NetworkSend(packet);
                }
            }
            catch { }
        }

    }
}
