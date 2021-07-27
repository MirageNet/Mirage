using System;
using System.Collections.Generic;
using System.Linq;
using Mirage.Serialization;
using Mirage.SocketLayer;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Mirage.Experimental.State2
{
    public class SnapshotState
    {
        public ulong Sequence;
    }
    public class StateTransfer
    {
        NetworkServer server;
        NetworkClient client;

        WorldSnapshot[] snapshots = new WorldSnapshot[256];
        Mirage.SocketLayer.Sequencer sequencer = new Mirage.SocketLayer.Sequencer(8);

        //server
        List<DemoNetworkIdentity> serverObjects;
        Dictionary<INetworkPlayer, ulong> ackedSnapshot = new Dictionary<INetworkPlayer, ulong>();

        //client
        Dictionary<uint, DemoNetworkIdentity> clientObjects;
        private Dictionary<uint, Func<GameObject>> clientSpawnDictionary;
        private Scene clientScene;

        private StateTransfer()
        {
            DeltaWriters.Init();
        }
        public static StateTransfer Create(NetworkServer server, List<DemoNetworkIdentity> serverObjects)
        {
            var stateTranfer = new StateTransfer()
            {
                server = server,
                serverObjects = serverObjects
            };
            server.Connected.AddListener(player =>
            {
                stateTranfer.ackedSnapshot.Add(player, stateTranfer.sequencer.MoveInBounds(ulong.MaxValue));
            });
            server.Disconnected.AddListener(player =>
            {
                stateTranfer.ackedSnapshot.Remove(player);
            });
            return stateTranfer;
        }
        public static StateTransfer Create(NetworkClient client, UnityEngine.SceneManagement.Scene clientScene, Dictionary<uint, DemoNetworkIdentity> clientObjects, Dictionary<uint, Func<GameObject>> clientSpawnDictionary)
        {
            var stateTranfer = new StateTransfer()
            {
                client = client,
                clientScene = clientScene,
                clientObjects = clientObjects,
                clientSpawnDictionary = clientSpawnDictionary,
            };
            client.Connected.AddListener(_ =>
            {
                client.MessageHandler.RegisterHandler<StateMessage>(stateTranfer.ReceiveState);
            });
            return stateTranfer;
        }

        private void ReceiveState(INetworkPlayer player, StateMessage message)
        {
            using (PooledNetworkReader reader = NetworkReaderPool.GetReader(message.segment))
            {
                ulong seq1 = reader.Read(8);
                ulong deltaSeq = reader.Read(6);
                ulong seq = sequencer.MoveInBounds(seq1 + deltaSeq);

                WorldSnapshot previous = snapshots[seq1];

                float time = DeltaValue<float>.Read(reader, previous.time);

                var newSnapshot = new Dictionary<uint, ObjectSnapshot>();
                // add all objects from previous snapshot
                for (int i = 0; i < previous.objects?.Length; i++)
                {
                    newSnapshot.Add(previous.objects[i].id, previous.objects[i]);
                }

                DecodeObjectList(newSnapshot, reader);

                snapshots[seq] = new WorldSnapshot
                {
                    time = time,
                    sequence = seq,
                    objects = newSnapshot.Select(x => x.Value).ToArray(),
                };
            }
        }

        void DecodeObjectList(Dictionary<uint, ObjectSnapshot> objects, NetworkReader reader)
        {
            while (reader.CanReadBytes(1))
            {
                uint nextId = reader.ReadPackedUInt32();
                Debug.Assert(nextId != 0);
                bool destroyed = reader.ReadBoolean();
                if (destroyed)
                {
                    //Debug.Log($"Read id:{nextId} state:Destroyed");

                    objects.Remove(nextId);
                    DestroyObject(nextId);
                }
                else
                {
                    // if Exist, Delta
                    if (objects.TryGetValue(nextId, out ObjectSnapshot previousObj))
                    {
                        //Debug.Log($"Read id:{nextId} state:Delta");
                        objects[nextId] = ApplyDeltaFields(reader, nextId, previousObj);
                    }
                    // else, Whole
                    else
                    {
                        //Debug.Log($"Read id:{nextId} state:New");
                        objects.Add(nextId, FindOrSpawnObject(reader, nextId));
                    }
                }
            }
        }

        private ObjectSnapshot ApplyDeltaFields(NetworkReader reader, uint id, ObjectSnapshot previousObj)
        {
            // todo tryGet
            DemoNetworkIdentity obj = clientObjects[id];

            var fields = new FieldSnapshot[previousObj.fields.Length];
            int fieldIndex = 0;

            {
                if (FieldSnapshot.ReadDelta(reader, previousObj.fields[fieldIndex], out FieldSnapshot newSnapshot, out string value))
                {
                    obj.name = value;
                }
                fields[fieldIndex] = newSnapshot;
                fieldIndex++;
            }

            if (obj.networkTransform != null)
            {
                {
                    if (FieldSnapshot.ReadDelta(reader, previousObj.fields[fieldIndex], out FieldSnapshot newSnapshot, out Vector3 value))
                    {
                        obj.networkTransform.Position = value;
                    }
                    fields[fieldIndex] = newSnapshot;
                    fieldIndex++;
                }

                {
                    if (FieldSnapshot.ReadDelta(reader, previousObj.fields[fieldIndex], out FieldSnapshot newSnapshot, out Quaternion value))
                    {
                        obj.networkTransform.Rotation = value;
                    }
                    fields[fieldIndex] = newSnapshot;
                    fieldIndex++;
                }
            }
            if (obj.health != null)
            {
                {
                    if (FieldSnapshot.ReadDelta(reader, previousObj.fields[fieldIndex], out FieldSnapshot newSnapshot, out int value))
                    {
                        obj.health.Health = value;
                    }
                    fields[fieldIndex] = newSnapshot;
                    fieldIndex++;
                }
            }
            if (obj.player != null)
            {
                {
                    if (FieldSnapshot.ReadDelta(reader, previousObj.fields[fieldIndex], out FieldSnapshot newSnapshot, out int value))
                    {
                        obj.player.Money = value;
                    }
                    fields[fieldIndex] = newSnapshot;
                    fieldIndex++;
                }

                {
                    if (FieldSnapshot.ReadDelta(reader, previousObj.fields[fieldIndex], out FieldSnapshot newSnapshot, out int value))
                    {
                        obj.player.Damage = value;
                    }
                    fields[fieldIndex] = newSnapshot;
                    fieldIndex++;
                }
            }

            return new ObjectSnapshot
            {
                id = id,
                spawnId = previousObj.spawnId,
                fields = fields,
            };
        }

        private ObjectSnapshot FindOrSpawnObject(NetworkReader reader, uint id)
        {
            // because spawned next read will be spawn id
            uint spawnId = reader.ReadUInt32();
            // object might exist on client, even if it isn't in previous snapshot
            //     in that case we dont spawn it, but find exist, but we still read whole state
            //     This can happen when server sends 2 snapshots before client acks
            if (!clientObjects.TryGetValue(id, out DemoNetworkIdentity obj))
            {
                obj = clientSpawnDictionary[spawnId].Invoke().GetComponent<DemoNetworkIdentity>();
                SceneManager.MoveGameObjectToScene(obj.gameObject, clientScene);
                obj.Init(id, spawnId);
                clientObjects[id] = obj;
            }

            return ReadWholeObjects(reader, obj);
        }

        private ObjectSnapshot ReadWholeObjects(NetworkReader reader, DemoNetworkIdentity obj)
        {
            var fields = new FieldSnapshot[1 +
                (obj.networkTransform != null ? 2 : 0) +
                (obj.health != null ? 1 : 0) +
                (obj.player != null ? 2 : 0)
            ];
            int fieldIndex = 0;

            {
                FieldSnapshot.ReadWhole(reader, out FieldSnapshot newSnapshot, out string value);
                obj.name = value;
                fields[fieldIndex] = newSnapshot;
                fieldIndex++;
            }

            if (obj.networkTransform != null)
            {
                {
                    FieldSnapshot.ReadWhole(reader, out FieldSnapshot newSnapshot, out Vector3 value);
                    obj.networkTransform.Position = value;
                    fields[fieldIndex] = newSnapshot;
                    fieldIndex++;
                }

                {
                    FieldSnapshot.ReadWhole(reader, out FieldSnapshot newSnapshot, out Quaternion value);
                    obj.networkTransform.Rotation = value;

                    fields[fieldIndex] = newSnapshot;
                    fieldIndex++;
                }
            }
            if (obj.health != null)
            {
                {
                    FieldSnapshot.ReadWhole(reader, out FieldSnapshot newSnapshot, out int value);
                    obj.health.Health = value;
                    fields[fieldIndex] = newSnapshot;
                    fieldIndex++;
                }
            }
            if (obj.player != null)
            {
                {
                    FieldSnapshot.ReadWhole(reader, out FieldSnapshot newSnapshot, out int value);
                    obj.player.Money = value;
                    fields[fieldIndex] = newSnapshot;
                    fieldIndex++;
                }

                {
                    FieldSnapshot.ReadWhole(reader, out FieldSnapshot newSnapshot, out int value);
                    obj.player.Damage = value;
                    fields[fieldIndex] = newSnapshot;
                    fieldIndex++;
                }
            }

            return new ObjectSnapshot
            {
                id = obj.Id,
                spawnId = obj.SpawnId,
                fields = fields,
            };
        }

        private void DestroyObject(uint id)
        {
            GameObject.Destroy(clientObjects[id]);
        }

        public void Update()
        {
            if (server == null) { throw new InvalidOperationException(); }

            ulong sequence = sequencer.Next();
            CreateSnapshot(sequence);
            foreach (KeyValuePair<INetworkPlayer, ulong> kvp in ackedSnapshot)
            {
                SendUpdate(kvp.Key, kvp.Value, sequence);
            }
        }


        private void SendUpdate(INetworkPlayer player, ulong previousSequence, ulong sequence)
        {
            WorldSnapshot a = snapshots[previousSequence];
            WorldSnapshot b = snapshots[sequence];
            using (PooledNetworkWriter writer = NetworkWriterPool.GetWriter())
            {
                // write previous, and delta
                //   reader will then know sequence of both snapshots
                writer.Write(previousSequence, 8);
                long deltaSeq = sequencer.Distance(sequence, previousSequence);
                Debug.Assert(deltaSeq > 0);
                writer.Write((ulong)deltaSeq, 6);
                CreateDelta(writer, a, b);

                Debug.Log($"Snapshot Size: {writer.ByteLength}");
                INotifyToken token = SendNotify(player, new StateMessage
                {
                    segment = writer.ToArraySegment(),
                });
                token.Delivered += () =>
                {
                    ackedSnapshot[player] = sequence;
                };
            }
        }

        private INotifyToken SendNotify<T>(INetworkPlayer player, T msg) where T : struct
        {
            using (PooledNetworkWriter writer = NetworkWriterPool.GetWriter())
            {
                MessagePacker.Pack(msg, writer);

                var segment = writer.ToArraySegment();
                return player.Connection.SendNotify(segment);
            }
        }

        private void CreateDelta(PooledNetworkWriter writer, WorldSnapshot a, WorldSnapshot b)
        {
            DeltaValue<float>.Write(writer, a.time, b.time);

            // assume arrays are not null here.....we can handle special case where a array is null later
            int ai = 0;
            int bi = 0;
            int loopCount = 0;
            // safety numbers to stop infinite loop
            while (loopCount < 100)
            {
                loopCount++;

                // if both over, then break
                if ((a.objects == null || ai >= a.objects.Length) && (b.objects == null || bi >= b.objects.Length))
                {
                    break;
                }

                // arrays should always be in order of netid
                ObjectSnapshot aobj = ai < a.objects?.Length ? a.objects[ai] : default;
                ObjectSnapshot bobj = bi < b.objects?.Length ? b.objects[bi] : default;


                if (aobj.Valid() && aobj.id == bobj.id)
                {
                    //Debug.Log($"Write a:{aobj.id} b:{bobj.id} state:Delta");
                    WriteObjectDelta(writer, aobj, bobj);
                    ai++;
                    bi++;
                    continue;
                }
                else
                {
                    if (!aobj.Valid() || aobj.id > bobj.id)
                    {
                        //Debug.Log($"Write a:{aobj.id} b:{bobj.id} state:New");
                        // b is new object
                        WriteWholeObject(writer, bobj);
                        bi++;
                        continue;
                    }
                    else
                    {
                        //Debug.Log($"Write a:{aobj.id} b:{bobj.id} state:Destroyed");
                        // a is destroyed object
                        WriteDestroyed(writer, aobj);
                        ai++;
                        continue;
                    }
                }
            }
        }

        private void WriteWholeObject(PooledNetworkWriter writer, ObjectSnapshot obj)
        {
            writer.WritePackedUInt32(obj.id);
            writer.WriteBoolean(0); // destroyed
            writer.WriteUInt32(obj.spawnId);
            foreach (FieldSnapshot field in obj.fields)
            {
                field.WriteWhole(writer);
            }
        }

        private void WriteDestroyed(PooledNetworkWriter writer, ObjectSnapshot obj)
        {
            writer.WritePackedUInt32(obj.id);
            writer.WriteBoolean(1);// destroyed
        }

        private void WriteObjectDelta(PooledNetworkWriter writer, ObjectSnapshot a, ObjectSnapshot b)
        {
            writer.WritePackedUInt32(a.id);
            writer.WriteBoolean(0); // destroyed
            Debug.Assert(a.fields.Length == b.fields.Length);
            Debug.Assert(a.id == b.id);
            for (int i = 0; i < a.fields.Length; i++)
            {
                b.fields[i].WriteDelta(writer, a.fields[i]);
            }
        }

        private void CreateSnapshot(ulong sequence)
        {
            var worldSnapshot = new WorldSnapshot()
            {
                time = FloatPacking.Quantize(Time.time),
                sequence = sequence,
                objects = new ObjectSnapshot[serverObjects.Count],
            };
            for (int i = 0; i < serverObjects.Count; i++)
            {
                DemoNetworkIdentity obj = serverObjects[i];
                worldSnapshot.objects[i] = new ObjectSnapshot
                {
                    id = obj.Id,
                    spawnId = obj.SpawnId,
                    fields = new FieldSnapshot[1 +
                        (obj.networkTransform != null ? 2 : 0) +
                        (obj.health != null ? 1 : 0) +
                        (obj.player != null ? 2 : 0)
                    ],
                };
                int fieldIndex = 0;

                worldSnapshot.objects[i].fields[fieldIndex] = FieldSnapshot.Create(obj.name);
                fieldIndex++;

                if (obj.networkTransform != null)
                {
                    worldSnapshot.objects[i].fields[fieldIndex] = FieldSnapshot.Create(FloatPacking.Quantize(obj.networkTransform.Position));
                    fieldIndex++;
                    worldSnapshot.objects[i].fields[fieldIndex] = FieldSnapshot.Create(obj.networkTransform.Rotation);
                    fieldIndex++;
                }
                if (obj.health != null)
                {
                    worldSnapshot.objects[i].fields[fieldIndex] = FieldSnapshot.Create(obj.health.Health);
                    fieldIndex++;
                }
                if (obj.player != null)
                {
                    worldSnapshot.objects[i].fields[fieldIndex] = FieldSnapshot.Create(obj.player.Money);
                    fieldIndex++;
                    worldSnapshot.objects[i].fields[fieldIndex] = FieldSnapshot.Create(obj.player.Damage);
                    fieldIndex++;
                }
            }
            snapshots[sequence] = worldSnapshot;
        }


        struct WorldSnapshot
        {
            // todo
            // active scene..

            public float time;
            public ulong sequence;
            public ObjectSnapshot[] objects;

            public bool Isvalid()
            {
                return objects != null;
            }
        }
        struct ObjectSnapshot
        {
            public uint id;
            public uint spawnId;
            public FieldSnapshot[] fields;

            public bool Valid() => id > 0;
        }
        abstract class FieldSnapshot
        {
            public static FieldSnapshot Create<T>(T value)
            {
                return new FieldSnapshotGeneric<T> { value = value };
            }
            public static bool ReadDelta<T>(NetworkReader reader, FieldSnapshot previousSnapshot, out FieldSnapshot newSnapshot, out T value)
            {
                if (((FieldSnapshotGeneric<T>)previousSnapshot).ReadDelta(reader, out value))
                {
                    newSnapshot = Create(value);
                    return true;
                }
                else
                {
                    newSnapshot = previousSnapshot;
                    return false;
                }
            }
            public static void ReadWhole<T>(NetworkReader reader, out FieldSnapshot newSnapshot, out T value)
            {
                value = reader.Read<T>();
                newSnapshot = Create(value);
            }



            public abstract void WriteWhole(NetworkWriter writer);
            public abstract void WriteDelta(NetworkWriter writer, FieldSnapshot oldField);


            class FieldSnapshotGeneric<T> : FieldSnapshot
            {
                public T value;

                public override void WriteWhole(NetworkWriter writer)
                {
                    writer.Write<T>(value);
                }
                public override void WriteDelta(NetworkWriter writer, FieldSnapshot oldField)
                {
                    T oldValue = ((FieldSnapshotGeneric<T>)oldField).value;
                    DeltaValue<T>.WriteDelta writeAction = DeltaValue<T>.Write;
                    if (writeAction != null)
                    {
                        writeAction.Invoke(writer, oldValue, value);
                    }
                    // no delta found, just write full value
                    else
                    {
                        if (EqualityComparer<T>.Default.Equals(oldValue, value))
                        {
                            writer.WriteBoolean(0);
                        }
                        else
                        {
                            writer.WriteBoolean(1);
                            writer.Write(value);
                        }
                    }
                }

                public bool ReadDelta(NetworkReader reader, out T value)
                {
                    DeltaValue<T>.ReadDelta readAction = DeltaValue<T>.Read;
                    if (readAction != null)
                    {
                        value = readAction.Invoke(reader, this.value);
                        // return true if value has changed
                        return !EqualityComparer<T>.Default.Equals(this.value, value);
                    }
                    // no delta found, just write full value
                    else
                    {
                        if (reader.ReadBoolean())
                        {
                            value = reader.Read<T>();
                            return true;
                        }
                        else
                        {
                            value = default;
                            return false;
                        }
                    }
                }
            }
        }
    }
    public static class DeltaValue<T>
    {
        public delegate void WriteDelta(NetworkWriter writer, T oldValue, T newValue);
        public static WriteDelta Write;

        public delegate T ReadDelta(NetworkReader reader, T oldValue);
        public static ReadDelta Read;
    }
    public static class DeltaWriters
    {
        public static void WriteDelta<T>(this NetworkWriter writer, T oldValue, T newValue)
        {
            if (DeltaValue<T>.Write == null)
                throw new KeyNotFoundException($"No delta found for {typeof(T)}. See https://miragenet.github.io/Mirage/Articles/General/Troubleshooting.html for details");

            DeltaValue<T>.Write(writer, oldValue, newValue);
        }
        public static T ReadDelta<T>(this NetworkReader reader, T oldValue)
        {
            if (DeltaValue<T>.Read == null)
                throw new KeyNotFoundException($"No delta reader found for {typeof(T)}. See https://miragenet.github.io/Mirage/Articles/General/Troubleshooting.html for details");

            return DeltaValue<T>.Read(reader, oldValue);
        }
        public static void Init()
        {
            DeltaValue<int>.Write = WriteDeltaInt;
            DeltaValue<int>.Read = ReadDeltaInt;

            DeltaValue<Vector3>.Write = WriteDeltaVector3;
            DeltaValue<Vector3>.Read = ReadDeltaVector3;

            DeltaValue<float>.Write = WriteDeltaFloat;
            DeltaValue<float>.Read = ReadDeltaFloat;
        }

        public static void WriteDeltaInt(NetworkWriter writer, int oldValue, int newValue)
        {
            if (oldValue == newValue)
            {
                writer.WriteBoolean(0);
            }
            else
            {
                int delta = newValue - oldValue;
                writer.WriteBoolean(1);
                writer.WritePackedInt32(delta);
            }
        }
        public static int ReadDeltaInt(NetworkReader reader, int oldValue)
        {
            if (reader.ReadBoolean())
            {
                int delta = reader.ReadPackedInt32();
                return delta + oldValue;
            }
            else
            {
                return oldValue;
            }
        }
        public static void WriteDeltaVector3(NetworkWriter writer, Vector3 oldValue, Vector3 newValue)
        {
            int p1 = writer.BitPosition;
            Vector3 delta = newValue - oldValue;
            if (delta == Vector3.zero)
            {
                writer.WriteBoolean(0);
            }
            else
            {
                writer.WriteBoolean(1);
                WriteDeltaFloat(writer, oldValue.x, newValue.x);
                WriteDeltaFloat(writer, oldValue.y, newValue.y);
                WriteDeltaFloat(writer, oldValue.z, newValue.z);
            }
            //Debug.Log($"WriteDeltaVector3 bits:{writer.BitPosition - p1}");
        }
        public static Vector3 ReadDeltaVector3(NetworkReader reader, Vector3 oldValue)
        {
            int p1 = reader.BitPosition;

            Vector3 result;
            if (reader.ReadBoolean())
            {
                result.x = oldValue.x + FloatPacking.UnPackFloat(reader);
                result.y = oldValue.y + FloatPacking.UnPackFloat(reader);
                result.z = oldValue.z + FloatPacking.UnPackFloat(reader);
            }
            else
            {
                result = oldValue;
            }
            //Debug.Log($"ReadDeltaVector3 bits:{reader.BitPosition - p1}");

            return result;
        }
        public static void WriteDeltaFloat(NetworkWriter writer, float oldValue, float newValue)
        {
            float delta = newValue - oldValue;
            FloatPacking.PackFloat(writer, delta);
        }
        public static float ReadDeltaFloat(NetworkReader reader, float oldValue)
        {
            return oldValue + FloatPacking.UnPackFloat(reader);
        }


    }
    public static class IntPacking
    {

    }
    public static class FloatPacking
    {
        const float resolution = 64f;

        public static Vector3 Quantize(Vector3 value)
        {
            value.x = Quantize(value.x);
            value.y = Quantize(value.y);
            value.z = Quantize(value.z);
            return value;
        }
        public static float Quantize(float value)
        {
            // todo add or subtract 0.5 to reduce rounding error
            //   use -0.5 if negative
            //   because sign is written as its own bit
            return ((int)(value * resolution)) / resolution;
        }

        public static void PackFloat(NetworkWriter writer, float value)
        {
            if (value == 0)
            {
                // 1 bit
                writer.WriteBoolean(0);
            }
            else
            {
                writer.WriteBoolean(1);
                // resolution 0.015625
                int scaledInt = (int)(value * resolution);
                uint scaled;
                if (scaledInt < 0)
                {
                    writer.WriteBoolean(1);
                    scaled = (uint)(-scaledInt);
                }
                else
                {
                    writer.WriteBoolean(0);
                    scaled = (uint)(scaledInt);

                }

                if (scaled < (1 << 4))
                {
                    // 6 bits
                    writer.WriteBoolean(0);
                    writer.Write(scaled, 4);
                }
                else
                {
                    writer.WriteBoolean(1);
                    if (scaled < (1 << 8))
                    {
                        // 11 bits
                        writer.WriteBoolean(0);
                        writer.Write(scaled, 8);
                    }
                    else
                    {
                        writer.WriteBoolean(1);

                        if (scaled < (1 << 12))
                        {
                            // 16 bits
                            writer.WriteBoolean(0);
                            writer.Write(scaled, 12);
                        }
                        else
                        {
                            // out of bounds
                            // 36 bits
                            writer.WriteBoolean(1);
                            writer.WriteSingle(value);
                        }
                    }
                }
            }
        }
        public static float UnPackFloat(NetworkReader reader)
        {
            if (reader.ReadBoolean())
            {
                ulong result;
                bool negative = reader.ReadBoolean();
                if (reader.ReadBoolean())
                {
                    if (reader.ReadBoolean())
                    {
                        if (reader.ReadBoolean())
                        {
                            return reader.ReadSingle();
                        }
                        else
                        {
                            result = reader.Read(12);
                        }
                    }
                    else
                    {
                        result = reader.Read(8);
                    }
                }
                else
                {
                    result = reader.Read(4);
                }

                if (negative)
                {
                    return result / (-resolution);
                }
                else
                {
                    return result / resolution;
                }
            }
            else
            {
                return 0;
            }
        }
    }

    public static class NetworkStreamSerializeExtensions
    {
        public static void WriteNetworkStream(this NetworkWriter writer, NetworkStream stream)
        {
            NetworkWriter streamWriter = stream.Writer;
            writer.WritePackedUInt32((uint)streamWriter.BitPosition);
            throw new NotImplementedException();
        }
    }
    public struct NetworkStream
    {
        PooledNetworkWriter writer;
        PooledNetworkReader reader;
        bool isWriting;
        bool isReading;

        public NetworkWriter Writer
        {
            get
            {
                if (!isWriting) throw new InvalidOperationException();
                return writer;
            }
        }
        public NetworkReader Reader
        {
            get
            {
                if (!isReading) throw new InvalidOperationException();
                return reader;
            }
        }

        public static NetworkStream CreateWriteSteam()
        {
            return new NetworkStream
            {
                isWriting = true,
                writer = NetworkWriterPool.GetWriter(),
            };
        }
        public static NetworkStream CreateReadStream(ArraySegment<byte> segment)
        {
            return new NetworkStream
            {
                isReading = true,
                reader = NetworkReaderPool.GetReader(segment)
            };
        }

        public void Dispose()
        {
            if (writer != null) writer.Release();
            if (reader != null) reader.Release();
        }
    }

    [NetworkMessage]
    internal struct StateMessage
    {
        public ArraySegment<byte> segment;
    }
}
