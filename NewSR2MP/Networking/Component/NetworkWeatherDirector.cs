
using Il2CppMonomiPark.SlimeRancher.Weather;
using NewSR2MP.Networking.Packet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace NewSR2MP.Networking.Component
{
    [RegisterTypeInIl2Cpp(false)]
    public class NetworkWeatherDirector : MonoBehaviour
    {
        WeatherRegistry dir;

        void Start()
        {
            dir = GetComponent<WeatherRegistry>();
        }

        public float timer = 0;
        
        void Update()
        {
            timer += Time.unscaledDeltaTime;

            if (timer > 2.25)
            {

                if (latestMessage == null)
                {
                    latestMessage = new WeatherSyncMessage(dir._model);
                    return;
                }

                if (latestMessage.timeStarted + WeatherSyncMessage.BUG_CHECK < Time.unscaledTime)
                {
                    latestMessage = null;
                    timer = 0;
                    return;
                }
                
                if (!latestMessage.initializedPacket)
                    return;

                MultiplayerManager.NetworkSend(latestMessage);

                latestMessage = null;
                timer = 0;
            }
        }

        private WeatherSyncMessage latestMessage;
    }
}
