
using NewSR2MP.Networking.Packet;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Epic.OnlineServices;
using Il2CppTMPro;
using UnityEngine;

namespace NewSR2MP.Networking.Component
{
    [RegisterTypeInIl2Cpp(false)]
    public class NetworkPlayer : MonoBehaviour
    {
        public ProductUserId epicID { get; private set; }

        public void SetUsername(string username)
        {
            usernamePanel = transform.GetChild(1).GetComponent<TextMesh>();
            usernamePanel.text = username;
            usernamePanel.characterSize = 0.2f;
            usernamePanel.anchor = TextAnchor.MiddleCenter;
            usernamePanel.fontSize = 24;
        }
        
        internal void Intialize(ProductUserId epicID)
        {
            this.epicID = epicID;
        }
        
        void Awake()
        {
            if (transform.GetComponents<NetworkPlayer>().Length > 1)
            {
                Destroy(this);
            }

            try
            {
            }
            catch { }
        }

        public TextMesh usernamePanel;
        
        public int id;
        float transformTimer = PlayerTimer;

        public void Update()
        {
            transformTimer -= Time.unscaledDeltaTime;
            if (transformTimer < 0)
            {
                transformTimer = PlayerTimer;
                
                var anim = GetComponent<Animator>();
                
                var packet = new PlayerUpdateMessage()
                {
                    id = id,
                    scene = (byte)sceneGroupsReverse[systemContext.SceneLoader.CurrentSceneGroup.name],
                    pos = transform.position,
                    rot = transform.rotation,
                    horizontalMovement = anim.GetFloat("HorizontalMovement"),
                    forwardMovement = anim.GetFloat("ForwardMovement"),
                    yaw = anim.GetFloat("Yaw"),
                    airborneState = anim.GetInteger("AirborneState"),
                    moving = anim.GetBool("Moving"),
                    horizontalSpeed = anim.GetFloat("HorizontalSpeed"),
                    forwardSpeed = anim.GetFloat("ForwardSpeed"),
                    sprinting = anim.GetBool("Sprinting"),
                };
                MultiplayerManager.NetworkSend(packet);

            }
        }
    }
}
