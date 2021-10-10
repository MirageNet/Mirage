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
using JamesFrowen.Logging;
using Mirror;
using UnityEngine;
using BitReader = JamesFrowen.BitPacking.NetworkReader;
using BitWriter = JamesFrowen.BitPacking.NetworkWriter;

namespace JamesFrowen.PositionSync
{
    [AddComponentMenu("Network/SyncPosition/SyncPositionSystem")]
    public class SyncPositionSystem : MonoBehaviour
    {
        // todo make this work with network Visibility
        // todo add maxMessageSize (splits up update message into multiple messages if too big)
        // todo test sync interval vs fixed hz 

        [Header("Reference")]
        public SyncPositionPacker packer;

        [NonSerialized] float nextSyncInterval;
        HashSet<SyncPositionBehaviour> toUpdate = new HashSet<SyncPositionBehaviour>();

        private void OnDrawGizmos()
        {
            if (packer != null)
                packer.DrawGizmo();
        }

        public void RegisterClientHandlers()
        {
            // todo find a way to register these handles so it doesn't need to be done from NetworkManager
            if (NetworkClient.active)
            {
                NetworkClient.RegisterHandler<NetworkPositionMessage>(ClientHandleNetworkPositionMessage);
            }
        }
        public void RegisterServerHandlers()
        {
            // todo find a way to register these handles so it doesn't need to be done from NetworkManager
            if (NetworkServer.active)
            {
                NetworkServer.RegisterHandler<NetworkPositionSingleMessage>(ServerHandleNetworkPositionMessage);
            }
        }

        public void UnregisterClientHandlers()
        {
            // todo find a way to unregister these handles so it doesn't need to be done from NetworkManager
            if (NetworkClient.active)
            {
                NetworkClient.UnregisterHandler<NetworkPositionMessage>();
            }
        }
        public void UnregisterServerHandlers()
        {
            // todo find a way to unregister these handles so it doesn't need to be done from NetworkManager
            if (NetworkServer.active)
            {
                NetworkServer.UnregisterHandler<NetworkPositionSingleMessage>();
            }
        }

        private void Awake()
        {
            packer.SetSystem(this);
        }
        private void OnDestroy()
        {
            packer.ClearSystem(this);
        }

        #region Sync Server -> Client
        [ServerCallback]
        private void LateUpdate()
        {
            if (packer.checkEveryFrame || ShouldSync())
            {
                var time = packer.Time;
                SendUpdateToAll(time);

                // host mode
                if (NetworkClient.active)
                    packer.InterpolationTime.OnMessage(time);
            }
        }
        [ClientCallback]
        private void Update()
        {
            packer.InterpolationTime.OnTick(packer.DeltaTime);
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
            if (packer.Behaviours.Count == 0) { return; }

            // todo dont create new buffer each time
            var bitWriter = new BitWriter(packer.Behaviours.Count * 32);
            PackBehaviours(bitWriter, time);

            // always send even if no behaviours so that time is sent
            var segment = bitWriter.ToArraySegment();
            NetworkServer.SendToAll(new NetworkPositionMessage
            {
                payload = segment
            });
        }

        internal void PackBehaviours(BitWriter bitWriter, float time)
        {
            packer.PackTime(bitWriter, time);

            toUpdate.Clear();
            foreach (SyncPositionBehaviour behaviour in packer.Behaviours.Values)
            {
                if (!behaviour.NeedsUpdate())
                    continue;

                toUpdate.Add(behaviour);
            }

            packer.PackCount(bitWriter, toUpdate.Count);
            foreach (SyncPositionBehaviour behaviour in toUpdate)
            {
                SimpleLogger.Debug($"Time {time:0.000}, Packing {behaviour.name}");
                packer.PackNext(bitWriter, behaviour);

                // todo handle client authority updates better
                behaviour.ClearNeedsUpdate(packer.syncInterval);
            }
        }

        internal void ClientHandleNetworkPositionMessage(NetworkPositionMessage msg)
        {
            // hostMode
            if (NetworkServer.active)
                return;

            int length = msg.payload.Count;
            // todo stop alloc
            using (var bitReader = new BitReader())
            {
                bitReader.Reset(msg.payload);
                float time = packer.UnpackTime(bitReader);
                ulong count = packer.UnpackCount(bitReader);

                for (uint i = 0; i < count; i++)
                {
                    packer.UnpackNext(bitReader, out uint id, out Vector3 pos, out Quaternion rot);

                    if (packer.Behaviours.TryGetValue(id, out SyncPositionBehaviour behaviour))
                    {
                        behaviour.ApplyOnClient(new TransformState(pos, rot), time);
                    }

                }

                packer.InterpolationTime.OnMessage(time);
            }
        }

        #endregion


        #region Sync Client Auth -> Server


        /// <summary>
        /// Position from client to server
        /// </summary>
        /// <param name="arg1"></param>
        /// <param name="arg2"></param>
        internal void ServerHandleNetworkPositionMessage(NetworkConnection _conn, NetworkPositionSingleMessage msg)
        {
            // todo stop alloc
            using (var bitReader = new BitReader())
            {
                bitReader.Reset(msg.payload);

                float time = packer.UnpackTime(bitReader);
                packer.UnpackNext(bitReader, out uint id, out Vector3 pos, out Quaternion rot);

                if (packer.Behaviours.TryGetValue(id, out SyncPositionBehaviour behaviour))
                {
                    behaviour.ApplyOnServer(new TransformState(pos, rot), time);
                }
                else
                {
                    SimpleLogger.DebugWarn($"Could not find behaviour with id {id}");
                }
            }
        }
        #endregion
    }

    public struct NetworkPositionMessage : NetworkMessage
    {
        public ArraySegment<byte> payload;
    }
    public struct NetworkPositionSingleMessage : NetworkMessage
    {
        public ArraySegment<byte> payload;
    }
}
