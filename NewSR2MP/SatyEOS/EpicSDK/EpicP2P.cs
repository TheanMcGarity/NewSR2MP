using Epic.OnlineServices;
using Epic.OnlineServices.P2P;
using NewSR2MP.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewSR2MP.EpicSDK
{
    public abstract class EpicP2P
    {
        internal class IncompletePacket
        {
            public byte[][] fragments;
            public byte fragIndex;
            public byte fragTotal;
            public float initTime;
        }
        internal Dictionary<PacketType, IncompletePacket> incompletePackets = new();
        
        private static SocketId SRMP_SOCKETID = new() { SocketName = "NewSR2MP" };

        private P2PInterface p2PInterface;
        private bool isServer;

        private ulong? notifyPeerConnectionRequestHandle;
        private ulong? notifyIncomingPacketQueueFullHandle;
        private ulong? notifyPeerConnectionClosedHandle;
        private ulong? notifyPeerConnectionEstablishedHandle;
        private ulong? notifyPeerConnectionInterruptedHandle;

        private List<Action> queuedActions = new List<Action>();

        public EpicP2P(P2PInterface p2PInterface, bool isServer)
        {
            this.p2PInterface = p2PInterface;
            this.isServer = isServer;
        }

        public void Tick()
        {
            // foreach (var packet in incompletePackets)
            // {
            //     if (packet.Value.initTime > Time.unscaledTime + 7.5f)
            //     {
            //         incompletePackets.Remove(packet.Key);
            //     }
            // }
            
            Receive();

            var actionCount = queuedActions.Count;
            for (int i = 0; i < actionCount; i++)
            {
                queuedActions[0]();
                queuedActions.RemoveAt(0);
            }
        }

        protected void SetRelayControl(RelayControl relayControl)
        {
            var setRelayControlOptions = new SetRelayControlOptions
            {
                RelayControl = relayControl
            };
            p2PInterface.SetRelayControl(ref setRelayControlOptions);
        }

        protected void SetupP2P()
        {
            var queueOptions = new SetPacketQueueSizeOptions
            {
                IncomingPacketQueueMaxSizeBytes = 128 * 1024 * 1024,
                OutgoingPacketQueueMaxSizeBytes = 128 * 1024 * 1024,
            };
            p2PInterface.SetPacketQueueSize(ref queueOptions);
            var addNotifyPeerConnectionRequestOptions = new AddNotifyPeerConnectionRequestOptions
            {
                LocalUserId = EpicApplication.Instance.Authentication.ProductUserId,
                SocketId = SRMP_SOCKETID
            };
            notifyPeerConnectionRequestHandle = p2PInterface.AddNotifyPeerConnectionRequest(ref addNotifyPeerConnectionRequestOptions, null, OnPeerConnectionRequest);

            var addNotifyIncomingPacketQueueFullOptions = new AddNotifyIncomingPacketQueueFullOptions();
            
            notifyIncomingPacketQueueFullHandle = p2PInterface.AddNotifyIncomingPacketQueueFull(ref addNotifyIncomingPacketQueueFullOptions, null, OnIncomingPacketQueueFull);

            var addNotifyPeerConnectionClosedOptions = new AddNotifyPeerConnectionClosedOptions
            {
                LocalUserId = EpicApplication.Instance.Authentication.ProductUserId,
                SocketId = SRMP_SOCKETID
            };
            notifyPeerConnectionClosedHandle = p2PInterface.AddNotifyPeerConnectionClosed(ref addNotifyPeerConnectionClosedOptions, null, OnPeerConnectionClosed);

            var addNotifyPeerConnectionEstablishedOptions = new AddNotifyPeerConnectionEstablishedOptions
            {
                LocalUserId = EpicApplication.Instance.Authentication.ProductUserId,
                SocketId = SRMP_SOCKETID
            };
            notifyPeerConnectionEstablishedHandle = p2PInterface.AddNotifyPeerConnectionEstablished(ref addNotifyPeerConnectionEstablishedOptions, null, OnPeerConnectionEstablished);

            var addNotifyPeerConnectionInterruptedOptions = new AddNotifyPeerConnectionInterruptedOptions
            {
                LocalUserId = EpicApplication.Instance.Authentication.ProductUserId,
                SocketId = SRMP_SOCKETID
            };
            notifyPeerConnectionInterruptedHandle = p2PInterface.AddNotifyPeerConnectionInterrupted(ref addNotifyPeerConnectionInterruptedOptions, null, OnPeerConnectionInterrupted);
            
        }

        public void Shutdown()
        {

            if (notifyPeerConnectionRequestHandle.HasValue)
            {
                p2PInterface.RemoveNotifyPeerConnectionRequest(notifyPeerConnectionRequestHandle.Value);
                notifyPeerConnectionRequestHandle = null;
            }
            if (notifyIncomingPacketQueueFullHandle.HasValue)
            {
                p2PInterface.RemoveNotifyIncomingPacketQueueFull(notifyIncomingPacketQueueFullHandle.Value);
                notifyIncomingPacketQueueFullHandle = null;
            }
            if (notifyPeerConnectionClosedHandle.HasValue)
            {
                p2PInterface.RemoveNotifyPeerConnectionClosed(notifyPeerConnectionClosedHandle.Value);
                notifyPeerConnectionClosedHandle = null;
            }
            if (notifyPeerConnectionEstablishedHandle.HasValue)
            {
                p2PInterface.RemoveNotifyPeerConnectionEstablished(notifyPeerConnectionEstablishedHandle.Value);
                notifyPeerConnectionEstablishedHandle = null;
            }
            if (notifyPeerConnectionInterruptedHandle.HasValue)
            {
                p2PInterface.RemoveNotifyPeerConnectionInterrupted(notifyPeerConnectionInterruptedHandle.Value);
                notifyPeerConnectionInterruptedHandle = null;
            }

            OnShutdown();
        }

        public virtual void OnShutdown() { }

        private void Receive()
        {
            var getNextReceivedPacketSizeOptions = new GetNextReceivedPacketSizeOptions()
            {
                LocalUserId = EpicApplication.Instance.Authentication.ProductUserId,
                RequestedChannel = null
            };
            while (true)
            {
                var result = p2PInterface.GetNextReceivedPacketSize(ref getNextReceivedPacketSizeOptions, out uint outPacketSizeBytes);

                if(result != Result.Success || outPacketSizeBytes == 0)
                {
                    break;
                }

                var receivePacketOptions = new ReceivePacketOptions()
                {
                    LocalUserId = EpicApplication.Instance.Authentication.ProductUserId,
                    RequestedChannel = 0,
                    MaxDataSizeBytes = outPacketSizeBytes
                };

                ProductUserId senderUserId = null;
                SocketId socketId = SocketId.Empty;
                ArraySegment<byte> buffer = new ArraySegment<byte>(new byte[outPacketSizeBytes]);
                p2PInterface.ReceivePacket(ref receivePacketOptions, ref senderUserId, ref socketId, out byte outChannel, buffer, out uint outBytesWritten);

                if(senderUserId != null && outBytesWritten > 0)
                {
                    IncomingMessage incomingMessage = new IncomingMessage();
                    incomingMessage.LengthBytes = (int)outBytesWritten;
                    incomingMessage.Data = buffer.Take((int)outBytesWritten).ToArray();
                    OnMessageReceived(senderUserId, outChannel, ref incomingMessage);
                }   
            }
        }

        public virtual void OnMessageReceived(ProductUserId senderUserId, byte channel, ref IncomingMessage incomingMessage)
        {
            
        }

        protected void SendDataInternal(ProductUserId receiverUserId, byte[] data, PacketReliability packetReliability = PacketReliability.ReliableOrdered)//, byte channel = 0)
        {
            var sendPacketOptions = new SendPacketOptions()
            {
                AllowDelayedDelivery = false,
                DisableAutoAcceptConnection = true,
                LocalUserId = EpicApplication.Instance.Authentication.ProductUserId,
                Reliability = packetReliability,
                Channel = 0,
                RemoteUserId = receiverUserId,
                SocketId = SRMP_SOCKETID,
                Data = new ArraySegment<byte>(data)
            };
            var result = p2PInterface.SendPacket(ref sendPacketOptions);
            if(result != Result.Success)
            {
                SRMP.Error($"Failed to send packet: Error - {result}");
            }
        }

        private void OnPeerConnectionInterrupted(ref OnPeerConnectionInterruptedInfo data)
        {
            var queuedData = data;
            queuedActions.Add(() =>
            {
                OnConnectionInterrupted(queuedData.RemoteUserId);
            });
        }

        public virtual void OnConnectionInterrupted(ProductUserId remoteUserId)
        {
            
        }

        private void OnPeerConnectionEstablished(ref OnPeerConnectionEstablishedInfo data)
        {
            SRMP.Log("Peer connection established!");
            var queuedData = data;
            queuedActions.Add(() =>
            {
                OnConnected(queuedData.RemoteUserId, queuedData.NetworkType, queuedData.ConnectionType);
            });
        }

        public virtual void OnConnected(ProductUserId remoteUserId, NetworkConnectionType networkType, ConnectionEstablishedType connectionType) { }

        private void OnPeerConnectionClosed(ref OnRemoteConnectionClosedInfo data)
        {
            var queuedData = data;
            queuedActions.Add(() =>
            {
                OnDisconnected(queuedData.RemoteUserId, queuedData.Reason);
            });
        }

        public virtual void OnDisconnected(ProductUserId remoteUserId, ConnectionClosedReason reason) { }

        private void OnIncomingPacketQueueFull(ref OnIncomingPacketQueueFullInfo data)
        {
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine($"{data.OverflowPacketChannel} {data.OverflowPacketSizeBytes} {data.PacketQueueCurrentSizeBytes} {data.PacketQueueMaxSizeBytes}");
        }

        private void OnPeerConnectionRequest(ref OnIncomingConnectionRequestInfo data)
        {
            if (isServer)
            {
                var queuedData = data;
                queuedActions.Add(new Action(() =>
                {
                    OnConnectionRequest(queuedData.RemoteUserId);
                }));
            }
            else
            {
                CloseConnection(data.RemoteUserId);
            }
        }

        protected void AcceptConnection(ProductUserId remoteUserId)
        {
            var acceptConnectionOptions = new AcceptConnectionOptions
            {
                LocalUserId = EpicApplication.Instance.Authentication.ProductUserId,
                RemoteUserId = remoteUserId,
                SocketId = SRMP_SOCKETID
            };
            p2PInterface.AcceptConnection(ref acceptConnectionOptions);
        }

        protected void CloseConnection(ProductUserId remoteUserId)
        {
            var closeConnectionOptions = new CloseConnectionOptions
            {
                LocalUserId = EpicApplication.Instance.Authentication.ProductUserId,
                RemoteUserId = remoteUserId,
                SocketId = SRMP_SOCKETID
            };
            p2PInterface.CloseConnection(ref closeConnectionOptions);
        }

        public virtual void OnConnectionRequest(ProductUserId remoteUserId)
        {
            
        }
    }
}
