using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace Mirage.Visibility.SpatialHash
{
    internal static class SpatialHashExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static Vector2 ToXZ(this Vector3 v) => new Vector2(v.x, v.z);
    }

    public class SpatialHashSystem : MonoBehaviour
    {
        public NetworkServer Server;

        /// <summary>
        /// How often (in seconds) that this object should update the list of observers that can see it.
        /// </summary>
        [Tooltip("How often (in seconds) that this object should update the list of observers that can see it.")]
        public float VisibilityUpdateInterval = 1;

        [Tooltip("height and width of 1 box in grid")]
        public float gridSize = 10;

        public Vector2 Centre = new Vector2(0, 0);

        [Tooltip("Bounds of the map used to calculate visibility. Objects out side of grid will not be visibility")]
        public Vector2 Size = new Vector2(100, 100);

        // todo is list vs hashset better? Set would be better for remove objects, list would be better for looping
        List<SpatialHashVisibility> all = new List<SpatialHashVisibility>();
        public GridHolder<INetworkPlayer> Grid;


        public void Awake()
        {
            Server.Started.AddListener(() =>
            {
                Server.World.onSpawn += World_onSpawn;
                Server.World.onUnspawn += World_onUnspawn;

                // skip first invoke, list will be empty
                InvokeRepeating(nameof(RebuildObservers), VisibilityUpdateInterval, VisibilityUpdateInterval);

                Grid = new GridHolder<INetworkPlayer>(gridSize, Centre, Size);
            });

            Server.Stopped.AddListener(() =>
            {
                CancelInvoke(nameof(RebuildObservers));
                Grid = null;
            });
        }

        private void World_onSpawn(NetworkIdentity identity)
        {
            NetworkVisibility visibility = identity.Visibility;
            if (visibility is SpatialHashVisibility obj)
            {
                obj.System = this;
                all.Add(obj);
            }
        }
        private void World_onUnspawn(NetworkIdentity identity)
        {
            NetworkVisibility visibility = identity.Visibility;
            if (visibility is SpatialHashVisibility obj)
            {
                all.Remove(obj);
            }
        }

        void RebuildObservers()
        {
            ClearGrid();
            AddPlayersToGrid();

            foreach (SpatialHashVisibility obj in all)
            {
                obj.Identity.RebuildObservers(false);
            }
        }

        private void ClearGrid()
        {
            for (int i = 0; i < Grid.Width; i++)
            {
                for (int j = 0; j < Grid.Width; j++)
                {
                    HashSet<INetworkPlayer> set = Grid.GetObjects(i, j);
                    if (set != null)
                    {
                        set.Clear();
                    }
                }
            }
        }

        private void AddPlayersToGrid()
        {
            foreach (INetworkPlayer player in Server.Players)
            {
                if (!player.HasCharacter)
                    continue;

                Vector2 position = player.Identity.transform.position.ToXZ();
                Grid.AddObject(position, player);
            }
        }


        public class GridHolder<T>
        {
            public readonly int Width;
            public readonly int Height;
            public readonly float GridSize;
            public readonly Vector2 Centre;
            public readonly Vector2 Size;

            public readonly GridPoint[] Points;

            public GridHolder(float gridSize, Vector2 centre, Vector2 size)
            {
                Centre = centre;
                Size = size;
                Width = Mathf.CeilToInt(size.x / gridSize);
                Height = Mathf.CeilToInt(size.y / gridSize);
                GridSize = gridSize;

                Points = new GridPoint[Width * Height];
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void AddObject(Vector2 position, T obj)
            {
                ToGridIndex(position, out int x, out int y);
                AddObject(x, y, obj);
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void AddObject(int i, int j, T obj)
            {
                int index = i + j * Width;
                if (Points[index].objects == null)
                {
                    Points[index].objects = new HashSet<T>();
                }

                Points[index].objects.Add(obj);
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public HashSet<T> GetObjects(int i, int j)
            {
                return Points[i + j * Width].objects;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void CreateSet(int i, int j)
            {
                Points[i + j * Width].objects = new HashSet<T>();
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool InBounds(Vector2 position)
            {
                float x = position.x - Centre.x;
                float y = position.y - Centre.y;

                return (0 < x && x < Size.x)
                    && (0 < y && y < Size.y);
            }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool InBounds(int x, int y)
            {
                return (0 < x && x < Width)
                    && (0 < y && y < Height);
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool IsVisible(Vector2 target, Vector2 player, int range)
            {
                // if either is out of bounds, not visible
                if (!InBounds(target) || !InBounds(player)) return false;

                ToGridIndex(target, out int xt, out int yt);
                ToGridIndex(player, out int xp, out int yp);

                return AreClose(xt, xp, range) && AreClose(yt, yp, range);
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            bool AreClose(int a, int b, int range)
            {
                int min = a - range;
                int max = a + range;

                return max <= b && b <= min;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void ToGridIndex(Vector2 position, out int x, out int y)
            {
                float fx = position.x - Centre.x;
                float fy = position.y - Centre.y;

                x = Mathf.RoundToInt(fx / GridSize);
                y = Mathf.RoundToInt(fy / GridSize);
            }

            public void BuildObservers(HashSet<T> observers, Vector2 position, int range)
            {
                // not visible if not in range
                if (!InBounds(position))
                    return;

                ToGridIndex(position - Centre, out int x, out int y);

                for (int i = x - range; i <= x + range; i++)
                {
                    for (int j = y - range; j <= y + range; j++)
                    {
                        if (InBounds(i, j))
                        {
                            observers.UnionWith(GetObjects(i, j));
                        }
                    }
                }
            }

            public struct GridPoint
            {
                public HashSet<T> objects;
            }
        }
    }
}
