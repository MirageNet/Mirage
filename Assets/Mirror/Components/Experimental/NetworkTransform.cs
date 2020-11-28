#region Statements

using System.Collections.Generic;
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
    public class NetworkTransform : NetworkBehaviour
    {
        #region Fields

        [Header("Configuration Properties")]

        [SerializeField] private SendChannel _channelSendData;

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
        [SerializeField, Range(15, 240)] private byte _snapShotSize = 15;

        #region Position Inspector

        [Header("Position Properties")]

        /// <summary>
        ///     Do we want to sync position updates.
        /// </summary>
        [SerializeField] public bool syncPosition = true;

        /// <summary>
        ///     Sensitivity range for x axis
        /// </summary>
        [SerializeField, Range(.01f, 1)] public float localPositionSensitivity = 0.01f;

        #endregion

        #region Rotation Inspector

        [Header("Rotation Properties")]

        /// <summary>
        ///     Do we want to sync rotation updates.
        /// </summary>
        [SerializeField] public bool syncRotation = true;

        /// <summary>
        ///     Sensitivity range for rotation
        /// </summary>
        [SerializeField, Range(.01f, 1)] public float localRotationSensitivity = 0.01f;

        #endregion

        #region Scale Inspector

        [Header("Scale Properties")]

        /// <summary>
        ///     Do we want to sync scale updates.
        /// </summary>
        [SerializeField] public bool syncScale = false;

        /// <summary>
        ///     Sensitivity range for scale
        /// </summary>
        [SerializeField, Range(.01f, 1)] public float localScaleSensitivity = 0.01f;

        #endregion

        private byte _previousStateId, _currentStateId, _currentSentStateId, _previousSentStateId;
        private NetworkTransformData _oldStateData, _currentStateData;
        private List<NetworkTransformData> _currentSnapShots;
        private readonly Dictionary<byte, NetworkTransformData> _dataStates = new Dictionary<byte, NetworkTransformData>();
        private byte _currentSnapShotSize = 0;
        private Vector3 _lastPosition;
        private Quaternion _lastRotation;
        private Vector3 _lastScale;

        // local position/rotation for VR support
        // SqrMagnitude is faster than Distance per Unity docs
        // https://docs.unity3d.com/ScriptReference/Vector3-sqrMagnitude.html

        private bool HasMoved => syncPosition && Vector3.SqrMagnitude(_lastPosition - transform.localPosition) > localPositionSensitivity * localPositionSensitivity;
        private bool HasRotated => syncRotation && Quaternion.Angle(_lastRotation, transform.localRotation) > localRotationSensitivity;
        private bool HasScaled => syncScale && Vector3.SqrMagnitude(_lastScale - transform.localScale) > localScaleSensitivity * localScaleSensitivity;

        #endregion

        #region Class Specific

        /// <summary>
        /// 
        /// </summary>
        /// <param name="data">The position data we want to set from.</param>
        /// <param name="position"></param>
        private NetworkTransformData SetPosition(ref NetworkTransformData data, Vector3 position)
        {
            if (!syncPosition)
            {
                data.PositionData = Vector3.zero;

                return data;
            }

            data.PositionData = _oldStateData.PositionData - transform.localPosition;

            return data;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="data">The rotation data we want to set from.</param>
        /// <param name="rotation"></param>
        private NetworkTransformData SetRotation(ref NetworkTransformData data, Quaternion rotation)
        {
            if (!syncRotation)
            {
                data.RotationData = Quaternion.identity;

                return data;
            }

            data.RotationData = new Quaternion(_oldStateData.RotationData.x - rotation.x,
                _oldStateData.RotationData.y - rotation.y, _oldStateData.RotationData.z - rotation.z,
                _oldStateData.RotationData.w - rotation.w);

            return data;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="data">The scale data we want to set from.</param>
        /// <param name="scale">The scale of the transform we want to check against and set from.</param>
        private NetworkTransformData SetScale(ref NetworkTransformData data, Vector3 scale)
        {
            if (!syncScale)
            {
                data.ScaleData = Vector3.zero;

                return data;
            }

            data.ScaleData = _oldStateData.ScaleData - transform.localScale;

            return data;
        }

        /// <summary>
        ///     Allows instant teleportation to a specific position passed in.
        ///     This will check if <see cref="_allowTeleportation"/> is been checked to allow teleportation
        ///     and also will depend on if its <see cref="_clientAuthority"/> has been check or not.
        ///
        ///     THIS DOES NOT DO ANY CHECKS BEFORE HAND. THIS IS UP TO THE END USER TO DO BASED ON INDIVIDUAL
        ///     GAME NEEDS AND WHAT THEY WANT TO CHECK OR DO NOT WANNA CHECK AGAINST.
        /// </summary>
        /// <param name="position"></param>
        public void TeleportToPosition(Vector3 position)
        {
            _currentStateData = new NetworkTransformData {PositionData = position};

            SetRotation(ref _currentStateData, transform.localRotation);
            SetScale(ref _currentStateData, transform.localScale);

            if (IsServer && !_clientAuthority)
                ConnectionToClient.Send(_currentStateData, (byte)_channelSendData);

            if (IsClient && _clientAuthority)
                ConnectionToServer.Send(_currentStateData, (byte)_channelSendData);

            _oldStateData = _currentStateData;

            _previousStateId = _currentStateId;

            _currentStateId++;
        }

        /// <summary>
        ///     Network transform data has been received from server or client.
        /// </summary>
        /// <param name="data">The data we have received from client or server.</param>
        private void TransformDataReceived(NetworkTransformData data)
        {
            if (_previousSentStateId.Equals(_currentSentStateId)) return;

            if (_dataStates.ContainsKey(data.SequenceData)) return;

            _previousSentStateId = _currentSentStateId;

            _dataStates.Add(data.SequenceData, data);

            _currentSentStateId = data.SequenceData;
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
                if (syncPosition) _lastPosition = transform.localPosition;
                if (syncRotation) _lastRotation = transform.localRotation;
                if (syncScale) _lastScale = transform.localScale;
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
            if (IsLocalPlayer)
            {
                ConnectionToClient?.RegisterHandler<NetworkTransformData>(TransformDataReceived);
            }
        }

        /// <summary>
        ///     Register our handlers during on start server for server to client
        ///     authority messages.
        /// </summary>
        private void OnStartClient()
        {
            ConnectionToServer?.RegisterHandler<NetworkTransformData>(TransformDataReceived);
        }

        #endregion

        #region Unity Methods

        private void Update()
        {
        }

        private void FixedUpdate()
        {
            if (_currentSnapShotSize >= _snapShotSize)
            {
                if (IsServer && HasEitherMovedRotatedScaled())
                {
                    if (HasAuthority && _clientAuthority) return;

                    Server.SendToAll(_currentSnapShots, (int)_channelSendData);

                }

                if (IsClient && !IsServer && _clientAuthority)
                {
                    ConnectionToServer.Send(_currentSnapShots);
                }

                _currentSnapShotSize = 0;
            }

            if(IsServer && _clientAuthority) return;

            if(!_clientAuthority && IsClient) return;

            _currentStateData = new NetworkTransformData();

            SetPosition(ref _currentStateData, transform.localPosition);
            SetRotation(ref _currentStateData, transform.localRotation);
            SetScale(ref _currentStateData, transform.localScale);

            _currentSnapShots.Add(_currentStateData);

            _oldStateData = _currentStateData;

            _currentSnapShotSize++;
        }

        private void Awake()
        {
            _currentSnapShots = new List<NetworkTransformData>(_snapShotSize);
            NetIdentity.OnStartServer.AddListener(OnStartServer);

            NetIdentity.OnStartClient.AddListener(OnStartClient);
        }

        private void OnDestroy()
        {
            ConnectionToClient?.UnregisterHandler<NetworkTransformData>();
            ConnectionToServer?.UnregisterHandler<NetworkTransformData>();

            NetIdentity.OnStartClient.RemoveListener(OnStartClient);

            NetIdentity.OnStartServer.RemoveListener(OnStartServer);
        }

        #endregion
    }
}
