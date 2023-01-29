using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Mirage
{
    [CustomEditor(typeof(NetworkPrefabs))]
    public partial class NetworkPrefabsDrawer : Editor
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
                RegisterPrefabs(prefabs);
            }

            EditorGUILayout.PropertyField(prefabs);

            serializedObject.ApplyModifiedProperties();
        }

        public void RegisterPrefabs(SerializedProperty property)
        {
            var loadedPrefabs = LoadPrefabsContaining<NetworkIdentity>("Assets");

            for (var i = 0; i < property.arraySize; i++)
            {
                var item = property.GetArrayElementAtIndex(i).objectReferenceValue;

                if (item != null && item is NetworkIdentity identity)
                {
                    loadedPrefabs.Add(identity);
                }
            }

            property.ClearArray();
            property.arraySize = loadedPrefabs.Count;

            var index = 0;
            foreach (var prefab in loadedPrefabs)
            {
                property.GetArrayElementAtIndex(index).objectReferenceValue = prefab;
                index++;
            }
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
