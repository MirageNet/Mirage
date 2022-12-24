#if UNITY_2022_2_OR_NEWER
#define USE_UI_TOOLKIT
#endif

using UnityEditor;
using UnityEngine;
#if USE_UI_TOOLKIT
using UnityEditor.UIElements;
using UnityEngine.UIElements;
#endif

namespace Mirage
{
    [CustomPropertyDrawer(typeof(SyncVarAttribute))]
    public class SyncVarAttributeDrawer : PropertyDrawer
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

#if USE_UI_TOOLKIT
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            var container = new VisualElement()
            {
                style =
                {
                    flexDirection = FlexDirection.Row
                }
            };

            var field = new PropertyField(property)
            {
                style =
                {
                    flexGrow = 1
                }
            };
            field.BindProperty(property);
            container.Add(field);

            var label = new Label(syncVarIndicatorContent.text)
            {
                tooltip = syncVarIndicatorContent.tooltip,
                style =
                {
                    fontSize = 10,
                    flexGrow = 0,
                    unityTextAlign = TextAnchor.MiddleLeft,
                    marginLeft = 2,
                    marginRight = 2
                }
            };

            container.Add(label);

            return container;
        }
#endif // USE_UI_TOOLKIT
    }
} //namespace
