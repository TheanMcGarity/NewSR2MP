using Epic.OnlineServices;
using Epic.OnlineServices.P2P;
using Mirror;
using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace EpicTransport {
    public class Client : Common {

        public SocketId socketId;
        public ProductUserId serverId;

        public bool Connected { get; private set; }
        public bool Error { get; private set; }

        private event Action<byte[], int> OnReceivedData;
        private event Action OnConnected;
        public event Action OnDisconnected;

        private TimeSpan ConnectionTimeout;

        public bool isConnecting = false;
        public string hostAddress = "";
        internal ProductUserId hostProductId = null;
        // private TaskCompletionSource<Task> connectedComplete; Do not use; will not work in il2cpp!
        private CancellationTokenSource cancelToken;

        private Client(EosTransport transport) : base(transport) {
            ConnectionTimeout = TimeSpan.FromSeconds(Math.Max(1, transport.timeout));
        }

        public static Client CreateClient(EosTransport transport, string host) {
            Client c = new Client(transport);

            c.hostAddress = host;
            c.socketId = new SocketId() { SocketName = RandomString.Generate(20) };

            c.OnConnected += () => transport.OnClientConnected.Invoke();
            c.OnDisconnected += () => transport.OnClientDisconnected.Invoke();
            c.OnReceivedData += (data, channel) => client.OnTransportData(new ArraySegment<byte>(data), channel);

            c.OnReceivedData += (_, _) => transport.lastPacketSentTime = 0f;

            return c;
        }

        public void Connect(string host) {
            cancelToken = new CancellationTokenSource();

            try {
                hostProductId = ProductUserId.FromString(host);
                serverId = hostProductId;


                SendInternal(hostProductId, socketId, InternalMessages.CONNECT);

                MelonCoroutines.Start(transport.InitialTimeoutCoroutine(this));
            } catch (FormatException) {
                SRMP.Error($"Connection string was not in the right format. Did you enter a ProductId?");
                Error = true;
                OnConnectionFailed(hostProductId);
            } catch (Exception ex) {
                SRMP.Error(ex.Message);
                Error = true;
                OnConnectionFailed(hostProductId);
            } finally {
                if (Error) {
                    OnConnectionFailed(null);
                }
            }

        }

        public void Disconnect() {
            if (serverId != null) {
                CloseP2PSessionWithUser(serverId, socketId);

                serverId = null;
            } else {
                return;
            }

            SendInternal(hostProductId, socketId, InternalMessages.DISCONNECT);

            Dispose();
            cancelToken?.Cancel();

            WaitForClose(hostProductId, socketId);
        }


        protected override void OnReceiveData(byte[] data, ProductUserId clientUserId, int channel) {
            if (ignoreAllMessages) {
                return;
            }

            if (clientUserId != hostProductId) {
                SRMP.Error("Received a message from an unknown");
                return;
            }

            OnReceivedData.Invoke(data, channel);
        }

        protected override void OnNewConnection(OnIncomingConnectionRequestInfo result) {
            if (ignoreAllMessages) {
                return;
            }

            if (deadSockets.Contains(result.SocketId.SocketName)) {
                SRMP.Error("Received incoming connection request from dead socket");
                return;
            }

            if (hostProductId == result.RemoteUserId) {
                EOSSDKComponent.GetP2PInterface().AcceptConnection(
                    new AcceptConnectionOptions() {
                        LocalUserId = EOSSDKComponent.LocalUserProductId,
                        RemoteUserId = result.RemoteUserId,
                        SocketId = result.SocketId
                    });
            } else {
                SRMP.Error("P2P Acceptance Request from unknown host ID.");
            }
        }

        protected override void OnReceiveInternalData(InternalMessages type, ProductUserId clientUserId, SocketId socketId) {
            if (ignoreAllMessages) {
                return;
            }

            switch (type) {
                case InternalMessages.ACCEPT_CONNECT:
                    Connected = true;
                    OnConnected.Invoke();
                    Debug.Log("Connection established.");
                    break;
                case InternalMessages.DISCONNECT:
                    Connected = false;
                    Debug.Log("Disconnected.");

                    OnDisconnected.Invoke();
                    break;
                default:
                    Debug.Log("Received unknown message type");
                    break;
            }
        }

        public void Send(byte[] data, int channelId) => Send(hostProductId, socketId, data, (byte) channelId);

        internal override void OnConnectionFailed(ProductUserId remoteId) => OnDisconnected.Invoke();
        public void EosNotInitialized() => OnDisconnected.Invoke();
    }
}