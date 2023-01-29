#if UNITY_2022_2_OR_NEWER
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace Mirage
{
    public partial class NetworkPrefabsDrawer
    {
        private ListView listView;

        public override VisualElement CreateInspectorGUI()
        {
            var root = new VisualElement();

            var button = new Button(() =>
            {
                serializedObject.Update();
                RegisterPrefabs(prefabs);
                serializedObject.ApplyModifiedProperties();
            }) { text = "Register All Prefabs" };

            root.Add(button);

            var prefabsField = new PropertyField(prefabs);

            prefabsField.RegisterCallback<GeometryChangedEvent, VisualElement>((evt, element) =>
            {
                if (listView == null)
                {
                    listView = element.Q<ListView>();
                    if (listView != null)
                    {
                        listView.style.maxHeight = new StyleLength(new Length(100, LengthUnit.Percent));
                    }
                }
            }, prefabsField);

            root.Add(prefabsField);

            return root;
        }
    }
}
#endif
