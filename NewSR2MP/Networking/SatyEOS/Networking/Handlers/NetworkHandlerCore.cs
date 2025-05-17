using NewSR2MP.Attributes;
using NewSR2MP.Networking;
using NewSR2MP.Networking.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace NewSR2MP.Networking
{
    public static partial class NetworkHandler
    {
        public class PacketHandler
        {
            public MethodInfo Method;
            public IPacket Packet;
            public PacketResponseAttribute Attribute;

            public PacketHandler(MethodInfo method, IPacket packet, PacketResponseAttribute attribute)
            {
                Method = method;
                Packet = packet;
                Attribute = attribute;
            }

            public bool Execute(Globals.PlayerState player, IncomingMessage im, byte channel)
            {
                Packet.Deserialize(im);
                var value = Method.Invoke(null, new object[] { player, Packet, channel });
                if (value == null) return true;

                return value is bool returnBool ? returnBool : true;
            }
        }
        static Dictionary<PacketType, PacketHandler> packetHandlers = new Dictionary<PacketType, PacketHandler>();
        public static void Initialize()
        {
            // This is my magic function so I don't have to keep rewriting packet handlers.
            // It grabs all the available Methods in the NetworkHandlers, creates a instance of the packet and saves it into a Dictionary with the packet type
            // The Handle methods then get executed accordingly by the HandleClientPacket and HandleServerPacket Methods
            foreach (var method in typeof(NetworkHandler).GetMethods(BindingFlags.Static | BindingFlags.NonPublic))
            {
                var parameters = method.GetParameters();
                if (parameters.Length == 3 && parameters[0].ParameterType == typeof(Globals.PlayerState) && typeof(IPacket).IsAssignableFrom(parameters[1].ParameterType) && parameters[2].ParameterType == typeof(byte))
                {
                    var packet = (IPacket)System.Activator.CreateInstance(parameters[1].ParameterType);
                    var attribute = method.GetCustomAttribute<PacketResponseAttribute>();
                    packetHandlers.Add(packet.Type, new PacketHandler(method, packet, attribute));
                }
            }
        }

        internal static void HandleClientPacket(PacketType type, byte channel, IncomingMessage im)
        {
            if (packetHandlers.TryGetValue(type, out var handler))
            {
                if (!handler.Execute(null, im, channel))
                    SRMP.Error($"Failed to handle packet {type}");
            }
            else
            {
            }
        }

        internal static void HandleServerPacket(Globals.PlayerState player, byte channel, PacketType type, IncomingMessage im)
        {
            if (packetHandlers.TryGetValue(type, out var handler))
            {
                if (!handler.Execute(player, im, channel))
                {
                    SRMP.Error($"Failed to handle packet {type}");
                    return;
                }

                if (handler.Attribute != null)
                {
                    channel = handler.Attribute.Channel.HasValue ? handler.Attribute.Channel.Value : channel;
                    if (handler.Attribute.ExcludeSender)
                    {
                        handler.Packet.SendPacket(handler.Packet.Reliability, channel, player);
                    }
                    else
                    {
                        handler.Packet.SendPacket(handler.Packet.Reliability, channel);
                    }
                }
            }
            else
            {
            }
        }
    }
}
