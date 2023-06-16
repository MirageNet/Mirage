using System.Collections.Generic;

namespace Mirage
{
    /// <summary>
    /// Adds collection to list with option to skip 1 or 2 items
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal static class ListHelper
    {
        /// <summary>
        /// Use this too avoid allocation of IEnumerator
        /// </summary>
        public static void AddToList<T, TEnumerator>(List<T> list, TEnumerator enumerator, T skip1 = null, T skip2 = null) where T : class where TEnumerator : struct, IEnumerator<T>
        {
            list.Clear();
            while (enumerator.MoveNext())
            {
                var player = enumerator.Current;
                if (player == skip1 || player == skip2)
                    continue;

                list.Add(player);
            }
            enumerator.Dispose();
        }
        public static void AddToList<T>(List<T> list, IEnumerable<T> players, T skip1 = null, T skip2 = null) where T : class
        {
            list.Clear();
            var enumerator = players.GetEnumerator();
            while (enumerator.MoveNext())
            {
                var player = enumerator.Current;
                if (player == skip1 || player == skip2)
                    continue;

                list.Add(player);
            }
            enumerator.Dispose();
        }
        public static void AddToList<T>(List<T> list, IReadOnlyList<T> inList, T skip1 = null, T skip2 = null) where T : class
        {
            list.Clear();
            var count = inList.Count;
            for (var i = 0; i < count; i++)
            {
                var player = inList[i];
                if (player == skip1 || player == skip2)
                    continue;

                list.Add(player);
            }
        }
    }
}
