#region Statements

using System.Collections.Concurrent;
using System.Collections.Generic;
using Mirror.Components.Experimental;
using UnityEngine;

#endregion

namespace Mirror.Experimental
{
    public enum SendChannel : byte
    {
        Reliable = 0,
        UnReliable = 1
    }

    [DisallowMultipleComponent]
    [AddComponentMenu("Network/Experimental/NetworkTransformExperimental")]
    [HelpURL("https://mirror-networking.com/docs/Components/NetworkTransform.html")]
    public class NetworkTransformState : NetworkBehaviour
    {
        #region Fields

        private static List<ObjectSnapShot> _pooledObjectSnapshotData;

        [Header("Configuration Properties")] [SerializeField]
        private SendChannel _channelSendData = SendChannel.UnReliable;

        /// <summary>
        ///     Do we want to do client or server authority.
        /// </summary>
        [SerializeField] private bool _clientAuthority;

        /// <summary>
        ///     This will allow to instant teleport to new location regardless of where they are
        /// </summary>
        [SerializeField] private bool _allowTeleportation;

        /// <summary>
        ///     How many snapshots to take into account before sending the data to other side.
        /// </summary>
        [SerializeField] [Range(15, 240)] private int _snapShotSize = 15;

        #region Position Inspector

        [Header("Position Properties")]

        /// <summary>
        ///     Do we want to sync position updates.
        /// </summary>
        public bool syncPosition = true;

        /// <summary>
        ///     Sensitivity range for x axis
        /// </summary>
        [Range(.01f, 0.2f)] public float localPositionSensitivity = 0.01f;

        #endregion

        #region Rotation Inspector

        [Header("Rotation Properties")]
        /// <summary>
        ///     Do we want to sync rotation updates.
        /// </summary>
        public bool syncRotation = true;

        /// <summary>
        ///     Sensitivity range for rotation
        /// </summary>
        [Range(.01f, 0.2f)] public float localRotationSensitivity = 0.01f;

        #endregion

        #region Scale Inspector

        [Header("Scale Properties")]
        /// <summary>
        ///     Do we want to sync scale updates.
        /// </summary>
        public bool syncScale;

        /// <summary>
        ///     Sensitivity range for scale
        /// </summary>
        [Range(.01f, 0.2f)] public float localScaleSensitivity = 0.01f;

        #endregion

        private byte _currentSquence = 0;
        private ConcurrentQueue<ObjectSnapShot> _frameBuffer = new ConcurrentQueue<ObjectSnapShot>();
        private Vector3 _lastPosition;
        private Quaternion _lastRotation;
        private Vector3 _lastScale;

        // local position/rotation for VR support
        // SqrMagnitude is faster than Distance per Unity docs
        // https://docs.unity3d.com/ScriptReference/Vector3-sqrMagnitude.html

        private bool HasMoved => syncPosition && Vector3.SqrMagnitude(_lastPosition - transform.localPosition) >
            localPositionSensitivity * localPositionSensitivity;

        private bool HasRotated => syncRotation &&
                                   Quaternion.Angle(_lastRotation, transform.localRotation) > localRotationSensitivity;

        private bool HasScaled => syncScale && Vector3.SqrMagnitude(_lastScale - transform.localScale) >
            localScaleSensitivity * localScaleSensitivity;

        #endregion

        #region Class Specific

        /// <summary>
        /// </summary>
        /// <param name="data">The position data we want to set from.</param>
        /// <param name="position"></param>
        private void SetPosition(ref ObjectSnapShot data, Vector3 position)
        {
            if (!syncPosition)
            {
                data.PositionData = Vector3.zero;

                return;
            }

            data.PositionData = position;
        }

        /// <summary>
        /// </summary>
        /// <param name="data">The rotation data we want to set from.</param>
        /// <param name="rotation"></param>
        private void SetRotation(ref ObjectSnapShot data, Quaternion rotation)
        {
            if (!syncRotation)
            {
                data.RotationData = Quaternion.identity;

                return;
            }

            data.RotationData = rotation;
        }

        /// <summary>
        /// </summary>
        /// <param name="data">The scale data we want to set from.</param>
        /// <param name="scale">The scale of the transform we want to check against and set from.</param>
        private void SetScale(ref ObjectSnapShot data, Vector3 scale)
        {
            if (!syncScale)
            {
                data.ScaleData = Vector3.zero;

                return;
            }

            data.ScaleData = scale;
        }

        /// <summary>
        ///     Allows instant teleportation to a specific position passed in.
        ///     This will check if <see cref="_allowTeleportation" /> is been checked to allow teleportation
        ///     and also will depend on if its <see cref="_clientAuthority" /> has been check or not.
        ///     THIS DOES NOT DO ANY CHECKS BEFORE HAND. THIS IS UP TO THE END USER TO DO BASED ON INDIVIDUAL
        ///     GAME NEEDS AND WHAT THEY WANT TO CHECK OR DO NOT WANNA CHECK AGAINST.
        /// </summary>
        /// <param name="position"></param>
        public void TeleportToPosition(Vector3 position)
        {
        }

        /// <summary>
        ///     Network transform data has been received from client to server.
        /// </summary>
        /// <param name="data">The data we have received from client to server.</param>
        private void TransformDataReceived(ObjectSnapShot data)
        {
            if(data.NetId != NetId) return;

            if(data.SequenceData < _currentSquence) return;

            _pooledObjectSnapshotData.Add(data);
        }

        /// <summary>
        ///     Network transform data has been received from server to client.
        /// </summary>
        /// <param name="data">The data we have received from server to client.</param>
        private void TransformDataReceived<T>(T data)
        {
            if (!(data is List<ObjectSnapShot> objectSnapShots)) return;

            for (int i = 0; i < objectSnapShots.Count; i++)
            {
                if (objectSnapShots[i].NetId != NetId) continue;

                if (objectSnapShots[i].SequenceData < _currentSquence) continue;

                _frameBuffer.Enqueue(objectSnapShots[i]);
            }
        }

        // moved or rotated or scaled since last time we checked it?
        private bool HasEitherMovedRotatedScaled()
        {
            // Save last for next frame to compare only if change was detected, otherwise
            // slow moving objects might never sync because of C#'s float comparison tolerance.
            // See also: https://github.com/vis2k/Mirror/pull/428)
            bool changed = HasMoved || HasRotated || HasScaled;
            if (changed)
            {
                // local position/rotation for VR support
                if (syncPosition)
                {
                    _lastPosition = transform.localPosition;
                }

                if (syncRotation)
                {
                    _lastRotation = transform.localRotation;
                }

                if (syncScale)
                {
                    _lastScale = transform.localScale;
                }
            }

            return changed;
        }

        #endregion


        #region Mirror Specific

        /// <summary>
        ///     Register our handlers during on start server for client to server
        ///     authority messages.
        /// </summary>
        private void OnStartServer()
        {
            ConnectionToClient?.RegisterHandler<ObjectSnapShot>(TransformDataReceived);
        }

        /// <summary>
        ///     Register our handlers during on start server for server to client
        ///     authority messages.
        /// </summary>
        private void OnStartClient()
        {
            NetIdentity.Client.Connection?.RegisterHandler<List<ObjectSnapShot>>(TransformDataReceived);
        }

        #endregion

        #region Unity Methods

        private void Update()
        {
            if (IsLocalPlayer || IsServer) return;

            _frameBuffer.TryDequeue(out ObjectSnapShot data);

            // TODO Implement a speed movement factor into equation.
            transform.localPosition =
                Vector3.MoveTowards(transform.localPosition, data.PositionData, 5 * Time.deltaTime);
        }

        private void FixedUpdate()
        {
            var newData = new ObjectSnapShot();

            if (IsServer)
            {
                if (_pooledObjectSnapshotData.Count >= _snapShotSize)
                {
                    Server.SendToAll(_pooledObjectSnapshotData, (int)_channelSendData);

                    _pooledObjectSnapshotData.Clear();
                }

                if (HasEitherMovedRotatedScaled() && !_clientAuthority)
                {
                    SetPosition(ref newData, transform.localPosition);
                    SetRotation(ref newData, transform.localRotation);
                    SetScale(ref newData, transform.localScale);

                    newData.SequenceData++;
                    newData.NetId = (byte)NetId;

                    _pooledObjectSnapshotData.Add(newData);
                }
            }

            if (!IsClient) return;

            if (!HasAuthority || !_clientAuthority) return;

            if (!HasEitherMovedRotatedScaled()) return;

            SetPosition(ref newData, transform.localPosition);
            SetRotation(ref newData, transform.localRotation);
            SetScale(ref newData, transform.localScale);

            newData.SequenceData++;
            newData.NetId = (byte)NetId;

            ConnectionToServer?.Send(newData, (int)_channelSendData);
        }

        private void Awake()
        {
            _pooledObjectSnapshotData = new List<ObjectSnapShot>(_snapShotSize);

            NetIdentity.OnStartServer.AddListener(OnStartServer);

            NetIdentity.OnStartClient.AddListener(OnStartClient);
        }

        private void OnDestroy()
        {
            NetIdentity.Client.Connection?.UnregisterHandler<ObjectSnapShot>();
            ConnectionToClient?.UnregisterHandler<List<ObjectSnapShot>>();

            NetIdentity.OnStartClient.RemoveListener(OnStartClient);

            NetIdentity.OnStartServer.RemoveListener(OnStartServer);
        }

        #endregion
    }
}
