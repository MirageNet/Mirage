using System;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Mirage
{
    [CustomPropertyDrawer(typeof(NetworkedPrefabAttribute))]
    public sealed partial class NetworkedPrefabAttributeDrawer : PropertyDrawer
    {
        private bool _targetObjectHasIdentity = false;
        private bool _targetObjectIsRegistered = false;

        private Object _currentTargetObject;

        private ClientObjectManager[] _clientObjects;

        private const string NOT_OBJECT_REFERENCE_ERROR_FORMAT = "{0} is not a object reference. NetworkedPrefab can only be used on object references";
        private const string INVALID_TYPE_ERROR = "NetworkedPrefab can only be used on GameObjects and Components";

        private const float HELP_BOX_HEIGHT = 40;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            // It's not an object reference, show an error.
            if (property.propertyType != SerializedPropertyType.ObjectReference)
            {
                var r = new Rect(position.x, position.y, position.width, HELP_BOX_HEIGHT);

                EditorGUI.HelpBox(r, string.Format(NOT_OBJECT_REFERENCE_ERROR_FORMAT, property.name), MessageType.Error);
                return;
            }

            // If's not a valid type (GameObject or subclass of component), show an error.
            if (!IsValidType(fieldInfo.FieldType))
            {
                var r = new Rect(position.x, position.y, position.width, HELP_BOX_HEIGHT);

                EditorGUI.HelpBox(r, INVALID_TYPE_ERROR, MessageType.Error);
                return;
            }

            // Stop here if client objects are null. They are fetched in GetPropertyHeight.
            if (_clientObjects == null)
            {
                return;
            }

            var y = position.y;

            // If there's a fix message, show it.
            var fixMessage = GetFixMessage(_targetObjectHasIdentity, _targetObjectIsRegistered);
            if (!string.IsNullOrEmpty(fixMessage))
            {
                var r = new Rect(position.x, position.y, position.width - 63, HELP_BOX_HEIGHT);

                EditorGUI.HelpBox(r, fixMessage, MessageType.Error);
                if (GUI.Button(new Rect(r.width + 3, r.y, 60, HELP_BOX_HEIGHT), "Fix"))
                {
                    Fix(_currentTargetObject, _targetObjectHasIdentity, _targetObjectIsRegistered);
                }

                y += HELP_BOX_HEIGHT + EditorGUIUtility.standardVerticalSpacing;
            }

            // Draw the property.
            EditorGUI.BeginChangeCheck();
            EditorGUI.PropertyField(new Rect(position.x, y, position.width, EditorGUIUtility.singleLineHeight), property, label);
            if (EditorGUI.EndChangeCheck())
            {
                // If the property changed, update the target object.
                Validate(property.objectReferenceValue);
            }
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            // If the type is not an object reference or is not a GameObject or Component, return the height of a help box.
            if (property.propertyType != SerializedPropertyType.ObjectReference || !IsValidType(fieldInfo.FieldType))
            {
                return HELP_BOX_HEIGHT;
            }

            // Validate the target object to update the bool values.
            _clientObjects = FindClientObjectManager();
            Validate(property.objectReferenceValue);

            var height = EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
            // If it has some issue, add the help box height.
            if (!_targetObjectHasIdentity || !_targetObjectIsRegistered)
            {
                height += HELP_BOX_HEIGHT + EditorGUIUtility.standardVerticalSpacing;
            }

            return height;
        }

        /// <summary>
        ///     Validates the target object and updates the state about the target object.
        /// </summary>
        /// <param name="obj">The object to check.</param>
        private void Validate(Object obj)
        {
            // If the object is null, there's no identity to check. Just set everything to false.
            if (obj == null)
            {
                _targetObjectHasIdentity = false;
                _targetObjectIsRegistered = false;
                _currentTargetObject = null;
                return;
            }

            NetworkIdentity identity = null;

            // Check for the identity on the object itself.
            if (obj is GameObject go)
            {
                identity = go.GetComponent<NetworkIdentity>();
            }
            else if (obj is Component c)
            {
                identity = c.GetComponent<NetworkIdentity>();
            }

            // Update the has identity bool depending on if the identity is null or not.
            _targetObjectHasIdentity = identity != null;

            // If the identity exists, check if it's been registered on all client object managers.
            // Else we can just assume it's not registered.
            if (_targetObjectHasIdentity)
            {
                _targetObjectIsRegistered = true;

                for (var i = 0; i < _clientObjects.Length; i++)
                {
                    if (!_clientObjects[i].spawnPrefabs.Contains(identity))
                    {
                        // The identity is not registered on this client object manager.
                        // Therefore, the target object is not registered.
                        _targetObjectIsRegistered = false;
                        break;
                    }
                }
            }
            else
            {
                _targetObjectIsRegistered = false;
            }

            _currentTargetObject = obj;
        }

        /// <summary>
        ///     Fixes the target object by adding a network behavior component and registering it on all found client object
        ///     managers.
        /// </summary>
        /// <param name="targetObject">The object to fix.</param>
        /// <param name="hasIdentity">Does the object have a network identity already?</param>
        /// <param name="isRegistered">Is the object registered already?</param>
        private void Fix(Object targetObject, bool hasIdentity, bool isRegistered)
        {
            NetworkIdentity identity = null;

            // If the target object doesn't have a network identity, add one.
            if (!hasIdentity)
            {
                if (targetObject is GameObject go)
                {
                    identity = Undo.AddComponent<NetworkIdentity>(go);
                }
                else if (targetObject is Component c)
                {
                    identity = Undo.AddComponent<NetworkIdentity>(c.gameObject);
                }

                EditorUtility.SetDirty(identity);
            }
            else // Else get the identity.
            {
                if (targetObject is GameObject go)
                {
                    identity = go.GetComponent<NetworkIdentity>();
                }
                else if (targetObject is Component c)
                {
                    identity = c.GetComponent<NetworkIdentity>();
                }
            }

            // If the identity is null, it couldn't be added or fetched for some reason.
            if (identity == null)
            {
                Debug.LogError($"Could not add NetworkIdentity to object {targetObject}.");
                return;
            }

            // If the object is not registered, register it.
            if (!isRegistered)
            {
                // Find all client object managers just to be sure we don't miss any.
                // Force a search to not use the cache, just to be sure.
                _clientObjects = FindClientObjectManager(true);

                for (var i = 0; i < _clientObjects.Length; i++)
                {
                    // If the object is already registered, skip it.
                    // It can happen that it is registered on one client object manager but not on another.
                    if (_clientObjects[i].spawnPrefabs.Contains(identity))
                    {
                        continue;
                    }

                    Undo.RecordObject(_clientObjects[i], "Add networked prefab");

                    // Check if the client object manager is a prefab.
                    var isPrefab = PrefabUtility.IsPartOfPrefabAsset(_clientObjects[i]) || PrefabUtility.IsPartOfPrefabInstance(_clientObjects[i]);

                    // We need to record prefab modifications if the client object manager is a prefab.
                    if (isPrefab)
                    {
                        PrefabUtility.RecordPrefabInstancePropertyModifications(_clientObjects[i]);
                    }

                    // Add the prefab to the spawn prefabs list and mark the object manager as dirty.
                    _clientObjects[i].spawnPrefabs.Add(identity);
                    EditorUtility.SetDirty(_clientObjects[i]);
                }
            }
        }

        /// <summary>
        ///     Finds all ClientObjectManager instances in the scene or the project.
        /// </summary>
        /// <param name="forceSearch">If true, it will not use the cache.</param>
        /// <returns>All the found client object managers.</returns>
        private ClientObjectManager[] FindClientObjectManager(bool forceSearch = false)
        {
            // If we've already found the client object manager, return it.
            // But only if we're not forcing a search.
            if (_clientObjects != null && !forceSearch)
            {
                return _clientObjects;
            }

            // Find the first object manager in the scene.
#if UNITY_2023_1_OR_NEWER
			var sceneObjects = Object.FindObjectsByType<ClientObjectManager>(FindObjectsInactive.Include, FindObjectsSortMode.None);
#else
            var sceneObjects = Object.FindObjectsOfType<ClientObjectManager>();
#endif

            // If there is an object manager, return it.
            if (sceneObjects != null)
            {
                return sceneObjects;
            }

            // There is no object manager in the scene, so find all object managers in the project.
            var prefabObjects = Resources.FindObjectsOfTypeAll<ClientObjectManager>();

            return prefabObjects;
        }

        /// <summary>
        ///     Checks if the type valid for the networked prefab attribute.
        ///     <para>To be valid it must be of a type GameObject or be a subclass of Component.</para>
        /// </summary>
        /// <param name="type">The type to check.</param>
        /// <returns>True if it's a valid type, otherwise false.</returns>
        private static bool IsValidType(Type type)
        {
            return type == typeof(GameObject) || type.IsSubclassOf(typeof(Component));
        }

        /// <summary>
        ///     Returns a proper fix message based on the current state of the target object.
        /// </summary>
        /// <param name="hasIdentity">If the object has a network identity.</param>
        /// <param name="isRegistered">If the object is registered on the network manager.</param>
        /// <returns>A fix message. Returns an empty string if there's no fix required.</returns>
        private static string GetFixMessage(bool hasIdentity, bool isRegistered)
        {
            if (!hasIdentity && !isRegistered)
            {
                return "The object does not have a NetworkIdentity and is not registered in the NetworkManager.";
            }

            if (!hasIdentity)
            {
                return "The object does not have a NetworkIdentity.";
            }

            if (!isRegistered)
            {
                return "The object is not registered in the NetworkManager.";
            }

            return string.Empty;
        }
    }
}
