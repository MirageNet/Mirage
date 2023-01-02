#if UNITY_2022_2_OR_NEWER // Unity uses UI toolkit by default for inspectors in 2022.2 and up.
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace Mirage
{
    public partial class NetworkedPrefabAttributeDrawer
    {
        private VisualElement _container;
        private HelpBox _errorBox;

        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            // Find all the client object managers first.
            _clientObjects = FindClientObjectManager();

            var root = new VisualElement();

            // It's not an object reference, show an error.
            if (property.propertyType != SerializedPropertyType.ObjectReference)
            {
                root.Add(new HelpBox(string.Format(NOT_OBJECT_REFERENCE_ERROR_FORMAT, property.name), HelpBoxMessageType.Error));
                return root;
            }

            // If's not a valid type (GameObject or subclass of component), show an error.
            if (!IsValidType(fieldInfo.FieldType))
            {
                root.Add(new HelpBox(INVALID_TYPE_ERROR, HelpBoxMessageType.Error));
                return root;
            }

            _container = new VisualElement { style = { flexDirection = FlexDirection.Row, display = DisplayStyle.None } };

            // Create the error box that will contain the error message.
            _errorBox = new HelpBox(string.Empty, HelpBoxMessageType.Error) { style = { flexGrow = 1 } };
            // Create the fix button.
            var fixButton = new Button(() =>
            {
                Fix(_currentTargetObject, _targetObjectHasIdentity, _targetObjectIsRegistered);
                Validate(_currentTargetObject);
                OnValidate();
            }) { text = "Fix", style = { minWidth = 60 } };

            _container.Add(_errorBox);
            _container.Add(fixButton);

            root.Add(_container);

            // Create the object field.
            var objField = new ObjectField(property.displayName) { objectType = fieldInfo.FieldType, value = property.objectReferenceValue, allowSceneObjects = false };

            // When the object field changes, validate the new value.
            objField.RegisterValueChangedCallback(evt =>
            {
                Validate(evt.newValue);
                OnValidate();
            });

            objField.BindProperty(property);
            // Add this class to the object field to make sure it gets the proper label width, as all other property fields.
            objField.AddToClassList("unity-base-field__aligned");

            root.Add(objField);

            return root;
        }

        /// <summary>
        ///     Called when the value of the property changes.
        /// </summary>
        private void OnValidate()
        {
            // If there's no target object, hide the error container with the error message and the fix button.
            if (_currentTargetObject == null)
            {
                _container.style.display = DisplayStyle.None;
                return;
            }

            // Get the fix message and apply it to the error box.
            var message = GetFixMessage(_targetObjectHasIdentity, _targetObjectIsRegistered);
            _errorBox.text = message;

            // Only show the error box and fix button if there's a fix message.
            _container.style.display = string.IsNullOrEmpty(message) ? DisplayStyle.None : DisplayStyle.Flex;
        }
    }
}
#endif // UNITY_2022_2_OR_NEWER
