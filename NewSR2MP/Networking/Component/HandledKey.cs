using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace NewSR2MP.Networking.Component
{
    /// <summary>
    /// Slime Key handler.
    /// </summary>
    [RegisterTypeInIl2Cpp(false)]
    public class HandledKey : MonoBehaviour
    {
        internal static bool collected = false;

        private static float timer;

        public static void StartTimer()
        {
            collected = true;
            timer = 0.075f;
        }

        void Update()
        {
            if (!collected) return;

            timer -= Time.unscaledDeltaTime;

            if (timer < 0)
            {
                collected = false;
            }
        }
    }
}
