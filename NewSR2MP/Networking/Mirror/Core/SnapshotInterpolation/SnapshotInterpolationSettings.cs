// snapshot interpolation settings struct.
// can easily be exposed in Unity inspectors.
using System;
using UnityEngine;

namespace Mirror
{
    // class so we can define defaults easily
    [Serializable]
    public class SnapshotInterpolationSettings
    {
        // decrease bufferTime at runtime to see the catchup effect.
        // increase to see slowdown.
        // 'double' so we can have very precise dynamic adjustment without rounding
        
        
        public double bufferTimeMultiplier = 2;

        
        public int bufferLimit = 32;

        // catchup /////////////////////////////////////////////////////////////
        // catchup thresholds in 'frames'.
        // half a frame might be too aggressive.
        
        
        public float catchupNegativeThreshold = -1; // careful, don't want to run out of snapshots

        
        public float catchupPositiveThreshold = 1;

        
        public double catchupSpeed = 0.02f; // see snap interp demo. 1% is too slow.

        
        public double slowdownSpeed = 0.04f; // slow down a little faster so we don't encounter empty buffer (= jitter)

        
        public int driftEmaDuration = 1; // shouldn't need to modify this, but expose it anyway

        // dynamic buffer time adjustment //////////////////////////////////////
        // dynamically adjusts bufferTimeMultiplier for smooth results.
        // to understand how this works, try this manually:
        //
        // - disable dynamic adjustment
        // - set jitter = 0.2 (20% is a lot!)
        // - notice some stuttering
        // - disable interpolation to see just how much jitter this really is(!)
        // - enable interpolation again
        // - manually increase bufferTimeMultiplier to 3-4
        //   ... the cube slows down (blue) until it's smooth
        // - with dynamic adjustment enabled, it will set 4 automatically
        //   ... the cube slows down (blue) until it's smooth as well
        //
        // note that 20% jitter is extreme.
        // for this to be perfectly smooth, set the safety tolerance to '2'.
        // but realistically this is not necessary, and '1' is enough.
        
        
        public bool dynamicAdjustment = true;

        
        public float dynamicAdjustmentTolerance = 1; // 1 is realistically just fine, 2 is very very safe even for 20% jitter. can be half a frame too. (see above comments)

        
        public int deliveryTimeEmaDuration = 2;   // 1-2s recommended to capture average delivery time
    }
}
