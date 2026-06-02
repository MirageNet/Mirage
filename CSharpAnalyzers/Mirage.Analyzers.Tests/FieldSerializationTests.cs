using NUnit.Framework;
using System.Threading.Tasks;

namespace Mirage.Analyzers.Tests
{
    [TestFixture]
    public class FieldSerializationTests
    {
        [Test]
        public async Task PrimitiveAndSupportedTypesDoNotReportError()
        {
            var code = VerifyCS.LoadTestData("FieldSerialization/PrimitiveAndSupportedTypesDoNotReportError.cs");
            await VerifyCS.VerifyAnalyzerAsync(code);
        }

        [Test]
        public async Task CustomTypeWithSerializerDoesNotReportError()
        {
            var code = VerifyCS.LoadTestData("FieldSerialization/CustomTypeWithSerializerDoesNotReportError.cs");
            await VerifyCS.VerifyAnalyzerAsync(code);
        }

        [Test]
        public async Task PrivateFieldsAreIgnored()
        {
            var code = VerifyCS.LoadTestData("FieldSerialization/PrivateFieldsAreIgnored.cs");
            await VerifyCS.VerifyAnalyzerAsync(code);
        }

        [Test]
        public async Task UnserializableFieldReportsError()
        {
            var code = VerifyCS.LoadTestData("FieldSerialization/UnserializableFieldReportsError.cs");
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
            var code = VerifyCS.LoadTestData("FieldSerialization/UnserializablePropertyReportsError.cs");
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
            var code = VerifyCS.LoadTestData("FieldSerialization/UnserializableRpcParameterReportsError.cs");
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
            var code = VerifyCS.LoadTestData("FieldSerialization/UnserializableRpcReturnTypeReportsError.cs");
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
            var code = VerifyCS.LoadTestData("FieldSerialization/StructWithUnserializableFieldReportsError.cs");
            var expected = VerifyCS.Diagnostic("MIRAGE1301")
                .WithLocation(0)
                .WithArguments("NestedUnserializable", "NetworkMessage field");

            await VerifyCS.VerifyAnalyzerAsync(code, expected);
        }

        [Test]
        public async Task RecursiveTypeReportsError()
        {
            var code = VerifyCS.LoadTestData("FieldSerialization/RecursiveTypeReportsError.cs");
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
            var code = VerifyCS.LoadTestData("FieldSerialization/MultiDimensionalArrayReportsError.cs");
            var expected = VerifyCS.Diagnostic("MIRAGE1301")
                .WithLocation(0)
                .WithArguments("Int32", "NetworkMessage field");

            await VerifyCS.VerifyAnalyzerAsync(code, expected);
        }

        [Test]
        public async Task WeaverSafeClassWithUnserializableFieldReportsError()
        {
            var code = VerifyCS.LoadTestData("FieldSerialization/WeaverSafeClassWithUnserializableFieldReportsError.cs");
            var expected = VerifyCS.Diagnostic("MIRAGE1301")
                .WithLocation(0)
                .WithArguments("SafeClassWithThread", "NetworkMessage field");

            await VerifyCS.VerifyAnalyzerAsync(code, expected);
        }

        [Test]
        public async Task WeaverSafeClassOnFieldSuppressesClassWarning()
        {
            var code = VerifyCS.LoadTestData("FieldSerialization/WeaverSafeClassOnFieldSuppressesClassWarning.cs");
            var expected = VerifyCS.Diagnostic("MIRAGE1301")
                .WithLocation(0)
                .WithArguments("Thread", "NetworkMessage field");

            await VerifyCS.VerifyAnalyzerAsync(code, expected);
        }

        [Test]
        public async Task WeaverSafeClassOnPropertySuppressesClassWarning()
        {
            var code = VerifyCS.LoadTestData("FieldSerialization/WeaverSafeClassOnPropertySuppressesClassWarning.cs");
            var expected = VerifyCS.Diagnostic("MIRAGE1301")
                .WithLocation(0)
                .WithArguments("Thread", "NetworkMessage property");

            await VerifyCS.VerifyAnalyzerAsync(code, expected);
        }

        [Test]
        public async Task WeaverSafeClassOnParameterSuppressesClassWarning()
        {
            var code = VerifyCS.LoadTestData("FieldSerialization/WeaverSafeClassOnParameterSuppressesClassWarning.cs");
            var expected = VerifyCS.Diagnostic("MIRAGE1301")
                .WithLocation(0)
                .WithArguments("Thread", "RPC parameter");

            await VerifyCS.VerifyAnalyzerAsync(code, expected);
        }
    }
}
