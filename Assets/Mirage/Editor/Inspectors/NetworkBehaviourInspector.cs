using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Mirage.Logging;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Mirage
{
#if !EXCLUDE_NETWORK_BEHAVIOUR_INSPECTOR
    [CustomEditor(typeof(NetworkBehaviour), true)]
    [CanEditMultipleObjects]
#endif
    // UITookit used in 2022+, see NetworkBehaviourInspectorUIToolkit.cs
    public partial class NetworkBehaviourInspector : Editor
    {
        private static readonly ILogger logger = LogFactory.GetLogger(typeof(NetworkBehaviourInspector));
        private SyncListDrawer _syncListDrawer;

        private void OnEnable()
        {
            if (target == null) { logger.LogWarning("NetworkBehaviourInspector had no target object"); return; }

            // If target's base class is changed from NetworkBehaviour to MonoBehaviour
            // then Unity temporarily keep using this Inspector causing things to break
            if (!(target is NetworkBehaviour)) { return; }

            _syncListDrawer = new SyncListDrawer(serializedObject.targetObject);
        }

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            _syncListDrawer.Draw();
        }
    }

    /// <summary>
    /// Draws sync lists in inspector
    /// <para>because synclists are not serailzied by unity they will not show up in the default inspector, so we need a custom draw to draw them</para>
    /// </summary>
    public partial class SyncListDrawer
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
                    var fieldValue = field.GetValue(_targetObject);
                    // only draw SyncObjects that are IEnumerable
                    if (fieldValue is IEnumerable)
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
            if (!syncListField.visible)
                return;

            EditorGUILayout.BeginVertical("OL box");
            syncListField.UpdateItems(_targetObject);
            var count = syncListField.items.Count;
            for (var i = 0; i < count; i++)
            {
                var item = syncListField.items[i];
                var itemValue = item != null ? item.ToString() : "NULL";
                var itemLabel = "Element " + i;
                EditorGUILayout.LabelField(itemLabel, itemValue);
            }

            if (count == 0)
            {
                EditorGUILayout.LabelField("List is empty");
            }
            EditorGUILayout.EndVertical();
        }

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
