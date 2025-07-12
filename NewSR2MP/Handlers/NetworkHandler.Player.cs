using NewSR2MP.Attributes;
using SRMP.Enums;

namespace NewSR2MP;

public partial class NetworkHandler
{
    
    [PacketResponse]
    private static void HandlePlayerJoin(NetPlayerState netPlayer, PlayerJoinMessage packet, byte channel)
    {

        try
        {
            if (packet.local)
            {
                var localPlayer = sceneContext.player.AddComponent<NetworkPlayer>();
                localPlayer.id = packet.id;
                currentPlayerID = localPlayer.id;
            }
            else
            {
                var playerObj = Object.Instantiate(MultiplayerManager.Instance.onlinePlayerPrefab);
                playerObj.name = $"Player{packet.id}";
                
                var netPlayerNew = playerObj.GetComponent<NetworkPlayer>();     
                
                netPlayerNew.usernamePanel = netPlayerNew.transform.GetChild(1).GetComponent<TextMesh>();
                netPlayerNew.usernamePanel.text = packet.username;
                netPlayerNew.usernamePanel.characterSize = 0.2f;
                netPlayerNew.usernamePanel.anchor = TextAnchor.MiddleCenter;
                netPlayerNew.usernamePanel.fontSize = 24;
                
                netPlayerNew.id = packet.id;

                playerUsernames.Add(packet.username, packet.id);
                playerUsernamesReverse.Add(packet.id, packet.username);
                players.Add(new NetPlayerState
                {
                    connectionState = NetworkPlayerConnectionState.Connected,
                    epicID = null,
                    gameObject = netPlayerNew,
                    playerID = (ushort)packet.id
                });
                
                playerObj.SetActive(true);
                Object.DontDestroyOnLoad(playerObj);
            }
        }
        catch
        {
        }
    }

    [PacketResponse]
    private static void HandlePlayerLeave(NetPlayerState netPlayer, PlayerLeaveMessage packet, byte channel)
    {

        var playerObj = netPlayer.gameObject;
        players.Remove(netPlayer);
        Object.Destroy(playerObj.gameObject);
    }
    [PacketResponse]
    private static void HandlePlayer(NetPlayerState netPlayer, PlayerUpdateMessage packet, byte channel)
    {

        try
        {
            if (!TryGetPlayer((ushort)packet.id, out var state))
                return;
            var playerObj = state.gameObject;
            playerObj.GetComponent<TransformSmoother>().nextPos = packet.pos;
            playerObj.GetComponent<TransformSmoother>().nextRot = packet.rot.eulerAngles;


            var anim = playerObj.GetComponent<Animator>();

            anim.SetFloat("HorizontalMovement", packet.horizontalMovement);
            anim.SetFloat("ForwardMovement", packet.forwardMovement);
            anim.SetFloat("Yaw", packet.yaw);
            anim.SetInteger("AirborneState", packet.airborneState);
            anim.SetBool("Moving", packet.moving);
            anim.SetFloat("HorizontalSpeed", packet.horizontalSpeed);
            anim.SetFloat("ForwardSpeed", packet.forwardSpeed);
            anim.SetBool("Sprinting", packet.sprinting);
        }
        catch
        {
        }
    }
}