using System.Collections.Generic;
using System.Linq;
using UnityEditor;

namespace Mirage
{
    public static class NetworkPrefabsCache
    {
        private static NetworkPrefabs[] _cache;

        /// <summary>
        /// Gets the List of NetworkPrefabs Objects.
        /// Only searches <see cref="AssetDatabase"/> in the cached field is null
        /// </summary>
        /// <returns></returns>
        public static IReadOnlyList<NetworkPrefabs> GetHolders()
        {
            if (_cache == null)
                _cache = FindHolders();

            return _cache;
        }

        private static NetworkPrefabs[] FindHolders()
        {
            return AssetDatabase.FindAssets($"t:{nameof(NetworkPrefabs)}")
                .Select(AssetDatabase.GUIDToAssetPath)
                .Select(AssetDatabase.LoadAssetAtPath<NetworkPrefabs>)
                .ToArray();
        }

        /// <summary>
        /// Clears the cached field so that <see cref="AssetDatabase"/> will be searched next time <see cref="GetHolders"/> is called
        /// </summary>
        public static void ClearCache()
        {
            _cache = null;
        }

    }
}
