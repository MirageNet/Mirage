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
            var code = VerifyCS.LoadTestData("Mirage1301Tests/PrimitiveAndSupportedTypesDoNotReportError.cs");
            await VerifyCS.VerifyAnalyzerAsync(code);
        }

        [Test]
        public async Task CustomTypeWithSerializerDoesNotReportError()
        {
            var code = VerifyCS.LoadTestData("Mirage1301Tests/CustomTypeWithSerializerDoesNotReportError.cs");
            await VerifyCS.VerifyAnalyzerAsync(code);
        }

        [Test]
        public async Task PrivateFieldsAreIgnored()
        {
            var code = VerifyCS.LoadTestData("Mirage1301Tests/PrivateFieldsAreIgnored.cs");
            await VerifyCS.VerifyAnalyzerAsync(code);
        }

        [Test]
        public async Task UnserializableFieldReportsError()
        {
            var code = VerifyCS.LoadTestData("Mirage1301Tests/UnserializableFieldReportsError.cs");
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
            var code = VerifyCS.LoadTestData("Mirage1301Tests/UnserializablePropertyReportsError.cs");
            var expected = VerifyCS.Diagnostic("MIRAGE1301")
                .WithLocation(0)
                .WithArguments("Thread", "NetworkMessage property");

            var expectedClassWarning = VerifyCS.Diagnostic("MIRAGE1201")
                .WithLocation(0)
                .WithArguments("NetworkMessage property", "ExecutionThread", "Thread");

            await VerifyCS.VerifyAnalyzerAsync(code, expected, expectedClassWarning);
        }

        [Test]
        public async Task UnserializableRpcParameterReportsError()
        {
            var code = VerifyCS.LoadTestData("Mirage1301Tests/UnserializableRpcParameterReportsError.cs");
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
            var code = VerifyCS.LoadTestData("Mirage1301Tests/UnserializableRpcReturnTypeReportsError.cs");
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
            var code = VerifyCS.LoadTestData("Mirage1301Tests/StructWithUnserializableFieldReportsError.cs");
            var expected = VerifyCS.Diagnostic("MIRAGE1301")
                .WithLocation(0)
                .WithArguments("NestedUnserializable", "NetworkMessage field");

            await VerifyCS.VerifyAnalyzerAsync(code, expected);
        }

        [Test]
        public async Task RecursiveTypeReportsError()
        {
            var code = VerifyCS.LoadTestData("Mirage1301Tests/RecursiveTypeReportsError.cs");
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
            var code = VerifyCS.LoadTestData("Mirage1301Tests/MultiDimensionalArrayReportsError.cs");
            var expected = VerifyCS.Diagnostic("MIRAGE1301")
                .WithLocation(0)
                .WithArguments("Int32", "NetworkMessage field");

            await VerifyCS.VerifyAnalyzerAsync(code, expected);
        }

        [Test]
        public async Task WeaverSafeClassWithUnserializableFieldReportsError()
        {
            var code = VerifyCS.LoadTestData("Mirage1301Tests/WeaverSafeClassWithUnserializableFieldReportsError.cs");
            var expected = VerifyCS.Diagnostic("MIRAGE1301")
                .WithLocation(0)
                .WithArguments("SafeClassWithThread", "NetworkMessage field");

            await VerifyCS.VerifyAnalyzerAsync(code, expected);
        }

        [Test]
        public async Task WeaverSafeClassOnFieldSuppressesClassWarning()
        {
            var code = VerifyCS.LoadTestData("Mirage1301Tests/WeaverSafeClassOnFieldSuppressesClassWarning.cs");
            var expected = VerifyCS.Diagnostic("MIRAGE1301")
                .WithLocation(0)
                .WithArguments("Thread", "NetworkMessage field");

            await VerifyCS.VerifyAnalyzerAsync(code, expected);
        }

        [Test]
        public async Task WeaverSafeClassOnPropertySuppressesClassWarning()
        {
            var code = VerifyCS.LoadTestData("Mirage1301Tests/WeaverSafeClassOnPropertySuppressesClassWarning.cs");
            var expected = VerifyCS.Diagnostic("MIRAGE1301")
                .WithLocation(0)
                .WithArguments("Thread", "NetworkMessage property");

            await VerifyCS.VerifyAnalyzerAsync(code, expected);
        }

        [Test]
        public async Task WeaverSafeClassOnParameterSuppressesClassWarning()
        {
            var code = VerifyCS.LoadTestData("Mirage1301Tests/WeaverSafeClassOnParameterSuppressesClassWarning.cs");
            var expected = VerifyCS.Diagnostic("MIRAGE1301")
                .WithLocation(0)
                .WithArguments("Thread", "RPC parameter");

            await VerifyCS.VerifyAnalyzerAsync(code, expected);
        }
    }
}
