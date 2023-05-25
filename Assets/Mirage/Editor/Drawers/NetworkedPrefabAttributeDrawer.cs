using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Mirage
{
    [CustomPropertyDrawer(typeof(NetworkedPrefabAttribute))]
    public sealed partial class NetworkedPrefabAttributeDrawer : PropertyDrawer
    {
        private static class Icons
        {
            public const string INVALID = "Invalid@2x";
            public const string ERR0R = "Error@2x";
            public const string WARN = "Warning@2x";
            public const string OK = "Installed@2x";
        }

        private static readonly StringBuilder builder = new StringBuilder();

        /// <summary>
        /// Draws error icon in square box (using rect from <paramref name="rect"/>) and returns remaining rect
        /// </summary>
        /// <param name="rect"></param>
        /// <param name="tooltip"></param>
        /// <returns></returns>
        /// <remarks>for default icons <see href="https://github.com/halak/unity-editor-icons"/></remarks>
        private static Rect DrawIcon(Rect rect, string iconType, string tooltip)
        {
            // Must have the pipe | first
            var rawIcon = EditorGUIUtility.IconContent(iconType);

            var content = new GUIContent(rawIcon);
            content.tooltip = $"{tooltip}";

            var size = rect.height;
            // sqaure at right side of box
            var sqaure = new Rect(rect.xMax - size, rect.y, size, size);
            EditorGUI.LabelField(sqaure, content);

            rect.width -= size;
            return rect;
        }

        /// <summary>
        /// Draws a button that fits <paramref name="text"/> and returns the remaining size
        /// </summary>
        /// <param name="position"></param>
        /// <param name="text"></param>
        /// <param name="remaining"></param>
        /// <returns></returns>
        private static bool DrawButton(Rect position, string text, out Rect remaining)
        {
            var content = new GUIContent(text);

            var size = EditorStyles.label.CalcSize(content);
            var width = size.x + 10;//10 padding
            var buttonRect = new Rect(position.xMax - width, position.y, width, position.height);

            remaining = position;
            remaining.width -= width;

            return GUI.Button(buttonRect, content);
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (InvalidFieldType(property))
            {
                var remaining = DrawIcon(position, Icons.INVALID, "Invalid field type. Must be GameObject or Component");
                EditorGUI.PropertyField(remaining, property, label);
                return;
            }

            // if field null, nothing to check
            if (property.objectReferenceValue == null)
            {
                EditorGUI.PropertyField(position, property, label);
                return;
            }

            var obj = property.objectReferenceValue;
            var gameObject = GetGameObject(obj);

            // if object is in scene, nothing to check
            if (gameObject.scene.IsValid())
            {
                var remaining = DrawIcon(position, Icons.ERR0R, "Scene object can not be Network Prefab");
                EditorGUI.PropertyField(remaining, property, label);
                return;
            }

            if (!gameObject.TryGetComponent<NetworkIdentity>(out var identity))
            {
                var remaining = DrawNoIdentityGui(position, gameObject);
                EditorGUI.PropertyField(remaining, property, label);
                return;
            }

            // add scope here because "remaining" is used in earlier if scopes
            {
                // check if prefab is in NetworkPrefabs
                var remaining = CheckPrefabIsRegistered(position, identity);
                EditorGUI.PropertyField(remaining, property, label);
            }
        }

        private bool InvalidFieldType(SerializedProperty property)
        {
            // not a referecne, invalid
            if (property.propertyType != SerializedPropertyType.ObjectReference)
                return true;

            var fieldType = fieldInfo.FieldType;

            // GameObject, valid
            if (fieldType == typeof(GameObject))
                return false;

            // Component, valid
            if (fieldType.IsSubclassOf(typeof(Component)))
                return false;

            return true;
        }

        private static GameObject GetGameObject(Object obj)
        {
            if (obj is GameObject go)
                return go;
            else if (obj is Component comp)
                return comp.gameObject;
            else
                throw new System.ArgumentException($"object was not GameObject or Component");
        }

        private static Rect DrawNoIdentityGui(Rect position, GameObject gameObject)
        {
            var remaining = DrawIcon(position, Icons.ERR0R, "Prefab does not have Network Identity");
            if (DrawButton(remaining, "Add Identity", out remaining))
            {
                Undo.AddComponent<NetworkIdentity>(gameObject);
            }

            return remaining;
        }

        /// <summary>
        /// Checks if prefab is in <see cref="NetworkPrefabs"/> list, If not shows fix or warning button. Will returns left over rect to draw field
        /// </summary>
        /// <param name="position"></param>
        /// <param name="gameObject"></param>
        /// <returns></returns>
        /// <exception cref="System.NotImplementedException"></exception>
        private static Rect CheckPrefabIsRegistered(Rect position, NetworkIdentity prefab)
        {
            var allHolders = NetworkPrefabsCache.GetHolders();

            if (allHolders.Count == 0)
            {
                return DrawIcon(position, Icons.WARN, "Could not find NetworkPrefabs ScriptableObject");
            }

            var registeredInAll = allHolders.All(x => x.Prefabs.Contains(prefab));
            var registeredInNone = allHolders.All(x => !x.Prefabs.Contains(prefab));

            builder.Clear();
            bool showRegisterButton;

            if (allHolders.Count == 1)
            {
                var holder = allHolders.First();
                showRegisterButton = CreateOneHolderMessage(holder, registeredInAll);
            }
            else // more than 1 holder
            {
                showRegisterButton = CreateManyHolderMessage(allHolders, prefab, registeredInAll, registeredInNone);
            }

            // make sure to do builder.ToString before drawing button, because we want to re-use builder for dialog message
            var message = builder.ToString();
            var iconType = GetIconType(registeredInAll, registeredInNone);
            var remaining = DrawIcon(position, iconType, message);

            if (showRegisterButton)
            {
                remaining = DrawRegisterButton(remaining, allHolders, prefab);
            }

            builder.Clear();
            return remaining;
        }

        private static Rect DrawRegisterButton(Rect rect, IReadOnlyList<NetworkPrefabs> allHolders, NetworkIdentity prefab)
        {
            if (DrawButton(rect, "Register", out var remaining))
            {
                builder.Clear();
                var holdersWithoutPrefab = allHolders.Where(x => !x.Prefabs.Contains(prefab));
                AppendHolderNames(holdersWithoutPrefab);

                if (EditorUtility.DisplayDialog("Register Prefab with NetworkPrefabs", $"Do you want to add {prefab.name} to:\n{builder}", "Add to all", "Cancel"))
                    Register(allHolders, prefab);
            }

            return remaining;
        }

        private static bool CreateOneHolderMessage(NetworkPrefabs holder, bool registeredInAll)
        {
            if (registeredInAll)
            {
                builder.AppendLine($"Prefab registed with {holder.name}");
                return false;
            }
            else
            {
                builder.AppendLine($"Prefab not registed with {holder.name}");
                return true;
            }
        }

        private static bool CreateManyHolderMessage(IReadOnlyList<NetworkPrefabs> allHolders, NetworkIdentity prefab, bool registeredInAll, bool registeredInNone)
        {
            if (registeredInAll)
            {
                builder.AppendLine("Prefab registed with all NetworkPrefabs:");
                AppendHolderNames(allHolders);
                return false;
            }
            else if (registeredInNone)
            {
                builder.AppendLine("Prefab not registed with any NetworkPrefabs:");
                AppendHolderNames(allHolders);
                return true;
            }
            else // in some
            {
                builder.AppendLine("Prefab registed with some NetworkPrefabs");
                builder.AppendLine("\nRegisted with:");
                AppendHolderNames(allHolders.Where(x => x.Prefabs.Contains(prefab)));
                builder.AppendLine("\nNot Registed with:");
                AppendHolderNames(allHolders.Where(x => !x.Prefabs.Contains(prefab)));

                return true;
            }
        }

        private static void AppendHolderNames(IEnumerable<NetworkPrefabs> allHolders)
        {
            foreach (var holder in allHolders)
            {
                builder.AppendLine($" - {holder.name}");
            }
        }

        private static string GetIconType(bool registeredInAll, bool registeredInNone)
        {
            string iconType;
            if (registeredInAll) iconType = Icons.OK;
            else if (registeredInNone) iconType = Icons.ERR0R;
            else  /*some*/ iconType = Icons.WARN;
            return iconType;
        }

        private static void Register(IEnumerable<NetworkPrefabs> allHolders, NetworkIdentity prefab)
        {
            foreach (var holder in allHolders)
            {
                Undo.RecordObject(holder, $"Adding {prefab.name} to {holder.name}");
                holder.Prefabs.Add(prefab);

                // seems like we need to also mark dirty for non-scene objects
                EditorUtility.SetDirty(holder);
            }
            GUIUtility.ExitGUI();
        }
    }
}
