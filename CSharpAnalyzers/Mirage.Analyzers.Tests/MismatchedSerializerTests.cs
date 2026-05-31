using NUnit.Framework;
using System.Threading.Tasks;

namespace Mirage.Analyzers.Tests
{
    [TestFixture]
    public class MismatchedSerializerTests
    {
        private const string MockDefinitions = @"
namespace Mirage.Serialization
{
    public class NetworkWriter {}
    public class NetworkReader {}
}
";

        [Test]
        public async Task MatchingReaderAndWriterDoesNotReportError()
        {
            var code = @"
using Mirage.Serialization;

public struct CustomType
{
    public int value;
}

public static class CustomSerialization
{
    public static void WriteCustomType(this NetworkWriter writer, CustomType value) {}
    public static CustomType ReadCustomType(this NetworkReader reader) => default;
}
" + MockDefinitions;

            await VerifyCS.VerifyAnalyzerAsync(code);
        }

        [Test]
        public async Task NonExtensionMethodsAreIgnored()
        {
            // Non-extension methods should not be treated as custom serializers,
            // so they should not trigger any mismatched serialization warnings.
            var code = @"
using Mirage.Serialization;

public struct CustomType {}

public static class CustomSerialization
{
    // Not extension methods (missing 'this')
    public static void WriteCustomType(NetworkWriter writer, CustomType value) {}
    public static CustomType ReadCustomType(NetworkReader reader) => default;
}
" + MockDefinitions;

            await VerifyCS.VerifyAnalyzerAsync(code);
        }

        [Test]
        public async Task WriterOnlyReportsError()
        {
            var code = @"
using Mirage.Serialization;

public struct CustomType
{
    public int value;
}

public static class CustomSerialization
{
    public static void {|#0:WriteCustomType|}(this NetworkWriter writer, CustomType value) {}
}
" + MockDefinitions;

            var expected = VerifyCS.Diagnostic("MIRAGE1303")
                .WithLocation(0)
                .WithArguments("CustomType", "Custom writer defined for 'CustomType' but matching custom reader is missing.");

            await VerifyCS.VerifyAnalyzerAsync(code, expected);
        }

        [Test]
        public async Task ReaderOnlyReportsError()
        {
            var code = @"
using Mirage.Serialization;

public struct CustomType
{
    public int value;
}

public static class CustomSerialization
{
    public static CustomType {|#0:ReadCustomType|}(this NetworkReader reader) => default;
}
" + MockDefinitions;

            var expected = VerifyCS.Diagnostic("MIRAGE1303")
                .WithLocation(0)
                .WithArguments("CustomType", "Custom reader defined for 'CustomType' but matching custom writer is missing.");

            await VerifyCS.VerifyAnalyzerAsync(code, expected);
        }

        [Test]
        public async Task NestedStaticClassWithMismatchedSerializerReportsError()
        {
            var code = @"
using Mirage.Serialization;

public struct CustomType {}

public static class OuterClass
{
    public static class InnerClass
    {
        public static void {|#0:WriteCustomType|}(this NetworkWriter writer, CustomType value) {}
    }
}
" + MockDefinitions;

            var expected = VerifyCS.Diagnostic("MIRAGE1303")
                .WithLocation(0)
                .WithArguments("CustomType", "Custom writer defined for 'CustomType' but matching custom reader is missing.");

            await VerifyCS.VerifyAnalyzerAsync(code, expected);
        }

        [Test]
        public async Task MismatchedArraySerializerReportsError()
        {
            // Custom serialization for arrays (e.g. CustomType[]) must also match
            var code = @"
using Mirage.Serialization;

public struct CustomType {}

public static class CustomSerialization
{
    public static void {|#0:WriteCustomArray|}(this NetworkWriter writer, CustomType[] value) {}
}
" + MockDefinitions;

            var expected = VerifyCS.Diagnostic("MIRAGE1303")
                .WithLocation(0)
                .WithArguments("CustomType[]", "Custom writer defined for 'CustomType[]' but matching custom reader is missing.");

            await VerifyCS.VerifyAnalyzerAsync(code, expected);
        }
    }
}
