namespace NewSR2MP.Networking.Packet;

/// <summary>
/// Message serialization interface. Required to use MultiplayerManager.NetworkSend
/// </summary>
public interface ICustomMessage
{
    public Message Serialize();
    
}