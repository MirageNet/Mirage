using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Mirage.Serialization;
using Mirage.SocketLayer;
using UnityEngine;

namespace Mirage.Experimental
{
    public class StateTransfer
    {
        NetworkServer server;
        NetworkClient client;
        List<DemoNetworkIdentity> serverObjects;

        WorldSnapshot[] snapshots = new WorldSnapshot[255];
        Sequencer sequencer = new Sequencer(8);
        Dictionary<NetworkPlayer, ulong> ackedSnapshot = new Dictionary<NetworkPlayer, ulong>();

        private StateTransfer()
        {
            DeltaWriters.Init();
        }
        public static StateTransfer Create(NetworkServer server, List<DemoNetworkIdentity> all)
        {
            return new StateTransfer()
            {
                server = server,
                serverObjects = all
            };
        }
        public static StateTransfer Create(NetworkClient client)
        {
            var stateTranfer = new StateTransfer()
            {
                client = client
            };
            client.MessageHandler.RegisterHandler<StateMessage>(stateTranfer.ReceiveState);
            return stateTranfer;
        }

        private void ReceiveState(INetworkPlayer player, StateMessage message)
        {
            throw new NotImplementedException();
        }

        public void Update()
        {
            if (server == null) { throw new InvalidOperationException(); }

            var sequence = this.sequencer.Next();
            CreateSnapshot(sequence);
            foreach (var kvp in ackedSnapshot)
            {
                SendUpdate(kvp, sequence);
            }
        }


        private void SendUpdate(KeyValuePair<NetworkPlayer, ulong> kvp, ulong sequence)
        {
            var a = snapshots[kvp.Value];
            var b = snapshots[sequence];
            using (var writer = NetworkWriterPool.GetWriter())
            {
                CreateDelta(writer, a, b);

                var player = kvp.Key;
                var token = SendNotify(player, writer.ToArraySegment());
                token.Delivered += () =>
                {
                    ackedSnapshot[player] = sequence;
                };
            }
        }

        private INotifyToken SendNotify(NetworkPlayer player, ArraySegment<byte> arraySegments)
        {
            using (PooledNetworkWriter writer = NetworkWriterPool.GetWriter())
            {
                var msg = new StateMessage
                {
                    segment = arraySegments,
                };
                MessagePacker.Pack(msg, writer);

                var segment = writer.ToArraySegment();
                return player.Connection.SendNotify(segment);
            }
        }

        private void CreateDelta(PooledNetworkWriter writer, WorldSnapshot a, WorldSnapshot b)
        {
            var deltaTime = b.time - a.time;
            writer.WriteSingle(deltaTime);

            var deltaSeq = b.sequence - a.sequence;
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
                var aobj = a.objects[ai];
                var bobj = b.objects[bi];

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
            writer.WriteBoolean(0);
            foreach (var field in obj.fields)
            {
                field.WriteWhole(writer);
            }
        }

        private void WriteDestroyed(PooledNetworkWriter writer, ObjectSnapshot obj)
        {
            writer.WritePackedUInt32(obj.id);
            writer.WriteBoolean(1);
        }

        private void WriteObjectDelta(PooledNetworkWriter writer, ObjectSnapshot a, ObjectSnapshot b)
        {
            writer.WritePackedUInt32(a.id);
            writer.WriteBoolean(0);
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
                var obj = serverObjects[i];
                worldSnapshot.objects[i] = new ObjectSnapshot
                {
                    id = obj.Id,
                    fields = new FieldSnapshot[
                        (obj.networkTransform != null ? 2 : 0) +
                        (obj.health != null ? 1 : 0) +
                        (obj.player != null ? 2 : 0)
                    ],
                };
                var fieldIndex = 0;
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
            public FieldSnapshot[] fields;
        }
        abstract class FieldSnapshot
        {


            public static FieldSnapshot Create<T>(T value)
            {
                return new FieldSnapshotGeneric<T> { value = value };
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
                    var oldValue = ((FieldSnapshotGeneric<T>)oldField).value;
                    var writeAction = DeltaWriter<T>.Write;
                    if (writeAction != null)
                    {
                        writeAction.Invoke(writer, oldValue, this.value);
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
            }
        }
    }
    public static class DeltaWriter<T>
    {
        public delegate void WriteDelta(NetworkWriter writer, T oldValue, T newValue);
        public static WriteDelta Write;
    }
    public static class DeltaWriters
    {
        public static void Init()
        {
            DeltaWriter<int>.Write = DeltaInt;
            DeltaWriter<Vector3>.Write = DeltaVector3;
        }
        public static void DeltaInt(NetworkWriter writer, int oldValue, int newValue)
        {
            var delta = oldValue - newValue;
            writer.WritePackedInt32(delta);
        }
        public static unsafe void DeltaVector3(NetworkWriter writer, Vector3 oldValue, Vector3 newValue)
        {
            var delta = oldValue - newValue;
            if (delta == Vector3.zero)
            {
                writer.WriteBoolean(0);
            }
            else
            {
                writer.WriteBoolean(1);
                //todo is this correct??
                var ptr = (int*)&delta;
                writer.WritePackedInt32(ptr[0]);
                writer.WritePackedInt32(ptr[1]);
                writer.WritePackedInt32(ptr[2]);
            }
        }
    }
    public static class NetworkStreamSerializeExtensions
    {
        public static void WriteNetworkStream(this NetworkWriter writer, NetworkStream stream)
        {
            var streamWriter = stream.Writer;
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
                return this.writer;
            }
        }
        public NetworkReader Reader
        {
            get
            {
                if (!isReading) throw new InvalidOperationException();
                return this.reader;
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

    /// <summary>
    /// Tells weaver how many bits to sue for field
    /// <para>Only works with interager fields (byte, int, ulong, etc)</para>
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public class SerializeAttribute : Attribute
    {
        public int BitCount { get; set; }
    }
    [NetworkMessage]
    internal struct StateMessageWithStructs
    {
        [Serialize(BitCount = 6)]
        byte updateSeqeuence;
        float deltaTime;
        ChangedObject[] changes;
    }

    internal struct ChangedObject
    {
        uint netid;
        ChangedField[] fields;
    }

    internal struct ChangedField
    {
        /// <summary>
        /// Field index inside Identity
        /// <para>Index is shared for all Behaviour on Identity, Behaviour[0] fields start at 0, Beahaviour[1] start at Behaviour[0].FieldCount</para>
        /// </summary>
        uint fieldIndex;

    }



    public class DemoNetworkIdentity : MonoBehaviour
    {
        private uint _id;
        public uint Id => _id;

        public DemoNetworkTransform networkTransform;
        public DemoHealth health;
        public DemoPlayer player;
    }
    public class DemoNetworkTransform : MonoBehaviour
    {
        private Vector3 _position;
        public Vector3 Position { get => _position; set => _position = value; }

        private Quaternion _rotation;
        public Quaternion Rotation { get => _rotation; set => _rotation = value; }
    }
    public class DemoHealth : MonoBehaviour
    {
        private float _health;
        public float Health { get => _health; set => _health = value; }
    }
    public class DemoPlayer : MonoBehaviour
    {
        private int _money;
        public int Money { get => _money; set => _money = value; }

        private int _damage;
        public int Damage { get => _damage; set => _damage = value; }
    }
}
