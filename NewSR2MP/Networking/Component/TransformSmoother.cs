using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace NewSR2MP.Networking.Component
{
    [RegisterTypeInIl2Cpp(false)]
    public class TransformSmoother : MonoBehaviour
    {
        public void SetRigidbodyState(bool enabled)
        {
            if (GetComponent<Rigidbody>() != null)
                GetComponent<Rigidbody>().constraints = enabled ? RigidbodyConstraints.None : RigidbodyConstraints.FreezeAll;
        }
        void Start()
        {
            SetRigidbodyState(false);

            if (GetComponent<NetworkPlayer>() != null)
            {

                thisPlayer = GetComponent<NetworkPlayer>();

                playerRegionCheckValues.Add(thisPlayer.id, (Vector3.one * 9999));
            }
        }
        public NetworkPlayer thisPlayer;

        /// <summary>
        /// Next rotation. The future rotation, this is the rotation the transform is smoothing to.
        /// </summary>
        public Vector3 nextRot;

        /// <summary>
        /// Next position. The future position, this is the position the transform is smoothing to.
        /// </summary>
        public Vector3 nextPos;

        /// <summary>
        ///  Interpolation Period. the speed at which the transform is smoothed.
        /// </summary>
        public float interpolPeriod = .1f;

        public Vector3 currPos => transform.position;
        private float positionTime;

        public Vector3 currRot => transform.eulerAngles;

        private uint frame;
        private bool wait = true;

        private float playerRegionTimer = 0f;

        void OnEnable()
        {
            SetRigidbodyState(false);
        }
        void OnDisable()
        {
            SetRigidbodyState(true);
        }
        public void Update()
        {
            if (GetComponent<NetworkActor>() != null)
            {
                if (GetComponent<NetworkActor>().IsOwned)
                {
                    SetRigidbodyState(true);
                    GetComponent<NetworkActor>().enabled = true;
                    enabled = false;
                    return;
                }
            }
            if (!(frame > 10))
            {
                frame++;
            }
            else
            {
                
                float t = 1.0f - ((positionTime - Time.unscaledTime) / interpolPeriod);
                transform.position = Vector3.Lerp(currPos, nextPos, t);

                transform.rotation = Quaternion.Lerp(Quaternion.Euler(currRot), Quaternion.Euler(nextRot), t);

                positionTime = Time.unscaledTime + interpolPeriod;
            }
        }
    }
}
