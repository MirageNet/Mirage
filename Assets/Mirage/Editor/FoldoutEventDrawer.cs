using System.Reflection;
using Mirage.Logging;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace Mirage
{
    [CustomPropertyDrawer(typeof(FoldoutEventAttribute))]
    public class FoldoutEventDrawer : PropertyDrawer
    {
        static readonly ILogger logger = LogFactory.GetLogger(typeof(FoldoutEventDrawer));

        UnityEventDrawer _unityEventDrawer;
        UnityEventDrawer UnityEventDrawer
        {
            get
            {
                if (_unityEventDrawer == null)
                {
                    _unityEventDrawer = new UnityEventDrawer();
                    // todo do we need to set fieldInfo for drawer to work correct?
                    typeof(PropertyDrawer).GetField("m_FieldInfo", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(_unityEventDrawer, fieldInfo);
                }
                return _unityEventDrawer;
            }
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorStyles.label.CalcSize(label).y +
                (property.isExpanded
                    ? EditorStyles.label.margin.vertical * 2 + UnityEventDrawer.GetPropertyHeight(property, label)
                    : EditorStyles.label.margin.vertical);
        }
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            float labelHeight = EditorStyles.label.CalcSize(label).y + EditorStyles.label.margin.vertical * 2;
            var labelRec = new Rect(position)
            {
                height = labelHeight
            };
            var eventRec = new Rect(position)
            {
                y = position.y + labelHeight,
                height = position.height - labelHeight
            };

            property.isExpanded = EditorGUI.BeginFoldoutHeaderGroup(labelRec, property.isExpanded, label);
            EditorGUI.EndFoldoutHeaderGroup();
            if (property.isExpanded)
            {
                UnityEventDrawer.OnGUI(eventRec, property, label);
            }
        }

        protected SceneAsset GetBuildSettingsSceneObject(string sceneName)
        {
            foreach (EditorBuildSettingsScene buildScene in EditorBuildSettings.scenes)
            {
                SceneAsset sceneAsset = AssetDatabase.LoadAssetAtPath<SceneAsset>(buildScene.path);
                if (sceneAsset.name == sceneName)
                {
                    return sceneAsset;
                }
            }
            return null;
        }
    }
}
