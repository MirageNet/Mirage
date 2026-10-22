using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Mirage
{
    [CustomPropertyDrawer(typeof(RequiredFieldAttribute))]
    // UIToolkit used in 2022+, see RequiredFieldDrawerUIToolkit.cs
    public partial class RequiredFieldDrawer : PropertyDrawer
    {
        private const float DEFAULT_HELPBOX_HEIGHT = 28f;
        private const float SPACING = 2f;
        // space taken by the error/warning icon, text wraps in the remaining width
        private const float HELPBOX_ICON_WIDTH = 40f;
        // IMGUI repaints many times a second, so only search for a missing component once per interval
        private const double AUTO_FIND_INTERVAL = 1.0;

        // static and keyed by target and field, so the limit doesn't depend on when unity creates or reuses drawer instances
        private static readonly Dictionary<(int, FieldInfo), double> lastAutoFindTime = new Dictionary<(int, FieldInfo), double>();

        private RequiredFieldAttribute RequiredAttribute => (RequiredFieldAttribute)attribute;

        // GetPropertyHeight isn't given a rect, so store the width from the last repaint to know where the message will wrap
        private float _drawnWidth;

        public static void CheckAutoFind(RequiredFieldAttribute attr, SerializedProperty property, FieldInfo fieldInfo, bool rateLimit)
        {
            if (!attr.AutoFind || property.propertyType != SerializedPropertyType.ObjectReference || property.objectReferenceValue != null)
                return;

            if (property.serializedObject.isEditingMultipleObjects)
                return;

            if (property.serializedObject.targetObject is Component targetComp && fieldInfo != null)
            {
                var targetType = fieldInfo.FieldType;
                if (!typeof(Component).IsAssignableFrom(targetType))
                    return;

                if (rateLimit && IsAutoFindRateLimited(targetComp, fieldInfo))
                    return;

                if (targetComp.TryGetComponent(targetType, out var found))
                {
                    property.objectReferenceValue = found;
                    property.serializedObject.ApplyModifiedProperties();
                }
            }
        }

        private static bool IsAutoFindRateLimited(Component target, FieldInfo fieldInfo)
        {
            var key = (target.GetInstanceID(), fieldInfo);
            var now = EditorApplication.timeSinceStartup;
            if (lastAutoFindTime.TryGetValue(key, out var lastTime) && now - lastTime < AUTO_FIND_INTERVAL)
                return true;

            lastAutoFindTime[key] = now;
            return false;
        }

        public static bool IsUnassigned(SerializedProperty property)
        {
            if (property.propertyType == SerializedPropertyType.ObjectReference)
                return property.objectReferenceValue == null;

            if (property.propertyType == SerializedPropertyType.String)
                return string.IsNullOrEmpty(property.stringValue);

            if (property.propertyType == SerializedPropertyType.ExposedReference)
                return property.exposedReferenceValue == null;

            return false;
        }

        public static string GetMessage(RequiredFieldAttribute attr, SerializedProperty property)
        {
            if (!string.IsNullOrEmpty(attr.Message))
                return attr.Message;

            // strings are values, so "a reference to" reads wrong for them
            if (property.propertyType == SerializedPropertyType.String)
                return $"{property.displayName} is required.";

            return $"A reference to {property.displayName} is required.";
        }

        private static MessageType GetMessageType(RequiredType type)
        {
            return type == RequiredType.Error ? MessageType.Error : MessageType.Warning;
        }

        private static float GetHelpBoxHeight(string message, float width)
        {
            if (width <= HELPBOX_ICON_WIDTH)
                return DEFAULT_HELPBOX_HEIGHT;

            var textHeight = EditorStyles.helpBox.CalcHeight(new GUIContent(message), width - HELPBOX_ICON_WIDTH);
            return Mathf.Max(DEFAULT_HELPBOX_HEIGHT, textHeight);
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            // only auto find here, height is calculated before OnGUI so it already knows if the help box is needed
            CheckAutoFind(RequiredAttribute, property, fieldInfo, rateLimit: true);

            var baseHeight = EditorGUI.GetPropertyHeight(property, label, true);
            if (IsUnassigned(property))
            {
                // before the first repaint use the inspector width, next layout will use the real width
                var width = _drawnWidth > 0 ? _drawnWidth : EditorGUIUtility.currentViewWidth;

                var message = GetMessage(RequiredAttribute, property);
                var helpBoxHeight = GetHelpBoxHeight(message, width);
                return baseHeight + helpBoxHeight + SPACING;
            }

            return baseHeight;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            // layout events give a placeholder rect, only repaint has the real width
            if (Event.current.type == EventType.Repaint)
                _drawnWidth = position.width;

            if (IsUnassigned(property))
            {
                var message = GetMessage(RequiredAttribute, property);
                var messageType = GetMessageType(RequiredAttribute.Type);

                // use the space GetPropertyHeight reserved, recalculating here can give a taller box that squashes the field
                var fieldHeight = EditorGUI.GetPropertyHeight(property, label, true);
                var helpBoxHeight = position.height - fieldHeight - SPACING;

                var helpBoxRect = new Rect(position.x, position.y, position.width, helpBoxHeight);
                EditorGUI.HelpBox(helpBoxRect, message, messageType);

                var fieldRect = new Rect(position.x, position.y + helpBoxHeight + SPACING, position.width, fieldHeight);
                EditorGUI.PropertyField(fieldRect, property, label, true);
            }
            else
            {
                EditorGUI.PropertyField(position, property, label, true);
            }
        }
    }
}
