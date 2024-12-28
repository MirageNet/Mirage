using UnityEditor;
using UnityEngine;

namespace Mirage
{
    [CustomEditor(typeof(NetworkIdentity), true)]
    public partial class NetworkIdentityInspector : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            var target = (NetworkIdentity)this.target;

            GUILayout.Space(12);
            EditorGUILayout.LabelField("Spawn Ids", EditorStyles.boldLabel);
            using (new EditorGUI.DisabledGroupScope(true))
            {
                EditorGUILayout.LabelField("Scene Id", target.SceneId != 0
                    ? target.SceneId.ToString("X")
                    : "<not part of a scene>");
                EditorGUILayout.LabelField("Prefab Hash", target.PrefabHash != 0
                    ? target.PrefabHash.ToString("X")
                    : "<no prefab>");
            }

            if (Application.isPlaying)
                RuntimeInfo(target);
        }

        private void RuntimeInfo(NetworkIdentity target)
        {


            GUILayout.Space(12);
            EditorGUILayout.LabelField("Runtime Values", EditorStyles.boldLabel);
            using (new EditorGUI.DisabledGroupScope(true))
            {
                EditorGUILayout.LabelField("Spawned", target.IsSpawned ? "Yes" : "No");
                EditorGUILayout.FloatField("Net Id", target.NetId);
            }

            GUILayout.Space(12);
            EditorGUILayout.LabelField("Server Values", EditorStyles.boldLabel);
            using (new EditorGUI.DisabledGroupScope(true))
            {
                EditorGUILayout.LabelField("Is Server", target.IsServer ? "Yes" : "No");
                EditorGUILayout.ObjectField("Server Object Manager", target.ServerObjectManager, typeof(ServerObjectManager), false);
                EditorGUILayout.TextField("Owner", (target.Owner != null)
                    ? "Client Authority: " + target.Owner
                    : "NULL");
            }

            GUILayout.Space(12);
            EditorGUILayout.LabelField("Client Values", EditorStyles.boldLabel);
            using (new EditorGUI.DisabledGroupScope(true))
            {
                EditorGUILayout.ObjectField("Client Object Manager", target.ClientObjectManager, typeof(ClientObjectManager), false);
                EditorGUILayout.LabelField("Is Client", target.IsClient ? "Yes" : "No");
                EditorGUILayout.LabelField("Is Local Player", target.IsLocalPlayer ? "Yes" : "No");
                EditorGUILayout.LabelField("Has Authority", target.HasAuthority ? "Yes" : "No");
            }

            GUILayout.Space(12);
            GUILayout.Label("Network Visibility", EditorStyles.boldLabel);
            using (new EditorGUI.DisabledGroupScope(true))
            {
                if (target.TryGetComponent<NetworkVisibility>(out var behaviour))
                    EditorGUILayout.ObjectField("Visibility", behaviour, typeof(MonoBehaviour), false);
                else
                    EditorGUILayout.TextField("Visibility", "Default ServerObjectManager");
            }

            GUILayout.Space(12);
            GUILayout.Label("Behaviours", EditorStyles.boldLabel);
            using (new EditorGUI.DisabledGroupScope(true))
            {
                EditorGUI.indentLevel++;
                foreach (var behaviour in target.NetworkBehaviours)
                {
                    if (behaviour == null)
                        // could be the case in the editor after existing play mode.
                        continue;

                    EditorGUILayout.ObjectField("Visibility", behaviour, typeof(MonoBehaviour), false);
                }
                EditorGUI.indentLevel--;
            }
        }
    }
}
