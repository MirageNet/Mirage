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
using Mirage;
using Mirage.Logging;
using Mirage.Serialization;
using UnityEngine;

namespace JamesFrowen.PositionSync
{
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

    [AddComponentMenu("Network/SyncPosition/SyncPositionSystem")]
    public class SyncPositionSystem : MonoBehaviour
    {
        static readonly ILogger logger = LogFactory.GetLogger<SyncPositionSystem>();

        // todo make this work with network Visibility
        // todo add maxMessageSize (splits up update message into multiple messages if too big)
        // todo test sync interval vs fixed hz 

        public NetworkClient Client;
        public NetworkServer Server;

        [Header("Reference")]
        public SyncPositionPacker packer;

        public SyncPositionBehaviourCollection Behaviours { get; }

        [NonSerialized] float nextSyncInterval;
        HashSet<SyncPositionBehaviour> toUpdate = new HashSet<SyncPositionBehaviour>();

        //private void OnDrawGizmos()
        //{
        //    if (packer != null)
        //        packer.DrawGizmo();
        //}

        private void Awake()
        {
            Server.Started.AddListener(AddServerHandlers);
            Client.Started.AddListener(AddClientHandlers);
        }
        private void OnDestroy()
        {
            Server.Started.RemoveListener(AddServerHandlers);
            Client.Started.RemoveListener(AddClientHandlers);
        }

        private void AddClientHandlers()
        {
            Client.MessageHandler.RegisterHandler<NetworkPositionMessage>(ClientHandleNetworkPositionMessage);
        }

        private void AddServerHandlers()
        {
            Server.MessageHandler.RegisterHandler<NetworkPositionSingleMessage>(ServerHandleNetworkPositionMessage);
        }


        #region Sync Server -> Client

        private void LateUpdate()
        {
            if (Server.Active)
                ServerUpdate();
        }

        private void Update()
        {
            if (Client.Active)
                ClientUpdate();
        }

        private void ServerUpdate()
        {
            if (packer.checkEveryFrame || ShouldSync())
            {
                float time = packer.Time;
                SendUpdateToAll(time);

                // host mode
                if (Client.Active)
                    packer.TimeSync.OnMessage(time);
            }
        }

        private void ClientUpdate()
        {
            packer.TimeSync.OnUpdate(packer.DeltaTime);
        }

        bool ShouldSync()
        {
            float now = Time.time;
            if (now > nextSyncInterval)
            {
                nextSyncInterval += packer.syncInterval;
                return true;
            }
            else
            {
                return false;
            }
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

            toUpdate.Clear();
            foreach (SyncPositionBehaviour behaviour in Behaviours.Dictionary.Values)
            {
                if (!behaviour.NeedsUpdate())
                    continue;

                toUpdate.Add(behaviour);
            }

            packer.PackCount(writer, toUpdate.Count);
            foreach (SyncPositionBehaviour behaviour in toUpdate)
            {
                if (logger.LogEnabled()) logger.Log($"Time {time:0.000}, Packing {behaviour.name}");
                packer.PackNext(writer, behaviour);

                // todo handle client authority updates better
                behaviour.ClearNeedsUpdate(packer.syncInterval);
            }
        }

        internal void ClientHandleNetworkPositionMessage(NetworkPositionMessage msg)
        {
            // hostMode
            if (Server.Active)
                return;

            using (PooledNetworkReader reader = NetworkReaderPool.GetReader(msg.payload))
            {
                float time = packer.UnpackTime(reader);
                int count = packer.UnpackCount(reader);

                for (uint i = 0; i < count; i++)
                {
                    packer.UnpackNext(reader, out uint id, out Vector3 pos, out Quaternion rot);

                    if (Behaviours.Dictionary.TryGetValue(id, out SyncPositionBehaviour behaviour))
                    {
                        behaviour.ApplyOnClient(new TransformState(pos, rot), time);
                    }

                }

                packer.TimeSync.OnMessage(time);
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
