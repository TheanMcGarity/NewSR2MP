/*using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;

using Il2CppMonomiPark.SlimeRancher.DataModel;
using Il2CppMonomiPark.SlimeRancher.Regions;
using NewSR2MP.Networking.Component;
using NewSR2MP.Networking.Packet;
using UnityEngine;
using UnityEngine.UIElements;

namespace NewSR2MP.Networking.Patches
{
    [HarmonyPatch(typeof(RegionLoader), nameof(RegionLoader.UpdateProxied))]
    public class RegionLoaderUpdateProxied
    {
        private static void CombineRegionList(Il2CppSystem.Collections.Generic.List<Region> load, Il2CppSystem.Collections.Generic.List<Region> unload)
        {
            RegionLoader._loadRegions = CombineRegionListInternal(load, RegionLoader._loadRegions);
            RegionLoader._unloadRegions = CombineRegionListInternal(unload, RegionLoader._unloadRegions);
        }

        static Il2CppSystem.Collections.Generic.List<Region> CombineRegionListInternal(Il2CppSystem.Collections.Generic.List<Region> a, Il2CppSystem.Collections.Generic.List<Region> b)
        {
            var ret = new Il2CppSystem.Collections.Generic.List<Region>();
            
            foreach (var item in b)
            {
                ret.Add(item);
            }
            foreach (var item in a)
            {
                if (!ret.Contains(item)) ret.Add(item);
            }

            return ret;
        }

        public static bool Prefix(RegionLoader __instance, Vector3 position)
        {
            RegionLoader._loadRegions.Clear();
            RegionLoader._unloadRegions.Clear();

            foreach (var player in players.Values)
            {
                if (player.id == 0 && ServerActive()) continue;

                Vector3 networkPos = player.transform.position;

                Il2CppSystem.Collections.Generic.List<Region> load = new Il2CppSystem.Collections.Generic.List<Region>();
                Il2CppSystem.Collections.Generic.List<Region> unload = new Il2CppSystem.Collections.Generic.List<Region>();

                Bounds bounds = new Bounds(networkPos, __instance.LoadSize / 4);
                Bounds bounds2 = new Bounds(networkPos, (__instance.LoadSize * (1f + __instance.UnloadBuffer)) / 4);
                
                __instance._regionReg.GetContaining(ref load, bounds);
                __instance._regionReg.GetContaining(ref load, bounds);


                CombineRegionList(load, unload);
            }


            Il2CppSystem.Collections.Generic.List<Region> load2 = new Il2CppSystem.Collections.Generic.List<Region>();
            Il2CppSystem.Collections.Generic.List<Region> unload2 = new Il2CppSystem.Collections.Generic.List<Region>();

            Bounds bounds3 = new Bounds(position, __instance.LoadSize );
            Bounds bounds4 = new Bounds(position, __instance.LoadSize * (1f + __instance.UnloadBuffer));

            __instance._regionReg.GetContaining(ref load2, bounds3);
            __instance._regionReg.GetContaining(ref unload2, bounds4);

            CombineRegionList(load2, unload2);

            int num = 0;
            int num2 = __instance._nonProxiedRegions.Count;
            while (num < num2)
            {
                Region region = __instance._nonProxiedRegions._items[num]; // Why do i have to do _items? this error is stupid
                if (RegionLoader._loadRegions.Contains(region))
                {
                    RegionLoader._loadRegions.Remove(region);
                    num++;
                }
                else if (!RegionLoader._unloadRegions.Contains(region))
                {
                    region.RemoveNonProxiedReference();
                    __instance._nonProxiedRegions.RemoveAt(num);
                    num2--;
                }
                else
                {
                    num++;
                }
            }

            num2 = RegionLoader._loadRegions.Count;
            if (num2 <= 0)
            {
                return false;
            }

            for (num = 0; num < num2; num++)
            {
                Region region2 = RegionLoader._loadRegions._items[num];
                if (!__instance._nonProxiedRegions.Contains(region2))
                {
                    region2.AddNonProxiedReference();
                    __instance._nonProxiedRegions.Add(region2);
                }
            }
            return false;
        }
    }
    [HarmonyPatch(typeof(RegionLoader), nameof(RegionLoader.Update))]
    public class RegionLoaderUpdate
    {
        private static bool CheckPlayerPositions(RegionLoader rl)
        {
            foreach (var player in players.Values)
            {
                var checkVal = (player.transform.position - playerRegionCheckValues[player.id]).sqrMagnitude >= 1f;

                playerRegionCheckValues[player.id] = player.transform.position;

                if (checkVal)
                {
                    return true;
                }
            }
            
            var localCheckVal = (rl.transform.position - rl._lastRegionCheckPos).sqrMagnitude >= 1f;

            if (localCheckVal)
            {
                return true;
            }

            return false;
        }

        public static bool Prefix(RegionLoader __instance)
        {
            if (ServerActive() || ClientActive())
            {

                if (CheckPlayerPositions(__instance))
                {
                    __instance.ForceUpdate();


                }
                return false;
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(RegionLoader), nameof(RegionLoader.UpdateHibernated))]
    public class RegionLoaderUpdateHibernated
    {
        private static void CombineRegionList(Il2CppSystem.Collections.Generic.List<Region> load, Il2CppSystem.Collections.Generic.List<Region> unload)
        {
            RegionLoader._loadRegions = CombineRegionListInternal(load, RegionLoader._loadRegions);
            RegionLoader._unloadRegions = CombineRegionListInternal(unload, RegionLoader._unloadRegions);
        }
        static Il2CppSystem.Collections.Generic.List<Region> CombineRegionListInternal(Il2CppSystem.Collections.Generic.List<Region> a, Il2CppSystem.Collections.Generic.List<Region> b)
        {
            var ret = new Il2CppSystem.Collections.Generic.List<Region>();
            
            foreach (var item in b)
            {
                ret.Add(item);
            }
            foreach (var item in a)
            {
                if (!ret.Contains(item)) ret.Add(item);
            }

            return ret;
        }
        private static void CombineRegionListSpecific(ref Il2CppSystem.Collections.Generic.List<Region> a, Il2CppSystem.Collections.Generic.List<Region> b)
        {
            a = CombineRegionListInternal(a, b);
        }


        /// <summary>
        /// CODE IS PROPERTY OF MONOMI PARK
        /// DO NOT COPY ANYWHERE
        /// </summary>
        public static bool Prefix(RegionLoader __instance, Vector3 position)
        {

            Il2CppSystem.Collections.Generic.List<Region> loadT = new Il2CppSystem.Collections.Generic.List<Region>();
            Il2CppSystem.Collections.Generic.List<Region> unloadT = new Il2CppSystem.Collections.Generic.List<Region>();

            RegionLoader._loadRegions.Clear();
            RegionLoader._unloadRegions.Clear();
            foreach (var player in players.Values)
            {
                if (player.id == 0 && ServerActive()) continue;

                Vector3 networkPos = player.transform.position;

                Il2CppSystem.Collections.Generic.List<Region> load = new Il2CppSystem.Collections.Generic.List<Region>();
                Il2CppSystem.Collections.Generic.List<Region> unload = new Il2CppSystem.Collections.Generic.List<Region>();

                Bounds bounds = new Bounds(networkPos, __instance.WakeSize / 5);
                Bounds bounds2 = new Bounds(networkPos, (__instance.WakeSize * (1f + __instance.UnloadBuffer)) / 5);

                __instance._regionReg.GetContaining(ref load, bounds);
                __instance._regionReg.GetContaining(ref unload, bounds2);


                CombineRegionListSpecific(ref loadT, load);
                CombineRegionListSpecific(ref unloadT, unload);
            }


            Il2CppSystem.Collections.Generic.List<Region> load2 = new Il2CppSystem.Collections.Generic.List<Region>();
            Il2CppSystem.Collections.Generic.List<Region> unload2 = new Il2CppSystem.Collections.Generic.List<Region>();

            Bounds bounds3 = new Bounds(position, __instance.WakeSize);
            Bounds bounds4 = new Bounds(position, __instance.WakeSize * (1f + __instance.UnloadBuffer));

            __instance._regionReg.GetContaining(ref load2, bounds3);
            __instance._regionReg.GetContaining(ref unload2, bounds4);

            CombineRegionListSpecific(ref loadT, load2);
            CombineRegionListSpecific(ref unloadT, unload2);


            CombineRegionList(loadT, unloadT);

            int num = 0;
            int num2 = __instance._nonHibernatedRegions.Count;
            while (num < num2)
            {
                Region region = __instance._nonHibernatedRegions._items[num];
                if (RegionLoader._loadRegions.Contains(region))
                {
                    RegionLoader._loadRegions.Remove(region);
                    num++;
                }
                else if (!RegionLoader._unloadRegions.Contains(region))
                {
                    region.RemoveNonHibernateReference();
                    __instance._nonHibernatedRegions.RemoveAt(num);
                    num2--;
                }
                else
                {
                    num++;
                }
            }

            num2 = RegionLoader._loadRegions.Count;
            if (num2 <= 0)
            {
                return false;
            }

            for (num = 0; num < num2; num++)
            {
                Region region2 = RegionLoader._loadRegions._items[num];
                if (!__instance._nonHibernatedRegions.Contains(region2))
                {
                    region2.AddNonHibernateReference();
                    __instance._nonHibernatedRegions.Add(region2);
                }
            }
            return false;
        }
    }
}*/
