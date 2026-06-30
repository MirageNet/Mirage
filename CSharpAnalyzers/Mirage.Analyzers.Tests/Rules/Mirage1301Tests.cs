using NUnit.Framework;
using System.Threading.Tasks;

namespace Mirage.Analyzers.Tests
{
    [TestFixture]
    public class Mirage1301Tests
    {
        [Test]
        public async Task PrimitiveAndSupportedTypesDoNotReportError()
        {
            var code = VerifyCS.LoadTestData("Mirage1301Tests/Valid_PrimitiveAndSupportedTypesDoNotReportError.cs");
            await VerifyCS.VerifyAnalyzerAsync(code);
        }

        [Test]
        public async Task CustomTypeWithSerializerDoesNotReportError()
        {
            var code = VerifyCS.LoadTestData("Mirage1301Tests/Valid_CustomTypeWithSerializerDoesNotReportError.cs");
            await VerifyCS.VerifyAnalyzerAsync(code);
        }

        [Test]
        public async Task CustomTypeWithLengthSerializerDoesNotReportError()
        {
            var code = VerifyCS.LoadTestData("Mirage1301Tests/Valid_CustomTypeWithLengthSerializerDoesNotReportError.cs");
            await VerifyCS.VerifyAnalyzerAsync(code);
        }

        [Test]
        public async Task PrivateFieldsAreIgnored()
        {
            var code = VerifyCS.LoadTestData("Mirage1301Tests/Valid_PrivateFieldsAreIgnored.cs");
            var expectedPrivateWarning = VerifyCS.Diagnostic("MIRAGE1302")
                .WithLocation(0)
                .WithArguments("executionThread", "MessageWithPrivateField");
            await VerifyCS.VerifyAnalyzerAsync(code, expectedPrivateWarning);
        }

        [Test]
        public async Task UnserializableFieldReportsError()
        {
            var code = VerifyCS.LoadTestData("Mirage1301Tests/Invalid_UnserializableFieldReportsError.cs");
            var expected = VerifyCS.Diagnostic("MIRAGE1301")
                .WithLocation(0)
                .WithArguments("Thread", "NetworkMessage field");

            var expectedClassWarning = VerifyCS.Diagnostic("MIRAGE1201")
                .WithLocation(0)
                .WithArguments("NetworkMessage field", "executionThread", "Thread");

            await VerifyCS.VerifyAnalyzerAsync(code, expected, expectedClassWarning);
        }

        [Test]
        public async Task UnserializablePropertyReportsError()
        {
            var code = VerifyCS.LoadTestData("Mirage1301Tests/Invalid_UnserializablePropertyReportsError.cs");
            var expected = VerifyCS.Diagnostic("MIRAGE1301")
                .WithLocation(0)
                .WithArguments("Thread", "NetworkMessage property");

            var expectedClassWarning = VerifyCS.Diagnostic("MIRAGE1201")
                .WithLocation(0)
                .WithArguments("NetworkMessage property", "ExecutionThread", "Thread");

            var expectedPrivateWarning = VerifyCS.Diagnostic("MIRAGE1302")
                .WithLocation(0)
                .WithArguments("ExecutionThread", "StartSessionMessage");

            await VerifyCS.VerifyAnalyzerAsync(code, expected, expectedClassWarning, expectedPrivateWarning);
        }

        [Test]
        public async Task UnserializableRpcParameterReportsError()
        {
            var code = VerifyCS.LoadTestData("Mirage1301Tests/Invalid_UnserializableRpcParameterReportsError.cs");
            var expected = VerifyCS.Diagnostic("MIRAGE1301")
                .WithLocation(0)
                .WithArguments("Thread", "RPC parameter");

            var expectedClassWarning = VerifyCS.Diagnostic("MIRAGE1201")
                .WithLocation(0)
                .WithArguments("RPC parameter", "executionThread", "Thread");

            await VerifyCS.VerifyAnalyzerAsync(code, expected, expectedClassWarning);
        }

        [Test]
        public async Task UnserializableRpcReturnTypeReportsError()
        {
            var code = VerifyCS.LoadTestData("Mirage1301Tests/Invalid_UnserializableRpcReturnTypeReportsError.cs");
            var expected = VerifyCS.Diagnostic("MIRAGE1301")
                .WithLocation(0)
                .WithArguments("Thread", "RPC return type");

            var expectedClassWarning = VerifyCS.Diagnostic("MIRAGE1201")
                .WithLocation(0)
                .WithArguments("RPC return type", "CmdGetSession", "Thread");

            await VerifyCS.VerifyAnalyzerAsync(code, expected, expectedClassWarning);
        }

        [Test]
        public async Task StructWithUnserializableFieldReportsError()
        {
            var code = VerifyCS.LoadTestData("Mirage1301Tests/Invalid_StructWithUnserializableFieldReportsError.cs");
            var expected = VerifyCS.Diagnostic("MIRAGE1301")
                .WithLocation(0)
                .WithArguments("NestedUnserializable", "NetworkMessage field");

            await VerifyCS.VerifyAnalyzerAsync(code, expected);
        }

        [Test]
        public async Task RecursiveTypeReportsError()
        {
            var code = VerifyCS.LoadTestData("Mirage1301Tests/Invalid_RecursiveTypeReportsError.cs");
            var expected1 = VerifyCS.Diagnostic("MIRAGE1301")
                .WithLocation(0)
                .WithArguments("RecursiveClass", "NetworkMessage field");

            var expected2 = VerifyCS.Diagnostic("MIRAGE1301")
                .WithLocation(1)
                .WithArguments("RecursiveClass", "NetworkMessage field");

            await VerifyCS.VerifyAnalyzerAsync(code, expected1, expected2);
        }

        [Test]
        public async Task MultiDimensionalArrayReportsError()
        {
            var code = VerifyCS.LoadTestData("Mirage1301Tests/Invalid_MultiDimensionalArrayReportsError.cs");
            var expected = VerifyCS.Diagnostic("MIRAGE1301")
                .WithLocation(0)
                .WithArguments("Int32", "NetworkMessage field");

            await VerifyCS.VerifyAnalyzerAsync(code, expected);
        }

        [Test]
        public async Task WeaverSafeClassWithUnserializableFieldReportsError()
        {
            var code = VerifyCS.LoadTestData("Mirage1301Tests/Invalid_WeaverSafeClassWithUnserializableFieldReportsError.cs");
            var expected1 = VerifyCS.Diagnostic("MIRAGE1301")
                .WithLocation(0)
                .WithArguments("Thread", "NetworkMessage field");

            var expected2 = VerifyCS.Diagnostic("MIRAGE1301")
                .WithLocation(1)
                .WithArguments("SafeClassWithThread", "NetworkMessage field");

            await VerifyCS.VerifyAnalyzerAsync(code, expected1, expected2);
        }

        [Test]
        public async Task WeaverSafeClassOnFieldSuppressesClassWarning()
        {
            var code = VerifyCS.LoadTestData("Mirage1301Tests/Invalid_WeaverSafeClassOnFieldSuppressesClassWarning.cs");
            var expected = VerifyCS.Diagnostic("MIRAGE1301")
                .WithLocation(0)
                .WithArguments("Thread", "NetworkMessage field");

            await VerifyCS.VerifyAnalyzerAsync(code, expected);
        }

        [Test]
        public async Task WeaverSafeClassOnPropertySuppressesClassWarning()
        {
            var code = VerifyCS.LoadTestData("Mirage1301Tests/Invalid_WeaverSafeClassOnPropertySuppressesClassWarning.cs");
            var expected = VerifyCS.Diagnostic("MIRAGE1301")
                .WithLocation(0)
                .WithArguments("Thread", "NetworkMessage property");

            var expectedPrivateWarning = VerifyCS.Diagnostic("MIRAGE1302")
                .WithLocation(0)
                .WithArguments("ExecutionThread", "StartSessionMessage");

            await VerifyCS.VerifyAnalyzerAsync(code, expected, expectedPrivateWarning);
        }

        [Test]
        public async Task WeaverSafeClassOnParameterSuppressesClassWarning()
        {
            var code = VerifyCS.LoadTestData("Mirage1301Tests/Invalid_WeaverSafeClassOnParameterSuppressesClassWarning.cs");
            var expected = VerifyCS.Diagnostic("MIRAGE1301")
                .WithLocation(0)
                .WithArguments("Thread", "RPC parameter");

            await VerifyCS.VerifyAnalyzerAsync(code, expected);
        }

        [Test]
        public async Task ClientRpcObserversWithINetworkPlayerArgReportsError()
        {
            var code = VerifyCS.LoadTestData("Mirage1301Tests/Invalid_ClientRpcObserversWithINetworkPlayerArg.cs");
            var expected = VerifyCS.Diagnostic("MIRAGE1301")
                .WithLocation(0)
                .WithArguments("INetworkPlayer", "RPC parameter");
            await VerifyCS.VerifyAnalyzerAsync(code, expected);
        }

        [Test]
        public async Task ClientRpcOwnerWithINetworkPlayerArgReportsError()
        {
            var code = VerifyCS.LoadTestData("Mirage1301Tests/Invalid_ClientRpcOwnerWithINetworkPlayerArg.cs");
            var expected = VerifyCS.Diagnostic("MIRAGE1301")
                .WithLocation(0)
                .WithArguments("INetworkPlayer", "RPC parameter");
            await VerifyCS.VerifyAnalyzerAsync(code, expected);
        }

        [Test]
        public async Task RpcWithTargetWithDoubleNetworkPlayerReportsError()
        {
            var code = VerifyCS.LoadTestData("Mirage1301Tests/Invalid_RpcWithTargetWithDoubleNetworkPlayer.cs");
            var expected = VerifyCS.Diagnostic("MIRAGE1301")
                .WithLocation(0)
                .WithArguments("INetworkPlayer", "RPC parameter");
            await VerifyCS.VerifyAnalyzerAsync(code, expected);
        }

        [Test]
        public async Task ClientRpcWithTargetDoesNotReportError()
        {
            var code = VerifyCS.LoadTestData("Mirage1301Tests/Valid_ClientRpcWithTarget.cs");
            await VerifyCS.VerifyAnalyzerAsync(code);
        }

        [Test]
        public async Task ServerRpcWithSenderDoesNotReportError()
        {
            var code = VerifyCS.LoadTestData("Mirage1301Tests/Valid_ServerRpcWithSender.cs");
            await VerifyCS.VerifyAnalyzerAsync(code);
        }

        [Test]
        public async Task StructFieldDoesNotReportError()
        {
            var code = VerifyCS.LoadTestData("Mirage1301Tests/Valid_StructField.cs");
            await VerifyCS.VerifyAnalyzerAsync(code);
        }
    }
}
