using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using Mirror;
using NewSR2MP.Networking.Component;
using UnityEngine;
namespace NewSR2MP.Networking.Patches
{
    [HarmonyPatch(typeof(SiloStorage), nameof(SiloStorage.InitAmmo))]
    public class SiloStorageInitAmmo
    {
        public static bool Prefix(SiloStorage __instance)
        {
            try
            {
                var aid = __instance.transform.GetComponentInParent<LandPlotLocation>().id;

                if (NetworkAmmo.all.ContainsKey(aid))
                {
                    aid += "_1";
                }
                if (NetworkAmmo.all.ContainsKey(aid))
                {
                    aid += "-2";
                }
                if (NetworkAmmo.all.ContainsKey(aid))
                {
                    aid += "-3";
                }
                if (NetworkAmmo.all.ContainsKey(aid))
                {
                    aid += "-4";
                }
                if (NetworkAmmo.all.ContainsKey(aid))
                {
                    aid += "-5";
                } // idk why this happens, i only ever indended for it to go to _1.
                if (__instance.ammo == null)
                {
                    
                    __instance.ammo = new NetworkAmmo(aid, __instance.type.GetContents(), __instance.numSlots, __instance.numSlots, new Predicate<Identifiable.Id>[__instance.numSlots], (Identifiable.Id id, int index) => __instance.maxAmmo);
                }
                else if (!(__instance.ammo is NetworkAmmo))
                {
                    __instance.ammo = new NetworkAmmo(aid, __instance.type.GetContents(), __instance.numSlots, __instance.numSlots, new Predicate<Identifiable.Id>[__instance.numSlots], (Identifiable.Id id, int index) => __instance.maxAmmo);
                }
                return false;
            }
            catch (Exception e)
            {
                if (SHOW_ERRORS) SRMP.Log($"Error in network ammo!\n{e}\nThis can cause major desync!");
            }
            return true;
        }
    }
}
