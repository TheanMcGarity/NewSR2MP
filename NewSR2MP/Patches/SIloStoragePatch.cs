using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;

using NewSR2MP.Networking.Component;
using UnityEngine;
namespace NewSR2MP.Networking.Patches
{
    [HarmonyPatch(typeof(SiloStorage), nameof(SiloStorage.InitAmmo))]
    public class SiloStorageInitAmmo
    {
        public static void Postfix(SiloStorage __instance)
        {
            try
            {
                __instance.RegisterAmmoPointer();
                
            }
            catch (Exception e)
            {
                SRMP.Error($"Error in network ammo!\n{e}\nThis can cause major desync!");
            }
        }
    }
}
