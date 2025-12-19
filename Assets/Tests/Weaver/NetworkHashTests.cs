using System.IO;
using NUnit.Framework;

namespace Mirage.Tests.Weaver
{
    public class NetworkHashTests : WeaverTestBase
    {
        [Test]
        public void NetworkHashIsGenerated()
        {
            IsSuccess(); // ensures weaver runs without errors

            var networkApiFileName = $"network_api_{testResult.assembly.Name.Name}.txt";
            Assert.That(File.Exists(networkApiFileName), "network_api.txt was not generated");
        }

        [Test]
        public void NetworkHashIsCorrect()
        {
            IsSuccess();

            var networkApiFileName = $"network_api_{testResult.assembly.Name.Name}.txt";
            var text = File.ReadAllText(networkApiFileName);

            // test class is in a folder with same name as this class
            var className = nameof(NetworkHashTests);
            var testName = nameof(NetworkHashIsCorrect);
            var behaviourName = $"{className}.{testName}.MyTestBehaviour";
            var messageName = $"{className}.{testName}.MyMessage";

            Assert.That(text, Does.Contain($"\nSyncVars for {behaviourName}:"));
            Assert.That(text, Does.Contain($"  System.Int32 mySyncVar"));

            Assert.That(text, Does.Contain($"\nRPCs for {behaviourName}:"));
            Assert.That(text, Does.Contain($"  System.Void RpcMyTestRpc(System.Int32 value)"));

            Assert.That(text, Does.Contain("\nMessages:"));
            Assert.That(text, Does.Contain($"  {messageName} {{ System.Int32 someValue }}"));

            Assert.That(text, Does.Contain("\nSerializers:"));
            Assert.That(text, Does.Contain($"  {messageName} => (writer:WriteMyMessage, reader:ReadMyMessage)"));
        }
    }
}
