using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using Mirror;
using Il2CppMonomiPark.SlimeRancher.DataModel;
using NewSR2MP.Networking.Component;
using NewSR2MP.Networking.Packet;
using UnityEngine;
namespace NewSR2MP.Networking.Patches
{
    [HarmonyPatch(typeof(TutorialDirector), nameof(TutorialDirector.MaybeShowPopup))]
    public class TutorialDirectorShowPopup
    {
        public static bool Prefix(TutorialDirector __instance, TutorialDirector.Id id) => !SRMLConfig.DEBUG_STOP_TUTORIALS;
    }
}
