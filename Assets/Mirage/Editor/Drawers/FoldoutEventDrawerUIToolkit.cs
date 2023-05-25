#if UNITY_2022_2_OR_NEWER
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace Mirage
{
    public partial class FoldoutEventDrawer
    {
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            var foldout = new Foldout { text = property.displayName };

            foldout.Add(UnityEventDrawer.CreatePropertyGUI(property));

            foldout.BindProperty(property);

            return foldout;
        }
    }
}
#endif
