#if UNITY_2022_2_OR_NEWER // Unity uses UI toolkit by default for inspectors in 2022.2 and up.
#define USE_UI_TOOLKIT
#endif

using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Mirage.Collections;
using Mirage.Logging;
using UnityEditor;
using UnityEngine;
#if USE_UI_TOOLKIT
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using System.Globalization;
#endif // USE_UI_TOOLKIT
using Object = UnityEngine.Object;

namespace Mirage
{
#if !EXCLUDE_NETWORK_BEHAVIOUR_INSPECTOR
    [CustomEditor(typeof(NetworkBehaviour), true)]
    [CanEditMultipleObjects]
    public class NetworkBehaviourInspector : Editor
    {
        private static readonly ILogger logger = LogFactory.GetLogger(typeof(NetworkBehaviourInspector));
        private NetworkBehaviourInspectorDrawer _drawer;

        private void OnEnable()
        {
            if (target == null) { logger.LogWarning("NetworkBehaviourInspector had no target object"); return; }

            // If target's base class is changed from NetworkBehaviour to MonoBehaviour
            // then Unity temporarily keep using this Inspector causing things to break
            if (!(target is NetworkBehaviour)) { return; }

            _drawer = new NetworkBehaviourInspectorDrawer(target, serializedObject);
        }

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            _drawer.DrawDefaultSyncLists();
            _drawer.DrawDefaultSyncSettings();
        }

#if USE_UI_TOOLKIT
        public override VisualElement CreateInspectorGUI()
        {
            var root = new VisualElement();

            // Create the default inspector.
            var iterator = serializedObject.GetIterator();
            for (bool enterChildren = true; iterator.NextVisible(enterChildren); enterChildren = false)
            {
                var field = new PropertyField(iterator);
                field.Bind(serializedObject);

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
#endif // USE_UI_TOOLKIT
    }
#endif

    /// <summary>
    /// split from Editor class so that people can use this with custom inspectoer
    /// </summary>
    public class NetworkBehaviourInspectorDrawer
    {
        /// <summary>
        /// List of all visible syncVars in target class
        /// </summary>
        private readonly List<string> _syncVarNames = new List<string>();
        private readonly bool _syncsAnything;
        private readonly SyncListDrawer _syncListDrawer;
        private readonly SerializedObject _serializedObject;

        public NetworkBehaviourInspectorDrawer(Object target, SerializedObject serializedObject)
        {
            _serializedObject = serializedObject;

            _syncVarNames = new List<string>();
            foreach (var field in InspectorHelper.GetAllFields(target.GetType(), typeof(NetworkBehaviour)))
            {
                if (field.IsSyncVar() && field.IsVisibleField())
                {
                    _syncVarNames.Add(field.Name);
                }
            }

            _syncListDrawer = new SyncListDrawer(serializedObject.targetObject);

            _syncsAnything = SyncsAnything(target);
        }

        /// <summary>
        /// does this type sync anything? otherwise we don't need to show syncInterval
        /// </summary>
        /// <param name="scriptClass"></param>
        /// <returns></returns>
        public static bool SyncsAnything(Object target)
        {
            var scriptClass = target.GetType();

            // check for all SyncVar fields, they don't have to be visible
            foreach (var field in InspectorHelper.GetAllFields(scriptClass, typeof(NetworkBehaviour)))
            {
                if (field.IsSyncVar())
                {
                    return true;
                }
            }

            // has OnSerialize that is not in NetworkBehaviour?
            // then it either has a syncvar or custom OnSerialize. either way
            // this means we have something to sync.
            var method = scriptClass.GetMethod("OnSerialize");
            if (method != null && method.DeclaringType != typeof(NetworkBehaviour))
            {
                return true;
            }

            // SyncObjects are serialized in NetworkBehaviour.OnSerialize, which
            // is always there even if we don't use SyncObjects. so we need to
            // search for SyncObjects manually.
            // Any SyncObject should be added to syncObjects when unity creates an
            // object so we can cheeck length of list so see if sync objects exists
            var syncObjectsField = scriptClass.GetField("syncObjects", BindingFlags.NonPublic | BindingFlags.Instance);
            var syncObjects = (List<ISyncObject>)syncObjectsField.GetValue(target);

            return syncObjects.Count > 0;
        }

        /// <summary>
        /// Draws Sync Objects that are IEnumerable
        /// </summary>
        public void DrawDefaultSyncLists()
        {
            _syncListDrawer?.Draw();
        }

        /// <summary>
        /// Draws SyncSettings if the NetworkBehaviour has anything to sync
        /// </summary>
        public void DrawDefaultSyncSettings()
        {
            // does it sync anything? then show extra properties
            // (no need to show it if the class only has Cmds/Rpcs and no sync)
            if (!_syncsAnything)
            {
                return;
            }

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Sync Settings", EditorStyles.boldLabel);

            EditorGUILayout.PropertyField(_serializedObject.FindProperty("syncMode"));
            EditorGUILayout.PropertyField(_serializedObject.FindProperty("syncInterval"));

            // apply
            _serializedObject.ApplyModifiedProperties();
        }

#if USE_UI_TOOLKIT
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
#endif // USE_UI_TOOLKIT
    }

    /// <summary>
    /// Draws sync lists in inspector
    /// <para>because synclists are not serailzied by unity they will not show up in the default inspector, so we need a custom draw to draw them</para>
    /// </summary>
    public class SyncListDrawer
    {
        private readonly Object _targetObject;
        private readonly List<SyncListField> _syncListFields;

        public SyncListDrawer(Object targetObject)
        {
            _targetObject = targetObject;
            _syncListFields = new List<SyncListField>();
            foreach (var field in InspectorHelper.GetAllFields(targetObject.GetType(), typeof(NetworkBehaviour)))
            {
                if (field.IsSyncObject() && field.IsVisibleSyncObject())
                {
                    _syncListFields.Add(new SyncListField(field));
                }
            }
        }

        public void Draw()
        {
            if (_syncListFields.Count == 0) { return; }

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Sync Lists", EditorStyles.boldLabel);

            for (var i = 0; i < _syncListFields.Count; i++)
            {
                DrawSyncList(_syncListFields[i]);
            }
        }

        private void DrawSyncList(SyncListField syncListField)
        {
            syncListField.visible = EditorGUILayout.Foldout(syncListField.visible, syncListField.label, true);
            if (syncListField.visible)
            {
                EditorGUILayout.BeginVertical("OL box");
                int count = 0;
                var fieldValue = syncListField.field.GetValue(_targetObject);
                if (fieldValue is IEnumerable synclist)
                {
                    var index = 0;
                    foreach (var item in synclist)
                    {
                        var itemValue = item != null ? item.ToString() : "NULL";
                        var itemLabel = "Element " + index;
                        EditorGUILayout.LabelField(itemLabel, itemValue);

                        index++;
                        count++;
                    }
                }

                if (count == 0)
                {
                    EditorGUILayout.LabelField("List is empty");
                }
                EditorGUILayout.EndVertical();
            }
        }

#if USE_UI_TOOLKIT
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
#endif // USE_UI_TOOLKIT

        private class SyncListField
        {
            public bool visible;
            public readonly FieldInfo field;
            public readonly string label;
            public readonly List<object> items;

            public SyncListField(FieldInfo field)
            {
                this.field = field;
                visible = false;
                label = field.Name + "  [" + field.FieldType.Name + "]";
                items = new List<object>();
            }

            public void UpdateItems(object target)
            {
                var fieldValue = field.GetValue(target);
                if (fieldValue is IEnumerable syncList)
                {
                    items.Clear();

                    foreach (var item in syncList)
                    {
                        items.Add(item);
                    }
                }
            }
        }
    }
}
