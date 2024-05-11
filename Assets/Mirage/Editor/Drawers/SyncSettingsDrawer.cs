using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Mirage
{
    [CustomPropertyDrawer(typeof(SyncSettings))]
    public class SyncSettingsDrawer : PropertyDrawer
    {
        /// <summary>
        /// no syncvar/syncobject/serailize Override
        /// </summary>
        /// <param name="property"></param>
        /// <returns></returns>
        private static bool NoSyncInBehaviour(SerializedProperty property)
        {
            var targets = property.serializedObject.targetObjects;
            foreach (var target in targets)
            {
                Debug.Assert(target is NetworkBehaviour);

                // show if syncvars
                var syncAny = InspectorHelper.SyncsAnything(target);
                if (syncAny)
                    return false;

                // show if attribute
                var showAttr = target.GetType().GetCustomAttributes<ShowSyncSettingsAttribute>(true);
                if (showAttr != null)
                    return false;
            }
            return true;
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            if (NoSyncInBehaviour(property))
                return 0;

            var baseHeight = base.GetPropertyHeight(property, label);
            if (property.isExpanded)
            {
                var height = (baseHeight * 3) + (EditorGUIUtility.standardVerticalSpacing * 2);

                // Check if the sync is invalid and increase height if it is
                if (InvalidDirection(property, out _))
                {
                    height += (EditorGUIUtility.singleLineHeight * 2) + EditorGUIUtility.standardVerticalSpacing; // Increase height to make room for warning message
                }

                return height;
            }
            else
            {
                return baseHeight;
            }
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (NoSyncInBehaviour(property))
                return;

            EditorGUI.BeginProperty(position, label, property);

            // Draw foldout arrow and label
            DrawFoldoutArrowAndLabel(position, property, label);

            if (property.isExpanded)
            {
                DrawFields(position, property);

                DisplayWarningMessageIfSyncIsInvalid(position, property);
            }

            EditorGUI.EndProperty();
        }

        private void DrawFoldoutArrowAndLabel(Rect position, SerializedProperty property, GUIContent label)
        {
            var foldoutRect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
            property.isExpanded = EditorGUI.BeginFoldoutHeaderGroup(foldoutRect, property.isExpanded, label);
            EditorGUI.EndFoldoutHeaderGroup();
        }

        private void DrawFields(Rect position, SerializedProperty property)
        {
            // Calculate rects
            var labelWidth = EditorGUIUtility.labelWidth;
            var fieldWidth = (position.width - labelWidth) / 2;
            var yOffset = EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing; // Offset to move fields down by one line

            var directionLabelRect = new Rect(position.x, position.y + yOffset, labelWidth, EditorGUIUtility.singleLineHeight);
            var fromRect = new Rect(position.x + labelWidth, position.y + yOffset, fieldWidth, EditorGUIUtility.singleLineHeight);
            var toRect = new Rect(position.x + labelWidth + fieldWidth, position.y + yOffset, fieldWidth, EditorGUIUtility.singleLineHeight);

            // Check if the sync is invalid and increase yOffset if it is
            if (InvalidDirection(property, out _))
            {
                yOffset += (EditorGUIUtility.singleLineHeight * 2) + EditorGUIUtility.standardVerticalSpacing; // Increase yOffset to move fields down by an additional line
            }

            var intervalLabelRect = new Rect(position.x, position.y + yOffset + EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing, labelWidth, EditorGUIUtility.singleLineHeight);
            var timingRect = new Rect(position.x + labelWidth + fieldWidth, position.y + yOffset + EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing, fieldWidth, EditorGUIUtility.singleLineHeight);
            var intervalRect = new Rect(position.x + labelWidth, position.y + yOffset + EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing, fieldWidth, EditorGUIUtility.singleLineHeight);

            const int FromLabelWidth = 30;
            const int ToLabelWidth = 18;

            // Calculate rects for the new labels
            var fromLabelRect = new Rect(fromRect.x, fromRect.y, FromLabelWidth, EditorGUIUtility.singleLineHeight);
            var toLabelRect = new Rect(toRect.x, toRect.y, ToLabelWidth, EditorGUIUtility.singleLineHeight);

            // Adjust the position and size of the From and To fields
            fromRect.x += FromLabelWidth;
            fromRect.width -= FromLabelWidth;

            toRect.x += ToLabelWidth;
            toRect.width -= ToLabelWidth;

            // main labels
            // indent main labels, but not the ones that are next to fields (or they will me shifted/hidden by field)
            EditorGUI.indentLevel++;
            EditorGUI.LabelField(directionLabelRect, "Direction");
            var intervalLabel = new GUIContent("Interval", tooltip: SyncSettings.INTERVAL_TOOLTIP);
            EditorGUI.LabelField(intervalLabelRect, intervalLabel);
            EditorGUI.indentLevel--;

            // Draw the new labels
            EditorGUI.LabelField(fromLabelRect, "From");
            EditorGUI.LabelField(toLabelRect, "To");

            // Draw direction label and fields
            var fromProperty = property.FindPropertyRelative("From");
            fromProperty.intValue = (int)(SyncFrom)EditorGUI.EnumFlagsField(fromRect, GUIContent.none, (SyncFrom)fromProperty.intValue);
            var toProperty = property.FindPropertyRelative("To");
            toProperty.intValue = (int)(SyncTo)EditorGUI.EnumFlagsField(toRect, GUIContent.none, (SyncTo)toProperty.intValue);

            // Draw interval label and fields
            EditorGUI.PropertyField(timingRect, property.FindPropertyRelative("Timing"), GUIContent.none);
            // disable the Interval box if NoInterval is set, and show 0 instead
            var guiEnabled = GUI.enabled;
            if ((SyncTiming)property.FindPropertyRelative("Timing").intValue == SyncTiming.NoInterval)
            {
                GUI.enabled = false;
                _ = EditorGUI.IntField(intervalRect, GUIContent.none, 0);
            }
            else
            {
                EditorGUI.PropertyField(intervalRect, property.FindPropertyRelative("Interval"), GUIContent.none);
            }
            GUI.enabled = guiEnabled;

        }

        private void DisplayWarningMessageIfSyncIsInvalid(Rect position, SerializedProperty property)
        {
            if (InvalidDirection(property, out var reason))
            {
                // Calculate warning message rect
                var yOffset = EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing; // Offset to move warning message down by one line
                var warningRect = new Rect(position.x, position.y + yOffset + EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing, position.width, EditorGUIUtility.singleLineHeight * 2);

                // Draw warning message
                EditorGUI.HelpBox(warningRect, reason, MessageType.Warning);
            }
        }

        private static bool InvalidDirection(SerializedProperty property, out string reason)
        {
            var from = (SyncFrom)property.FindPropertyRelative("From").intValue;
            var to = (SyncTo)property.FindPropertyRelative("To").intValue;
            reason = SyncSettings.InvalidReason(from, to);
            return !string.IsNullOrEmpty(reason);
        }
    }
}
