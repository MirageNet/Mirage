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
            NetworkPrefabUtils.AddToPrefabList(com.NetworkPrefabs.Prefabs, com.spawnPrefabs);

            // clear old
            var listProp = serializedObject.FindProperty(nameof(ClientObjectManager.spawnPrefabs));
            Undo.RecordObject(target, "Clearing com.spawnPrefabs");
            listProp.arraySize = 0;
            serializedObject.ApplyModifiedProperties();
        }

        public void RegisterAllPrefabs()
        {
            var com = (ClientObjectManager)target;
            var foundPrefabs = NetworkPrefabUtils.LoadAllNetworkIdentities();

            // first use networkprefabs for list, if null then use the list field
            if (com.NetworkPrefabs != null)
            {
                Undo.RecordObject(com.NetworkPrefabs, "Register prefabs for spawn");
                NetworkPrefabUtils.AddToPrefabList(com.NetworkPrefabs.Prefabs, foundPrefabs);
            }
            else
            {
                Undo.RecordObject(target, "Register prefabs for spawn");
                PrefabUtility.RecordPrefabInstancePropertyModifications(target);
                NetworkPrefabUtils.AddToPrefabList(com.spawnPrefabs, foundPrefabs);
            }
        }
    }
}
