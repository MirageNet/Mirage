using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Mirage
{
    [CustomEditor(typeof(ClientObjectManager), true)]
    [CanEditMultipleObjects]
    public class ClientObjectManagerInspector : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            var com = (ClientObjectManager)target;
            if (com.NetworkPrefabs != null && com.spawnPrefabs.Count > 0)
            {
                if (GUILayout.Button("Move 'spawnPrefabs' to 'NetworkPrefabs'"))
                {
                    MovePrefabsToSO();
                }
            }

            if (GUILayout.Button("Register All Prefabs"))
            {
                RegisterAllPrefabs();
            }
        }

        public void MovePrefabsToSO()
        {
            var com = (ClientObjectManager)target;

            // add to new
            Undo.RecordObject(com.NetworkPrefabs, "Adding prefabs from com.spawnPrefabs");
            AddToPrefabList(com.NetworkPrefabs.Prefabs, com.spawnPrefabs);

            // clear old
            var listProp = serializedObject.FindProperty(nameof(ClientObjectManager.spawnPrefabs));
            Undo.RecordObject(target, "Clearing com.spawnPrefabs");
            listProp.arraySize = 0;
            serializedObject.ApplyModifiedProperties();
        }

        public void RegisterAllPrefabs()
        {
            var com = (ClientObjectManager)target;
            var foundPrefabs = LoadAllNetworkIdentities();

            // first use networkprefabs for list, if null then use the list field
            if (com.NetworkPrefabs != null)
            {
                Undo.RecordObject(com.NetworkPrefabs, "Register prefabs for spawn");
                AddToPrefabList(com.NetworkPrefabs.Prefabs, foundPrefabs);
            }
            else
            {
                Undo.RecordObject(target, "Register prefabs for spawn");
                PrefabUtility.RecordPrefabInstancePropertyModifications(target);
                AddToPrefabList(com.spawnPrefabs, foundPrefabs);
            }
        }

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
