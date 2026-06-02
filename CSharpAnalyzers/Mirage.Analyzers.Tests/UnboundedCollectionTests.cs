using NUnit.Framework;
using System.Threading.Tasks;

namespace Mirage.Analyzers.Tests
{
    [TestFixture]
    public class UnboundedCollectionTests
    {
        [Test]
        public async Task Positive_BoundedStringAndCollection()
        {
            var code = VerifyCS.LoadTestData("UnboundedCollection/Positive_BoundedStringAndCollection.cs");
            await VerifyCS.VerifyAnalyzerAsync(code);
        }

        [Test]
        public async Task Positive_NonNetworkContext()
        {
            var code = VerifyCS.LoadTestData("UnboundedCollection/Positive_NonNetworkContext.cs");
            await VerifyCS.VerifyAnalyzerAsync(code);
        }

        [Test]
        public async Task Negative_UnboundedFieldAndPropertyInMessage()
        {
            var code = VerifyCS.LoadTestData("UnboundedCollection/Negative_UnboundedFieldAndPropertyInMessage.cs");
            var expectedField1 = VerifyCS.Diagnostic("MIRAGE1502").WithLocation(0).WithArguments("Name", "String");
            var expectedField2 = VerifyCS.Diagnostic("MIRAGE1502").WithLocation(1).WithArguments("Scores", "Int32[]");
            var expectedProp = VerifyCS.Diagnostic("MIRAGE1502").WithLocation(2).WithArguments("Positions", "List");

            await VerifyCS.VerifyAnalyzerAsync(code, expectedField1, expectedField2, expectedProp);
        }

        [Test]
        public async Task Negative_UnboundedParameterInRpc()
        {
            var code = VerifyCS.LoadTestData("UnboundedCollection/Negative_UnboundedParameterInRpc.cs");
            var expectedParam1 = VerifyCS.Diagnostic("MIRAGE1502").WithLocation(0).WithArguments("text", "String");
            var expectedParam2 = VerifyCS.Diagnostic("MIRAGE1502").WithLocation(1).WithArguments("items", "List");

            await VerifyCS.VerifyAnalyzerAsync(code, expectedParam1, expectedParam2);
        }
    }
}
