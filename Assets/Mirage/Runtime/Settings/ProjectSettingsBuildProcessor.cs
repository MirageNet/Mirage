// Must be in runtime folder because it needs to be accessed at runtime in the Unity editor.

#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace Mirage.Settings
{
    internal class ProjectSettingsBuildProcessor : IPreprocessBuildWithReport, IPostprocessBuildWithReport
    {
        int IOrderedCallback.callbackOrder { get { return -10000; } }

        private bool createdResources = false;
        private bool createdPackageFolder = false;

        List<ScriptableObject> objects;
        List<string> names;

        public static event Action<List<ScriptableObject>, List<string>> OnBuild;

        void IPreprocessBuildWithReport.OnPreprocessBuild(BuildReport report)
        {
            objects = new List<ScriptableObject>();
            names = new List<string>();

            OnBuild?.Invoke(objects, names);

            createdResources = false;
            createdPackageFolder = false;

            for (int i = 0; i < objects.Count; i++)
            {
                if (File.Exists($"{SettingsLoader.SettingsFolder}/{names[i]}.asset"))
                {
                    ScriptableObject setting = objects[i];

                    if (!Directory.Exists($"{Application.dataPath}/Resources"))
                    {
                        createdResources = true;
                        Directory.CreateDirectory($"{Application.dataPath}/Resources");
                    }

                    if (!Directory.Exists($"{Application.dataPath}/Resources/{SettingsLoader.PACKAGE_NAME}"))
                    {
                        createdPackageFolder = true;
                        Directory.CreateDirectory($"{Application.dataPath}/Resources/{SettingsLoader.PACKAGE_NAME}");
                    }

                    setting.hideFlags = HideFlags.None;
                    AssetDatabase.CreateAsset(setting, $"Assets/Resources/{SettingsLoader.PACKAGE_NAME}/{names[i]}.asset");
                    AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
                }
            }
        }

        void IPostprocessBuildWithReport.OnPostprocessBuild(BuildReport report)
        {
            string resourcesPath = $"{Application.dataPath}/Resources";
            string packagePath = $"{resourcesPath}/{SettingsLoader.PACKAGE_NAME}";

            if (createdResources && createdPackageFolder && Directory.Exists(packagePath))
            {
                Directory.Delete(resourcesPath, true);
                if (File.Exists($"{resourcesPath}.meta"))
                {
                    File.Delete($"{resourcesPath}.meta");
                }
            }

            if (!createdResources && createdPackageFolder)
            {
                Directory.Delete(packagePath, true);
                if (File.Exists($"{packagePath}.meta"))
                {
                    File.Delete($"{packagePath}.meta");
                }
            }

            if (!createdResources && !createdPackageFolder)
            {
                for (int i = 0; i < names.Count; i++)
                {
                    string path = $"{packagePath}/{names[i]}.asset";
                    if (File.Exists(path))
                    {
                        File.Delete(path);
                        string meta = $"{packagePath}/{names[i]}.asset.meta";
                        if (File.Exists(meta))
                        {
                            File.Delete(meta);
                        }
                    }
                }
            }

            AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);

            objects = null;
            names = null;
        }
    }
}
#endif
