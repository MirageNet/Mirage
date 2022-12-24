#if UNITY_2022_2_OR_NEWER
#define USE_UI_TOOLKIT
#endif // UNITY_2022_2_OR_NEWER

using UnityEditor;
using UnityEngine;
#if USE_UI_TOOLKIT
using UnityEditor.UIElements;
using UnityEngine.UIElements;
#endif // USE_UI_TOOLKIT

namespace Mirage
{
    [CustomPropertyDrawer(typeof(ReadOnlyInspectorAttribute))]
    public class ReadOnlyDecoratorDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            GUI.enabled = false;
            EditorGUI.PropertyField(position, property, label, true);
            GUI.enabled = true;
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUI.GetPropertyHeight(property, label, true);
        }

#if USE_UI_TOOLKIT
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            var field = new PropertyField(property);
            field.SetEnabled(false);
            return field;
        }
#endif // USE_UI_TOOLKIT
    }
}
