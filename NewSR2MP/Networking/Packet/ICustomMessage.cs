namespace NewSR2MP.Networking.Packet;

/// <summary>
/// Message serialization interface. Required to use MultiplayerManager.NetworkSend
/// </summary>
public interface ICustomMessage
{
    public Message Serialize();
    public void Deserialize(Message msg);

    public static T Deserialize<T>(Message msg) where T : ICustomMessage
    {
        var packet = (T)typeof(T).GetConstructor(Array.Empty<Type>()).Invoke(Array.Empty<object>());
        
        packet.Deserialize(msg);
        
        return packet;
    }
}