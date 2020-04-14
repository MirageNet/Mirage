using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;


namespace Mirror
{
    [CustomEditor(typeof(NetworkClient), true)]
    [CanEditMultipleObjects]
    public class NetworkClientInspector : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            if (GUILayout.Button("Register All Prefabs"))
            {
                RegisterPrefabs((NetworkClient)target);
                Undo.RecordObject(target, "Changed Area Of Effect");
            }
        }

        public void RegisterPrefabs(NetworkClient gameObject)
        {
            ISet<GameObject> prefabs = LoadPrefabsContaining<NetworkIdentity>(".");

            foreach (var existing in gameObject.spawnPrefabs)
            {
                prefabs.Add(existing);
            }
            gameObject.spawnPrefabs.Clear();
            gameObject.spawnPrefabs.AddRange(prefabs);
        }

        public static ISet<GameObject> LoadPrefabsContaining<T>(string path) where T : UnityEngine.Component
        {
            var result = new HashSet<GameObject>();

            var guids = AssetDatabase.FindAssets("t:Object", new[] { "Assets" });

            foreach (var guid in guids)
            {
                var assetPath = AssetDatabase.GUIDToAssetPath(guid);

                NetworkIdentity obj = AssetDatabase.LoadAssetAtPath<NetworkIdentity>(assetPath);

                if (obj != null)
                    result.Add(obj.gameObject);
            }
            return result;
        }
    }
}
