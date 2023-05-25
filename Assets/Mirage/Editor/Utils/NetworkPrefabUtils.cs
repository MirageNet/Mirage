using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Mirage
{
    public static class NetworkPrefabUtils
    {
        public static void AddToPrefabList(List<NetworkIdentity> existingList, IEnumerable<NetworkIdentity> newPrefabs)
        {
            var set = new HashSet<NetworkIdentity>();
            set.UnionWith(newPrefabs);
            set.UnionWith(existingList);
            existingList.Clear();
            existingList.AddRange(set);
        }

        public static ISet<NetworkIdentity> LoadAllNetworkIdentities()
        {
            return LoadPrefabsContaining<NetworkIdentity>("Assets");
        }

        private static ISet<T> LoadPrefabsContaining<T>(string path) where T : Component
        {
            var result = new HashSet<T>();

            var guids = AssetDatabase.FindAssets("t:GameObject", new[] { path });

            for (var i = 0; i < guids.Length; i++)
            {
                var assetPath = AssetDatabase.GUIDToAssetPath(guids[i]);

                var obj = AssetDatabase.LoadAssetAtPath<T>(assetPath);

                if (obj != null)
                {
                    result.Add(obj);
                }

                if (i % 100 == 99)
                {
                    EditorUtility.UnloadUnusedAssetsImmediate();
                }
            }
            EditorUtility.UnloadUnusedAssetsImmediate();
            return result;
        }
    }
}
