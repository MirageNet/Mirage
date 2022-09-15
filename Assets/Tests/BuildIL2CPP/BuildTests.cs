using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Mirage.Tests.PlayerTests;
using UnityEditor;
using UnityEditor.TestTools.TestRunner.Api;
using Debug = UnityEngine.Debug;

namespace Mirage.Tests.BuildIL2CPP
{
    /// <summary>
    /// Builds test scripts with il2cpp.
    /// This will make sure that il2cpp doesn't break for any of the weaver generated code in the test dlls
    ///<para>
    /// this method can be called from command line using `--executeMethod Mirage.Tests.BuildIL2CPP.BuildTests.BuildWithIl2CPP
    ///</para>
    /// </summary>
    public static class BuildTests
    {
        // menu to run this from editor,
        // this should only show up for the mirage project itself, not for people just using mirage (because the asmdef is in the test folder)
        [MenuItem("Build/Build tests with IL2CPP", priority = 1002)]
        public static void BuildWithIl2CPP()
        {
            BuildWithIl2CPP(true);
        }
        [MenuItem("Build/Build tests with IL2CPP (no Clean up)", priority = 1003)]
        public static void BuildWithIl2CPP_NoClean()
        {
            BuildWithIl2CPP(false);
        }

        public static void BuildWithIl2CPP(bool cleanup)
        {
            var hasErrors = false;

            var startTime = DateTime.Now;
            Debug.Log("Start BuildWithIl2CPP");

            var target = EditorUserBuildSettings.activeBuildTarget;
            var targetFolder = "./Temp/TestPlayerBuild";
            var exePath = $"{targetFolder}/mirage.exe";

            var runner = UnityEngine.ScriptableObject.CreateInstance<TestRunnerApi>();
            try
            {
                using (new IL2CPPApplier(target, true))
                {
                    var filter = new Filter
                    {
                        targetPlatform = target,
                        testMode = TestMode.PlayMode,
                    };
                    var testSettings = new ExecutionSettings
                    {
                        overloadTestRunSettings = GetRunSettings(exePath),
                        filters = new Filter[] { filter },
                        // sync so that IL2CPPApplier will revert after
                        runSynchronously = true,
                    };

                    var group = BuildPipeline.GetBuildTargetGroup(target);
                    using (var buildDefines = new CustomBuildDefines(group))
                    {
                        buildDefines.AddDefine("MIRAGE_TESTS");

                        using (var logCatcher = new LogErrorChecker())
                        {
                            runner.Execute(testSettings);

                            hasErrors = logCatcher.HasErrors;
                        }
                    }
                }
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(runner);
                if (cleanup)
                    CleanUpBuildFolder(targetFolder);
            }
            var duration = DateTime.Now - startTime;
            Debug.Log($"End BuildWithIl2CPP duration:{duration.TotalSeconds:0.0}s");

            if (hasErrors)
            {
                // throw so it fails in both editor and CI
                throw new Exception("BuildWithIl2CPP failed");
            }
        }

        private static void CleanUpBuildFolder(string targetFolder)
        {
            // try catch, because it is ok if this fails, but we also want to log the failure
            try
            {
                if (Directory.Exists(targetFolder))
                {
                    Directory.Delete(targetFolder, true);
                }
                Debug.Log($"Deleted Temp folder {targetFolder}");
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogException(e);
            }
        }

        private static ITestRunSettings GetRunSettings(string targetPath)
        {
            // reflection because that is the only way to create buildOnly only with TestRunnerApi
            var assembly = Assembly.Load("UnityEditor.TestRunner");
            var typeName = "UnityEditor.TestTools.TestRunner.PlayerLauncherTestRunSettings";
            var runSettings = assembly.CreateInstance(typeName);
            runSettings.GetType().GetProperty("buildOnly" +
                "").SetValue(runSettings, true);
            runSettings.GetType().GetProperty("buildOnlyLocationPath").SetValue(runSettings, targetPath);

            return (ITestRunSettings)runSettings;
        }
    }

    // code from: https://github.com/James-Frowen/NetworkingBuildWindow
    internal class IL2CPPApplier : IDisposable
    {
        private BuildTargetGroup group;
        private ScriptingImplementation? startingBackend;

        public IL2CPPApplier(BuildTarget target, bool il2cpp)
        {
            UnityEngine.Debug.Log("Apply IL2CPP");
            group = BuildPipeline.GetBuildTargetGroup(target);
            var backend = PlayerSettings.GetScriptingBackend(group);
            if (il2cpp && backend != ScriptingImplementation.IL2CPP)
            {
                startingBackend = backend;
                PlayerSettings.SetScriptingBackend(group, ScriptingImplementation.IL2CPP);
            }
            else if (!il2cpp && backend == ScriptingImplementation.IL2CPP)
            {
                startingBackend = backend;
                PlayerSettings.SetScriptingBackend(group, ScriptingImplementation.Mono2x);
            }

        }
        public void Dispose()
        {
            if (startingBackend.HasValue)
            {
                UnityEngine.Debug.Log("Revert IL2CPP");
                PlayerSettings.SetScriptingBackend(group, startingBackend.Value);
            }
        }
    }

    // code from: https://github.com/James-Frowen/NetworkingBuildWindow
    public class CustomBuildDefines : IDisposable
    {
        private readonly string symbols;
        private readonly BuildTargetGroup target;

        public CustomBuildDefines(BuildTargetGroup target = BuildTargetGroup.Standalone)
        {
            this.target = target;
            symbols = PlayerSettings.GetScriptingDefineSymbolsForGroup(target);
        }

        public void AddDefine(string toAdd)
        {
            Debug.Log($"adding defines: {toAdd}");
            var defines = symbols.Split(';');
            var newDefines = toAdd.Split(';');

            var buildDefines = new HashSet<string>(defines);
            buildDefines.UnionWith(newDefines);

            var defineString = string.Join(";", buildDefines);
            PlayerSettings.SetScriptingDefineSymbolsForGroup(target, defineString);
        }
        public void RemoveDefine(string toRemove)
        {
            Debug.Log($"removing defines: {toRemove}");
            var defines = symbols.Split(';');
            var badDefines = toRemove.Split(';');

            var buildDefines = new HashSet<string>(defines);
            foreach (var bad in badDefines)
            {
                buildDefines.Remove(bad);
            }

            var defineString = string.Join(";", buildDefines);
            PlayerSettings.SetScriptingDefineSymbolsForGroup(target, defineString);
        }

        public void Dispose() => AfterBuild();

        public void AfterBuild()
        {
            Debug.Log("reseting defines");
            PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.Standalone, symbols);
        }
    }
}
