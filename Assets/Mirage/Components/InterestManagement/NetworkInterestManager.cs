using UnityEngine;

namespace Mirage.Components.InterestManagement
{
    public class NetworkInterestManager : MonoBehaviour
    {
        #region Fields

        [Header("Network Interest Manager Settings")]
        [SerializeField, Tooltip("World size for main bounding box.")] private float _initialWorldSize = 1000;
        [SerializeField, Tooltip("Minimum size of each node. Will grow and shrink on its own.")] private float _minimumNodeSize = 1;

        /// <summary>
        ///     Loose: The octree's nodes can be larger than 1/2 their parent's length and width, so they overlap to some extent.
        ///     This can alleviate the problem of even tiny objects ending up in large nodes if they're near boundaries.
        ///     A looseness value of 1.0 will make it a "normal" octree.
        /// </summary>
        [Range(1,2)]
        [SerializeField, Tooltip("Normal quad tree will be used if 1.0f set, Anything higher will become loose quad tree.")] private float _looseness = 1.25f;

        [SerializeField] private ServerObjectManager _server;

        [Header("Debug Settings.")]
        [SerializeField] private bool _visualDebug = false;

        internal BoundsOctree<NetworkIdentity> Octree;

        #endregion

        #region Class Specific

        /// <summary>
        ///     When player gets authenticated we want to get some info to add them
        ///     to the quad tree list and maintain them from there.
        /// </summary>
        /// <param name="netId"></param>
        private void OnPlayerAuthenticated(NetworkIdentity netId)
        {
            netId.TryGetComponent(out Collider colliderComponent);

            netId.TryGetComponent(out OctreeInterestChecker octreeInterestChecker);

            Octree.Add(netId,
                new Bounds(netId.transform.position,
                    colliderComponent.bounds.size * octreeInterestChecker.CurrentPlayerVisibilityRange));
        }

        #endregion

        #region Unity Methods

        private void Awake()
        {
            _server ??= FindObjectOfType<ServerObjectManager>();

            Octree = new BoundsOctree<NetworkIdentity>(_initialWorldSize, transform.position, _minimumNodeSize,
                _looseness);

            if (_server == null)
            {
                return;
            }

            _server.Spawned.AddListener(OnPlayerAuthenticated);
        }

        private void OnValidate()
        {
            _server ??= FindObjectOfType<ServerObjectManager>();
        }

        private void OnDrawGizmos()
        {
            if(!Application.isPlaying || !_visualDebug) return;

            Octree.DrawAllBounds();
            Octree.DrawAllObjects();
            Octree.DrawCollisionChecks();
        }

        #endregion
    }
}
