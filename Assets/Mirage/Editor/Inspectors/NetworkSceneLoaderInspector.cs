using Mirage.Components;
using UnityEditor;

namespace Mirage
{
    [CustomEditor(typeof(NetworkSceneLoader), true)]
    [CanEditMultipleObjects]
    public class NetworkSceneLoaderInspector : Editor
    {
        public override void OnInspectorGUI()
        {
            EditorGUILayout.HelpBox(
                "NetworkSceneLoader provides simple scene management and character spawning:\n\n" +
                "• If Target Scene is set, connecting clients will load it before spawning.\n" +
                "• If empty, characters spawn immediately upon authentication in the active scene.\n" +
                "• Calling ServerLoadScene() transitions all clients to a new scene and respawns their characters.\n\n" +
                "For advanced use cases (such as additive scenes, custom spawn logic, or persistent player proxies), copy and customize this script for your project.",
                MessageType.Info);

            DrawDefaultInspector();
        }
    }
}
