using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Mirage
{
    [CustomEditor(typeof(NetworkScene), true)]
    [CanEditMultipleObjects]
    public class NetworkSceneInspector : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            if (GUILayout.Button("Register All Prefabs"))
            {
                Undo.RecordObject(target, "Register prefabs for spawn");
                PrefabUtility.RecordPrefabInstancePropertyModifications(target);
                OnPostProcessScene((NetworkScene)target);
            }
        }

        public static void OnPostProcessScene(NetworkScene target)
        {
            target.SceneObjects.Clear();

            IEnumerable<NetworkIdentity> identities = Resources.FindObjectsOfTypeAll<NetworkIdentity>()
                .Where(identity => identity.gameObject.hideFlags != HideFlags.NotEditable &&
                                   identity.gameObject.hideFlags != HideFlags.HideAndDontSave &&
                                   identity.gameObject.scene.name != "DontDestroyOnLoad" &&
                                   !PrefabUtility.IsPartOfPrefabAsset(identity.gameObject));

            foreach (NetworkIdentity identity in identities)
            {
                target.SceneObjects.Add(identity);

                identity.SetSceneIdSceneHashPartInternal();

                identity.gameObject.SetActive(false);
            }
        }
    }
}
