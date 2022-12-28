#if UNITY_2022_2_OR_NEWER
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace Mirage
{
    public partial class ReadOnlyDecoratorDrawer
    {
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            var field = new PropertyField(property);
            field.SetEnabled(false);
            return field;
        }
    }
}
#endif // UNITY_2022_2_OR_NEWER
