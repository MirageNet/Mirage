#if UNITY_EDITOR
using System;
using System.IO;
using System.Reflection;
using Mirage.Tests.PlayerTests.BuildScripts;
using UnityEditor;
using UnityEditor.TestTools;
using UnityEditor.TestTools.TestRunner.Api;
using UnityEngine;
using UnityEngine.TestTools;

[assembly: TestPlayerBuildModifier(typeof(MakeRestsReleaseMode))]
[assembly: PostBuildCleanup(typeof(MakeRestsReleaseMode))]

namespace Mirage.Tests.PlayerTests.BuildScripts
{
    public static class BuildReleaseTestPlayer
    {
        // [MenuItem("Build/Build performance tests", priority = 1005)]
        public static void BuildPerformanceTestsUsingRunner()
        {
            BuildTarget target = EditorUserBuildSettings.activeBuildTarget;
            // use Temp folder so unity cleans it up on close
            string targetFolder = "./Temp/TestPlayerBuild";
            string exePath = $"{targetFolder}/mirage.exe";

            // deleta any old build
            CleanUpBuildFolder(targetFolder);

            TestRunnerApi runner = ScriptableObject.CreateInstance<TestRunnerApi>();
            MakeRestsReleaseMode.ApplyReleaseMode = true;
            // Create ExecutionSettings is the same as the settings created by the testrunner gui builds

            var filter = new Filter
            {
                testMode = TestMode.PlayMode,
                testNames = new string[] {
                    // IMPORTANT add tests here before running
                    // todo create window so this doesn't require editing code
                    "Mirage.Tests.Performance.NetworkIdentitySpawningPerformance.SpawnManyObjects"
                }
            };
            var testSettings = new ExecutionSettings
            {
                // DONT USE runSynchronously IT BREAKS THE TEST RUNNER
                overloadTestRunSettings = GetRunSettings(exePath),
                filters = new Filter[] { filter },
            };
            // targetPlatform is internal but is used by TestRunnerApi, so we need to use reflection to set value
            typeof(ExecutionSettings).GetField("targetPlatform", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(testSettings, target);

            runner.Execute(testSettings);
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
            string typeName = "UnityEditor.TestTools.TestRunner.PlayerLauncherTestRunSettings";
            object runSettings = assembly.CreateInstance(typeName);
            runSettings.GetType().GetProperty("buildOnly").SetValue(runSettings, true);
            runSettings.GetType().GetProperty("buildOnlyLocationPath").SetValue(runSettings, targetPath);

            return (ITestRunSettings)runSettings;
        }
    }

    /// <summary>
    /// Converts PlayerWithRest to use ReleaseMode instead of debug mode
    /// <para>use <see cref="MakeRestsReleaseMode.ApplyReleaseMode"/> before build to enable</para>
    /// </summary>
    public class MakeRestsReleaseMode : ITestPlayerBuildModifier, IPostBuildCleanup
    {
        public static bool ApplyReleaseMode = true;

        public BuildPlayerOptions ModifyOptions(BuildPlayerOptions playerOptions)
        {
            if (ApplyReleaseMode)
            {
                Debug.Log("Appling MakeRestsReleaseMode");

                BuildOptions options = playerOptions.options;
                // Do not launch the player after the build completes.
                options &= ~BuildOptions.AutoRunPlayer;
                options |= BuildOptions.ShowBuiltPlayer;
                // Use release mode and dont connect to editor
                options &= ~BuildOptions.Development;
                options &= ~BuildOptions.ConnectToHost;
                options &= ~BuildOptions.WaitForPlayerConnection;

                playerOptions.options = options;
            }
            return playerOptions;
        }

        public void Cleanup()
        {
            ApplyReleaseMode = false;
        }
    }
}
#endif

