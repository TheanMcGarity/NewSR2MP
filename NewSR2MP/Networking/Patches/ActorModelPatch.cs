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
    [HarmonyPatch(typeof(ActorModel), nameof(ActorModel.GetPos))]
    public class ActorModelGetPos
    {
        public static bool Prefix(ActorModel __instance, ref Vector3 __result)
        {
            try
            {
                __result = __instance.transform.position;
                return false;
            }
            catch
            {
                if (SHOW_ERRORS)
                {
                    SRMP.Log($"Error when getting actor position (probably during saving!)\n{StackTraceUtility.ExtractStackTrace()}");
                }
            }
            __result = Vector3.zero;
            return false;
        }

    }
    [HarmonyPatch(typeof(ActorModel), nameof(ActorModel.GetRot))]
    public class ActorModelGetRot
    {
        public static bool Prefix(ActorModel __instance, ref Quaternion __result)
        {
            try
            {
                __result = __instance.transform.rotation;
            }
            catch
            {
                if (SHOW_ERRORS)
                {
                    SRMP.Log($"Error when getting actor position (probably during saving!)\n{StackTraceUtility.ExtractStackTrace()}");
                }
            }
            return false;
        }

    }
}
