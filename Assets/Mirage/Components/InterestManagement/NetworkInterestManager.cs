using UnityEngine;

namespace Mirage.Components.InterestManagement
{
    public class NetworkInterestManager : MonoBehaviour
    {
        #region Fields

        [SerializeField] private float _initialWorldSize = 1000;
        [SerializeField] private float _minimumNodeSize = 1;

        /// <summary>
        ///     Loose: The octree's nodes can be larger than 1/2 their parent's length and width, so they overlap to some extent.
        ///     This can alleviate the problem of even tiny objects ending up in large nodes if they're near boundaries.
        ///     A looseness value of 1.0 will make it a "normal" octree.
        /// </summary>
        [SerializeField] private float _looseness = 1.2f;

        [SerializeField] private ServerObjectManager _server;

        internal BoundsOctree<NetworkIdentity> QuadTree;

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

            var boundBox = new Bounds(netId.transform.position,
                colliderComponent != null ? colliderComponent.bounds.size : Vector3.one * _minimumNodeSize);

            QuadTree.Add(netId, boundBox);
        }

        #endregion

        #region Unity Methods

        private void Awake()
        {
            QuadTree = new BoundsOctree<NetworkIdentity>(_initialWorldSize, transform.position, _minimumNodeSize,
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

        #endregion
    }
}
