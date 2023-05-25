using Mirage.EditorScripts.Logging;
using UnityEditor;
using UnityEngine;

namespace Mirage
{
    [CustomPropertyDrawer(typeof(NetworkPrefabs))]
    public class NetworkPrefabsPropertyDrawer : PropertyDrawer
    {
        private const float REF_WIDTH = 0.3f;
        private const float LABEL_WIDTH = 0.4f;
        private const float BUTTON_WIDTH = 0.3f;

        public override void OnGUI(Rect rect, SerializedProperty property, GUIContent label)
        {
            if (property.objectReferenceValue == null)
            {
                noValueGUI(rect, property, label);
            }
            else
            {
                valueGUI(rect, property, label);
            }
        }

        private void noValueGUI(Rect rect, SerializedProperty property, GUIContent label)
        {
            var x = rect.x;
            var refWidth = rect.width * REF_WIDTH;
            var labelWidth = rect.width * LABEL_WIDTH;
            var buttonWidth = rect.width * BUTTON_WIDTH;

            EditorGUI.LabelField(new Rect(x, rect.y, labelWidth, rect.height), label);
            x += labelWidth;

            var createNew = GUI.Button(new Rect(x, rect.y, buttonWidth, rect.height), "Create New");
            x += buttonWidth;

            if (createNew)
            {
                createNewValue(property);
            }

            EditorGUI.BeginChangeCheck();
            EditorGUI.PropertyField(new Rect(x, rect.y, refWidth, rect.height), property, GUIContent.none, false);
            if (EditorGUI.EndChangeCheck())
            {
                property.serializedObject.ApplyModifiedProperties();
            }
        }

        private void createNewValue(SerializedProperty property)
        {
            // name of gameobject that this property is on
            var gameObjectName = property.serializedObject.targetObject.name;

            var value = ScriptableObjectUtility.CreateAsset<NetworkPrefabs>("NetworkPrefabs_" + gameObjectName, "Assets");
            NetworkPrefabsCache.ClearCache();

            property.objectReferenceValue = value;
            property.serializedObject.ApplyModifiedProperties();
            GUIUtility.ExitGUI();
        }

        private void valueGUI(Rect rect, SerializedProperty property, GUIContent label)
        {
            var labelHeight = EditorGUIUtility.singleLineHeight;
            var labelWidth = EditorGUIUtility.labelWidth;
            var labelRect = new Rect(rect.x, rect.y, labelWidth, labelHeight);

            var referenceRect = new Rect(rect.x + labelWidth, rect.y, rect.width - labelWidth, labelHeight);
            EditorGUI.LabelField(labelRect, label);

            EditorGUI.BeginChangeCheck();
            EditorGUI.PropertyField(referenceRect, property, GUIContent.none);
            if (EditorGUI.EndChangeCheck())
                property.serializedObject.ApplyModifiedProperties();

            // value might have changed, so return if now null
            if (property.objectReferenceValue == null)
                return;

            var listY = rect.y + labelHeight + EditorGUIUtility.standardVerticalSpacing;
            var listRect = new Rect(rect.x, listY, rect.width, rect.height - listY);
            DrawList(listRect, property);
        }

        private static void DrawList(Rect rect, SerializedProperty property)
        {
            var obj = new SerializedObject(property.objectReferenceValue);
            var prop = obj.FindProperty(nameof(NetworkPrefabs.Prefabs));

            EditorGUI.BeginChangeCheck();

            EditorGUI.indentLevel++;
            EditorGUI.PropertyField(rect, prop, true);
            EditorGUI.indentLevel--;

            if (EditorGUI.EndChangeCheck())
                obj.ApplyModifiedProperties();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            if (property.objectReferenceValue == null)
            {
                return EditorGUI.GetPropertyHeight(property, label);
            }
            else
            {
                var height = 0f;
                height += EditorGUI.GetPropertyHeight(property, label);

                // spacing between foldout and list
                height += EditorGUIUtility.standardVerticalSpacing;

                if (property.objectReferenceValue != null)
                {
                    var obj = new SerializedObject(property.objectReferenceValue);
                    var prop = obj.FindProperty(nameof(NetworkPrefabs.Prefabs));
                    height += EditorGUI.GetPropertyHeight(prop, true);
                }

                // spacing after list
                height += EditorGUIUtility.standardVerticalSpacing;

                return height;
            }
        }
    }
}
