using Mirror;
using Il2CppMonomiPark.SlimeRancher.Regions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace NewSR2MP.Networking.Component
{
    [RegisterTypeInIl2Cpp(false)]
    public class NetworkRegion : MonoBehaviour
    {
        public Il2CppSystem.Collections.Generic.List<int> players = new Il2CppSystem.Collections.Generic.List<int>();

        public static Dictionary<string, NetworkRegion> all = new Dictionary<string, NetworkRegion>();

        void Awake()
        {
            all.Add(gameObject.name, this);
        }

        public void AddPlayer(int player)
        {
            players.Add(player);
        }

        public void RemovePlayer(int player)
        {
            players.Remove(player);
        }

    }
}
