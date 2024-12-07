using UnityEngine;

namespace Mirror
{
    public enum RemovePlayerOptions
    {
        /// <summary>Player Object remains active on server and clients. Only ownership is removed</summary>
        KeepActive,
        /// <summary>Player Object is unspawned on clients but remains on server</summary>
        Unspawn,
        /// <summary>Player Object is destroyed on server and clients</summary>
        Destroy
    }

    /// <summary>NetworkServer handles remote connections and has a local connection for a local client.</summary>
    public static partial class NetworkServer
    {

        static bool initialized;
        public static int maxConnections;

        /// <summary>Server Update frequency, per second. Use around 60Hz for fast paced games like Counter-Strike to minimize latency. Use around 30Hz for games like WoW to minimize computations. Use around 1-10Hz for slow paced games like EVE.</summary>
        // overwritten by NetworkManager (if any)
        public static int tickRate = 60;

        // tick rate is in Hz.
        // convert to interval in seconds for convenience where needed.
        //
        // send interval is 1 / sendRate.
        // but for tests we need a way to set it to exactly 0.
        // 1 / int.max would not be exactly 0, so handel that manually.
        public static float tickInterval => tickRate < int.MaxValue ? 1f / tickRate : 0; // for 30 Hz, that's 33ms

        // time & value snapshot interpolation are separate.
        // -> time is interpolated globally on NetworkClient / NetworkConnection
        // -> value is interpolated per-component, i.e. NetworkTransform.
        // however, both need to be on the same send interval.
        public static int sendRate => tickRate;
        public static float sendInterval => sendRate < int.MaxValue ? 1f / sendRate : 0; // for 30 Hz, that's 33ms
        static double lastSendTime;

        /// <summary>Connection to host mode client (if any)</summary>
        public static LocalConnectionToClient localConnection { get; private set; }

        /// <summary>Dictionary of all server connections, with connectionId as key</summary>
        public static Dictionary<int, NetworkConnectionToClient> connections =
            new Dictionary<int, NetworkConnectionToClient>();

        /// <summary>Message Handlers dictionary, with messageId as key</summary>
        internal static Dictionary<ushort, NetworkMessageDelegate> handlers =
            new Dictionary<ushort, NetworkMessageDelegate>();


        /// <summary>Single player mode can use dontListen to not accept incoming connections</summary>
        // see also: https://github.com/vis2k/Mirror/pull/2595
        public static bool dontListen;

        /// <summary>active checks if the server has been started either has standalone or as host server.</summary>
        public static bool active { get; internal set; }

        /// <summary>active checks if the server has been started in host mode.</summary>
        // naming consistent with NetworkClient.activeHost.
        public static bool activeHost => localConnection != null;

        // scene loading
        public static bool isLoadingScene;


        // For security, it is recommended to disconnect a player if a networked
        // action triggers an exception\nThis could prevent components being
        // accessed in an undefined state, which may be an attack vector for
        // exploits.
        //
        // However, some games may want to allow exceptions in order to not
        // interrupt the player's experience.
        public static bool exceptionsDisconnect = true; // security by default

        // Mirror global disconnect inactive option, independent of Transport.
        // not all Transports do this properly, and it's easiest to configure this just once.
        // this is very useful for some projects, keep it.
        public static bool disconnectInactiveConnections;
        public static float disconnectInactiveTimeout = 60;

        // OnConnected / OnDisconnected used to be NetworkMessages that were
        // invoked. this introduced a bug where external clients could send
        // Connected/Disconnected messages over the network causing undefined
        // behaviour.
        // => public so that custom NetworkManagers can hook into it
        public static Action<NetworkConnectionToClient> OnConnectedEvent;
        public static Action<NetworkConnectionToClient> OnDisconnectedEvent;
        public static Action<NetworkConnectionToClient, TransportError, string> OnErrorEvent;
        public static Action<NetworkConnectionToClient, Exception> OnTransportExceptionEvent;

        // keep track of actual achieved tick rate.
        // might become lower under heavy load.
        // very useful for profiling etc.
        // measured over 1s each, same as frame rate. no EMA here.
        public static int actualTickRate;
        static double actualTickRateStart;   // start time when counting
        static int actualTickRateCounter; // current counter since start

        // profiling
        // includes transport update time, because transport calls handlers etc.
        // averaged over 1s by passing 'tickRate' to constructor.
        public static TimeSample earlyUpdateDuration;
        public static TimeSample lateUpdateDuration;

        // capture full Unity update time from before Early- to after LateUpdate
        public static TimeSample fullUpdateDuration;

        /// <summary>Starts server and Listens to incoming connections with max connections limit.</summary>
        public static void Listen(int maxConns)
        {
            Initialize();
            maxConnections = maxConns;

            // only start server if we want to Listen
            if (!dontListen)
            {
                Transport.active.ServerStart();

                if (Transport.active is PortTransport portTransport)
                {
                    if (Utils.IsHeadless())
                    {
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine($"Server Listening on port {portTransport.Port}");
                        Console.ResetColor();
                    }
                }
                else
                    Debug.Log("Server started Listening");
            }

            active = true;
        }

        // initialization / shutdown ///////////////////////////////////////////
        static void Initialize()
        {
            if (initialized)
                return;

            // safety: ensure Weaving succeded.
            // if it silently failed, we would get lots of 'writer not found'
            // and other random errors at runtime instead. this is cleaner.
            if (!WeaverFuse.Weaved())
            {
                // if it failed, throw an exception to early exit all Listen calls.
                throw new Exception("NetworkServer won't start because Weaving failed or didn't run.");
            }

            // Debug.Log($"NetworkServer Created version {Version.Current}");

            //Make sure connections are cleared in case any old connections references exist from previous sessions
            connections.Clear();


            // reset NetworkTime
            NetworkTime.ResetStatics();

            Debug.Assert(Transport.active != null, "There was no active transport when calling NetworkServer.Listen, If you are calling Listen manually then make sure to set 'Transport.active' first");
            AddTransportHandlers();
            RegisterMessageHandlers();

            initialized = true;

            // profiling
            earlyUpdateDuration = new TimeSample(sendRate);
            lateUpdateDuration = new TimeSample(sendRate);
            fullUpdateDuration = new TimeSample(sendRate);
        }

        static void AddTransportHandlers()
        {
            // += so that other systems can also hook into it (i.e. statistics)
            Transport.active.OnServerConnected += OnTransportConnected;
            Transport.active.OnServerConnectedWithAddress += OnTransportConnectedWithAddress;
            Transport.active.OnServerDataReceived += OnTransportData;
            Transport.active.OnServerDisconnected += OnTransportDisconnected;
            Transport.active.OnServerError += OnTransportError;
            Transport.active.OnServerTransportException += OnTransportException;
        }

        /// <summary>Shuts down the server and disconnects all clients</summary>
        // RuntimeInitializeOnLoadMethod -> fast playmode without domain reload
        public static void Shutdown()
        {
            if (initialized)
            {
                DisconnectAll();

                // stop the server.
                // we do NOT call Transport.Shutdown, because someone only
                // called NetworkServer.Shutdown. we can't assume that the
                // client is supposed to be shut down too!
                //
                // NOTE: stop no matter what, even if 'dontListen':
                //       someone might enabled dontListen at runtime.
                //       but we still need to stop the server.
                //       fixes https://github.com/vis2k/Mirror/issues/2536
                Transport.active.ServerStop();

                // transport handlers are hooked into when initializing.
                // so only remove them when shutting down.
                RemoveTransportHandlers();

                initialized = false;
            }

            // Reset all statics here....
            dontListen = false;
            isLoadingScene = false;
            lastSendTime = 0;
            actualTickRate = 0;

            localConnection = null;

            connections.Clear();
            connectionsCopy.Clear();
            handlers.Clear();

            // destroy all spawned objects, _then_ set inactive.
            // make sure .active is still true before calling this.
            // otherwise modifying SyncLists in OnStopServer would throw
            // because .IsWritable() check checks if NetworkServer.active.
            // https://github.com/MirrorNetworking/Mirror/issues/3344
            CleanupSpawned();
            active = false;

            // sets nextNetworkId to 1
            // sets clientAuthorityCallback to null
            // sets previousLocalPlayer to null

            // clear events. someone might have hooked into them before, but
            // we don't want to use those hooks after Shutdown anymore.
            OnConnectedEvent = null;
            OnDisconnectedEvent = null;
            OnErrorEvent = null;
            OnTransportExceptionEvent = null;

        }

        static void RemoveTransportHandlers()
        {
            // -= so that other systems can also hook into it (i.e. statistics)
            Transport.active.OnServerConnected -= OnTransportConnected;
            Transport.active.OnServerDataReceived -= OnTransportData;
            Transport.active.OnServerDisconnected -= OnTransportDisconnected;
            Transport.active.OnServerError -= OnTransportError;
        }

        // Note: NetworkClient.DestroyAllClientObjects does the same on client.
        static void CleanupSpawned()
        {
        }

        internal static void RegisterMessageHandlers()
        {
            RegisterHandler<ReadyMessage>(OnClientReadyMessage);
            RegisterHandler<CommandMessage>(OnCommandMessage);
            RegisterHandler<NetworkPingMessage>(NetworkTime.OnServerPing, false);
            RegisterHandler<NetworkPongMessage>(NetworkTime.OnServerPong, false);
            RegisterHandler<EntityStateMessage>(OnEntityStateMessage, true);
            RegisterHandler<TimeSnapshotMessage>(OnTimeSnapshotMessage, true);

        }

        // remote calls ////////////////////////////////////////////////////////
        // Handle command from specific player, this could be one of multiple
        // players on a single client
        // default ready handler.
        static void OnClientReadyMessage(NetworkConnectionToClient conn, ReadyMessage msg)
        {
            // Debug.Log($"Default handler for ready message from {conn}");
            SetClientReady(conn);
        }

        static void OnCommandMessage(NetworkConnectionToClient conn, CommandMessage msg, int channelId)
        {
            if (!conn.isReady)
            {
                // Clients may be set NotReady due to scene change or other game logic by user, e.g. respawning.
                // Ignore commands that may have been in flight before client received NotReadyMessage message.
                // Unreliable messages may be out of order, so don't spam warnings for those.
                if (channelId == Channels.Reliable)
                {
                    // Attempt to identify the target object, component, and method to narrow down the cause of the error.
                   
                    Debug.LogWarning("Command received while client is not ready.\nThis may be ignored if client intentionally set NotReady.");
                }
                return;
            }

           

            // Commands can be for player objects, OR other objects with client-authority
            // -> so if this connection's controller has a different netId then
            //    only allow the command if clientAuthorityOwner
            

            // Debug.Log($"OnCommandMessage for netId:{msg.netId} conn:{conn}");

            
        }

        // client to server broadcast //////////////////////////////////////////
        // for client's owned ClientToServer components.
        static void OnEntityStateMessage(NetworkConnectionToClient connection, EntityStateMessage message)
        {}

        // client sends TimeSnapshotMessage every sendInterval.
        // batching already includes the remoteTimestamp.
        // we simply insert it on-message here.
        // => only for reliable channel. unreliable would always arrive earlier.
        static void OnTimeSnapshotMessage(NetworkConnectionToClient connection, TimeSnapshotMessage _)
        {
            // insert another snapshot for snapshot interpolation.
            // before calling OnDeserialize so components can use
            // NetworkTime.time and NetworkTime.timeStamp.

            // TODO validation?
            // maybe we shouldn't allow timeline to deviate more than a certain %.
            // for now, this is only used for client authority movement.

            // Unity 2019 doesn't have Time.timeAsDouble yet
            //
            // NetworkTime uses unscaled time and ignores Time.timeScale.
            // fixes Time.timeScale getting server & client time out of sync:
            // https://github.com/MirrorNetworking/Mirror/issues/3409
            connection.OnTimeSnapshot(new TimeSnapshot(connection.remoteTimeStamp, NetworkTime.localTime));
        }

        // connections /////////////////////////////////////////////////////////
        /// <summary>Add a connection and setup callbacks. Returns true if not added yet.</summary>
        public static bool AddConnection(NetworkConnectionToClient conn)
        {
            if (!connections.ContainsKey(conn.connectionId))
            {
                // connection cannot be null here or conn.connectionId
                // would throw NRE
                connections[conn.connectionId] = conn;
                return true;
            }
            // already a connection with this id
            return false;
        }

        /// <summary>Removes a connection by connectionId. Returns true if removed.</summary>
        public static bool RemoveConnection(int connectionId) =>
            connections.Remove(connectionId);

        // called by LocalClient to add itself. don't call directly.
        // TODO consider internal setter instead?
        internal static void SetLocalConnection(LocalConnectionToClient conn)
        {
            if (localConnection != null)
            {
                Debug.LogError("Local Connection already exists");
                return;
            }

            localConnection = conn;
        }

        // removes local connection to client
        internal static void RemoveLocalConnection()
        {
            if (localConnection != null)
            {
                localConnection.Disconnect();
                localConnection = null;
            }
            RemoveConnection(0);
        }

        /// <summary>True if we have external connections (that are not host)</summary>
        public static bool HasExternalConnections()
        {
            // any connections?
            if (connections.Count > 0)
            {
                // only host connection?
                if (connections.Count == 1 && localConnection != null)
                    return false;

                // otherwise we have real external connections
                return true;
            }
            return false;
        }

        // send ////////////////////////////////////////////////////////////////
        /// <summary>Send a message to all clients, even those that haven't joined the world yet (non ready)</summary>
        public static void SendToAll<T>(T message, int channelId = Channels.Reliable, bool sendToReadyOnly = false)
            where T : struct, NetworkMessage
        {
            if (!active)
            {
                Debug.LogWarning("Can not send using NetworkServer.SendToAll<T>(T msg) because NetworkServer is not active");
                return;
            }

            // Debug.Log($"Server.SendToAll {typeof(T)}");
            using (NetworkWriterPooled writer = NetworkWriterPool.Get())
            {
                writer.WriteBool(false);

                // pack message only once
                NetworkMessages.Pack(message, writer);
                ArraySegment<byte> segment = writer.ToArraySegment();

                // validate packet size immediately.
                // we know how much can fit into one batch at max.
                // if it's larger, log an error immediately with the type <T>.
                // previously we only logged in Update() when processing batches,
                // but there we don't have type information anymore.
                int max = NetworkMessages.MaxMessageSize(channelId);
                if (writer.Position > max)
                {
                    Debug.LogError($"NetworkServer.SendToAll: message of type {typeof(T)} with a size of {writer.Position} bytes is larger than the max allowed message size in one batch: {max}.\nThe message was dropped, please make it smaller.");
                    return;
                }

                // filter and then send to all internet connections at once
                // -> makes code more complicated, but is HIGHLY worth it to
                //    avoid allocations, allow for multicast, etc.
                int count = 0;
                foreach (NetworkConnectionToClient conn in connections.Values)
                {
                    if (sendToReadyOnly && !conn.isReady)
                        continue;

                    count++;
                    conn.Send(segment, channelId);
                }

                NetworkDiagnostics.OnSend(message, channelId, segment.Count, count);
            }
        }



        public static void SRMPSendToAll<T>(T message, int channelId = Channels.Reliable, bool sendToReadyOnly = false)
            where T : struct, NetworkMessage
        {
            if (!active)
            {
                Debug.LogWarning("Can not send using NetworkServer.SendToAll<T>(T msg) because NetworkServer is not active");
                return;
            }

            // Debug.Log($"Server.SendToAll {typeof(T)}");
            using (NetworkWriterPooled writer = NetworkWriterPool.Get())
            {
                writer.WriteBool(true);
                // pack message
                writer.WriteUShort(NetworkMessageId<T>.Id);
                writer.Write(message);

                // validate packet size immediately.
                // we know how much can fit into one batch at max.
                // if it's larger, log an error immediately with the type <T>.
                // previously we only logged in Update() when processing batches,
                // but there we don't have type information anymore.
                int max = NetworkMessages.MaxMessageSize(channelId);
                if (writer.Position > max)
                {
                    Debug.LogError($"NetworkServer.SendToAll: message of type {typeof(T)} with a size of {writer.Position} bytes is larger than the max allowed message size in one batch: {max}.\nThe message was dropped, please make it smaller.");
                    return;
                }

                // filter and then send to all internet connections at once
                // -> makes code more complicated, but is HIGHLY worth it to
                //    avoid allocations, allow for multicast, etc.
                int count = 0;
                foreach (NetworkConnectionToClient conn in connections.Values)
                {
                    if (sendToReadyOnly && !conn.isReady)
                        continue;

                    count++;
                    
                    (Transport.active as EpicTransport.EosTransport).ServerSend(conn.connectionId, writer.ToArraySegment(), channelId);
                }
            }
        }
        public static void SRMPSend<T>(T message, NetworkConnectionToClient conn, int channelId = Channels.Reliable, bool sendToReadyOnly = false)
                    where T : struct, NetworkMessage
        {
            if (!active)
            {
                Debug.LogWarning("Can not send using NetworkServer.SendToAll<T>(T msg) because NetworkServer is not active");
                return;
            }


            // Debug.Log($"Server.SendToAll {typeof(T)}");
            using (NetworkWriterPooled writer = NetworkWriterPool.Get())
            {
                writer.WriteBool(true);
                // pack message
                writer.WriteUShort(NetworkMessageId<T>.Id);
                writer.Write(message);

                // validate packet size immediately.
                // we know how much can fit into one batch at max.
                // if it's larger, log an error immediately with the type <T>.
                // previously we only logged in Update() when processing batches,
                // but there we don't have type information anymore.
                int max = NetworkMessages.MaxMessageSize(channelId);
                if (writer.Position > max)
                {
                    Debug.LogError($"NetworkServer.SendToAll: message of type {typeof(T)} with a size of {writer.Position} bytes is larger than the max allowed message size in one batch: {max}.\nThe message was dropped, please make it smaller.");
                    return;
                }

                (Transport.active as EpicTransport.EosTransport).ServerSend(conn.connectionId, writer.ToArraySegment(), channelId);
            }
        }








        public static NetworkConnectionToClient[] NetworkConnectionListExcept(NetworkConnectionToClient except)
        {
            List<NetworkConnectionToClient> list = new List<NetworkConnectionToClient>();

            foreach (var conn in connections.Values)
            {
                list.Add(conn);
            }

            list.Remove(except);
            list.Remove(connections[0]);

            return list.ToArray();
        }
        public static void SendToConnections<T>(T message, NetworkConnectionToClient[] sendTo, int channelId = Channels.Reliable, bool sendToReadyOnly = false)
                    where T : struct, NetworkMessage
        {
            if (!active)
            {
                Debug.LogWarning("Can not send using NetworkServer.SendToAll<T>(T msg) because NetworkServer is not active");
                return;
            }

            // Debug.Log($"Server.SendToAll {typeof(T)}");
            using (NetworkWriterPooled writer = NetworkWriterPool.Get())
            {
                writer.WriteBool(false);
                writer.WriteDouble(Time.unscaledTime);
                Compression.CompressVarUInt(writer, (ulong)writer.buffer.Length);

                // pack message only once
                NetworkMessages.Pack(message, writer);
                ArraySegment<byte> segment = writer.ToArraySegment();

                // validate packet size immediately.
                // we know how much can fit into one batch at max.
                // if it's larger, log an error immediately with the type <T>.
                // previously we only logged in Update() when processing batches,
                // but there we don't have type information anymore.
                int max = NetworkMessages.MaxMessageSize(channelId);
                if (writer.Position > max)
                {
                    Debug.LogError($"NetworkServer.SendToAllExcept: message of type {typeof(T)} with a size of {writer.Position} bytes is larger than the max allowed message size in one batch: {max}.\nThe message was dropped, please make it smaller.");
                    return;
                }

                // filter and then send to all internet connections at once
                // -> makes code more complicated, but is HIGHLY worth it to
                //    avoid allocations, allow for multicast, etc.
                int count = 0;
                foreach (NetworkConnectionToClient conn in sendTo)
                {
                    if (sendToReadyOnly && !conn.isReady)
                        continue;

                    count++;
                    (Transport.active as EpicTransport.EosTransport).ServerSend(conn.connectionId, writer.ToArraySegment(), channelId);
                }

                NetworkDiagnostics.OnSend(message, channelId, segment.Count, count);
            }
        }
        public static void SRMPSendToConnections<T>(T message, NetworkConnectionToClient[] sendTo, int channelId = Channels.Reliable, bool sendToReadyOnly = false)
                            where T : struct, NetworkMessage
        {
            if (!active)
            {
                Debug.LogWarning("Can not send using NetworkServer.SendToAll<T>(T msg) because NetworkServer is not active");
                return;
            }

            // Debug.Log($"Server.SendToAll {typeof(T)}");
            using (NetworkWriterPooled writer = NetworkWriterPool.Get())
            {
                writer.WriteBool(true);

                // pack message only once
                NetworkMessages.Pack(message, writer);
                ArraySegment<byte> segment = writer.ToArraySegment();

                // validate packet size immediately.
                // we know how much can fit into one batch at max.
                // if it's larger, log an error immediately with the type <T>.
                // previously we only logged in Update() when processing batches,
                // but there we don't have type information anymore.
                int max = NetworkMessages.MaxMessageSize(channelId);
                if (writer.Position > max)
                {
                    Debug.LogError($"NetworkServer.SendToAllExcept: message of type {typeof(T)} with a size of {writer.Position} bytes is larger than the max allowed message size in one batch: {max}.\nThe message was dropped, please make it smaller.");
                    return;
                }

                // filter and then send to all internet connections at once
                // -> makes code more complicated, but is HIGHLY worth it to
                //    avoid allocations, allow for multicast, etc.
                int count = 0;
                foreach (NetworkConnectionToClient conn in sendTo)
                {
                    if (sendToReadyOnly && !conn.isReady)
                        continue;
                    (Transport.active as EpicTransport.EosTransport).ServerSend(conn.connectionId, writer.ToArraySegment(), channelId);
                }

                NetworkDiagnostics.OnSend(message, channelId, segment.Count, count);
            }
        }

        /// <summary>Send a message to all clients which have joined the world (are ready).</summary>
        // TODO put rpcs into NetworkServer.Update WorldState packet, then finally remove SendToReady!
        public static void SendToReady<T>(T message, int channelId = Channels.Reliable)
            where T : struct, NetworkMessage
        {
            if (!active)
            {
                Debug.LogWarning("Can not send using NetworkServer.SendToReady<T>(T msg) because NetworkServer is not active");
                return;
            }

            SendToAll(message, channelId, true);
        }

        // transport events ////////////////////////////////////////////////////
        // called by transport
        static void OnTransportConnected(int connectionId)
            => OnTransportConnectedWithAddress(connectionId, Transport.active.ServerGetClientAddress(connectionId));

        static void OnTransportConnectedWithAddress(int connectionId, string clientAddress)
        {
            if (IsConnectionAllowed(connectionId))
            {
                // create a connection
                NetworkConnectionToClient conn = new NetworkConnectionToClient(connectionId, clientAddress);
                OnConnected(conn);
            }
            else
            {
                // kick the client immediately
                Transport.active.ServerDisconnect(connectionId);
            }
        }

        static bool IsConnectionAllowed(int connectionId)
        {
            // connectionId needs to be != 0 because 0 is reserved for local player
            // note that some transports like kcp generate connectionId by
            // hashing which can be < 0 as well, so we need to allow < 0!
            if (connectionId == 0)
            {
                Debug.LogError($"Server.HandleConnect: invalid connectionId: {connectionId} . Needs to be != 0, because 0 is reserved for local player.");
                return false;
            }

            // connectionId not in use yet?
            if (connections.ContainsKey(connectionId))
            {
                Debug.LogError($"Server connectionId {connectionId} already in use...client will be kicked");
                return false;
            }

            // are more connections allowed? if not, kick
            // (it's easier to handle this in Mirror, so Transports can have
            //  less code and third party transport might not do that anyway)
            // (this way we could also send a custom 'tooFull' message later,
            //  Transport can't do that)
            if (connections.Count >= maxConnections)
            {
                Debug.LogError($"Server full, client {connectionId} will be kicked");
                return false;
            }

            return true;
        }

        internal static void OnConnected(NetworkConnectionToClient conn)
        {
            // Debug.Log($"Server accepted client:{conn}");

            // add connection and invoke connected event
            AddConnection(conn);
            OnConnectedEvent?.Invoke(conn);
        }

        static bool UnpackAndInvoke(NetworkConnectionToClient connection, NetworkReader reader, int channelId)
        {
            if (NetworkMessages.UnpackId(reader, out ushort msgType))
            {
                if (false /*please remove if found*/) SRMP.Log(msgType.ToString());
                // try to invoke the handler for that message
                if (handlers.TryGetValue(msgType, out NetworkMessageDelegate handler))
                {
                    handler.Invoke(connection, reader, channelId);
                    connection.lastMessageTime = Time.time;
                    return true;
                }
                else
                {
                    // message in a batch are NOT length prefixed to save bandwidth.
                    // every message needs to be handled and read until the end.
                    // otherwise it would overlap into the next message.
                    // => need to warn and disconnect to avoid undefined behaviour.
                    // => WARNING, not error. can happen if attacker sends random data.
                    if (false /*please remove if found*/) SRMP.Log($"Unknown message id: {msgType} for connection: {connection}. This can happen if no handler was registered for this message.");
                    // simply return false. caller is responsible for disconnecting.
                    //connection.Disconnect();
                    return false;
                }
            }
            else
            {
                // => WARNING, not error. can happen if attacker sends random data.
                if (false /*please remove if found*/) SRMP.Log($"Invalid message header for connection: {connection}.");
                // simply return false. caller is responsible for disconnecting.
                //connection.Disconnect();
                return false;
            }
        }

        // called by transport
        internal static void OnTransportData(int connectionId, ArraySegment<byte> dataRaw, int channelId)
        {
            if (connectionId == 0) return;

            if (connections.TryGetValue(connectionId, out NetworkConnectionToClient connection))
            {
                var isSRPacket = SRNetworkManager.SRDataTransport(dataRaw);

                var data = isSRPacket.Item2;

                if (isSRPacket.Item1)
                {
                    using (NetworkReaderPooled reader = NetworkReaderPool.Get(data))
                    {
                        // enough to read at least header size?
                        if (!UnpackAndInvoke(connection, reader, channelId)) // Using connection unbatcher reader is strange, idk if good.
                        {
                            // warn, disconnect and return if failed
                            // -> warning because attackers might send random data
                            // -> messages in a batch aren't length prefixed.
                            //    failing to read one would cause undefined
                            //    behaviour for every message afterwards.
                            //    so we need to disconnect.
                            // -> return to avoid the below unbatches.count error.
                            //    we already disconnected and handled it.
                            Debug.LogWarning($"NetworkServer: failed to unpack and invoke message from connectionId:{connectionId}.");

                        }
                    }
                    return;
                }

                if (false /*please remove if found*/) SRMP.Log($"Recieved connection from {connectionId}.");
                connection.unbatcher.reader.Position = 0;
                // client might batch multiple messages into one packet.
                // feed it to the Unbatcher.
                // NOTE: we don't need to associate a channelId because we
                //       always process all messages in the batch.
                if (!connection.unbatcher.AddBatch(data))
                {
                    Debug.LogWarning($"NetworkServer: received message from connectionId:{connectionId} was too short (messages should start with message id).");

                    return;
                }
                // process all messages in the batch.
                // only while NOT loading a scene.
                // if we get a scene change message, then we need to stop
                // processing. otherwise we might apply them to the old scene.
                // => fixes https://github.com/vis2k/Mirror/issues/2651
                //
                // NOTE: if scene starts loading, then the rest of the batch
                //       would only be processed when OnTransportData is called
                //       the next time.
                //       => consider moving processing to NetworkEarlyUpdate.
                while (!isLoadingScene &&
                       connection.unbatcher.GetNextMessage(out ArraySegment<byte> message, out double remoteTimestamp))
                {
                    using (NetworkReaderPooled reader = NetworkReaderPool.Get(message))
                    {
                        // enough to read at least header size?
                        if (reader.Remaining >= NetworkMessages.IdSize)
                        {
                            // make remoteTimeStamp available to the user
                            connection.remoteTimeStamp = remoteTimestamp;

                            // handle message
                            if (!UnpackAndInvoke(connection, reader, channelId)) // Using connection unbatcher reader is strange, idk if good.
                            {
                                // warn, disconnect and return if failed
                                // -> warning because attackers might send random data
                                // -> messages in a batch aren't length prefixed.
                                //    failing to read one would cause undefined
                                //    behaviour for every message afterwards.
                                //    so we need to disconnect.
                                // -> return to avoid the below unbatches.count error.
                                //    we already disconnected and handled it.
                                Debug.LogWarning($"NetworkServer: failed to unpack and invoke message from connectionId:{connectionId}.");

                                return;
                            }
                        }
                        // otherwise disconnect
                        else
                        {
                            Debug.LogWarning($"NetworkServer: received message from connectionId:{connectionId} was too short (messages should start with message id).");

                            return;
                        }
                    }
                }

                // if we weren't interrupted by a scene change,
                // then all batched messages should have been processed now.
                // otherwise batches would silently grow.
                // we need to log an error to avoid debugging hell.
                //
                // EXAMPLE: https://github.com/vis2k/Mirror/issues/2882
                // -> UnpackAndInvoke silently returned because no handler for id
                // -> Reader would never be read past the end
                // -> Batch would never be retired because end is never reached
                //
                // NOTE: prefixing every message in a batch with a length would
                //       avoid ever not reading to the end. for extra bandwidth.
                //
                // IMPORTANT: always keep this check to detect memory leaks.
                //            this took half a day to debug last time.
                if (!isLoadingScene && connection.unbatcher.BatchesCount > 0)
                {
                    Debug.LogError($"Still had {connection.unbatcher.BatchesCount} batches remaining after processing, even though processing was not interrupted by a scene change. This should never happen, as it would cause ever growing batches.\nPossible reasons:\n* A message didn't deserialize as much as it serialized\n*There was no message handler for a message id, so the reader wasn't read until the end.");
                }
            }
            else Debug.LogError($"HandleData Unknown connectionId:{connectionId}");
        }

        // called by transport
        // IMPORTANT: often times when disconnecting, we call this from Mirror
        //            too because we want to remove the connection and handle
        //            the disconnect immediately.
        //            => which is fine as long as we guarantee it only runs once
        //            => which we do by removing the connection!
        internal static void OnTransportDisconnected(int connectionId)
        {
            // Debug.Log($"Server disconnect client:{connectionId}");
            if (connections.TryGetValue(connectionId, out NetworkConnectionToClient conn))
            {
                conn.Cleanup();
                RemoveConnection(connectionId);
                // Debug.Log($"Server lost client:{connectionId}");

                // NetworkManager hooks into OnDisconnectedEvent to make
                // DestroyPlayerForConnection(conn) optional, e.g. for PvP MMOs
                // where players shouldn't be able to escape combat instantly.
                if (OnDisconnectedEvent != null)
                {
                    OnDisconnectedEvent.Invoke(conn);
                }
                // if nobody hooked into it, then simply call DestroyPlayerForConnection
                else
                {
                }
            }
        }

        // transport errors are forwarded to high level
        static void OnTransportError(int connectionId, TransportError error, string reason)
        {
            // transport errors will happen. logging a warning is enough.
            // make sure the user does not panic.
            Debug.LogWarning($"Server Transport Error for connId={connectionId}: {error}: {reason}. This is fine.");
            // try get connection. passes null otherwise.
            connections.TryGetValue(connectionId, out NetworkConnectionToClient conn);
            OnErrorEvent?.Invoke(conn, error, reason);
        }

        // transport errors are forwarded to high level
        static void OnTransportException(int connectionId, Exception exception)
        {
            // transport errors will happen. logging a warning is enough.
            // make sure the user does not panic.
            Debug.LogWarning($"Server Transport Exception for connId={connectionId}: {exception}");
            // try get connection. passes null otherwise.
            connections.TryGetValue(connectionId, out NetworkConnectionToClient conn);
            OnTransportExceptionEvent?.Invoke(conn, exception);
        }

        // message handlers ////////////////////////////////////////////////////
        /// <summary>Register a handler for message type T. Most should require authentication.</summary>
        // TODO obsolete this some day to always use the channelId version.
        //      all handlers in this version are wrapped with 1 extra action.
        public static void RegisterHandler<T>(Action<NetworkConnectionToClient, T> handler, bool requireAuthentication = true)
            where T : struct, NetworkMessage
        {
            ushort msgType = NetworkMessageId<T>.Id;
            if (handlers.ContainsKey(msgType))
            {
                Debug.LogWarning($"NetworkServer.RegisterHandler replacing handler for {typeof(T).FullName}, id={msgType}. If replacement is intentional, use ReplaceHandler instead to avoid this warning.");
            }

            // register Id <> Type in lookup for debugging.
            NetworkMessages.Lookup[msgType] = typeof(T);

            handlers[msgType] = NetworkMessages.WrapHandler(handler, requireAuthentication, exceptionsDisconnect);
        }

        /// <summary>Register a handler for message type T. Most should require authentication.</summary>
        // This version passes channelId to the handler.
        public static void RegisterHandler<T>(Action<NetworkConnectionToClient, T, int> handler, bool requireAuthentication = true)
            where T : struct, NetworkMessage
        {
            ushort msgType = NetworkMessageId<T>.Id;
            if (handlers.ContainsKey(msgType))
            {
                Debug.LogWarning($"NetworkServer.RegisterHandler replacing handler for {typeof(T).FullName}, id={msgType}. If replacement is intentional, use ReplaceHandler instead to avoid this warning.");
            }

            // register Id <> Type in lookup for debugging.
            NetworkMessages.Lookup[msgType] = typeof(T);

            handlers[msgType] = NetworkMessages.WrapHandler(handler, requireAuthentication, exceptionsDisconnect);
        }

        /// <summary>Replace a handler for message type T. Most should require authentication.</summary>
        public static void ReplaceHandler<T>(Action<T> handler, bool requireAuthentication = true)
            where T : struct, NetworkMessage
        {
            ReplaceHandler<T>((_, value) => { handler(value); }, requireAuthentication);
        }

        /// <summary>Replace a handler for message type T. Most should require authentication.</summary>
        public static void ReplaceHandler<T>(Action<NetworkConnectionToClient, T> handler, bool requireAuthentication = true)
            where T : struct, NetworkMessage
        {
            ushort msgType = NetworkMessageId<T>.Id;

            // register Id <> Type in lookup for debugging.
            NetworkMessages.Lookup[msgType] = typeof(T);

            handlers[msgType] = NetworkMessages.WrapHandler(handler, requireAuthentication, exceptionsDisconnect);
        }

        /// <summary>Replace a handler for message type T. Most should require authentication.</summary>
        public static void ReplaceHandler<T>(Action<NetworkConnectionToClient, T, int> handler, bool requireAuthentication = true)
            where T : struct, NetworkMessage
        {
            ushort msgType = NetworkMessageId<T>.Id;

            // register Id <> Type in lookup for debugging.
            NetworkMessages.Lookup[msgType] = typeof(T);

            handlers[msgType] = NetworkMessages.WrapHandler(handler, requireAuthentication, exceptionsDisconnect);
        }

        /// <summary>Unregister a handler for a message type T.</summary>
        public static void UnregisterHandler<T>()
            where T : struct, NetworkMessage
        {
            ushort msgType = NetworkMessageId<T>.Id;
            handlers.Remove(msgType);
        }

        /// <summary>Clears all registered message handlers.</summary>
        public static void ClearHandlers() => handlers.Clear();

        

        // disconnect //////////////////////////////////////////////////////////
        /// <summary>Disconnect all connections, including the local connection.</summary>
        // synchronous: handles disconnect events and cleans up fully before returning!
        public static void DisconnectAll()
        {
            // disconnect and remove all connections.
            // we can not use foreach here because if
            //   conn.Disconnect -> Transport.ServerDisconnect calls
            //   OnDisconnect -> NetworkServer.OnDisconnect(connectionId)
            // immediately then OnDisconnect would remove the connection while
            // we are iterating here.
            //   see also: https://github.com/vis2k/Mirror/issues/2357
            // this whole process should be simplified some day.
            // until then, let's copy .Values to avoid InvalidOperationException.
            // note that this is only called when stopping the server, so the
            // copy is no performance problem.
            foreach (NetworkConnectionToClient conn in connections.Values.ToList())
            {
                // disconnect via connection->transport
                conn.Disconnect();

                // we want this function to be synchronous: handle disconnect
                // events and clean up fully before returning.
                // -> OnTransportDisconnected can safely be called without
                //    waiting for the Transport's callback.
                // -> it has checks to only run once.

                // call OnDisconnected unless local player in host mod
                // TODO unnecessary check?
                if (conn.connectionId != NetworkConnection.LocalConnectionId)
                    OnTransportDisconnected(conn.connectionId);
            }

            // cleanup
            connections.Clear();
            localConnection = null;
            // this used to set active=false.
            // however, then Shutdown can't properly destroy objects:
            // https://github.com/MirrorNetworking/Mirror/issues/3344
            // "DisconnectAll" should only disconnect all, not set inactive.
            // active = false;
        }

        // add/remove/replace player ///////////////////////////////////////////
        /// <summary>Called by server after AddPlayer message to add the player for the connection.</summary>
        // When a player is added for a connection, the client for that
        // connection is made ready automatically. The player object is
        // automatically spawned, so you do not need to call NetworkServer.Spawn
        // for that object. This function is used for "adding" a player, not for
        // "replacing" the player on a connection. If there is already a player
        // on this playerControllerId for this connection, this will fail.
        public static bool AddPlayerForConnection(NetworkConnectionToClient conn, GameObject player, uint assetId)
        {
            
            return AddPlayerForConnection(conn, player);
        }

        /// <summary>Called by server after AddPlayer message to add the player for the connection.</summary>
        // When a player is added for a connection, the client for that
        // connection is made ready automatically. The player object is
        // automatically spawned, so you do not need to call NetworkServer.Spawn
        // for that object. This function is used for "adding" a player, not for
        // "replacing" the player on a connection. If there is already a player
        // on this playerControllerId for this connection, this will fail.
        public static bool AddPlayerForConnection(NetworkConnectionToClient conn, GameObject player)
        {
            
            // set ready if not set yet
            SetClientReady(conn);

            // Debug.Log($"Adding new playerGameObject object netId: {identity.netId} asset ID: {identity.assetId}");

            return true;
        }

        /// <summary>Replaces connection's player object. The old object is not destroyed.</summary>
        // This does NOT change the ready state of the connection, so it can
        // safely be used while changing scenes.
        public static bool ReplacePlayerForConnection(NetworkConnectionToClient conn, GameObject player, bool keepAuthority = false)
        {
            return true;
        }

        /// <summary>Replaces connection's player object. The old object is not destroyed.</summary>
        // This does NOT change the ready state of the connection, so it can
        // safely be used while changing scenes.
        public static bool ReplacePlayerForConnection(NetworkConnectionToClient conn, GameObject player, uint assetId, bool keepAuthority = false)
        {
            return ReplacePlayerForConnection(conn, player, keepAuthority);
        }

        /// <summary>Removes the player object from the connection</summary>
        // destroyServerObject: Indicates whether the server object should be destroyed
        // Deprecated 2024-06-06
        [Obsolete("Use RemovePlayerForConnection(NetworkConnectionToClient conn, RemovePlayerOptions removeOptions) instead")]
        public static void RemovePlayerForConnection(NetworkConnectionToClient conn, bool destroyServerObject)
        {
            if (destroyServerObject)
                RemovePlayerForConnection(conn, RemovePlayerOptions.Destroy);
            else
                RemovePlayerForConnection(conn, RemovePlayerOptions.Unspawn);
        }

        /// <summary>Removes player object for the connection. Options to keep the object in play, unspawn it, or destroy it.</summary>
        public static void RemovePlayerForConnection(NetworkConnectionToClient conn, RemovePlayerOptions removeOptions = RemovePlayerOptions.KeepActive)
        {
        }

        // ready ///////////////////////////////////////////////////////////////
        /// <summary>Flags client connection as ready (=joined world).</summary>
        // When a client has signaled that it is ready, this method tells the
        // server that the client is ready to receive spawned objects and state
        // synchronization updates. This is usually called in a handler for the
        // SYSTEM_READY message. If there is not specific action a game needs to
        // take for this message, relying on the default ready handler function
        // is probably fine, so this call wont be needed.
        public static void SetClientReady(NetworkConnectionToClient conn)
        {
            // Debug.Log($"SetClientReadyInternal for conn:{conn}");

            // set ready
            conn.isReady = true;

        }

        static void SpawnObserversForConnection(NetworkConnectionToClient conn)
        {
            //Debug.Log($"Spawning {spawned.Count} objects for conn {conn}");

            if (!conn.isReady)
            {
                // client needs to finish initializing before we can spawn objects
                // otherwise it would not find them.
                return;
            }

            // let connection know that we are about to start spawning...
            conn.Send(new ObjectSpawnStartedMessage());

            // add connection to each nearby NetworkIdentity's observers, which
            // internally sends a spawn message for each one to the connection.
            
            

            // let connection know that we finished spawning, so it can call
            // OnStartClient on each one (only after all were spawned, which
            // is how Unity's Start() function works too)
            conn.Send(new ObjectSpawnFinishedMessage());
        }

        /// <summary>Marks the client of the connection to be not-ready.</summary>
        // Clients that are not ready do not receive spawned objects or state
        // synchronization updates. They client can be made ready again by
        // calling SetClientReady().
        public static void SetClientNotReady(NetworkConnectionToClient conn)
        {
            conn.isReady = false;
            
            conn.Send(new NotReadyMessage());
        }

        /// <summary>Marks all connected clients as no longer ready.</summary>
        // All clients will no longer be sent state synchronization updates. The
        // player's clients can call ClientManager.Ready() again to re-enter the
        // ready state. This is useful when switching scenes.
        public static void SetAllClientsNotReady()
        {
            foreach (NetworkConnectionToClient conn in connections.Values)
            {
                SetClientNotReady(conn);
            }
        }

        // helper function to check a connection for inactivity and disconnect if necessary
        // returns true if disconnected
        static bool DisconnectIfInactive(NetworkConnectionToClient connection)
        {
            // check for inactivity
            if (disconnectInactiveConnections &&
                !connection.IsAlive(disconnectInactiveTimeout))
            {
                Debug.LogWarning($"Disconnecting {connection} for inactivity!");
                connection.Disconnect();
                return true;
            }
            return false;
        }

        // NetworkLateUpdate called after any Update/FixedUpdate/LateUpdate
        // (we add this to the UnityEngine in NetworkLoop)
        // internal for tests
        internal static readonly List<NetworkConnectionToClient> connectionsCopy =
            new List<NetworkConnectionToClient>();

        static void Broadcast()
        {
            // copy all connections into a helper collection so that
            // OnTransportDisconnected can be called while iterating.
            // -> OnTransportDisconnected removes from the collection
            // -> which would throw 'can't modify while iterating' errors
            // => see also: https://github.com/vis2k/Mirror/issues/2739
            // (copy nonalloc)
            // TODO remove this when we move to 'lite' transports with only
            //      socket send/recv later.
            connectionsCopy.Clear();
            foreach (var conn in connections.Values)
                connectionsCopy.Add(conn);

            // go through all connections
            foreach (NetworkConnectionToClient connection in connectionsCopy)
            {
                // check for inactivity. disconnects if necessary.
                if (DisconnectIfInactive(connection))
                    continue;

                // has this connection joined the world yet?
                // for each READY connection:
                //   pull in UpdateVarsMessage for each entity it observes
                if (connection.isReady)
                {
                    // send time for snapshot interpolation every sendInterval.
                    // BroadcastToConnection() may not send if nothing is new.
                    //
                    // sent over unreliable.
                    // NetworkTime / Transform both use unreliable.
                    //
                    // make sure Broadcast() is only called every sendInterval,
                    // even if targetFrameRate isn't set in host mode (!)
                    // (done via AccurateInterval)
                    connection.Send(new TimeSnapshotMessage(), Channels.Unreliable);

                }

                // update connection to flush out batched messages
                connection.Update();
            }

            // TODO this is way too slow because we iterate ALL spawned :/
            // TODO this is way too complicated :/
            // to understand what this tries to prevent, consider this example:
            //   monster has health=100
            //   we change health=200, dirty bit is set
            //   player comes in range, gets full serialization spawn packet.
            //   next Broadcast(), player gets the health=200 change because dirty bit was set.
            //
            // this code clears all dirty bits if no players are around to prevent it.
            // BUT there are two issues:
            //   1. what if a playerB was around the whole time?
            //   2. why don't we handle broadcast and spawn packets both HERE?
            //      handling spawn separately is why we need this complex magic
            //
            // see test: DirtyBitsAreClearedForSpawnedWithoutObservers()
            // see test: SyncObjectChanges_DontGrowWithoutObservers()
            //
            // PAUL: we also do this to avoid ever growing SyncList .changes
            //ClearSpawnedDirtyBits();
            //
            // this was moved to NetworkIdentity.AddObserver!
            // same result, but no more O(N) loop in here!
            // TODO remove this comment after moving spawning into Broadcast()!
        }

        // update //////////////////////////////////////////////////////////////
        // NetworkEarlyUpdate called before any Update/FixedUpdate
        // (we add this to the UnityEngine in NetworkLoop)
        internal static void NetworkEarlyUpdate()
        {
            // measure update time for profiling.
            if (active)
            {
                earlyUpdateDuration.Begin();
                fullUpdateDuration.Begin();
            }

            // process all incoming messages first before updating the world
            if (Transport.active != null)
                Transport.active.ServerEarlyUpdate();

            // step each connection's local time interpolation in early update.
            foreach (NetworkConnectionToClient connection in connections.Values)
                connection.UpdateTimeInterpolation();

            if (active) earlyUpdateDuration.End();
        }

        internal static void NetworkLateUpdate()
        {
            if (active)
            {
                // measure update time for profiling.
                lateUpdateDuration.Begin();

                // only broadcast world if active
                // broadcast every sendInterval.
                // AccurateInterval to avoid update frequency inaccuracy issues:
                // https://github.com/vis2k/Mirror/pull/3153
                //
                // for example, host mode server doesn't set .targetFrameRate.
                // Broadcast() would be called every tick.
                // snapshots might be sent way too often, etc.
                //
                // during tests, we always call Broadcast() though.
                //
                // also important for syncInterval=0 components like
                // NetworkTransform, so they can sync on same interval as time
                // snapshots _but_ not every single tick.
                // Unity 2019 doesn't have Time.timeAsDouble yet
                if (!Application.isPlaying || AccurateInterval.Elapsed(NetworkTime.localTime, sendInterval, ref lastSendTime))
                    Broadcast();
            }

            // process all outgoing messages after updating the world
            // (even if not active. still want to process disconnects etc.)
            if (Transport.active != null)
                Transport.active.ServerLateUpdate();

            // measure actual tick rate every second.
            if (active)
            {
                ++actualTickRateCounter;

                // NetworkTime.localTime has defines for 2019 / 2020 compatibility
                if (NetworkTime.localTime >= actualTickRateStart + 1)
                {
                    // calculate avg by exact elapsed time.
                    // assuming 1s wouldn't be accurate, usually a few more ms passed.
                    float elapsed = (float)(NetworkTime.localTime - actualTickRateStart);
                    actualTickRate = Mathf.RoundToInt(actualTickRateCounter / elapsed);
                    actualTickRateStart = NetworkTime.localTime;
                    actualTickRateCounter = 0;
                }

                // measure total update time. including transport.
                // because in early update, transport update calls handlers.
                lateUpdateDuration.End();
                fullUpdateDuration.End();
            }
        }
    }
}
