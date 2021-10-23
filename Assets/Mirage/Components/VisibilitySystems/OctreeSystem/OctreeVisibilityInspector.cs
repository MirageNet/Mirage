using Assets.Mirage.Components;
using UnityEngine;

namespace Mirage.Components
{
    public class OctreeVisibilityInspector : BaseVisibilityInspector
    {
        #region Fields

        [Header("Network Interest Manager Settings")]
        [SerializeField, Tooltip("World size for main bounding box.")]
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

        [Header("Debug Settings.")]
        [SerializeField]
        private bool _visualDebug = false;

        public OctreeVisibility OctreeVisibility;

        #endregion

        #region Unity Methods

        protected override void Awake()
        {
            base.Awake();

            NetworkVisibility = OctreeVisibility = new OctreeVisibility(ServerObjectManager, _initialWorldSize,
                transform.position, _minimumNodeSize, _looseness);
        }

        private void OnDrawGizmos()
        {
            if (!Application.isPlaying || !_visualDebug) return;

#if UNITY_EDITOR
            OctreeVisibility?.Octree.DrawAllBounds();
            OctreeVisibility?.Octree.DrawAllObjects();
            OctreeVisibility?.Octree.DrawCollisionChecks();
#endif
        }

        #endregion
    }
}
