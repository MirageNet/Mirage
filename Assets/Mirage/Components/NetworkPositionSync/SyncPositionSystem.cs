/*
MIT License

Copyright (c) 2021 James Frowen

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
*/

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Mirage;
using Mirage.Logging;
using Mirage.Serialization;
using UnityEngine;

namespace JamesFrowen.PositionSync
{
    public static class Benchmark
    {
        static long[] frames;
        static int index;
        static bool isRecording;
        static long start;

        public static event Action<long[]> RecordingFinished;

        public static bool IsRecording => isRecording;

        public static void StartRecording(int frameCount)
        {
            frames = new long[frameCount];
            isRecording = true;
            index = 0;
        }

        public static void StartFrame()
        {
            if (!isRecording) return;

            start = Stopwatch.GetTimestamp();
        }
        public static void EndFrame()
        {
            if (!isRecording) return;

            long end = Stopwatch.GetTimestamp();
            frames[index] = end - start;
            index++;
            if (index >= frames.Length)
            {
                RecordingFinished?.Invoke(frames);
                RecordingFinished = null;
                isRecording = false;
            }
        }
    }

    public class SyncPositionBehaviourCollection
    {
        static readonly ILogger logger = LogFactory.GetLogger<SyncPositionBehaviourCollection>();

        private Dictionary<uint, SyncPositionBehaviour> _behaviours = new Dictionary<uint, SyncPositionBehaviour>();

        public IReadOnlyDictionary<uint, SyncPositionBehaviour> Dictionary => _behaviours;

        public void AddBehaviour(SyncPositionBehaviour thing)
        {
            uint netId = thing.NetId;
            _behaviours.Add(netId, thing);


            if (_behaviours.TryGetValue(netId, out SyncPositionBehaviour existingValue))
            {
                if (existingValue != thing)
                {
                    // todo what is this log?
                    logger.LogError("Parent can't be set without control");
                }
            }
            else
            {
                _behaviours.Add(netId, thing);
            }
        }

        public void RemoveBehaviour(SyncPositionBehaviour thing)
        {
            uint netId = thing.NetId;
            _behaviours.Remove(netId);
        }
        public void ClearBehaviours()
        {
            _behaviours.Clear();
        }
    }

    [Serializable]
    public enum SyncMode
    {
        SendToAll = 1,
        SendToObservers_PlayerDirty = 2,
        SendToObservers_DirtyObservers = 3,
        SendToDirtyObservers_PackOnce = 4,
    }

    [AddComponentMenu("Network/SyncPosition/SyncPositionSystem")]
    public class SyncPositionSystem : MonoBehaviour
    {
        static readonly ILogger logger = LogFactory.GetLogger<SyncPositionSystem>();

        // todo make this work with network Visibility
        // todo add maxMessageSize (splits up update message into multiple messages if too big)
        // todo test sync interval vs fixed hz 

        public NetworkClient Client;
        public NetworkServer Server;

        public SyncSettings PackSettings;
        [NonSerialized] public SyncPacker packer;

        [Tooltip("How many updates per second")]
        public float tickRate = 20;
        public float TickInterval => 1 / tickRate;

        [Header("Snapshot Interpolation")]
        [Tooltip("Number of ticks to delay interpolation to make sure there is always a snapshot to interpolate towards. High delay can handle more jitter, but adds latancy to the position.")]
        public float TickDelayCount = 2;

        [Tooltip("Skips Visibility and sends position to all ready connections")]
        public SyncMode syncMode = SyncMode.SendToAll;


        // cached object for update list
        HashSet<SyncPositionBehaviour> dirtySet = new HashSet<SyncPositionBehaviour>();
        HashSet<SyncPositionBehaviour> toUpdateObserverCache = new HashSet<SyncPositionBehaviour>();

        public SyncPositionBehaviourCollection Behaviours { get; } = new SyncPositionBehaviourCollection();

        [NonSerialized] TickRunner tickRunner;
        [NonSerialized] TimeSync _timeSync;
        public TimeSync TimeSync
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _timeSync;
        }
        //public float Time
        //{
        //    [MethodImpl(MethodImplOptions.AggressiveInlining)]
        //    get => UnityEngine.Time.unscaledTime;
        //}

        //public float DeltaTime
        //{
        //    [MethodImpl(MethodImplOptions.AggressiveInlining)]
        //    get => UnityEngine.Time.unscaledDeltaTime;
        //}

        public bool ClientActive => Client?.Active ?? false;
        public bool ServerActive => Server?.Active ?? false;


        //private void OnDrawGizmos()
        //{
        //    if (packer != null)
        //        packer.DrawGizmo();
        //}

        private void Awake()
        {
            Server?.Started.AddListener(ServerStarted);
            Client?.Started.AddListener(ClientStarted);


            _timeSync = new TimeSync(1 / tickRate, tickDelay: TickDelayCount, timeScale: 0.1f);
            tickRunner = new TickRunner(tickRate);
            packer = new SyncPacker(PackSettings);
        }
        private void OnValidate()
        {
            packer = new SyncPacker(PackSettings ?? new SyncSettings());
        }
        private void OnDestroy()
        {
            Server?.Started.RemoveListener(ServerStarted);
            Client?.Started.RemoveListener(ClientStarted);
        }

        private void ClientStarted()
        {
            Client.MessageHandler.RegisterHandler<NetworkPositionMessage>(ClientHandleNetworkPositionMessage);
        }

        private void ServerStarted()
        {
            Server.MessageHandler.RegisterHandler<NetworkPositionSingleMessage>(ServerHandleNetworkPositionMessage);
            tickRunner.OnTick += ServerUpdate;
        }


        #region Sync Server -> Client

        private void LateUpdate()
        {
            tickRunner.OnUpdate(UnityEngine.Time.unscaledDeltaTime);
        }
        public void Update()
        {
            ClientUpdate(UnityEngine.Time.unscaledDeltaTime);
        }

        private void ServerUpdate(TickRunner runner)
        {
            float time = runner.FixedTime;
            Benchmark.StartFrame();
            // syncs every frame, each Behaviour will track its own timer
            switch (syncMode)
            {
                case SyncMode.SendToAll:
                    SendUpdateToAll(time);
                    break;
                case SyncMode.SendToObservers_PlayerDirty:
                    SendUpdateToObservers_PlayerDirty(time);
                    break;
                case SyncMode.SendToObservers_DirtyObservers:
                    SendUpdateToObservers_DirtyObservers(time);
                    break;
                case SyncMode.SendToDirtyObservers_PackOnce:
                    SendUpdateToObservers_DirtyObservers_PackOnce(time);
                    break;
            }
            Benchmark.EndFrame();

            // host mode
            if (Client?.Active ?? false)
                TimeSync.OnMessage(time);
        }

        private void ClientUpdate(float deltaTime)
        {
            TimeSync.OnRender(deltaTime);
        }

        internal void SendUpdateToAll(float time)
        {
            // dont send message if no behaviours
            if (Behaviours.Dictionary.Count == 0) { return; }

            using (PooledNetworkWriter writer = NetworkWriterPool.GetWriter())
            {
                PackBehaviours(writer, time);

                Server.SendToAll(new NetworkPositionMessage
                {
                    payload = writer.ToArraySegment()
                });
            }
        }
        internal void PackBehaviours(NetworkWriter writer, float time)
        {
            packer.PackTime(writer, time);

            UpdateDirtySet();

            foreach (SyncPositionBehaviour behaviour in dirtySet)
            {
                if (logger.LogEnabled()) logger.Log($"Time {time:0.000}, Packing {behaviour.name}");
                packer.PackNext(writer, behaviour);

                // todo handle client authority updates better
                behaviour.ClearNeedsUpdate();
            }
        }

        /// <summary>
        /// Loops through all players, then through all dirty object and checks if palyer can see each
        /// </summary>
        /// <param name="time"></param>
        internal void SendUpdateToObservers_PlayerDirty(float time)
        {
            // dont send message if no behaviours
            if (Behaviours.Dictionary.Count == 0) { return; }


            UpdateDirtySet();

            using (PooledNetworkWriter writer = NetworkWriterPool.GetWriter())
            {
                writer.Reset();

                foreach (INetworkPlayer player in Server.Players)
                {
                    packer.PackTime(writer, time);
                    foreach (SyncPositionBehaviour behaviour in dirtySet)
                    {
                        if (!behaviour.Identity.observers.Contains(player))
                            continue;

                        packer.PackNext(writer, behaviour);
                    }

                    player.Send(new NetworkPositionMessage
                    {
                        payload = writer.ToArraySegment()
                    });
                }
            }

            ClearDirtySet();
        }

        Dictionary<INetworkPlayer, PooledNetworkWriter> writerPool = new Dictionary<INetworkPlayer, PooledNetworkWriter>();
        /// <summary>
        /// Loops through all dirty objects, and then their observers and then writes that behaviouir to a cahced writer
        /// </summary>
        /// <param name="time"></param>
        internal void SendUpdateToObservers_DirtyObservers(float time)
        {
            // dont send message if no behaviours
            if (Behaviours.Dictionary.Count == 0) { return; }

            UpdateDirtySet();

            foreach (SyncPositionBehaviour behaviour in dirtySet)
            {
                foreach (INetworkPlayer observer in behaviour.Identity.observers)
                {
                    if (!writerPool.TryGetValue(observer, out PooledNetworkWriter writer))
                    {
                        writer = NetworkWriterPool.GetWriter();
                        packer.PackTime(writer, time);
                    }

                    packer.PackNext(writer, behaviour);
                }
            }

            foreach (INetworkPlayer player in Server.Players)
            {
                if (!writerPool.TryGetValue(player, out PooledNetworkWriter writer))
                {
                    writer = NetworkWriterPool.GetWriter();
                    packer.PackTime(writer, time);
                }

                player.Send(new NetworkPositionMessage { payload = writer.ToArraySegment() });
                writer.Release();
            }

            ClearDirtySet();
        }

        /// <summary>
        /// Loops through all dirty objects, and then their observers and then writes that behaviouir to a cahced writer
        /// <para>But Packs once and copies bytes</para>
        /// </summary>
        /// <param name="time"></param>
        internal void SendUpdateToObservers_DirtyObservers_PackOnce(float time)
        {
            // dont send message if no behaviours
            if (Behaviours.Dictionary.Count == 0) { return; }

            UpdateDirtySet();
            using (PooledNetworkWriter packWriter = NetworkWriterPool.GetWriter())
            {
                foreach (SyncPositionBehaviour behaviour in dirtySet)
                {
                    if (behaviour.Identity.observers.Count == 0) { continue; }

                    packWriter.Reset();
                    packer.PackNext(packWriter, behaviour);


                    foreach (INetworkPlayer observer in behaviour.Identity.observers)
                    {
                        if (!writerPool.TryGetValue(observer, out PooledNetworkWriter writer))
                        {
                            writer = NetworkWriterPool.GetWriter();
                            packer.PackTime(writer, time);
                        }

                        writer.CopyFromWriter(packWriter);
                    }
                }
            }

            foreach (INetworkPlayer player in Server.Players)
            {
                if (!writerPool.TryGetValue(player, out PooledNetworkWriter writer))
                {
                    writer = NetworkWriterPool.GetWriter();
                    packer.PackTime(writer, time);
                }

                player.Send(new NetworkPositionMessage { payload = writer.ToArraySegment() });
                writer.Release();
            }

            ClearDirtySet();
        }

        private void UpdateDirtySet()
        {
            dirtySet.Clear();
            foreach (SyncPositionBehaviour behaviour in Behaviours.Dictionary.Values)
            {
                //if (!behaviour.NeedsUpdate())
                //    continue;

                dirtySet.Add(behaviour);
            }
        }

        private void ClearDirtySet()
        {
            foreach (SyncPositionBehaviour behaviour in dirtySet)
            {
                behaviour.ClearNeedsUpdate();
            }
            dirtySet.Clear();
        }



        internal void ClientHandleNetworkPositionMessage(NetworkPositionMessage msg)
        {
            // hostMode
            if (ServerActive)
                return;

            using (PooledNetworkReader reader = NetworkReaderPool.GetReader(msg.payload))
            {
                float time = packer.UnpackTime(reader);

                while (packer.TryUnpackNext(reader, out uint id, out Vector3 pos, out Quaternion rot))
                {
                    if (Behaviours.Dictionary.TryGetValue(id, out SyncPositionBehaviour behaviour))
                    {
                        behaviour.ApplyOnClient(new TransformState(pos, rot), time);
                    }
                }

                TimeSync.OnMessage(time);
            }
        }

        #endregion


        #region Sync Client Auth -> Server


        /// <summary>
        /// Position from client to server
        /// </summary>
        /// <param name="arg1"></param>
        /// <param name="arg2"></param>
        internal void ServerHandleNetworkPositionMessage(INetworkPlayer _, NetworkPositionSingleMessage msg)
        {
            using (PooledNetworkReader reader = NetworkReaderPool.GetReader(msg.payload))
            {
                float time = packer.UnpackTime(reader);
                packer.UnpackNext(reader, out uint id, out Vector3 pos, out Quaternion rot);

                if (Behaviours.Dictionary.TryGetValue(id, out SyncPositionBehaviour behaviour))
                {
                    behaviour.ApplyOnServer(new TransformState(pos, rot), time);
                }
                else
                {
                    if (logger.WarnEnabled()) logger.LogWarning($"Could not find behaviour with id {id}");
                }
            }
        }
        #endregion
    }

    [NetworkMessage]
    public struct NetworkPositionMessage
    {
        public ArraySegment<byte> payload;
    }
    [NetworkMessage]
    public struct NetworkPositionSingleMessage
    {
        public ArraySegment<byte> payload;
    }
}
