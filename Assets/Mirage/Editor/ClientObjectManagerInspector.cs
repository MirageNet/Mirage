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

            if (GUILayout.Button("Register All Prefabs"))
            {
                Undo.RecordObject(target, "Register prefabs for spawn");
                PrefabUtility.RecordPrefabInstancePropertyModifications(target);
                RegisterPrefabs((ClientObjectManager)target);
            }
        }

        public void RegisterPrefabs(ClientObjectManager gameObject)
        {
            var prefabs = LoadPrefabsContaining<NetworkIdentity>("Assets");

            foreach (var existing in gameObject.spawnPrefabs)
            {
                prefabs.Add(existing);
            }
            gameObject.spawnPrefabs.Clear();
            gameObject.spawnPrefabs.AddRange(prefabs);
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
