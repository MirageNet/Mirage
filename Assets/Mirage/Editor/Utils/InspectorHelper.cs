using System;
using System.Collections.Generic;
using System.Reflection;
using Mirage.Collections;
using UnityEngine;

namespace Mirage
{
    public static class InspectorHelper
    {
        /// <summary>
        /// Gets all public and private fields for a type
        /// </summary>
        /// <param name="type"></param>
        /// <param name="deepestBaseType">Stops at this base type (exclusive)</param>
        /// <returns></returns>
        public static IEnumerable<FieldInfo> GetAllFields(Type type, Type deepestBaseType)
        {
            const BindingFlags publicFields = BindingFlags.Public | BindingFlags.Instance;
            const BindingFlags privateFields = BindingFlags.NonPublic | BindingFlags.Instance;

            // get public fields (includes fields from base type)
            var allPublicFields = type.GetFields(publicFields);
            foreach (var field in allPublicFields)
            {
                yield return field;
            }

            // get private fields in current type, then move to base type
            while (type != null)
            {
                var allPrivateFields = type.GetFields(privateFields);
                foreach (var field in allPrivateFields)
                {
                    yield return field;
                }

                type = type.BaseType;

                // stop early
                if (type == deepestBaseType)
                {
                    break;
                }
            }
        }

        public static bool IsSyncVar(this FieldInfo field)
        {
            var fieldMarkers = field.GetCustomAttributes(typeof(SyncVarAttribute), true);
            return fieldMarkers.Length > 0;
        }
        public static bool IsSerializeField(this FieldInfo field)
        {
            var fieldMarkers = field.GetCustomAttributes(typeof(SerializeField), true);
            return fieldMarkers.Length > 0;
        }
        public static bool IsVisibleField(this FieldInfo field)
        {
            return field.IsPublic || IsSerializeField(field);
        }

        public static bool IsSyncObject(this FieldInfo field)
        {
            return typeof(ISyncObject).IsAssignableFrom(field.FieldType);
        }
        public static bool HasShowInInspector(this FieldInfo field)
        {
            var fieldMarkers = field.GetCustomAttributes(typeof(ShowInInspectorAttribute), true);
            return fieldMarkers.Length > 0;
        }
        public static bool IsVisibleSyncObject(this FieldInfo field)
        {
            return field.IsPublic || HasShowInInspector(field);
        }

        /// <summary>
        /// does this type sync anything? otherwise we don't need to show syncInterval
        /// </summary>
        /// <param name="scriptClass"></param>
        /// <returns></returns>
        public static bool SyncsAnything(UnityEngine.Object target)
        {
            var scriptClass = target.GetType();

            // check for all SyncVar fields, they don't have to be visible
            foreach (var field in GetAllFields(scriptClass, typeof(NetworkBehaviour)))
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
    }
}
