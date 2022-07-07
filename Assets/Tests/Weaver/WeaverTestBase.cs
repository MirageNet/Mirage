using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Mirage.Logging;
using Mirage.Weaver;
using Mono.Cecil;
using NUnit.Framework;
using Unity.CompilationPipeline.Common.Diagnostics;
using UnityEngine;
using UnityEngine.TestTools;

namespace Mirage.Tests.Weaver
{
    public class AssertionMethodAttribute : Attribute { }

    /// <summary>
    /// Add this to test that can be batched and run together
    /// <para>Most of the time this will be only success test</para>
    /// </summary>
    public class BatchSafeAttribute : Attribute { }

    [TestFixture]
    public abstract class WeaverTestBase
    {
        public static readonly ILogger logger = LogFactory.GetLogger<WeaverTestBase>(LogType.Exception);

        protected Result testResult { get; private set; }
        protected Result batchResult { get; private set; }
        protected IReadOnlyList<DiagnosticMessage> Diagnostics => testResult.weaverLog.Diagnostics;

        private bool currentTestIsBatch;
        private Task batchTask;
        private HashSet<string> batchedTests = new HashSet<string>();

        private static Task<Result> BuildAndWeave(string className, string testName)
        {
            string testSourceDirectory = className + "~";

            string outputFile = testName + ".dll";
            string[] sourceFiles = new string[] { Path.Combine(testSourceDirectory, testName + ".cs") };

            return buildAndWeave(outputFile, sourceFiles);
        }

        private static Task<Result> BuildAndWeaveBatch(string className, string[] testNames)
        {
            string testSourceDirectory = className + "~";

            string outputFile = className + ".dll";
            string[] sourceFiles = testNames.Select(x => Path.Combine(testSourceDirectory, x + ".cs")).ToArray();

            return buildAndWeave(outputFile, sourceFiles);
        }

        private static async Task<Result> buildAndWeave(string outputFile, string[] sourceFiles)
        {
            var weaverLog = new WeaverLogger();
            var assembler = new Assembler(outputFile, sourceFiles);
            AssemblyDefinition assembly = await assembler.BuildAsync(weaverLog);
            return new Result(weaverLog, assembly, assembler);
        }


        [OneTimeSetUp]
        public virtual void OneTimeSetUp()
        {
            var fullName = TestContext.CurrentContext.Test.ClassName;
            var type = Type.GetType(fullName);
            // check we found the right type
            Debug.Assert(type.FullName == fullName);
            Debug.Assert(type.IsSubclassOf(typeof(WeaverTestBase)));

            // tests must be public, so default flags are ok
            var testMethods = type.GetMethods().Where(IsTest);
            var testMethodCount = testMethods.Count();
            var batchMethods = testMethods.Where(IsBatchSafe);

            var methodNames = batchMethods.Select(x => x.Name).ToArray();
            if (methodNames.Length == 0)
                return;
            Debug.Log($"Batching {methodNames.Length} out of {testMethodCount} tests for {GetClassName(fullName)}");

            var className = TestContext.CurrentContext.Test.ClassName.Split('.').Last();
            var buildTask = BuildAndWeaveBatch(className, methodNames);
            batchTask = waitForResults(buildTask, methodNames);
        }

        private async Task waitForResults(Task<Result> buildTask, string[] methodNames)
        {
            batchResult = await buildTask;

            // if there are no compile errors, then add tests to batchedTests
            // else, add none and run seperatly so that we know which one failed
            if (!batchResult.assembler.CompilerErrors)
            {
                batchedTests.UnionWith(methodNames);
            }
        }

        /// <summary>Checks if method has Test</summary>
        private bool IsTest(MethodInfo method)
        {
            return method.GetCustomAttribute<TestAttribute>() != null;
        }

        /// <summary>Checks if method has Test and BatchSafe attritubes</summary>
        private bool IsBatchSafe(MethodInfo method)
        {
            return method.GetCustomAttribute<BatchSafeAttribute>() != null;
        }

        private static string GetClassName(string fullname)
        {
            return fullname.Split('.').Last();
        }

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            batchResult.assembler?.DeleteOutput();
        }

        [UnitySetUp]
        public IEnumerator SetUp()
        {
            Debug.Log($"UnitySetUp");
            if (batchTask != null)
            {
                // wait for batch to finish first
                while (!batchTask.IsCompleted)
                {
                    yield return null;
                }
            }

            string className = GetClassName(TestContext.CurrentContext.Test.ClassName);
            string testName = TestContext.CurrentContext.Test.Name;

            currentTestIsBatch = batchedTests.Contains(testName);

            if (currentTestIsBatch)
            {
                // dont need to build, just sete the results
                testResult = batchResult;
            }
            else
            {
                Task<Result> task = BuildAndWeave(className, TestContext.CurrentContext.Test.Name);
                while (!task.IsCompleted)
                {
                    yield return null;
                }
                testResult = task.Result;
                testResult.AssertNoCompileErrors();
            }
        }

        [TearDown]
        public void TearDown()
        {
            if (!currentTestIsBatch)
            {
                testResult.assembler.DeleteOutput();
            }
        }

        [AssertionMethod]
        protected void IsSuccess()
        {
            Assert.That(Diagnostics, Is.Empty, $"Failed because there are Diagnostics messages: \n  {string.Join("\n  ", Diagnostics.Select(x => x.MessageData))}\n");
        }

        /// <summary>
        /// Like <see cref="IsSuccess"/> but doesn't fail if there are warnings
        /// </summary>
        [AssertionMethod]
        protected void NoErrors()
        {
            var errors = Diagnostics.Where(x => x.DiagnosticType == DiagnosticType.Error).ToArray();
            Assert.That(errors, Is.Empty, $"Failed because there are Error messages: \n  {string.Join("\n  ", errors.Select(d => d.MessageData))}\n");
        }

        [AssertionMethod]
        protected void HasErrorCount(int count)
        {
            var errorMessages = Diagnostics
                .Where(d => d.DiagnosticType == DiagnosticType.Error)
                .Select(d => d.MessageData).ToArray();

            Assert.That(errorMessages.Length, Is.EqualTo(count), $"Error messages: \n  {string.Join("\n  ", errorMessages)}\n");
        }

        [AssertionMethod]
        protected void HasError(string messsage, string atType)
        {
            var fullMessage = $"{messsage} (at {atType})";
            var errorMessages = Diagnostics
                .Where(d => d.DiagnosticType == DiagnosticType.Error)
                .Select(d => d.MessageData).ToArray();

            Assert.That(errorMessages, Contains.Item(fullMessage),
                $"Could not find error message in list\n" +
                $"  Message: \n    {fullMessage}\n" +
                $"  Errors: \n    {string.Join("\n    ", errorMessages)}\n"
                );
        }

        [AssertionMethod]
        protected void HasWarning(string messsage, string atType)
        {
            var fullMessage = $"{messsage} (at {atType})";
            var warningMessages = Diagnostics
                .Where(d => d.DiagnosticType == DiagnosticType.Warning)
                .Select(d => d.MessageData).ToArray();

            Assert.That(warningMessages, Contains.Item(fullMessage),
                $"Could not find warning message in list\n" +
                $"  Message: \n    {fullMessage}\n" +
                $"  Warings: \n    {string.Join("\n    ", warningMessages)}\n"
                );
        }

        protected struct Result
        {
            public readonly WeaverLogger weaverLog;
            public readonly AssemblyDefinition assembly;
            public readonly Assembler assembler;

            public Result(WeaverLogger weaverLog, AssemblyDefinition assembly, Assembler assembler)
            {
                this.weaverLog = weaverLog;
                this.assembly = assembly;
                this.assembler = assembler;
            }

            /// <summary>
            /// Check that there are no c# compile errors
            /// </summary>
            [AssertionMethod]
            public void AssertNoCompileErrors()
            {
                Assert.That(assembler.CompilerErrors, Is.False);
                foreach (var error in weaverLog.Diagnostics)
                {
                    // ensure all errors have a location
                    Assert.That(error.MessageData, Does.Match(@"\(at .*\)$"));
                }
            }
        }
    }
}
