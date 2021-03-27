using System;
using Mirage.InterestManagement;
using Mirage.Logging;
using UnityEngine;

namespace Mirage.Components.InterestManagement
{
    public class OctreeInterestChecker : NetworkBehaviour
    {
        #region Fields

        static readonly ILogger logger = LogFactory.GetLogger(typeof(OctreeInterestChecker));

        /// <summary>
        ///     The real player visibility range to process incoming or outgoing data
        ///     to all player's using this visibility range.
        /// </summary>
        [SyncVar, NonSerialized] public float CurrentPlayerVisibilityRange;

        [Header("Interest Management Settings")]
        [SerializeField, Tooltip("Intended for server to check against hackers.")] private float _minimumVisibilityRange = 1;
        [SerializeField, Tooltip("Intended for server to check against hackers.")] private float _maximumVisibilityRange = 10;

        public NetworkInterestManager InterestManager;

        private Vector3 _colliderSize;
        protected internal Bounds CurrentBounds;
        private Vector3 _currentPosition;
        private Transform _currentTransform;

        #endregion

        #region Unity Methods

        private void OnValidate()
        {
            InterestManager ??= FindObjectOfType<NetworkInterestManager>();
            _colliderSize = GetComponent<Collider>().bounds.size;
        }

        private void Awake()
        {
            NetIdentity.OnStartServer.AddListener(OnStartServer);
        }

        private void OnStartServer()
        {
            InterestManager ??= FindObjectOfType<NetworkInterestManager>();
            _colliderSize = GetComponent<Collider>().bounds.size;
            _currentTransform = GetComponent<Transform>();

            CurrentPlayerVisibilityRange = Mathf.Max(_minimumVisibilityRange, _maximumVisibilityRange);
        }

        private void Update()
        {
            if (!IsServer || NetIdentity is null || Vector3.Distance(_currentPosition, _currentTransform.position) < 0.1f) return;

            InterestManager.Octree.Remove(NetIdentity);

            CurrentBounds = new Bounds(_currentTransform.position, _colliderSize * CurrentPlayerVisibilityRange);

            InterestManager.Octree.Add(NetIdentity, CurrentBounds);

            _currentPosition = _currentTransform.position;
        }

        #endregion

        #region Class Specific

        /// <summary>
        ///     Allow end users to change there visibility range for powerful computers.
        ///     Make it fair tho between powerful and low end users or advantages make occur.
        /// </summary>
        /// <param name="visibilityRange"></param>
        [Server]
        public void CmdChangeVisibilityRange(float visibilityRange)
        {
            // Ignore this change someone tried to hack or error
            // in code trying to set higher then maximum visibility range.
            if (visibilityRange > _maximumVisibilityRange)
            {
                if (logger.logEnabled)
                    logger.LogWarning("Attempt to change visibility range to outside of maximum range set.");

                return;
            }

            if (visibilityRange < _minimumVisibilityRange)
            {
                if (logger.logEnabled)
                    logger.LogWarning("Attempt to change visibility range to outside of minimum range set.");

                return;
            }

            CurrentPlayerVisibilityRange = visibilityRange;
        }

        #endregion
    }
}
