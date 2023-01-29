using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Mirage
{
    [CustomEditor(typeof(NetworkPrefabs))]
    public partial class NetworkPrefabsInspector : Editor
    {
        private SerializedProperty prefabs;

        private void OnEnable()
        {
            prefabs = serializedObject.FindProperty(nameof(NetworkPrefabs.Prefabs));
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            if (GUILayout.Button("Register All Prefabs"))
            {
                RegisterPrefabs();
            }

            EditorGUILayout.PropertyField(prefabs);

            serializedObject.ApplyModifiedProperties();
        }

        public void RegisterPrefabs()
        {
            var loadedPrefabs = LoadPrefabsContaining<NetworkIdentity>("Assets");

            for (var i = 0; i < prefabs.arraySize; i++)
            {
                var item = prefabs.GetArrayElementAtIndex(i).objectReferenceValue;

                if (item != null && item is NetworkIdentity identity)
                {
                    loadedPrefabs.Add(identity);
                }
            }

            prefabs.ClearArray();
            prefabs.arraySize = loadedPrefabs.Count;

            var index = 0;
            foreach (var prefab in loadedPrefabs)
            {
                prefabs.GetArrayElementAtIndex(index).objectReferenceValue = prefab;
                index++;
            }

            serializedObject.ApplyModifiedProperties();
        }

        private static ISet<T> LoadPrefabsContaining<T>(string path) where T : Component
        {
            var result = new HashSet<T>();

            var guids = AssetDatabase.FindAssets("t:GameObject", new[]
            {
                path
            });

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
