using System;
using System.Collections.Generic;
using Mirage.Components.InterestManagement;
using UnityEditor;
using UnityEngine;

namespace Mirage.InterestManagement
{
    public class NetworkInterestManager : InterestManager
    {
        #region Fields

        [Header("Network Interest Manager Settings")] [SerializeField, Tooltip("World size for main bounding box.")]
        private float _initialWorldSize = 1000;

        [SerializeField, Tooltip("Minimum size of each node. Will grow and shrink on its own.")]
        private float _minimumNodeSize = 1;

        /// <summary>
        ///     Loose: The octree's nodes can be larger than 1/2 their parent's length and width, so they overlap to some extent.
        ///     This can alleviate the problem of even tiny objects ending up in large nodes if they're near boundaries.
        ///     A looseness value of 1.0 will make it a "normal" octree.
        /// </summary>
        [Range(1, 2)]
        [SerializeField,
         Tooltip("Normal quad tree will be used if 1.0f set, Anything higher will become loose quad tree.")]
        private float _looseness = 1.25f;

        [SerializeField] private ServerObjectManager _server;

        [Header("Debug Settings.")] [SerializeField]
        private bool _visualDebug = false;

        internal BoundsOctree<NetworkIdentity> Octree;
        private List<NetworkIdentity> List = new List<NetworkIdentity>();

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
            if (!Application.isPlaying || !_visualDebug) return;

            Octree.DrawAllBounds();
            Octree.DrawAllObjects();
            Octree.DrawCollisionChecks();
        }

        #endregion

        #region Overrides of InterestManager

        /// <summary>
        /// Invoked when a player joins the server
        /// It should show all objects relevant to that player
        /// </summary>
        /// <param name="identity"></param>
        protected override void OnAuthenticated(INetworkPlayer player)
        {
            foreach (NetworkIdentity identity in ServerObjectManager.SpawnedObjects.Values)
            {
                //if(!Octree.IsColliding(identity.GetComponent<OctreeInterestChecker>().CurrentBounds, player)) continue;

                ServerObjectManager.ShowForConnection(identity, player);
            }
        }

        /// <summary>
        /// Invoked when an object is spawned in the server
        /// It should show that object to all relevant players
        /// </summary>
        /// <param name="identity">The object just spawned</param>
        protected override void OnSpawned(NetworkIdentity identity)
        {
            identity.TryGetComponent(out OctreeInterestChecker colliderComponent);

            foreach (INetworkPlayer player in ServerObjectManager.Server.Players)
            {
                if (!Octree.IsColliding(colliderComponent.CurrentBounds, player.Identity) && identity != player.Identity) continue;

                ServerObjectManager.ShowForConnection(identity, player);
            }
        }

        /// <summary>
        /// Find out all the players that can see an object
        /// </summary>
        /// <param name="identity"></param>
        /// <returns></returns>
        public override IReadOnlyCollection<INetworkPlayer> Observers(NetworkIdentity identity)
        {
            List.Clear();

            identity.TryGetComponent(out OctreeInterestChecker colliderComponent);

            Octree.GetColliding(List, colliderComponent.CurrentBounds);

            var temp = new List<INetworkPlayer>();

            for (int index = 0; index < List.Count; index++)
            {
                NetworkIdentity player = List[index];

                temp.Add(player.ConnectionToClient);
            }

            return temp;
        }

        #endregion
    }
}
