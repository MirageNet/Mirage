using System.Collections.Generic;
using UnityEngine;

namespace Mirage.Components.InterestManagement
{
    public class QuadTreeInterestChecker : NetworkVisibility
    {
        #region Fields

        /// <summary>
        ///     The visibility range of how far they can see in the entire scene
        ///     through the camera. This can be useful for powerful pc's to allow
        ///     spawning in the character but we wont use this for data coming in.
        /// </summary>
        public float SceneVisibilityRange = 100;

        /// <summary>
        ///     The real player visibility range to process incoming or outgoing data
        ///     to all player's using this visibility range.
        /// </summary>
        public float PlayerVisibilityRange => SceneVisibilityRange / 2;

        public NetworkInterestManager InterestManager;

        private Vector3 _currentPosition;

        #endregion

        #region Unity Methods

        private void OnValidate()
        {
            InterestManager ??= FindObjectOfType<NetworkInterestManager>();
        }

        private void Update()
        {
            if (_currentPosition * .1f != transform.position)
            {
                _currentPosition = transform.position;

                InterestManager.QuadTree.Remove(NetIdentity);

                InterestManager.QuadTree.Add(NetIdentity, new Bounds(transform.position, Vector3.one));
            }
        }

        #endregion

        #region Mirage Overrides

        public override bool OnCheckObserver(INetworkConnection conn)
        {
            var boundBox = new Bounds(conn.Identity.transform.position, Vector3.one * PlayerVisibilityRange);

            return InterestManager.QuadTree.IsColliding(boundBox);
        }

        public override void OnRebuildObservers(HashSet<INetworkConnection> observers, bool initialize)
        {
            foreach (INetworkConnection conn in Server.connections)
            {
                var boundBox = new Bounds(conn.Identity.transform.position, Vector3.one * PlayerVisibilityRange);

                if (conn != null && conn.Identity != null && InterestManager.QuadTree.IsColliding(boundBox))
                {
                    observers.Add(conn);
                }
            }
        }

        #endregion
    }
}
