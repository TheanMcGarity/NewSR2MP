// snapshot interpolation settings struct.
// can easily be exposed in Unity inspectors.
using System;
using UnityEngine;

namespace Mirror
{
    // class so we can define defaults easily
    [Serializable]
    public class LagCompensationSettings
    {
        
        
        public int historyLimit = 6;

        
        public float captureInterval = 0.100f; // 100 ms
    }
}
