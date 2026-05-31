using NUnit.Framework;
using System.Threading.Tasks;

namespace Mirage.Analyzers.Tests
{
    [TestFixture]
    public class DirectMutationTests
    {
        [Test]
        public async Task ModifyingLocalArrayDoesNotReportWarning()
        {
            var code = VerifyCS.LoadTestData("DirectMutation/ModifyingLocalArrayDoesNotReportWarning.cs");
            await VerifyCS.VerifyAnalyzerAsync(code);
        }

        [Test]
        public async Task ModifyingStandardListDoesNotReportWarning()
        {
            var code = VerifyCS.LoadTestData("DirectMutation/ModifyingStandardListDoesNotReportWarning.cs");
            await VerifyCS.VerifyAnalyzerAsync(code);
        }

        [Test]
        public async Task AssigningEntireElementDoesNotReportWarning()
        {
            var code = VerifyCS.LoadTestData("DirectMutation/AssigningEntireElementDoesNotReportWarning.cs");
            await VerifyCS.VerifyAnalyzerAsync(code);
        }

        [Test]
        public async Task ReadingElementMemberDoesNotReportWarning()
        {
            var code = VerifyCS.LoadTestData("DirectMutation/ReadingElementMemberDoesNotReportWarning.cs");
            await VerifyCS.VerifyAnalyzerAsync(code);
        }

        [Test]
        public async Task DirectMutationOfSyncListElementReportsWarning()
        {
            var code = VerifyCS.LoadTestData("DirectMutation/DirectMutationOfSyncListElementReportsWarning.cs");
            var expected = VerifyCS.Diagnostic("MIRAGE1003").WithLocation(0).WithArguments("mySyncList");
            await VerifyCS.VerifyAnalyzerAsync(code, expected);
        }

        [Test]
        public async Task DirectMutationOfSyncDictionaryElementReportsWarning()
        {
            var code = VerifyCS.LoadTestData("DirectMutation/DirectMutationOfSyncDictionaryElementReportsWarning.cs");
            var expected = VerifyCS.Diagnostic("MIRAGE1003").WithLocation(0).WithArguments("mySyncDict");
            await VerifyCS.VerifyAnalyzerAsync(code, expected);
        }

        [Test]
        public async Task DirectMutationWithCompoundAssignmentReportsWarning()
        {
            var code = VerifyCS.LoadTestData("DirectMutation/DirectMutationWithCompoundAssignmentReportsWarning.cs");
            var expected = VerifyCS.Diagnostic("MIRAGE1003").WithLocation(0).WithArguments("mySyncList");
            await VerifyCS.VerifyAnalyzerAsync(code, expected);
        }

        [Test]
        public async Task DirectMutationWithUnaryExpressionReportsWarning()
        {
            var code = VerifyCS.LoadTestData("DirectMutation/DirectMutationWithUnaryExpressionReportsWarning.cs");
            var expected = VerifyCS.Diagnostic("MIRAGE1003").WithLocation(0).WithArguments("mySyncList");
            await VerifyCS.VerifyAnalyzerAsync(code, expected);
        }

        [Test]
        public async Task PassingElementMemberAsRefParamReportsWarning()
        {
            var code = VerifyCS.LoadTestData("DirectMutation/PassingElementMemberAsRefParamReportsWarning.cs");
            var expected = VerifyCS.Diagnostic("MIRAGE1003").WithLocation(0).WithArguments("mySyncList");
            await VerifyCS.VerifyAnalyzerAsync(code, expected);
        }

        [Test]
        public async Task NestedMemberAccessMutationReportsWarning()
        {
            var code = VerifyCS.LoadTestData("DirectMutation/NestedMemberAccessMutationReportsWarning.cs");
            var expected = VerifyCS.Diagnostic("MIRAGE1003").WithLocation(0).WithArguments("mySyncList");
            await VerifyCS.VerifyAnalyzerAsync(code, expected);
        }
    }
}
