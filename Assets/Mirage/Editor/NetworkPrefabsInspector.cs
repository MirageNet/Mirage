using UnityEditor;
using UnityEngine;

namespace Mirage
{
    [CustomEditor(typeof(NetworkPrefabs))]
    public partial class NetworkPrefabsInspector : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            if (GUILayout.Button("Register All Prefabs"))
            {
                RegisterPrefabs();
            }
        }

        public void RegisterPrefabs()
        {
            var foundPrefabs = NetworkPrefabUtils.LoadAllNetworkIdentities();

            var target = (NetworkPrefabs)this.target;

            Undo.RecordObject(target, $"Register All Prefabs to SO {target.name}");
            NetworkPrefabUtils.AddToPrefabList(target.Prefabs, foundPrefabs);
            EditorUtility.SetDirty(target);
        }
    }
}
