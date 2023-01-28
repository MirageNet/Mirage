#if UNITY_2022_2_OR_NEWER // Unity uses UI toolkit by default for inspectors in 2022.2 and up.
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using System.Globalization;
using UnityEngine;

namespace Mirage
{
    public partial class NetworkBehaviourInspector
    {
        public override VisualElement CreateInspectorGUI()
        {
            var root = new VisualElement();

            // Create the default inspector.
            var iterator = serializedObject.GetIterator();
            for (bool enterChildren = true; iterator.NextVisible(enterChildren); enterChildren = false)
            {
                var field = new PropertyField(iterator);

                // Disable the script field.
                if (iterator.propertyPath == "m_Script")
                {
                    field.SetEnabled(false);
                }

                root.Add(field);
            }

            // Create the sync lists editor.
            var syncLists = _drawer.CreateDefaultSyncLists();
            if (syncLists != null)
            {
                root.Add(syncLists);
            }

            // Crete the sync settings editor.
            var syncSettings = _drawer.CreateDefaultSyncSettings();
            if (syncSettings != null)
            {
                root.Add(syncSettings);
            }

            return root;
        }
    }

    public partial class NetworkBehaviourInspectorDrawer
    {
        public VisualElement CreateDefaultSyncLists()
        {
            return _syncListDrawer?.Create();
        }

        public VisualElement CreateDefaultSyncSettings()
        {
            if (!_syncsAnything)
            {
                return null;
            }

            var root = new VisualElement();

            root.Add(CreateHeader("Sync Settings"));

            var syncMode = new PropertyField(_serializedObject.FindProperty("syncMode"));
            syncMode.Bind(_serializedObject);
            root.Add(syncMode);

            var syncInterval = new PropertyField(_serializedObject.FindProperty("syncInterval"));
            syncInterval.Bind(_serializedObject);
            root.Add(syncInterval);

            return root;
        }

        public static VisualElement CreateHeader(string text)
        {
            var root = new VisualElement();
            root.AddToClassList("unity-decorator-drawers-container");

            var label = new Label(text);
            label.AddToClassList("unity-header-drawer__label");

            root.Add(label);

            return root;
        }
    }

    public partial class SyncListDrawer
    {
        public VisualElement Create()
        {
            if (_syncListFields.Count == 0) { return null; }

            var root = new VisualElement();

            root.Add(NetworkBehaviourInspectorDrawer.CreateHeader("Sync Lists"));

            foreach (var syncListField in _syncListFields)
            {
                root.Add(CreateSyncList(syncListField));
            }

            return root;
        }

        private VisualElement CreateSyncList(SyncListField syncListField)
        {
            syncListField.UpdateItems(_targetObject);

            var list = new ListView(syncListField.items)
            {
                showBorder = true,
                showFoldoutHeader = true,
                headerTitle = syncListField.label,
                showAddRemoveFooter = false,
                showBoundCollectionSize = false,
                showAlternatingRowBackgrounds = AlternatingRowBackground.All,
                makeItem = MakeSyncListItem,
                bindItem = (element, i) => BindSyncListItem(element, i, syncListField),
                viewDataKey = "mirage-sync-list-" + syncListField.field.Name // Used for the expanded state
            };

            // Because we can't listen for any lists updates, just refresh the list completely every 1 second.
            list.schedule.Execute(() =>
            {
                // No need to update the game is not playing.
                if (Application.isPlaying)
                {
                    syncListField.UpdateItems(_targetObject);

                    list.Rebuild();
                }
            }).Every(1000);

            return list;
        }

        private static VisualElement MakeSyncListItem()
        {
            var root = new VisualElement();
            root.AddToClassList("unity-base-field");
            root.AddToClassList("unity-base-field__aligned");
            root.AddToClassList("unity-base-field__inspector-field");

            var elementLabel = new Label
            {
                name = "element-label"
            };

            elementLabel.AddToClassList("unity-base-field__label");
            elementLabel.AddToClassList("unity-property-field__label");

            var elementValue = new Label("Test")
            {
                name = "element-value"
            };

            root.Add(elementLabel);
            root.Add(elementValue);

            return root;
        }

        private static void BindSyncListItem(VisualElement element, int index, SyncListField field)
        {
            var elementLabel = element.Q<Label>("element-label");
            var elementValue = element.Q<Label>("element-value");

            elementLabel.text = $"Element {index.ToString(CultureInfo.InvariantCulture)}";

            elementValue.text = field.items[index] != null ? field.items[index].ToString() : "NULL";
        }
    }
}
#endif // UNITY_2022_2_OR_NEWER
