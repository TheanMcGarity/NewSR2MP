namespace NewSR2MP.Networking.Packet;

/// <summary>
/// Message serialization interface. Required to use MultiplayerManager.NetworkSend
/// </summary>
public interface ICustomMessage
{
    public Message Serialize();
    public void Deserialize(Message msg);

    /// <summary>
    /// Creates a packet class using a network message input
    /// </summary>
    /// <param name="msg">The message from the network</param>
    /// <typeparam name="T">The packet type</typeparam>
    /// <returns>The packet</returns>
    public static T Deserialize<T>(Message msg) where T : ICustomMessage
    {
        var packet = (T)typeof(T).GetConstructor(Array.Empty<Type>()).Invoke(Array.Empty<object>());
        
        packet.Deserialize(msg);
        
        return packet;
    }
}