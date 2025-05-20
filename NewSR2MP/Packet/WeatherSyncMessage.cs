
using System.Collections;
using Il2CppMonomiPark.SlimeRancher.DataModel;
using Il2CppMonomiPark.SlimeRancher.Weather;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace NewSR2MP.Packet
{
    public class WeatherSyncMessage : IPacket
    {
        public PacketReliability Reliability => PacketReliability.UnreliableUnordered;

        public PacketType Type => WeatherUpdate;
        
        public const float BUG_CHECK = 3.5f;

        public float timeStarted;
        // Only here for ICustomMessage to work
        public WeatherSyncMessage() { }

        public bool initializedPacket;
        
        public IEnumerator Initialize(WeatherModel model)
        {
            timeStarted = Time.unscaledTime;
            
            byte b = 0;
            sync = new NetworkWeatherModel();
            sync.zones = new Dictionary<byte, NetworkWeatherZoneData>();
            
            yield return null;
            
            foreach (var zone in model._zoneDatas)
            {
                var networkZone = new NetworkWeatherZoneData
                {
                    forcast = new List<NetworkWeatherForcast>()
                };

                yield return null;

                foreach (var f in zone.Value.Forecast)
                {
                    if (f.StartTime < sceneContext.TimeDirector._worldModel.worldTime && f.EndTime > sceneContext.TimeDirector._worldModel.worldTime)
                    {
                        var networkForcast = new NetworkWeatherForcast();

                        networkForcast.state = f.State.Cast<WeatherStateDefinition>();
                        networkForcast.started = f.Started;
                        
                        yield return null;
                        
                        networkZone.forcast.Add(networkForcast);
                    }
                    yield return null;
                }

                networkZone.windSpeed = zone.value.Parameters.WindDirection;
                
                yield return null;

                sync.zones.Add(b, networkZone);
                b++;
                yield return null;
            }
            
            initializedPacket = true;
        } 
        
        public WeatherSyncMessage(WeatherModel model)
        {
            MelonCoroutines.Start(Initialize(model));
        }
        
        public NetworkWeatherModel sync;
        
        public void Serialize(OutgoingMessage msg)
        {
            

            sync.Write(msg);
            
            
        }

        public void Deserialize(IncomingMessage msg)
        {
            sync = new NetworkWeatherModel();
            sync.Read(msg);
        }
    }
    public struct NetworkWeatherModel
    {
        public Dictionary<byte, NetworkWeatherZoneData> zones;
        
        public void Write(OutgoingMessage msg)
        {
            msg.Write(zones.Count);
            foreach (var zone in zones)
            {
                msg.Write(zone.Key);
                zone.Value.Write(msg);
            }
        }
        public void Read(IncomingMessage msg)
        {
            var c = msg.ReadInt32();
            zones = new Dictionary<byte, NetworkWeatherZoneData>();
            for (var i = 0; i < c; i++)
            {
                var id = msg.ReadByte();
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
        
        public void Write(OutgoingMessage msg)
        {
            msg.Write(weatherStatesReverseLookup[state.name]);
            msg.Write(started);
        }
        public void Read(IncomingMessage msg)
        {
            state = weatherStates[msg.ReadInt32()];
            started = msg.ReadBoolean();
        }
    }
    
    public struct NetworkWeatherZoneData
    {
        public List<NetworkWeatherForcast> forcast;
        
        public Vector3 windSpeed;
        public void Write(OutgoingMessage msg)
        {
            msg.Write(forcast.Count);
            foreach (var f in forcast)
            {
                f.Write(msg);
            }
            msg.Write(windSpeed);
        }
        public void Read(IncomingMessage msg)
        {
            var c = msg.ReadInt32();
            forcast = new List<NetworkWeatherForcast>();
            for (var i = 0; i < c; i++)
            {
                var f = new NetworkWeatherForcast();
                f.Read(msg);
                forcast.Add(f);
            }

            windSpeed = msg.ReadVector3();
        }
    }
}
