#if UNITY_EDITOR
using System.IO;
using UnityEngine;

namespace Mirage
{
    /// <summary>
    /// Used for saving and loading project settings.
    /// <b>ONLY USED IN THE EDITOR!</b>
    /// </summary>
    internal static class ProjectSettingsHelper
    {
        public static void Save<T>(T settings, string path) where T : ScriptableObject
        {
            if (!Directory.Exists(MirageProjectSettings.ROOT_FOLDER))
            {
                Directory.CreateDirectory(MirageProjectSettings.ROOT_FOLDER);
            }

            try
            {
                UnityEditorInternal.InternalEditorUtility.SaveToSerializedFileAndForget(new Object[] { settings }, path, true);
            }
            catch (System.Exception ex)
            {
                Debug.LogError("Could not save project settings!\n" + ex);
            }
        }

        public static T Load<T>(string path) where T : ScriptableObject
        {
            T settings;

            try
            {
                settings = (T)UnityEditorInternal.InternalEditorUtility.LoadSerializedFileAndForget(path)[0];
            }
            catch (System.Exception)
            {
                Debug.LogError("Could not load project settings. Settings will be reset.");
                settings = null;
            }

            if (settings == null)
            {
                RemoveFile(path);
                settings = ScriptableObject.CreateInstance<T>();
                Save(settings, path);
            }

            return settings;
        }

        private static void RemoveFile(string path)
        {
            if (!File.Exists(path))
            {
                return;
            }

            FileAttributes attributes = File.GetAttributes(path);
            if (attributes.HasFlag(FileAttributes.ReadOnly))
            {
                File.SetAttributes(path, attributes & ~FileAttributes.ReadOnly);
            }

            File.Delete(path);
        }
    }
}
#endif
