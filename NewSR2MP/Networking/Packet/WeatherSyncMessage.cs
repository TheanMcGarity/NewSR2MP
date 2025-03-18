
using Il2CppMonomiPark.SlimeRancher.DataModel;
using Il2CppMonomiPark.SlimeRancher.Weather;
using Riptide;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace NewSR2MP.Networking.Packet
{
    public class WeatherSyncMessage : ICustomMessage
    {
        public WeatherSyncMessage(WeatherModel model)
        {
            byte b = 0;
            sync = new NetworkWeatherModel();
            sync.zones = new Dictionary<byte, NetworkWeatherZoneData>();
            foreach (var zone in model._zoneDatas)
            {
                var networkZone = new NetworkWeatherZoneData();
                networkZone.forcast = new List<NetworkWeatherForcast>();


                foreach (var f in zone.Value.Forecast)
                {
                    if (f.StartTime < sceneContext.TimeDirector._worldModel.worldTime && f.EndTime > sceneContext.TimeDirector._worldModel.worldTime)
                    {
                        var networkForcast = new NetworkWeatherForcast();

                        networkForcast.state = f.State.Cast<WeatherStateDefinition>();
                        networkForcast.started = f.Started;
                        
                        networkZone.forcast.Add(networkForcast);
                    }
                }

                networkZone.windSpeed = zone.value.Parameters.WindDirection;
                
                sync.zones.Add(b, networkZone);
                b++;
            }
        }
        public WeatherSyncMessage(Message reader)
        {
            sync = new NetworkWeatherModel();
            sync.Read(reader);
        }
        
        public NetworkWeatherModel sync;
        
        public Message Serialize()
        {
            Message msg = Message.Create(MessageSendMode.Unreliable, PacketType.WeatherUpdate);

            sync.Write(msg);
            
            return msg;
        }
    }
    public struct NetworkWeatherModel
    {
        public Dictionary<byte, NetworkWeatherZoneData> zones;
        
        public void Write(Message msg)
        {
            msg.AddInt(zones.Count);
            foreach (var zone in zones)
            {
                msg.AddByte(zone.Key);
                zone.Value.Write(msg);
            }
        }
        public void Read(Message msg)
        {
            var c = msg.GetInt();
            zones = new Dictionary<byte, NetworkWeatherZoneData>();
            for (var i = 0; i < c; i++)
            {
                var id = msg.GetByte();
                var data = new NetworkWeatherZoneData();

                data.Read(msg);
                zones.Add(id,data);
            }
        }
    }

    public struct NetworkWeatherForcast
    {
        /// <summary>
        /// Keep it as this to make it easier for me.
        /// </summary>
        public WeatherStateDefinition state;

        public bool started;
        
        public void Write(Message msg)
        {
            msg.AddInt(weatherStatesReverseLookup[state.name]);
            msg.AddBool(started);
        }
        public void Read(Message msg)
        {
            state = weatherStates[msg.GetInt()];
            started = msg.GetBool();
        }
    }
    
    public struct NetworkWeatherZoneData
    {
        public List<NetworkWeatherForcast> forcast;
        
        public Vector3 windSpeed;
        public void Write(Message msg)
        {
            msg.AddInt(forcast.Count);
            foreach (var f in forcast)
            {
                f.Write(msg);
            }
            msg.AddVector3(windSpeed);
        }
        public void Read(Message msg)
        {
            var c = msg.GetInt();
            forcast = new List<NetworkWeatherForcast>();
            for (var i = 0; i < c; i++)
            {
                var f = new NetworkWeatherForcast();
                f.Read(msg);
                forcast.Add(f);
            }

            windSpeed = msg.GetVector3();
        }
    }
}
