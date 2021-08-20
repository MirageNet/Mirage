using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace Mirage
{
    public class ProximityGridSettings : ScriptableObject
    {
        [Header("grid")]
        [Tooltip("range of grid")]
        [SerializeField] Bounds bounds;
        [Tooltip("resolution of grid (if range is +-100, and resolution is 5 then there will be 40 grid spaces)")]
        [SerializeField] internal float resolution;

        [Header("update")]
        [SerializeField] float updateInterval = 0.5f;

        // Runtime settings
        NetworkProximityGridRunner runner;

        internal void CheckInit()
        {
            if (runner != null) { return; }

            runner = new GameObject($"{name} Runner").AddComponent<NetworkProximityGridRunner>();
            runner.Setup(bounds, resolution, updateInterval);
        }

        internal void Add(NetworkProximityGrid behaviour)
        {
            CheckInit();
            runner.Add(behaviour);
        }

        internal void Remove(NetworkProximityGrid behaviour)
        {
            if (runner != null)
                runner.Remove(behaviour);
        }
    }
    public class NetworkProximityGridRunner : MonoBehaviour
    {
        List<NetworkProximityGrid>[] grid;
        List<NetworkProximityGrid> all;
        private Bounds bounds;
        private float resolution;

        int xSize, zSize;

        internal void Setup(Bounds bounds, float resolution, float updateInterval)
        {
            this.bounds = bounds;
            this.resolution = resolution;
            xSize = Mathf.CeilToInt(bounds.size.x / resolution);
            zSize = Mathf.CeilToInt(bounds.size.z / resolution);

            grid = new List<NetworkProximityGrid>[xSize * zSize];
            all = new List<NetworkProximityGrid>();

            InvokeRepeating(nameof(Rebuild), 0, updateInterval);
        }

        void Rebuild()
        {
            // build grid
            foreach (NetworkProximityGrid behaviour in all)
            {
                Vector3 position1 = behaviour.GetCurrentPosition();
                int x1 = Mathf.RoundToInt(position1.x / resolution);
                int z1 = Mathf.RoundToInt(position1.z / resolution);

                Vector3 position2 = behaviour.GetPreviousPosition();
                int x2 = Mathf.RoundToInt(position2.x / resolution);
                int z2 = Mathf.RoundToInt(position2.z / resolution);

                // has moved
                if (x1 != x2 && z1 != z2)
                {
                    gridRemove(x1, z1, behaviour);
                    gridAdd(x2, z2, behaviour);
                }
            }

            // tell mirage to check if visibility changed
            foreach (NetworkProximityGrid behaviour in all)
            {
                behaviour.NetIdentity.RebuildObservers(false);
            }
        }

        internal void Add(NetworkProximityGrid behaviour)
        {
            Vector3 position = behaviour.GetCurrentPosition();
            int x = Mathf.RoundToInt(position.x / resolution);
            int z = Mathf.RoundToInt(position.z / resolution);

            gridAdd(x, z, behaviour);
        }

        internal void Remove(NetworkProximityGrid behaviour)
        {
            Vector3 position = behaviour.GetPreviousPosition();

            int x = Mathf.RoundToInt(position.x / resolution);
            int z = Mathf.RoundToInt(position.z / resolution);

            gridRemove(x, z, behaviour);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void gridAdd(int x, int z, NetworkProximityGrid behaviour)
        {
            if (grid[x + z * xSize] == null)
                grid[x + z * xSize] = new List<NetworkProximityGrid>();

            grid[x + z * xSize].Add(behaviour);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void gridRemove(int x, int z, NetworkProximityGrid behaviour)
        {
            if (grid[x + z * xSize] == null)
                return;

            grid[x + z * xSize].Remove(behaviour);
        }
    }
    public class NetworkProximityGrid : NetworkVisibility
    {
        [SerializeField] ProximityGridSettings gridSettings;
        [SerializeField] float range;

        int gridRange;

        private Vector3 previous;

        internal Vector3 GetCurrentPosition()
        {
            Vector3 current = transform.position;
            previous = current;
            return current;
        }

        internal Vector3 GetPreviousPosition()
        {
            return previous;
        }

        private void Awake()
        {
            NetIdentity.OnStartServer.AddListener(OnStartServer);
            NetIdentity.OnStopServer.AddListener(OnStopServer);
        }

        private void OnStartServer()
        {
            gridSettings.Add(this);
        }

        private void OnStopServer()
        {
            gridSettings.Remove(this);
        }

        public override bool OnCheckObserver(INetworkPlayer player)
        {
            if (player.Identity == null) { return false; }
            // only visible to players with NetworkProximityGrid
            if (!(player.Identity.Visibility is NetworkProximityGrid playerVisibility)) { return false; }


        }

        public override void OnRebuildObservers(HashSet<INetworkPlayer> observers, bool initialize)
        {
            throw new System.NotImplementedException();
        }
    }
}
