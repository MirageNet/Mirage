using System;
using System.IO;
using Mirage.Tests.PlayerTests;
using NUnit.Framework.Interfaces;
using UnityEngine;
using UnityEngine.TestRunner;

[assembly: TestRunCallback(typeof(PlayerWithTestsCallbacks))]

namespace Mirage.Tests.PlayerTests
{
    /// <summary>
    /// Saves test results when running outside of editor
    /// </summary>
    public class PlayerWithTestsCallbacks : ITestRunCallback
    {
        public void RunStarted(ITest testsToRun)
        {
            // we only run these callbacks if we are in player
            if (Application.isEditor) return;

            // set framerate so 60 so player doesn't use all cpu
            Application.targetFrameRate = 60;
        }

        public void RunFinished(ITestResult testResults)
        {
            if (Application.isEditor) return;

            TryWriteToFile(testResults);

            // quit after tests are finished
            Debug.Log($"Quiting after test run finished");
            Application.Quit();
        }

        private static void TryWriteToFile(ITestResult testResults)
        {
            try
            {
                var xml = testResults.ToXml(true).OuterXml;
                var savePath = Path.Combine(Application.persistentDataPath, "PlayerWithTests-results.xml");
                Debug.Log($"Saving results to {savePath}");
                File.WriteAllText(savePath, xml);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }

        public void TestStarted(ITest test) { }

        public void TestFinished(ITestResult result) { }
    }
}
