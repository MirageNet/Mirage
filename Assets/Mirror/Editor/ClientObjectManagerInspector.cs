using System.Collections.Generic;
using UnityEditor;
using UnityEngine;


namespace Mirror
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
                Undo.RecordObject(target, "Changed Area Of Effect");
                RegisterPrefabs((ClientObjectManager)target);
            }
        }

        public void RegisterPrefabs(ClientObjectManager gameObject)
        {
            ISet<NetworkIdentity> prefabs = LoadPrefabsContaining<NetworkIdentity>("Assets");

            foreach (NetworkIdentity existing in gameObject.spawnPrefabs)
            {
                prefabs.Add(existing);
            }
            gameObject.spawnPrefabs.Clear();
            gameObject.spawnPrefabs.AddRange(prefabs);
        }

        private static ISet<T> LoadPrefabsContaining<T>(string path) where T : UnityEngine.Component
        {
            var result = new HashSet<T>();

            var guids = AssetDatabase.FindAssets("t:Object", new[] { path });

            int index = 0;

            foreach (string guid in guids)
            {
                var assetPath = AssetDatabase.GUIDToAssetPath(guid);

                T obj = AssetDatabase.LoadAssetAtPath<T>(assetPath);

                if (obj != null)
                {
                    result.Add(obj);
                }

                if (index++ > 100)
                {
                    EditorUtility.UnloadUnusedAssetsImmediate();
                    index = 0;
                }
            }
            EditorUtility.UnloadUnusedAssetsImmediate();
            return result;
        }
    }
}
