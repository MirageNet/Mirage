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

        private double _previousStateId, _currentStateId, _currentSentStateId, _previousSentStateId;
        private NetworkTransformData _oldStateSentData;
        private readonly Dictionary<double, NetworkTransformData> _dataStates = new Dictionary<double, NetworkTransformData>();

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

            data.PositionData = _oldStateSentData.PositionData - transform.localPosition;

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

            data.RotationData = new Quaternion(_oldStateSentData.RotationData.x - rotation.x,
                _oldStateSentData.RotationData.y - rotation.y, _oldStateSentData.RotationData.z - rotation.z,
                _oldStateSentData.RotationData.w - rotation.w);

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

            data.ScaleData = _oldStateSentData.ScaleData - transform.localScale;

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
            var newData = new NetworkTransformData {PositionData = position};

            SetRotation(ref newData, transform.localRotation);
            SetScale(ref newData, transform.localScale);

            if (IsServer && !_clientAuthority)
                ConnectionToClient.Send(newData, (byte)_channelSendData);

            if (IsClient && _clientAuthority)
                ConnectionToServer.Send(newData, (byte)_channelSendData);

            _oldStateSentData = newData;

            _previousStateId = _currentStateId;

            _currentStateId = NetIdentity.Client.Time.Time;
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

        private void FixedUpdate()
        {
        }

        private void Awake()
        {
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
