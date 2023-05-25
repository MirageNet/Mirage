using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace Mirage
{
    [CustomPropertyDrawer(typeof(FoldoutEventAttribute))]
    // UITookit used in 2022+, see FoldoutEventDrawerUIToolkit.cs
    public partial class FoldoutEventDrawer : PropertyDrawer
    {
        private UnityEventDrawer _unityEventDrawer;

        private UnityEventDrawer UnityEventDrawer => _unityEventDrawer ?? (_unityEventDrawer = new UnityEventDrawer());

        private static GUIStyle Style => EditorStyles.label;

        private static float Margin => Style.margin.vertical;

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            var labelHeight = GetLabelHeight(label);
            if (property.isExpanded)
            {
                return labelHeight + Margin + UnityEventDrawer.GetPropertyHeight(property, label);
            }
            else
            {
                return labelHeight;
            }
        }

        private static float GetLabelHeight(GUIContent label)
        {
            return Style.CalcSize(label).y + Margin;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var labelHeight = GetLabelHeight(label) + Margin;
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
    }
}
