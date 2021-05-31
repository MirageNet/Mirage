using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace Mirage
{
    [CustomPropertyDrawer(typeof(FoldoutEventAttribute))]
    public class FoldoutEventDrawer : PropertyDrawer
    {
        UnityEventDrawer _unityEventDrawer;
        UnityEventDrawer UnityEventDrawer => _unityEventDrawer ?? (_unityEventDrawer = new UnityEventDrawer());

        static GUIStyle Style => EditorStyles.label;
        static float Margin => Style.margin.vertical;

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            float labelHeight = GetLabelHeight(label);
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
            float labelHeight = GetLabelHeight(label) + Margin;
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
