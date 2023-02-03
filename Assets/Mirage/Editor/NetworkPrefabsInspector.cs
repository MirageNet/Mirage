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

            Undo.RecordObject(target, "Register prefabs for spawn");
            NetworkPrefabUtils.AddToPrefabList(target.Prefabs, foundPrefabs);
        }
    }
}
