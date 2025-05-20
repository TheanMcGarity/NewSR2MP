/*using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using Il2CppInterop.Runtime.Runtime;
using Il2CppMonomiPark.SlimeRancher.DataModel;
using Il2CppMonomiPark.SlimeRancher.Regions;
using NewSR2MP.Networking.Component;
using NewSR2MP.Networking.Packet;
using UnityEngine;
using UnityEngine.UIElements;

namespace NewSR2MP.Networking.Patches
{
    [HarmonyPatch(typeof(RegionLoader), nameof(RegionLoader.UpdateHibernated))]
    public class RegionLoaderUpdateHibernated
    {
        public static void GetColliding(Bounds checkBounds, BoundsQuadtreeNode<Region> node, Il2CppSystem.Collections.Generic.List<Region> result)
        {
            if (node._objects.Count == 0 && (node._children == null || node._children.Length == 0))
            {
                return;
            }
            for (int i = 0; i < node._objects.Count; i++)
            {
                if (node._objects._items[i].Bounds.Intersects(checkBounds))
                {
                    result.Add(node._objects._items[i].Obj);
                }
            }
            if (node._children != null)
            {
                bool flag = checkBounds.min.y <= node._children[0]._bounds.max.y;
                bool flag2 = checkBounds.max.y >= node._children[2]._bounds.min.y;
                bool flag3 = checkBounds.min.x <= node._children[0]._bounds.max.x;
                bool flag4 = checkBounds.max.x >= node._children[1]._bounds.min.x;
                if (flag3)
                {
                    if (flag)
                    {
                        GetColliding(checkBounds, node._children[0], result);
                    }
                    if (flag2)
                    {
                        GetColliding(checkBounds, node._children[2], result);
                    }
                }
                if (flag4)
                {
                    if (flag)
                    {
                        GetColliding(checkBounds, node._children[1], result);
                    }
                    if (flag2)
                    {
                        GetColliding(checkBounds, node._children[3], result);
                    }
                }
            }
        }

        public static Il2CppSystem.Collections.Generic.List<Region> TreeGetColliding(Bounds checkBounds, BoundsQuadtree<Region> tree)
        {
            var ret = new Il2CppSystem.Collections.Generic.List<Region>();
            GetColliding(checkBounds, tree._rootNode, ret);
            return ret;
        }
        private static void AddList(Il2CppSystem.Collections.Generic.List<Region> a, Il2CppSystem.Collections.Generic.List<Region> b)
        {
            foreach (Region region in a)
                b.Add(region);
        }
        
        public static void Postfix(RegionLoader __instance, Vector3 position)
        {
            try
            {


                Il2CppSystem.Collections.Generic.List<Region> all = new Il2CppSystem.Collections.Generic.List<Region>();

                foreach (var player in players.Values)
                {
                    Bounds bounds = new Bounds(player.transform.position, __instance.WakeSize);
                    Il2CppSystem.Collections.Generic.List<Region> regions = TreeGetColliding(bounds, __instance._regionReg._sceneGroupQuadTrees[__instance._regionReg.CurrentSceneGroup]);
                    AddList(regions, all);
                }

                foreach (var h in __instance._nonHibernatedRegions)
                    if (!all.Contains(h))
                        foreach (var actor in h._members.Data)
                            actor.GetComponent<NetworkActorOwnerToggle>().OwnActor();
            }
            catch
            {
            }
        }
    }
}
*/