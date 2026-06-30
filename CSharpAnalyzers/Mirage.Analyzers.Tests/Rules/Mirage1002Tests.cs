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
            var code = VerifyCS.LoadTestData("Mirage1002Tests/Negative_ModifyingLocalArray.cs");
            await VerifyCS.VerifyAnalyzerAsync(code);
        }

        [Test]
        public async Task ModifyingStandardListDoesNotReportWarning()
        {
            var code = VerifyCS.LoadTestData("Mirage1002Tests/Negative_ModifyingStandardList.cs");
            await VerifyCS.VerifyAnalyzerAsync(code);
        }

        [Test]
        public async Task AssigningEntireElementDoesNotReportWarning()
        {
            var code = VerifyCS.LoadTestData("Mirage1002Tests/Negative_AssigningEntireElement.cs");
            await VerifyCS.VerifyAnalyzerAsync(code);
        }

        [Test]
        public async Task ReadingElementMemberDoesNotReportWarning()
        {
            var code = VerifyCS.LoadTestData("Mirage1002Tests/Negative_ReadingElementMember.cs");
            await VerifyCS.VerifyAnalyzerAsync(code);
        }

        [Test]
        public async Task DirectMutationOfSyncListElementReportsWarning()
        {
            var code = VerifyCS.LoadTestData("Mirage1002Tests/Positive_DirectMutationOfSyncListElement.cs");
            var expected = VerifyCS.Diagnostic("MIRAGE1002").WithLocation(0).WithArguments("mySyncList");
            await VerifyCS.VerifyAnalyzerAsync(code, expected);
        }

        [Test]
        public async Task DirectMutationOfSyncDictionaryElementReportsWarning()
        {
            var code = VerifyCS.LoadTestData("Mirage1002Tests/Positive_DirectMutationOfSyncDictionaryElement.cs");
            var expected = VerifyCS.Diagnostic("MIRAGE1002").WithLocation(0).WithArguments("mySyncDict");
            await VerifyCS.VerifyAnalyzerAsync(code, expected);
        }

        [Test]
        public async Task DirectMutationWithCompoundAssignmentReportsWarning()
        {
            var code = VerifyCS.LoadTestData("Mirage1002Tests/Positive_DirectMutationWithCompoundAssignment.cs");
            var expected = VerifyCS.Diagnostic("MIRAGE1002").WithLocation(0).WithArguments("mySyncList");
            await VerifyCS.VerifyAnalyzerAsync(code, expected);
        }

        [Test]
        public async Task DirectMutationWithUnaryExpressionReportsWarning()
        {
            var code = VerifyCS.LoadTestData("Mirage1002Tests/Positive_DirectMutationWithUnaryExpression.cs");
            var expected = VerifyCS.Diagnostic("MIRAGE1002").WithLocation(0).WithArguments("mySyncList");
            await VerifyCS.VerifyAnalyzerAsync(code, expected);
        }

        [Test]
        public async Task PassingElementMemberAsRefParamReportsWarning()
        {
            var code = VerifyCS.LoadTestData("Mirage1002Tests/Positive_PassingElementMemberAsRefParam.cs");
            var expected = VerifyCS.Diagnostic("MIRAGE1002").WithLocation(0).WithArguments("mySyncList");
            await VerifyCS.VerifyAnalyzerAsync(code, expected);
        }

        [Test]
        public async Task NestedMemberAccessMutationReportsWarning()
        {
            var code = VerifyCS.LoadTestData("Mirage1002Tests/Positive_NestedMemberAccessMutation.cs");
            var expected = VerifyCS.Diagnostic("MIRAGE1002").WithLocation(0).WithArguments("mySyncList");
            await VerifyCS.VerifyAnalyzerAsync(code, expected);
        }
    }
}
