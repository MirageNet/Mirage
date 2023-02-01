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
                EditorGUI.PropertyField(rect, property, label);
                //valueGUI(rect, property, label);
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

            var value = ScriptableObjectUtility.CreateAsset<NetworkPrefabs>(gameObjectName + "_NetworkPrefabs", "Assets");

            property.objectReferenceValue = value;
            property.serializedObject.ApplyModifiedProperties();
        }

        protected virtual void valueGUI(Rect rect, SerializedProperty property, GUIContent label)
        {
            // todo draw internal list here
            //drawCanEditVar(pos, property, label);
            //drawPropertyField(refWidth, refRect, property, GUIContent.none);
        }
    }
}
