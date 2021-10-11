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
using System.Runtime.CompilerServices;
using Mirage;
using Mirage.Logging;
using Mirage.Serialization;
using UnityEngine;

namespace JamesFrowen.PositionSync
{
    /// <summary>
    /// Behaviour to sync position and rotation, This behaviour is used by <see cref="SyncPositionSystem"/> in order to sync
    /// </summary>
    [AddComponentMenu("Network/SyncPosition/SyncPositionBehaviour")]
    public class SyncPositionBehaviour : NetworkBehaviour
    {
        static readonly ILogger logger = LogFactory.GetLogger<SyncPositionBehaviour>();

        /// <summary>
        /// Checks if object needs syncing to clients
        /// <para>Called on server</para>
        /// </summary>
        /// <returns></returns>
        internal bool NeedsUpdate()
        {
            if (IsControlledByServer)
            {
                return IsTimeToUpdate() && (HasMoved() || HasRotated());
            }
            else
            {
                // dont care about time here, if client authority has sent snapshot then always relay it to other clients
                // todo do we need a check for attackers sending too many snapshots?
                return _needsUpdate;
            }
        }


        internal void ApplyOnServer(TransformState state, float time)
        {
            // this should not happen, Exception to disconnect attacker
            if (!clientAuthority) { throw new InvalidOperationException("Client is not allowed to send updated when clientAuthority is false"); }

            // see comment in NeedsUpdate 
            _needsUpdate = true;
            _latestState = state;

            // if host apply using interpolation otherwise apply exact 
            if (IsClient)
            {
                AddSnapShotToBuffer(state, time);
            }
            else
            {
                ApplyStateNoInterpolation(state);
            }
        }

        internal void ApplyOnClient(TransformState state, float time)
        {
            // not host
            // host will have already handled movement in servers code
            if (IsServer)
                return;

            AddSnapShotToBuffer(state, time);
        }


        [Header("References")]
        [SerializeField] SyncPositionPacker packer;

        [Tooltip("Which transform to sync")]
        [SerializeField] Transform target;

        [Header("Authority")]
        [Tooltip("Set to true if moves come from owner client, set to false if moves always come from server")]
        [SerializeField] bool clientAuthority = false;

        [Tooltip("If true uses local position and rotation, if value uses world position and rotation")]
        [SerializeField] bool useLocalSpace = true;

        // todo make 0 Sensitivity always send (and avoid doing distance/angle check)
        [Tooltip("How far position has to move before it is synced")]
        [SerializeField] float positionSensitivity = 0.1f;

        [Tooltip("How far rotation has to move before it is synced")]
        [SerializeField] float rotationSensitivity = 0.1f;


        [Tooltip("Client Authority Sync Interval")]
        [SerializeField] float clientSyncInterval = 0.1f;

        [SerializeField] bool showDebugGui = false;

        /// <summary>
        /// Set when client with authority updates the server
        /// </summary>
        bool _needsUpdate;

        /// <summary>
        /// latest values from client
        /// </summary>
        TransformState? _latestState;

        float _nextSyncInterval;

        // values for HasMoved/Rotated
        Vector3 lastPosition;
        Quaternion lastRotation;

        // client
        readonly SnapshotBuffer<TransformState> snapshotBuffer = new SnapshotBuffer<TransformState>(TransformState.GetInterpolator());

#if DEBUG
        void OnGUI()
        {
            if (showDebugGui)
            {
                GUILayout.Label($"ServerTime: {_system.TimeSync.LatestServerTime:0.000}");
                GUILayout.Label($"InterpTime: {_system.InterpolationTime:0.000}");
                GUILayout.Label(snapshotBuffer.ToDebugString());
            }
        }
#endif

        void OnValidate()
        {
            if (target == null)
                target = transform;
        }

        /// <summary>
        /// server auth or no owner, or host
        /// </summary>
        bool IsControlledByServer
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => !clientAuthority || Owner == null || Owner == Server.LocalPlayer;
        }

        /// <summary>
        /// client auth and owner
        /// </summary>
        bool IsLocalClientInControl
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => clientAuthority && HasAuthority;
        }

        Vector3 Position
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                return useLocalSpace ? target.localPosition : target.position;
            }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set
            {
                if (useLocalSpace)
                {
                    target.localPosition = value;
                }
                else
                {
                    target.position = value;
                }
            }
        }

        Quaternion Rotation
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                return useLocalSpace ? target.localRotation : target.rotation;
            }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set
            {
                if (useLocalSpace)
                {
                    target.localRotation = value;
                }
                else
                {
                    target.rotation = value;
                }
            }
        }

        public TransformState TransformState
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            // in client auth, we want to use the state given by the client.
            // that will be _latestState,
            get => _latestState ?? new TransformState(Position, Rotation);
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        bool IsTimeToUpdate()
        {
            return packer.Time > _nextSyncInterval;
        }

        /// <summary>
        /// Resets values, called after syncing to client
        /// <para>Called on server</para>
        /// </summary>
        internal void ClearNeedsUpdate(float interval)
        {
            _needsUpdate = false;
            _latestState = null;
            _nextSyncInterval = packer.Time + interval;
            lastPosition = Position;
            lastRotation = Rotation;
        }

        /// <summary>
        /// Has target moved since we last checked
        /// </summary>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        bool HasMoved()
        {
            bool moved = Vector3.Distance(lastPosition, Position) > positionSensitivity;

            if (moved)
            {
                lastPosition = Position;
            }
            return moved;
        }

        /// <summary>
        /// Has target moved since we last checked
        /// </summary>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        bool HasRotated()
        {
            bool rotated = Quaternion.Angle(lastRotation, Rotation) > rotationSensitivity;

            if (rotated)
            {
                lastRotation = Rotation;
            }
            return rotated;
        }

        private void Awake()
        {
            Identity.OnStartClient.AddListener(OnStartClient);
            Identity.OnStopClient.AddListener(OnStopClient);

            Identity.OnStartServer.AddListener(OnStartServer);
            Identity.OnStopServer.AddListener(OnStopServer);
        }
        SyncPositionSystem _system;
        void FindSystem()
        {
            if (IsServer)
                _system = ServerObjectManager.GetComponent<SyncPositionSystem>();
            else if (IsClient)
                _system = ClientObjectManager.GetComponent<SyncPositionSystem>();
            else throw new InvalidOperationException("System can't be found when object is not spawned");
        }
        public void OnStartClient()
        {
            // dont add twice in host mode
            if (IsServer) return;
            FindSystem();
            _system.Behaviours.AddBehaviour(this);
        }
        public void OnStartServer()
        {
            FindSystem();
            _system.Behaviours.AddBehaviour(this);
        }
        public void OnStopClient()
        {
            // dont add twice in host mode
            if (IsServer) return;
            _system.Behaviours.RemoveBehaviour(this);
            _system = null;
        }
        public void OnStopServer()
        {
            _system.Behaviours.RemoveBehaviour(this);
            _system = null;
        }

        void Update()
        {
            if (IsClient)
            {
                if (IsLocalClientInControl)
                {
                    ClientAuthorityUpdate();
                }
                else
                {
                    ClientInterpolation();
                }
            }
        }

        #region Server Sync Update

        /// <summary>
        /// Applies values to target transform on client
        /// <para>Adds to buffer for interpolation</para>
        /// </summary>
        /// <param name="state"></param>
        void AddSnapShotToBuffer(TransformState state, float serverTime)
        {
            // dont apply on local owner
            if (IsLocalClientInControl)
                return;

            // todo do we need this, or do we set it elsewhere?
            //this.interpolationTime.OnMessage(serverTime);

            // buffer will be empty if first snapshot or hasn't moved for a while.
            // in this case we can add a snapshot for (serverTime-syncinterval) for interoplation
            // this assumes snapshots are sent in order!
            if (snapshotBuffer.IsEmpty)
            {
                // use new state here instead of TranformState incase update is from client auth when runing in host mode
                snapshotBuffer.AddSnapShot(new TransformState(Position, Rotation), serverTime - clientSyncInterval);
            }
            snapshotBuffer.AddSnapShot(state, serverTime);
        }
        #endregion


        #region Client Sync Update 
        void ClientAuthorityUpdate()
        {
            // host client doesn't need to update server
            if (IsServer) { return; }

            if (IsTimeToUpdate() && (HasMoved() || HasRotated()))
            {
                SendMessageToServer();
                ClearNeedsUpdate(clientSyncInterval);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void SendMessageToServer()
        {
            using (PooledNetworkWriter writer = NetworkWriterPool.GetWriter())
            {
                _system.PackTime(writer, (float)NetworkTime.Time);
                _system.PackNext(writer, this);

                Client.Send(new NetworkPositionSingleMessage
                {
                    payload = writer.ToArraySegment()
                });
            }
        }

        /// <summary>
        /// Applies values to target transform on server
        /// <para>no need to interpolate on server</para>
        /// </summary>
        /// <param name="state"></param>
        void ApplyStateNoInterpolation(TransformState state)
        {
            Position = state.position;
            Rotation = state.rotation;
        }
        #endregion


        #region Client Interpolation
        void ClientInterpolation()
        {
            if (snapshotBuffer.IsEmpty) { return; }

            float snapshotTime = _system.InterpolationTime;
            TransformState state = snapshotBuffer.GetLinearInterpolation(snapshotTime);
            // todo add trace log
            if (logger.LogEnabled()) logger.Log($"p1:{Position.x} p2:{state.position.x} delta:{Position.x - state.position.x}");

            Position = state.position;

            if (packer.SyncRotation)
                Rotation = state.rotation;

            // remove snapshots older than 2times sync interval, they will never be used by Interpolation
            float removeTime = snapshotTime - (packer.ClientDelay * 1.5f);
            snapshotBuffer.RemoveOldSnapshots(removeTime);
        }
        #endregion
    }
}
