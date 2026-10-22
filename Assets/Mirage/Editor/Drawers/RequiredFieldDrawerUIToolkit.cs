#if UNITY_2022_2_OR_NEWER
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace Mirage
{
    public partial class RequiredFieldDrawer
    {
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            // no rate limit, this only runs when the inspector is built (eg after adding a component), so it must always check
            CheckAutoFind(RequiredAttribute, property, fieldInfo, rateLimit: false);

            var container = new VisualElement();

            var messageType = RequiredAttribute.Type == RequiredType.Error
                ? HelpBoxMessageType.Error
                : HelpBoxMessageType.Warning;

            var message = GetMessage(RequiredAttribute, property);
            var helpBox = new HelpBox(message, messageType);
            helpBox.style.marginBottom = 2;

            var field = new PropertyField(property);
            field.BindProperty(property);

            void UpdateHelpBox(SerializedProperty prop)
            {
                helpBox.style.display = IsUnassigned(prop) ? DisplayStyle.Flex : DisplayStyle.None;
            }

            UpdateHelpBox(property);
            container.TrackPropertyValue(property, UpdateHelpBox);

            container.Add(helpBox);
            container.Add(field);

            return container;
        }
    }
}
#endif // UNITY_2022_2_OR_NEWER
