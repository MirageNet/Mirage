using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace Mirage.Visibility.SpatialHash
{
    internal static class SpatialHashExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static Vector2 ToXZ(this Vector3 v) => new Vector2(v.x, v.z);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static Vector3 FromXZ(this Vector2 v) => new Vector3(v.x, 0, v.y);
    }

    public class SpatialHashSystem : MonoBehaviour
    {
        public NetworkServer Server;

        /// <summary>
        /// How often (in seconds) that this object should update the list of observers that can see it.
        /// </summary>
        [Tooltip("How often (in seconds) that this object should update the list of observers that can see it.")]
        public float VisibilityUpdateInterval = 1;

        [Tooltip("Bounds of the map used to calculate visibility. Objects out side of grid will not be visibility")]
        public Bounds Bounds = new Bounds(Vector3.zero, 100 * Vector3.one);

        [Tooltip("How many points to split the grid into (in each xz axis)")]
        public Vector2Int GridCount = Vector2Int.one * 10;

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

                Grid = new GridHolder<INetworkPlayer>(Bounds, GridCount);
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
                Debug.Assert(obj.System == null);
                obj.System = this;
                all.Add(obj);
            }
        }
        private void World_onUnspawn(NetworkIdentity identity)
        {
            NetworkVisibility visibility = identity.Visibility;
            if (visibility is SpatialHashVisibility obj)
            {
                Debug.Assert(obj.System == this);
                obj.System = null;
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
            public readonly Vector2 Offset;
            public readonly Vector2 Size;
            public readonly Vector2 Extents;
            public readonly Vector2 GridSize;

            public readonly GridPoint[] Points;

            public GridHolder(Bounds bounds, Vector2Int gridCount)
            {
                Offset = (bounds.center - bounds.extents).ToXZ();
                Size = bounds.size.ToXZ();
                Extents = bounds.extents.ToXZ();

                Width = gridCount.x;
                Height = gridCount.y;

                GridSize = Size / gridCount;

                Points = new GridPoint[Width * Height];
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void AddObject(Vector2 position, T obj)
            {
                if (InBounds(position))
                {
                    ToGridIndex(position, out int x, out int y);
                    AddObject(x, y, obj);
                }
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
#if DEBUG
                if (i < 0) throw new IndexOutOfRangeException($"i ({i}) is less than zero");
                if (j < 0) throw new IndexOutOfRangeException($"j ({j}) is less than zero");
                if (i >= Width) throw new IndexOutOfRangeException($"i ({i}) is greater than {Width}");
                if (j >= Height) throw new IndexOutOfRangeException($"j ({j}) is greater than {Height}");
#endif
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
                return (-Extents.x <= position.x && position.x <= Extents.x)
                    && (-Extents.y <= position.y && position.y <= Extents.y);
            }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool InBounds(int x, int y)
            {
                // inclusive lower bound
                return (0 <= x && x < Width)
                    && (0 <= y && y < Height);
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
                float fx = position.x - Offset.x;
                float fy = position.y - Offset.y;

                x = Mathf.RoundToInt(fx / GridSize.x);
                y = Mathf.RoundToInt(fy / GridSize.y);
            }

            public void BuildObservers(HashSet<T> observers, Vector2 position, int range)
            {
                // not visible if not in range
                if (!InBounds(position))
                    return;

                ToGridIndex(position, out int x, out int y);

                for (int i = x - range; i <= x + range; i++)
                {
                    for (int j = y - range; j <= y + range; j++)
                    {
                        if (InBounds(i, j))
                        {
                            HashSet<T> set = GetObjects(i, j);
                            if (set != null)
                                UnionWithNonAlloc(observers, set);
                        }
                    }
                }
            }

            void UnionWithNonAlloc(HashSet<T> first, HashSet<T> second)
            {
                HashSet<T>.Enumerator enumerator = second.GetEnumerator();
                while (enumerator.MoveNext())
                {
                    first.Add(enumerator.Current);
                }
                enumerator.Dispose();
            }

            public struct GridPoint
            {
                public HashSet<T> objects;
            }
        }
    }
}
