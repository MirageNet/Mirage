using UnityEditor;
using UnityEngine;

namespace Mirage
{
    [CustomEditor(typeof(ClientObjectManager), true)]
    [CanEditMultipleObjects]
    public class ClientObjectManagerInspector : Editor
    {
        private SerializedProperty networkPrefabs;

        private void OnEnable()
        {
            networkPrefabs = serializedObject.FindProperty(nameof(ClientObjectManager.NetworkPrefabs));
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            if (networkPrefabs.objectReferenceValue == null)
            {
                if (GUILayout.Button("Create NetworkPrefabs"))
                {
                    var path = EditorUtility.SaveFilePanelInProject("Create NetworkPrefabs", "NetworkPrefabs", "asset", "Create NetworkPrefabs");
                    CreateNetworkPrefabs(path);
                }
            }
        }

        public void CreateNetworkPrefabs(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                return;
            }

            var prefabs = CreateInstance<NetworkPrefabs>();
            AssetDatabase.CreateAsset(prefabs, path);
            AssetDatabase.SaveAssets();
            networkPrefabs.objectReferenceValue = prefabs;
            serializedObject.ApplyModifiedProperties();

            RegisterOldPrefabs(prefabs);
        }

#pragma warning disable CS0618 // Type or member is obsolete
        private void RegisterOldPrefabs(NetworkPrefabs prefabs)
        {
            var so = new SerializedObject(prefabs);
            so.Update();

            var spawnPrefabs = so.FindProperty(nameof(NetworkPrefabs.Prefabs));

            // Disable warning about obsolete field because we are using it for backwards compatibility.
            spawnPrefabs.arraySize = ((ClientObjectManager)target).spawnPrefabs.Count;

            for (var i = 0; i < spawnPrefabs.arraySize; i++)
            {
                spawnPrefabs.GetArrayElementAtIndex(i).objectReferenceValue = ((ClientObjectManager)target).spawnPrefabs[i];
            }

            so.ApplyModifiedProperties();
        }
#pragma warning restore CS0618
    }
}
