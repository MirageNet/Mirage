using NUnit.Framework;
using System.Threading.Tasks;

namespace Mirage.Analyzers.Tests
{
    [TestFixture]
    public class Mirage1002Tests
    {
        [Test]
        public async Task ModifyingLocalArrayDoesNotReportWarning()
        {
            var code = VerifyCS.LoadTestData("Mirage1002Tests/ModifyingLocalArrayDoesNotReportWarning.cs");
            await VerifyCS.VerifyAnalyzerAsync(code);
        }

        [Test]
        public async Task ModifyingStandardListDoesNotReportWarning()
        {
            var code = VerifyCS.LoadTestData("Mirage1002Tests/ModifyingStandardListDoesNotReportWarning.cs");
            await VerifyCS.VerifyAnalyzerAsync(code);
        }

        [Test]
        public async Task AssigningEntireElementDoesNotReportWarning()
        {
            var code = VerifyCS.LoadTestData("Mirage1002Tests/AssigningEntireElementDoesNotReportWarning.cs");
            await VerifyCS.VerifyAnalyzerAsync(code);
        }

        [Test]
        public async Task ReadingElementMemberDoesNotReportWarning()
        {
            var code = VerifyCS.LoadTestData("Mirage1002Tests/ReadingElementMemberDoesNotReportWarning.cs");
            await VerifyCS.VerifyAnalyzerAsync(code);
        }

        [Test]
        public async Task DirectMutationOfSyncListElementReportsWarning()
        {
            var code = VerifyCS.LoadTestData("Mirage1002Tests/DirectMutationOfSyncListElementReportsWarning.cs");
            var expected = VerifyCS.Diagnostic("MIRAGE1002").WithLocation(0).WithArguments("mySyncList");
            await VerifyCS.VerifyAnalyzerAsync(code, expected);
        }

        [Test]
        public async Task DirectMutationOfSyncDictionaryElementReportsWarning()
        {
            var code = VerifyCS.LoadTestData("Mirage1002Tests/DirectMutationOfSyncDictionaryElementReportsWarning.cs");
            var expected = VerifyCS.Diagnostic("MIRAGE1002").WithLocation(0).WithArguments("mySyncDict");
            await VerifyCS.VerifyAnalyzerAsync(code, expected);
        }

        [Test]
        public async Task DirectMutationWithCompoundAssignmentReportsWarning()
        {
            var code = VerifyCS.LoadTestData("Mirage1002Tests/DirectMutationWithCompoundAssignmentReportsWarning.cs");
            var expected = VerifyCS.Diagnostic("MIRAGE1002").WithLocation(0).WithArguments("mySyncList");
            await VerifyCS.VerifyAnalyzerAsync(code, expected);
        }

        [Test]
        public async Task DirectMutationWithUnaryExpressionReportsWarning()
        {
            var code = VerifyCS.LoadTestData("Mirage1002Tests/DirectMutationWithUnaryExpressionReportsWarning.cs");
            var expected = VerifyCS.Diagnostic("MIRAGE1002").WithLocation(0).WithArguments("mySyncList");
            await VerifyCS.VerifyAnalyzerAsync(code, expected);
        }

        [Test]
        public async Task PassingElementMemberAsRefParamReportsWarning()
        {
            var code = VerifyCS.LoadTestData("Mirage1002Tests/PassingElementMemberAsRefParamReportsWarning.cs");
            var expected = VerifyCS.Diagnostic("MIRAGE1002").WithLocation(0).WithArguments("mySyncList");
            await VerifyCS.VerifyAnalyzerAsync(code, expected);
        }

        [Test]
        public async Task NestedMemberAccessMutationReportsWarning()
        {
            var code = VerifyCS.LoadTestData("Mirage1002Tests/NestedMemberAccessMutationReportsWarning.cs");
            var expected = VerifyCS.Diagnostic("MIRAGE1002").WithLocation(0).WithArguments("mySyncList");
            await VerifyCS.VerifyAnalyzerAsync(code, expected);
        }
    }
}
