using System;
using System.Collections.Generic;
using System.Linq;
using Mirage.Serialization;
using Mirage.SocketLayer;
using UnityEngine;

namespace Mirage.Experimental
{
    public class StateTransfer
    {
        NetworkServer server;
        NetworkClient client;

        WorldSnapshot[] snapshots = new WorldSnapshot[255];
        Sequencer sequencer = new Sequencer(8);

        //server
        List<DemoNetworkIdentity> serverObjects;
        Dictionary<NetworkPlayer, ulong> ackedSnapshot = new Dictionary<NetworkPlayer, ulong>();

        //client
        ulong lastReceived;
        Dictionary<uint, DemoNetworkIdentity> clientObjects;
        private Dictionary<uint, Func<GameObject>> clientSpawnDictionary;

        private StateTransfer()
        {
            DeltaWriters.Init();
        }
        public static StateTransfer Create(NetworkServer server, List<DemoNetworkIdentity> serverObjects)
        {
            return new StateTransfer()
            {
                server = server,
                serverObjects = serverObjects
            };
        }
        public static StateTransfer Create(NetworkClient client, Dictionary<uint, DemoNetworkIdentity> clientObjects, Dictionary<uint, Func<GameObject>> clientSpawnDictionary)
        {
            var stateTranfer = new StateTransfer()
            {
                client = client,
                clientObjects = clientObjects,
                clientSpawnDictionary = clientSpawnDictionary,
            };
            client.MessageHandler.RegisterHandler<StateMessage>(stateTranfer.ReceiveState);
            return stateTranfer;
        }

        private void ReceiveState(INetworkPlayer player, StateMessage message)
        {
            WorldSnapshot previous = snapshots[lastReceived];
            using (PooledNetworkReader reader = NetworkReaderPool.GetReader(message.segment))
            {
                float deltaTime = reader.ReadSingle();
                float time = previous.time + deltaTime;

                ulong deltaSeq = reader.ReadByte();
                ulong seq = previous.sequence + deltaSeq;

                var newSnapshot = new Dictionary<uint, ObjectSnapshot>();
                // add all objects from previous snapshot
                for (int i = 0; i < previous.objects.Length; i++)
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
                bool destroyed = reader.ReadBoolean();
                if (destroyed)
                {
                    objects.Remove(nextId);
                    DestroyObject(nextId);
                }
                else
                {
                    // if Exist
                    if (objects.TryGetValue(nextId, out ObjectSnapshot previousObj))
                    {
                        objects[nextId] = ApplyDeltaFields(reader, nextId, previousObj);
                    }
                    // else spawend
                    else
                    {
                        objects.Add(nextId, SpawnObject(reader, nextId));
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

        private ObjectSnapshot SpawnObject(NetworkReader reader, uint id)
        {
            // because spawned next read will be spawn id
            uint spawnId = reader.ReadUInt32();
            DemoNetworkIdentity clone = clientSpawnDictionary[spawnId].Invoke().GetComponent<DemoNetworkIdentity>();
            clone.Init(id, spawnId);
            return ReadWholeObjects(reader, clone);
        }

        private ObjectSnapshot ReadWholeObjects(NetworkReader reader, DemoNetworkIdentity obj)
        {
            var fields = new FieldSnapshot[
                (obj.networkTransform != null ? 2 : 0) +
                (obj.health != null ? 1 : 0) +
                (obj.player != null ? 2 : 0)
            ];
            int fieldIndex = 0;
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
            foreach (KeyValuePair<NetworkPlayer, ulong> kvp in ackedSnapshot)
            {
                SendUpdate(kvp.Key, kvp.Value, sequence);
            }
        }


        private void SendUpdate(NetworkPlayer player, ulong previousSequence, ulong sequence)
        {
            WorldSnapshot a = snapshots[previousSequence];
            WorldSnapshot b = snapshots[sequence];
            using (PooledNetworkWriter writer = NetworkWriterPool.GetWriter())
            {
                CreateDelta(writer, a, b);

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

        private INotifyToken SendNotify<T>(NetworkPlayer player, T msg) where T : struct
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
            float deltaTime = b.time - a.time;
            writer.WriteSingle(deltaTime);

            ulong deltaSeq = b.sequence - a.sequence;
            writer.WriteByte((byte)deltaSeq);

            // assume arrays are not null here.....we can handle special case where a array is null later
            int ai = 0;
            int bi = 0;
            int loopCount = 0;
            // safety numbers to stop infinite loop
            while (loopCount < 100_000)
            {
                loopCount++;

                // arrays should always be in order of netid
                ObjectSnapshot aobj = a.objects[ai];
                ObjectSnapshot bobj = b.objects[bi];

                if (aobj.id == bobj.id)
                {
                    WriteObjectDelta(writer, aobj, bobj);
                    ai++;
                    bi++;
                    continue;
                }
                else
                {
                    if (aobj.id > bobj.id)
                    {
                        // b is new object
                        WriteWholeObject(writer, bobj);
                        bi++;
                        continue;
                    }
                    else
                    {
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
                time = Time.time,
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
                    fields = new FieldSnapshot[
                        (obj.networkTransform != null ? 2 : 0) +
                        (obj.health != null ? 1 : 0) +
                        (obj.player != null ? 2 : 0)
                    ],
                };
                int fieldIndex = 0;
                if (obj.networkTransform != null)
                {
                    worldSnapshot.objects[i].fields[fieldIndex] = FieldSnapshot.Create(obj.networkTransform.Position);
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
                throw new NotImplementedException();
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
                        return EqualityComparer<T>.Default.Equals(this.value, value);
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
        public static void Init()
        {
            DeltaValue<int>.Write = WriteDeltaInt;
            DeltaValue<int>.Read = ReadDeltaInt;
            DeltaValue<Vector3>.Write = WriteDeltaVector3;
            DeltaValue<Vector3>.Read = ReadDeltaVector3;
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
        public static unsafe void WriteDeltaVector3(NetworkWriter writer, Vector3 oldValue, Vector3 newValue)
        {
            Vector3 delta = oldValue - newValue;
            if (delta == Vector3.zero)
            {
                writer.WriteBoolean(0);
            }
            else
            {
                writer.WriteBoolean(1);
                //todo is this correct??
                int* ptr = (int*)&delta;
                writer.WritePackedInt32(ptr[0]);
                writer.WritePackedInt32(ptr[1]);
                writer.WritePackedInt32(ptr[2]);
            }
        }
        public static unsafe Vector3 ReadDeltaVector3(NetworkReader reader, Vector3 oldValue)
        {
            if (reader.ReadBoolean())
            {
                Vector3 newValue;
                //todo is this correct??
                int* ptr = (int*)&newValue;
                ptr[0] = reader.ReadPackedInt32();
                ptr[1] = reader.ReadPackedInt32();
                ptr[2] = reader.ReadPackedInt32();

                return newValue;
            }
            else
            {
                return oldValue;
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
