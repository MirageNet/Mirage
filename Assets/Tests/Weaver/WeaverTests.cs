using System.Collections.Generic;
using System.IO;
using System.Linq;
using Mono.Cecil;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Mirror.Weaver.Tests
{
    public abstract class WeaverTestsBuildFromTestName : WeaverTests
    {
        [SetUp]
        public virtual void TestSetup()
        {
            string className = TestContext.CurrentContext.Test.ClassName.Split('.').Last();

            BuildAndWeaveTestAssembly(className, TestContext.CurrentContext.Test.Name);
            
        }

        protected void IsSuccess()
        {
            Assert.That(weaverLog.Diagnostics, Is.Empty);
        }

        protected void HasError(string messsage, string atType)
        {
            Assert.That(weaverLog.Diagnostics, Contains.Item($"{messsage} (at {atType})"));
        }

        protected void HasWarning(string messsage, string atType)
        {
            Assert.That(weaverLog.Diagnostics, Contains.Item($"{messsage} (at {atType})"));
        }
    }

    [TestFixture]
    [Category("Weaver")]
    public abstract class WeaverTests
    {
        public static readonly ILogger logger = LogFactory.GetLogger<WeaverTests>(LogType.Exception);

        protected Logger weaverLog = new Logger();

        protected void BuildAndWeaveTestAssembly(string className, string testName)
        {
            string testSourceDirectory = className + "~";
            WeaverAssembler.OutputFile = Path.Combine(testSourceDirectory, testName + ".dll");
            WeaverAssembler.AddSourceFiles(new string[] { Path.Combine(testSourceDirectory, testName + ".cs") });
            WeaverAssembler.Build();

            Assert.That(WeaverAssembler.CompilerErrors, Is.False);
            foreach (var error in weaverLog.Diagnostics)
            {
                // ensure all errors have a location
                Assert.That(error, Does.Match(@"\(at .*\)$"));
            }
        }

        
        [OneTimeSetUp]
        public void FixtureSetup()
        {

        }

        [OneTimeTearDown]
        public void FixtureCleanup()
        {
        }
        

        [TearDown]
        public void TestCleanup()
        {
            WeaverAssembler.DeleteOutputOnClear = true;
            WeaverAssembler.Clear();
        }
    }
}
