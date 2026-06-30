using NUnit.Framework;
using System.Threading.Tasks;

namespace Mirage.Analyzers.Tests
{
    [TestFixture]
    public class Mirage1303Tests
    {
        [Test]
        public async Task MatchingReaderAndWriterDoesNotReportError()
        {
            var code = VerifyCS.LoadTestData("Mirage1303Tests/MatchingReaderAndWriterDoesNotReportError.cs");
            await VerifyCS.VerifyAnalyzerAsync(code);
        }

        [Test]
        public async Task NonExtensionMethodsAreIgnored()
        {
            var code = VerifyCS.LoadTestData("Mirage1303Tests/NonExtensionMethodsAreIgnored.cs");
            await VerifyCS.VerifyAnalyzerAsync(code);
        }

        [Test]
        public async Task WriterOnlyReportsError()
        {
            var code = VerifyCS.LoadTestData("Mirage1303Tests/WriterOnlyReportsError.cs");
            var expected = VerifyCS.Diagnostic("MIRAGE1303")
                .WithLocation(0)
                .WithArguments("CustomType", "Custom writer defined for 'CustomType' but matching custom reader is missing.");

            await VerifyCS.VerifyAnalyzerAsync(code, expected);
        }

        [Test]
        public async Task ReaderOnlyReportsError()
        {
            var code = VerifyCS.LoadTestData("Mirage1303Tests/ReaderOnlyReportsError.cs");
            var expected = VerifyCS.Diagnostic("MIRAGE1303")
                .WithLocation(0)
                .WithArguments("CustomType", "Custom reader defined for 'CustomType' but matching custom writer is missing.");

            await VerifyCS.VerifyAnalyzerAsync(code, expected);
        }

        [Test]
        public async Task NestedStaticClassWithMismatchedSerializerReportsError()
        {
            var code = VerifyCS.LoadTestData("Mirage1303Tests/NestedStaticClassWithMismatchedSerializerReportsError.cs");
            var expected = VerifyCS.Diagnostic("MIRAGE1303")
                .WithLocation(0)
                .WithArguments("CustomType", "Custom writer defined for 'CustomType' but matching custom reader is missing.");

            await VerifyCS.VerifyAnalyzerAsync(code, expected);
        }

        [Test]
        public async Task MismatchedArraySerializerReportsError()
        {
            var code = VerifyCS.LoadTestData("Mirage1303Tests/MismatchedArraySerializerReportsError.cs");
            var expected = VerifyCS.Diagnostic("MIRAGE1303")
                .WithLocation(0)
                .WithArguments("CustomType[]", "Custom writer defined for 'CustomType[]' but matching custom reader is missing.");

            await VerifyCS.VerifyAnalyzerAsync(code, expected);
        }
    }
}
