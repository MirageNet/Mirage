using UnityEditor;
using UnityEngine;

namespace Mirage
{
    [CustomPropertyDrawer(typeof(SyncVarAttribute))]
    // UITookit used in 2022+, see SyncVarAttributeDrawerUIToolkit.cs
    public partial class SyncVarAttributeDrawer : PropertyDrawer
    {
        private static readonly GUIContent syncVarIndicatorContent = new GUIContent("SyncVar", "This variable has been marked with the [SyncVar] attribute.");

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var syncVarIndicatorRect = EditorStyles.miniLabel.CalcSize(syncVarIndicatorContent);
            var valueWidth = position.width - syncVarIndicatorRect.x;

            var valueRect = new Rect(position.x, position.y, valueWidth, position.height);
            var labelRect = new Rect(position.x + valueWidth, position.y, syncVarIndicatorRect.x, position.height);

            EditorGUI.PropertyField(valueRect, property, label, true);
            GUI.Label(labelRect, syncVarIndicatorContent, EditorStyles.miniLabel);
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUI.GetPropertyHeight(property);
        }
    }
}
