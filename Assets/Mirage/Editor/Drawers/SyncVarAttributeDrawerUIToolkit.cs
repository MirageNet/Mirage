#if UNITY_2022_2_OR_NEWER
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Mirage
{
    public partial class SyncVarAttributeDrawer
    {
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
    }
}
#endif // UNITY_2022_2_OR_NEWER
