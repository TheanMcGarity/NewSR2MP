using Mirror.Discovery;
using Il2CppMonomiPark.SlimeRancher.Regions;
using NewSR2MP.Networking.Packet;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UIElements;

namespace Mirror
{
    // Mirror's Weaver automatically detects all NetworkReader function types,
    // but they do all need to be extensions.
    public static class NetworkReaderExtensions
    {
        public static byte ReadByte(this NetworkReader reader) => reader.ReadBlittable<byte>();
        public static byte? ReadByteNullable(this NetworkReader reader) => reader.ReadBlittableNullable<byte>();

        public static sbyte ReadSByte(this NetworkReader reader) => reader.ReadBlittable<sbyte>();
        public static sbyte? ReadSByteNullable(this NetworkReader reader) => reader.ReadBlittableNullable<sbyte>();

        // bool is not blittable. read as ushort.
        public static char ReadChar(this NetworkReader reader) => (char)reader.ReadBlittable<ushort>();
        public static char? ReadCharNullable(this NetworkReader reader) => (char?)reader.ReadBlittableNullable<ushort>();

        // bool is not blittable. read as byte.
        public static bool ReadBool(this NetworkReader reader) => reader.ReadBlittable<byte>() != 0;
        public static bool? ReadBoolNullable(this NetworkReader reader)
        {
            byte? value = reader.ReadBlittableNullable<byte>();
            return value.HasValue ? (value.Value != 0) : default(bool?);
        }

        public static short ReadShort(this NetworkReader reader) => (short)reader.ReadUShort();
        public static short? ReadShortNullable(this NetworkReader reader) => reader.ReadBlittableNullable<short>();

        public static ushort ReadUShort(this NetworkReader reader) => reader.ReadBlittable<ushort>();
        public static ushort? ReadUShortNullable(this NetworkReader reader) => reader.ReadBlittableNullable<ushort>();

        public static int ReadInt(this NetworkReader reader) => reader.ReadBlittable<int>();
        public static int? ReadIntNullable(this NetworkReader reader) => reader.ReadBlittableNullable<int>();

        public static uint ReadUInt(this NetworkReader reader) => reader.ReadBlittable<uint>();
        public static uint? ReadUIntNullable(this NetworkReader reader) => reader.ReadBlittableNullable<uint>();

        public static long ReadLong(this NetworkReader reader) => reader.ReadBlittable<long>();
        public static long? ReadLongNullable(this NetworkReader reader) => reader.ReadBlittableNullable<long>();

        public static ulong ReadULong(this NetworkReader reader) => reader.ReadBlittable<ulong>();
        public static ulong? ReadULongNullable(this NetworkReader reader) => reader.ReadBlittableNullable<ulong>();

        public static float ReadFloat(this NetworkReader reader) => reader.ReadBlittable<float>();
        public static float? ReadFloatNullable(this NetworkReader reader) => reader.ReadBlittableNullable<float>();

        public static double ReadDouble(this NetworkReader reader) => reader.ReadBlittable<double>();
        public static double? ReadDoubleNullable(this NetworkReader reader) => reader.ReadBlittableNullable<double>();

        public static decimal ReadDecimal(this NetworkReader reader) => reader.ReadBlittable<decimal>();
        public static decimal? ReadDecimalNullable(this NetworkReader reader) => reader.ReadBlittableNullable<decimal>();

        /// <exception cref="T:System.ArgumentException">if an invalid utf8 string is sent</exception>
        public static string ReadString(this NetworkReader reader)
        {
            // read number of bytes
            ushort size = reader.ReadUShort();

            // null support, see NetworkWriter
            if (size == 0)
                return null;

            ushort realSize = (ushort)(size - 1);

            // make sure it's within limits to avoid allocation attacks etc.
            if (realSize > NetworkWriter.MaxStringLength)
                throw new EndOfStreamException($"NetworkReader.ReadString - Value too long: {realSize} bytes. Limit is: {NetworkWriter.MaxStringLength} bytes");

            ArraySegment<byte> data = reader.ReadBytesSegment(realSize);

            // convert directly from buffer to string via encoding
            // throws in case of invalid utf8.
            // see test: ReadString_InvalidUTF8()
            return reader.encoding.GetString(data.Array, data.Offset, data.Count);
        }

        public static byte[] ReadBytes(this NetworkReader reader, int count)
        {
            // prevent allocation attacks with a reasonable limit.
            //   server shouldn't allocate too much on client devices.
            //   client shouldn't allocate too much on server in ClientToServer [SyncVar]s.
            if (count > NetworkReader.AllocationLimit)
            {
                // throw EndOfStream for consistency with ReadBlittable when out of data
                throw new EndOfStreamException($"NetworkReader attempted to allocate {count} bytes, which is larger than the allowed limit of {NetworkReader.AllocationLimit} bytes.");
            }

            byte[] bytes = new byte[count];
            reader.ReadBytes(bytes, count);
            return bytes;
        }

        /// <exception cref="T:OverflowException">if count is invalid</exception>
        public static byte[] ReadBytesAndSize(this NetworkReader reader)
        {
            // count = 0 means the array was null
            // otherwise count -1 is the length of the array
            uint count = reader.ReadUInt();
            // Use checked() to force it to throw OverflowException if data is invalid
            return count == 0 ? null : reader.ReadBytes(checked((int)(count - 1u)));
        }
        // Reads ArraySegment and size header
        /// <exception cref="T:OverflowException">if count is invalid</exception>
        public static ArraySegment<byte> ReadArraySegmentAndSize(this NetworkReader reader)
        {
            // count = 0 means the array was null
            // otherwise count - 1 is the length of the array
            uint count = reader.ReadUInt();
            // Use checked() to force it to throw OverflowException if data is invalid
            return count == 0 ? default : reader.ReadBytesSegment(checked((int)(count - 1u)));
        }

        public static Vector2 ReadVector2(this NetworkReader reader) => reader.ReadBlittable<Vector2>();
        public static Vector2? ReadVector2Nullable(this NetworkReader reader) => reader.ReadBlittableNullable<Vector2>();

        public static Vector3 ReadVector3(this NetworkReader reader) => reader.ReadBlittable<Vector3>();
        public static Vector3? ReadVector3Nullable(this NetworkReader reader) => reader.ReadBlittableNullable<Vector3>();

        public static Vector4 ReadVector4(this NetworkReader reader) => reader.ReadBlittable<Vector4>();
        public static Vector4? ReadVector4Nullable(this NetworkReader reader) => reader.ReadBlittableNullable<Vector4>();

        public static Vector2Int ReadVector2Int(this NetworkReader reader) => reader.ReadBlittable<Vector2Int>();
        public static Vector2Int? ReadVector2IntNullable(this NetworkReader reader) => reader.ReadBlittableNullable<Vector2Int>();

        public static Vector3Int ReadVector3Int(this NetworkReader reader) => reader.ReadBlittable<Vector3Int>();
        public static Vector3Int? ReadVector3IntNullable(this NetworkReader reader) => reader.ReadBlittableNullable<Vector3Int>();

        public static Color ReadColor(this NetworkReader reader) => reader.ReadBlittable<Color>();
        public static Color? ReadColorNullable(this NetworkReader reader) => reader.ReadBlittableNullable<Color>();

        public static Color32 ReadColor32(this NetworkReader reader) => reader.ReadBlittable<Color32>();
        public static Color32? ReadColor32Nullable(this NetworkReader reader) => reader.ReadBlittableNullable<Color32>();

        public static Quaternion ReadQuaternion(this NetworkReader reader) => reader.ReadBlittable<Quaternion>();
        public static Quaternion? ReadQuaternionNullable(this NetworkReader reader) => reader.ReadBlittableNullable<Quaternion>();

        // Rect is a struct with properties instead of fields
        public static Rect ReadRect(this NetworkReader reader) => new Rect(reader.ReadVector2(), reader.ReadVector2());
        public static Rect? ReadRectNullable(this NetworkReader reader) => reader.ReadBool() ? ReadRect(reader) : default(Rect?);

        
        // Ray is a struct with properties instead of fields
        public static Ray ReadRay(this NetworkReader reader) => new Ray(reader.ReadVector3(), reader.ReadVector3());
        public static Ray? ReadRayNullable(this NetworkReader reader) => reader.ReadBool() ? ReadRay(reader) : default(Ray?);

        // LayerMask is a struct with properties instead of fields
        public static LayerMask ReadLayerMask(this NetworkReader reader)
        {
            // LayerMask doesn't have a constructor that takes an initial value.
            // 32 layers as a flags enum, max value of 496, we only need a UShort.
            LayerMask layerMask = default;
            layerMask.value = reader.ReadUShort();
            return layerMask;
        }

        public static LayerMask? ReadLayerMaskNullable(this NetworkReader reader) => reader.ReadBool() ? ReadLayerMask(reader) : default(LayerMask?);

        public static Matrix4x4 ReadMatrix4x4(this NetworkReader reader) => reader.ReadBlittable<Matrix4x4>();
        public static Matrix4x4? ReadMatrix4x4Nullable(this NetworkReader reader) => reader.ReadBlittableNullable<Matrix4x4>();

        public static Guid ReadGuid(this NetworkReader reader)
        {
#if !UNITY_2021_3_OR_NEWER
            // Unity 2019 doesn't have Span yet
            return new Guid(reader.ReadBytes(16));
#else
            // ReadBlittable(Guid) isn't safe. see ReadBlittable comments.
            // Guid is Sequential, but we can't guarantee packing.
            if (reader.Remaining >= 16)
            {
                ReadOnlySpan<byte> span = new ReadOnlySpan<byte>(reader.buffer.Array, reader.buffer.Offset + reader.Position, 16);
                reader.Position += 16;
                return new Guid(span);
            }
            throw new EndOfStreamException($"ReadGuid out of range: {reader}");
#endif
        }
        public static Guid? ReadGuidNullable(this NetworkReader reader) => reader.ReadBool() ? ReadGuid(reader) : default(Guid?);

        

        public static NetworkBehaviourSyncVar ReadNetworkBehaviourSyncVar(this NetworkReader reader)
        {
            uint netId = reader.ReadUInt();
            byte componentIndex = default;

            // if netId is not 0, then index is also sent to read before returning
            if (netId != 0)
            {
                componentIndex = reader.ReadByte();
            }

            return new NetworkBehaviourSyncVar(netId, componentIndex);
        }

        

        // while SyncList<T> is recommended for NetworkBehaviours,
        // structs may have .List<T> members which weaver needs to be able to
        // fully serialize for NetworkMessages etc.
        // note that Weaver/Readers/GenerateReader() handles this manually.
        public static Il2CppSystem.Collections.Generic.List<T> ReadList<T>(this NetworkReader reader)
        {
            int length = reader.ReadInt();

            // 'null' is encoded as '-1'
            if (length < 0) return null;

            // prevent allocation attacks with a reasonable limit.
            //   server shouldn't allocate too much on client devices.
            //   client shouldn't allocate too much on server in ClientToServer [SyncVar]s.
            if (length > NetworkReader.AllocationLimit)
            {
                // throw EndOfStream for consistency with ReadBlittable when out of data
                throw new EndOfStreamException($"NetworkReader attempted to allocate a Il2CppSystem.Collections.Generic.List<{typeof(T)}> {length} elements, which is larger than the allowed limit of {NetworkReader.AllocationLimit}.");
            }

            Il2CppSystem.Collections.Generic.List<T> result = new Il2CppSystem.Collections.Generic.List<T>(length);
            for (int i = 0; i < length; i++)
            {
                result.Add(reader.Read<T>());
            }
            return result;
        }

        // while SyncSet<T> is recommended for NetworkBehaviours,
        // structs may have .Set<T> members which weaver needs to be able to
        // fully serialize for NetworkMessages etc.
        // note that Weaver/Readers/GenerateReader() handles this manually.
        // TODO writer not found. need to adjust weaver first. see tests.
        /*
        public static HashSet<T> ReadHashSet<T>(this NetworkReader reader)
        {
            int length = reader.ReadInt();
            if (length < 0)
                return null;
            HashSet<T> result = new HashSet<T>();
            for (int i = 0; i < length; i++)
            {
                result.Add(reader.Read<T>());
            }
            return result;
        }
        */

        public static T[] ReadArray<T>(this NetworkReader reader)
        {
            int length = reader.ReadInt();

            // 'null' is encoded as '-1'
            if (length < 0) return null;

            // prevent allocation attacks with a reasonable limit.
            //   server shouldn't allocate too much on client devices.
            //   client shouldn't allocate too much on server in ClientToServer [SyncVar]s.
            if (length > NetworkReader.AllocationLimit)
            {
                // throw EndOfStream for consistency with ReadBlittable when out of data
                throw new EndOfStreamException($"NetworkReader attempted to allocate an Array<{typeof(T)}> with {length} elements, which is larger than the allowed limit of {NetworkReader.AllocationLimit}.");
            }

            // we can't check if reader.Remaining < length,
            // because we don't know sizeof(T) since it's a managed type.
            // if (length > reader.Remaining) throw new EndOfStreamException($"Received array that is too large: {length}");

            T[] result = new T[length];
            for (int i = 0; i < length; i++)
            {
                result[i] = reader.Read<T>();
            }
            return result;
        }

        public static Uri ReadUri(this NetworkReader reader)
        {
            string uriString = reader.ReadString();
            return (string.IsNullOrWhiteSpace(uriString) ? null : new Uri(uriString));
        }

        public static Texture2D ReadTexture2D(this NetworkReader reader)
        {
            // support 'null' textures for [SyncVar]s etc.
            // https://github.com/vis2k/Mirror/issues/3144
            short width = reader.ReadShort();
            if (width == -1) return null;

            // read height
            short height = reader.ReadShort();

            // prevent allocation attacks with a reasonable limit.
            //   server shouldn't allocate too much on client devices.
            //   client shouldn't allocate too much on server in ClientToServer [SyncVar]s.
            // log an error and return default.
            // we don't want attackers to be able to trigger exceptions.
            int totalSize = width * height;
            if (totalSize > NetworkReader.AllocationLimit)
            {
                Debug.LogWarning($"NetworkReader attempted to allocate a Texture2D with total size (width * height) of {totalSize}, which is larger than the allowed limit of {NetworkReader.AllocationLimit}.");
                return null;
            }

            Texture2D texture2D = new Texture2D(width, height);

            // read pixel content
            Color32[] pixels = reader.ReadArray<Color32>();
            texture2D.SetPixels32(pixels);
            texture2D.Apply();
            return texture2D;
        }

        public static Sprite ReadSprite(this NetworkReader reader)
        {
            // support 'null' textures for [SyncVar]s etc.
            // https://github.com/vis2k/Mirror/issues/3144
            Texture2D texture = reader.ReadTexture2D();
            if (texture == null) return null;

            // otherwise create a valid sprite
            return Sprite.Create(texture, reader.ReadRect(), reader.ReadVector2());
        }

        public static DateTime ReadDateTime(this NetworkReader reader) => DateTime.FromOADate(reader.ReadDouble());
        public static DateTime? ReadDateTimeNullable(this NetworkReader reader) => reader.ReadBool() ? ReadDateTime(reader) : default(DateTime?);





        // Custom Non-Mirror stuff

        public static TestLogMessage ReadTestLogMessage(this NetworkReader reader)
        {
            var log = reader.ReadString();
            return new TestLogMessage() { MessageToLog = log };
        }

        public static ServerRequest ReadDiscoveryRequestMessage(this NetworkReader reader)
        {
            return new ServerRequest();
        }
        public static SetMoneyMessage ReadMoneyMessage(this NetworkReader reader)
        {
            var mon = reader.ReadInt();
            if (false /*please remove if found*/) SRMP.Log(mon.ToString());
            return new SetMoneyMessage()
            {
                newMoney = mon
            };
        }
        public static SetKeysMessage ReadKeysMessge(this NetworkReader reader)
        {
            var mon = reader.ReadInt();
            return new SetKeysMessage()
            {
                newMoney = mon
            };
        }
        public static NetworkPingMessage ReadPingMessage(this NetworkReader reader)
        {
            var time = reader.ReadDouble();
            var pred = reader.ReadDouble();
            return new NetworkPingMessage()
            {
                localTime = time,
                predictedTimeAdjusted = pred
            };
        }
        public static NotReadyMessage ReadUnreadyMessage(this NetworkReader reader) => new NotReadyMessage();
        public static ReadyMessage ReadReadyMessage(this NetworkReader reader) => new ReadyMessage();
        public static AddPlayerMessage ReadAddPlayerMessage(this NetworkReader reader) => new AddPlayerMessage();
        public static TimeSnapshotMessage ReadTimeSnapshotMessage(this NetworkReader reader) => new TimeSnapshotMessage();
        public static ServerResponse ReadDiscoveryResponseMessage(this NetworkReader reader)
        {
            Uri path = reader.ReadUri();

            //int port = reader.ReadInt();
            //long address = reader.ReadLong();

            long id = reader.ReadLong();
            string pc = reader.ReadString();

            ServerResponse res = new ServerResponse()
            {
                serverId = id,
                uri = path,
                ServerName = pc
            };

            return res;
        }
        public static PlayerUpdateMessage ReadPlayerMessage(this NetworkReader reader)
        {
            var id = reader.ReadInt();
            var pos = reader.ReadVector3();
            var rot = reader.ReadQuaternion();

            var returnval = new PlayerUpdateMessage()
            {
                id = id,
                pos = pos,
                rot = rot
            };
            return returnval;
        }

        public static SceneMessage ReadSceneMessage(this NetworkReader reader)
        {
            string name = reader.ReadString();
            SceneOperation op = (SceneOperation)reader.ReadInt();
            bool ch = reader.ReadBool();

            return new SceneMessage()
            {
                sceneName = name,
                sceneOperation = op,
                customHandling = ch
            };
        }
        public static LandPlotMessage ReadLandPlotMessage(this NetworkReader reader)
        {
            LandplotUpdateType mode = (LandplotUpdateType)reader.ReadByte();
            string id = reader.ReadString();
            LandPlotMessage message = new LandPlotMessage()
            {
                messageType = mode,
                id = id,
            };
            if (mode == LandplotUpdateType.SET)
                message.type = (LandPlot.Id)reader.ReadByte();
            else
                message.upgrade = (LandPlot.Upgrade)reader.ReadByte();

            return message;
        }
        public static PlayerJoinMessage ReadPlayerJoinMessage(this NetworkReader reader)
        {
            return new PlayerJoinMessage()
            {
                id = reader.ReadInt(),
                local = reader.ReadBool()
            };
        }
        
        public static ClientUserMessage ReadClientUserMessage(this NetworkReader reader)
        {
            return new ClientUserMessage()
            {
                guid = reader.ReadGuid(),
                name = reader.ReadString(),
            };
        }

        public static PlayerLeaveMessage ReadPlayerLeaveMessage(this NetworkReader reader)
        {
            return new PlayerLeaveMessage()
            {
                id = reader.ReadInt(),
            };
        }
        public static GordoEatMessage ReadGordoEatMessage(this NetworkReader reader)
        {
            return new GordoEatMessage()
            {
                id = reader.ReadString(),
                count = reader.ReadInt()
            };
        }
        public static GordoBurstMessage ReadGordoBurstMessage(this NetworkReader reader)
        {
            return new GordoBurstMessage()
            {
                id = reader.ReadString()
            };
        }
        public static PediaMessage ReadPediaMessage(this NetworkReader reader)
        {
            return new PediaMessage()
            {
                id = reader.ReadString()
            };
        }
        public static AmmoAddMessage ReadAmmoAddMessage(this NetworkReader reader)
        {
            return new AmmoAddMessage()
            {
                ident = reader.ReadString(),
                id = reader.ReadString()
            };
        }

        public static AmmoRemoveMessage ReadAmmoRemoveMessage(this NetworkReader reader)
        {
            return new AmmoRemoveMessage()
            {
                index = reader.ReadInt(),
                id = reader.ReadString(),
                count = reader.ReadInt()
            };
        }
        public static MapUnlockMessage ReadMapUnlockMessage(this NetworkReader reader)
        {
            return new MapUnlockMessage()
            {
                id = reader.ReadString()
            };
        }
        public static AmmoEditSlotMessage ReadAmmoAddToSlotMessage(this NetworkReader reader)
        {
            return new AmmoEditSlotMessage()
            {
                ident = reader.ReadString(),
                slot = reader.ReadInt(),
                count = reader.ReadInt(),
                id = reader.ReadString()
            };
        }
        public static GardenPlantMessage ReadGardenPlantMessage(this NetworkReader reader)
        {
            return new GardenPlantMessage()
            {
                ident = reader.ReadString(),
                replace = reader.ReadBool(),
                id = reader.ReadString(),
            };
        }
        
        public static AmmoData ReadAmmoData(this NetworkReader reader)
        {
            AmmoData data = new AmmoData()
            {
                count = reader.ReadInt(),
                slot = reader.ReadInt(),
                id = reader.ReadString(),
            };
            return data;
        }

        public static LoadMessage ReadLoadMessage(this NetworkReader reader)
        {

            int length = reader.ReadInt();

            Il2CppSystem.Collections.Generic.List<InitActorData> actors = new Il2CppSystem.Collections.Generic.List<InitActorData>();
            for (int i = 0; i < length; i++) 
            {
                long id = reader.ReadLong();
                string ident = reader.ReadString();
                Vector3 actorPos = reader.ReadVector3();
                actors.Add(new InitActorData()
                {
                    id = id,
                    ident = ident,
                    pos = actorPos
                });
            }
            int length2 = reader.ReadInt();
            Il2CppSystem.Collections.Generic.List<InitPlayerData> players = new Il2CppSystem.Collections.Generic.List<InitPlayerData>();
            for (int i = 0; i < length2; i++)
            {
                int id = reader.ReadInt();
                players.Add(new InitPlayerData()
                {
                    id = id
                });
            }
            int length3 = reader.ReadInt();
            Il2CppSystem.Collections.Generic.List<InitPlotData> plots = new Il2CppSystem.Collections.Generic.List<InitPlotData>();
            for (int i = 0; i < length3; i++)
            {
                string id = reader.ReadString();
                LandPlot.Id type = (LandPlot.Id)reader.ReadInt();
                int upgLength = reader.ReadInt();
                HashSet<LandPlot.Upgrade> upgrades = new HashSet<LandPlot.Upgrade>();
                for (int i2 = 0; i2 < upgLength; i2++)
                {
                    upgrades.Add((LandPlot.Upgrade)reader.ReadInt());
                }
                InitSiloData siloData;
                int slots = reader.ReadInt();
                int ammLength = reader.ReadInt();
                HashSet<AmmoData> ammoDatas = new HashSet<AmmoData>();
                for (int i2 = 0; i2 < ammLength; i2++)
                {
                    var data = reader.ReadAmmoData();
                    ammoDatas.Add(data);
                }
                siloData = new InitSiloData()
                {
                    slots = slots,
                    ammo = ammoDatas
                };
                var crop = reader.ReadString();
                plots.Add(new InitPlotData()
                {
                    type = type,
                    id = id,
                    upgrades = upgrades,
                    siloData = siloData,
                    cropIdent = crop
                });
            }
            int length4 = reader.ReadInt();
            HashSet<InitGordoData> gordos = new HashSet<InitGordoData>();
            for (int i = 0; i < length4; i++)
            {
                string id = reader.ReadString();
                int eaten = reader.ReadInt();
                gordos.Add(new InitGordoData()
                {
                    id = id,
                    eaten = eaten,
                });
            }

            int pedLength = reader.ReadInt();
            Il2CppSystem.Collections.Generic.List<string> pedias = new Il2CppSystem.Collections.Generic.List<string>();
            for (int i = 0; i < pedLength; i++)
            {
                pedias.Add(reader.ReadString());
            }
            int mapLength = reader.ReadInt();
            Il2CppSystem.Collections.Generic.List<string> maps = new Il2CppSystem.Collections.Generic.List<string>();
            for (int i = 0; i < mapLength; i++)
            {
                maps.Add(reader.ReadString());
            }
            int accLength = reader.ReadInt();
            Il2CppSystem.Collections.Generic.List<InitAccessData> access = new  Il2CppSystem.Collections.Generic.List<InitAccessData>();
            for (int i = 0; i < accLength; i++)
            {
                string id = reader.ReadString();
                bool open = reader.ReadBool();
                InitAccessData accessData = new InitAccessData()
                {
                    id = id,
                    open = open,
                };
                access.Add(accessData);
            }

            var pid = reader.ReadInt();
            var pos = reader.ReadVector3();
            var rot = reader.ReadVector3();
            
            var localAmmoCount = reader.ReadInt();
            
            Il2CppSystem.Collections.Generic.List<AmmoData> localAmmo = new Il2CppSystem.Collections.Generic.List<AmmoData>();
            for (int i = 0; i < localAmmoCount; i++)
            {
                localAmmo.Add(reader.ReadAmmoData());
            }

            var player = new LocalPlayerData()
            {
                pos = pos,
                rot = rot,
                ammo = localAmmo
            };

            var money = reader.ReadInt();
            var keys = reader.ReadInt();

            var pUpgradesCount = reader.ReadInt();
            Il2CppSystem.Collections.Generic.List<string> pUpgrades = new Il2CppSystem.Collections.Generic.List<string>();
            for (int i = 0; i < pUpgradesCount; i++)
            {
                var upg = reader.ReadString();

                pUpgrades.Add(upg);
            }

            var time = reader.ReadDouble();

            var sm = reader.ReadBool();
            var sk = reader.ReadBool();
            var su = reader.ReadBool();

            return new LoadMessage()
            {
                initActors = actors,
                initPlayers = players,
                initPlots = plots,
                initGordos = gordos,
                initPedias = pedias,
                initAccess = access,
                initMaps = maps,
                localPlayerSave = player,
                playerID = pid,
                money = money,
                keys = keys,
                upgrades = pUpgrades,
                time = time,
                sharedKeys=sk,
                sharedMoney=sm,
                sharedUpgrades=su,
            };
        }
        public static TimeSyncMessage ReadTimeMessage(this NetworkReader reader)
        {
            return new TimeSyncMessage()
            {
                time = reader.ReadDouble()
            };
        }


        public static ActorSpawnClientMessage ReadActorSpawnClientMessage(this NetworkReader reader)
        {
            var ident = reader.ReadString();
            var pos = reader.ReadVector3();
            var rot = reader.ReadVector3();
            var vel = reader.ReadVector3();
            var p = reader.ReadInt();
            return new ActorSpawnClientMessage()
            {
                ident = ident,
                position = pos,
                rotation = rot,
                velocity = vel,
                player = p
            };
        }
        public static ActorDestroyGlobalMessage ReadActorDestroyMessage(this NetworkReader reader)
        {
            return new ActorDestroyGlobalMessage()
            {
                id = reader.ReadLong()
            };
        }
        public static ResourceStateMessage ReadResourceStateMessage(this NetworkReader reader)
        {
            return new ResourceStateMessage()
            {
                state = (ResourceCycle.State)reader.ReadByte(),
                id = reader.ReadLong(),
            };
        }
        public static ActorUpdateOwnerMessage ReadActorOwnMessage(this NetworkReader reader)
        {
            return new ActorUpdateOwnerMessage()
            {
                id = reader.ReadLong(),
                player = reader.ReadInt(),
            };
        }
        public static ActorUpdateClientMessage ReadActorClientMessage(this NetworkReader reader)
        {
            var id = reader.ReadLong();
            var pos = reader.ReadVector3();
            var rot = reader.ReadVector3();
            return new ActorUpdateClientMessage()
            {
                id = id,
                position = pos,
                rotation = rot
            };
        }
        public static ActorUpdateMessage ReadActorMessage(this NetworkReader reader)
        {
            var id = reader.ReadLong();
            var pos = reader.ReadVector3();
            var rot = reader.ReadVector3();
            return new ActorUpdateMessage()
            {
                id = id,
                position = pos,
                rotation = rot
            };
        }
        public static ActorSpawnMessage ReadActorSpawnMessage(this NetworkReader reader)
        {
            var id = reader.ReadLong();
            var ident = reader.ReadString();
            var pos = reader.ReadVector3();
            var rot = reader.ReadVector3();
            var p = reader.ReadInt();
            return new ActorSpawnMessage()
            {
                ident = ident,
                position = pos,
                rotation = rot,
                id = id,
                player = p
            };
        }
        public static DoorOpenMessage ReadDoorOpenMessage(this NetworkReader reader)
        {
            return new DoorOpenMessage()
            {
                id = reader.ReadString()
            };
        }
        public static SleepMessage ReadSleepMessage(this NetworkReader reader)
        {
            return new SleepMessage()
            {
                time = reader.ReadDouble()
            };
        }
        public static ActorChangeHeldOwnerMessage ReadActorChangeHeldOwnerMessage(this NetworkReader reader)
        {
            return new ActorChangeHeldOwnerMessage()
            {
                id = reader.ReadLong()
            };
        }
    }
}
