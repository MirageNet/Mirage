using UnityEditor;
using UnityEngine;

namespace Mirage
{
    /// <summary>
    /// Hides the <see cref="ClientObjectManager.spawnPrefabs"/> field when it is empty and <see cref="ClientObjectManager.NetworkPrefabs"/> is not null
    /// </summary>
    [CustomEditor(typeof(ClientObjectManager), true)]
    [CanEditMultipleObjects]
    public class ClientObjectManagerInspector : Editor
    {
        public override void OnInspectorGUI()
        {
            DefaultInspector();

            DrawButtons();
        }

        private void DefaultInspector()
        {
            serializedObject.UpdateIfRequiredOrScript();
            var itt = serializedObject.GetIterator();
            //script field
            itt.NextVisible(true);
            GUI.enabled = false;
            EditorGUILayout.PropertyField(itt);
            GUI.enabled = true;

            // other fields
            while (itt.NextVisible(false))
            {
                if (OverrideProperty(itt))
                    continue;

                EditorGUILayout.PropertyField(itt, true);
            }
            serializedObject.ApplyModifiedProperties();
        }

        private bool OverrideProperty(SerializedProperty property)
        {
            if (property.propertyPath == nameof(ClientObjectManager.spawnPrefabs))
                return OverrideSpawnPrefabProperty(property);

            return false;
        }

        private bool OverrideSpawnPrefabProperty(SerializedProperty property)
        {
            var com = (ClientObjectManager)target;

            // if networkPrefab has value and no items in spawnprefabs list,
            // then hide the list
            var removeField = com.NetworkPrefabs != null && property.arraySize == 0;
            if (removeField)
            {
                EditorGUILayout.Space(5);
                EditorGUILayout.LabelField("Prefabs", EditorStyles.boldLabel);
                EditorGUILayout.Space(3);
            }

            return removeField;
        }

        public void DrawButtons()
        {
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
            Undo.RecordObject(com.NetworkPrefabs, "Adding prefabs to SO {com.NetworkPrefabs.name}");
            NetworkPrefabUtils.AddToPrefabList(com.NetworkPrefabs.Prefabs, com.spawnPrefabs);
            EditorUtility.SetDirty(com.NetworkPrefabs);

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
                Undo.RecordObject(com.NetworkPrefabs, $"Register All Prefabs to SO {com.NetworkPrefabs.name}");
                NetworkPrefabUtils.AddToPrefabList(com.NetworkPrefabs.Prefabs, foundPrefabs);
                EditorUtility.SetDirty(com.NetworkPrefabs);
            }
            else
            {
                Undo.RecordObject(com, $"Register All Prefabs to COM {com.name}");
                PrefabUtility.RecordPrefabInstancePropertyModifications(target);
                NetworkPrefabUtils.AddToPrefabList(com.spawnPrefabs, foundPrefabs);
            }
        }
    }
}
