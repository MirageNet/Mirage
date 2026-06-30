using NUnit.Framework;
using System.Threading.Tasks;

namespace Mirage.Analyzers.Tests
{
    [TestFixture]
    public class Mirage1003Tests
    {
        [Test]
        public async Task ReadonlySyncObjectDoesNotReportWarning()
        {
            var code = VerifyCS.LoadTestData("Mirage1003Tests/Negative_ReadonlySyncObjectDoesNotReportWarning.cs");
            await VerifyCS.VerifyAnalyzerAsync(code);
        }

        [Test]
        public async Task NonSyncObjectFieldNotReadonlyDoesNotReportWarning()
        {
            var code = VerifyCS.LoadTestData("Mirage1003Tests/Negative_NonSyncObjectFieldNotReadonlyDoesNotReportWarning.cs");
            await VerifyCS.VerifyAnalyzerAsync(code);
        }

        [Test]
        public async Task SyncObjectAssignedInConstructorDoesNotReportWarning()
        {
            var code = VerifyCS.LoadTestData("Mirage1003Tests/Negative_SyncObjectAssignedInConstructorDoesNotReportWarning.cs");
            await VerifyCS.VerifyAnalyzerAsync(code);
        }

        [Test]
        public async Task NonReadonlySyncObjectReportsError()
        {
            var code = VerifyCS.LoadTestData("Mirage1003Tests/Positive_NonReadonlySyncObjectReportsError.cs");
            var expected = VerifyCS.Diagnostic("MIRAGE1003").WithLocation(0).WithArguments("mySyncList");
            await VerifyCS.VerifyAnalyzerAsync(code, expected);
        }

        [Test]
        public async Task SyncObjectReassignmentInMethodReportsError()
        {
            var code = VerifyCS.LoadTestData("Mirage1003Tests/Positive_SyncObjectReassignmentInMethodReportsError.cs");
            var expected = VerifyCS.Diagnostic("MIRAGE1003").WithLocation(0).WithArguments("mySyncList");
            await VerifyCS.VerifyAnalyzerAsync(code, expected);
        }

        [Test]
        public async Task SyncObjectReassignmentInLocalFunctionInConstructorReportsError()
        {
            var code = VerifyCS.LoadTestData("Mirage1003Tests/Positive_SyncObjectReassignmentInLocalFunctionInConstructorReportsError.cs");
            var expected = VerifyCS.Diagnostic("MIRAGE1003").WithLocation(0).WithArguments("mySyncList");
            await VerifyCS.VerifyAnalyzerAsync(code, expected);
        }

        [Test]
        public async Task SyncObjectReassignmentInLambdaInConstructorReportsError()
        {
            var code = VerifyCS.LoadTestData("Mirage1003Tests/Positive_SyncObjectReassignmentInLambdaInConstructorReportsError.cs");
            var expected = VerifyCS.Diagnostic("MIRAGE1003").WithLocation(0).WithArguments("mySyncList");
            await VerifyCS.VerifyAnalyzerAsync(code, expected);
        }
    }
}
