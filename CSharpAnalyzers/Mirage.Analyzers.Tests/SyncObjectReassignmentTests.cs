using NUnit.Framework;
using System.Threading.Tasks;

namespace Mirage.Analyzers.Tests
{
    [TestFixture]
    public class SyncObjectReassignmentTests
    {
        [Test]
        public async Task ReadonlySyncObjectDoesNotReportWarning()
        {
            var code = VerifyCS.LoadTestData("SyncObjectReassignment/ReadonlySyncObjectDoesNotReportWarning.cs");
            await VerifyCS.VerifyAnalyzerAsync(code);
        }

        [Test]
        public async Task NonSyncObjectFieldNotReadonlyDoesNotReportWarning()
        {
            var code = VerifyCS.LoadTestData("SyncObjectReassignment/NonSyncObjectFieldNotReadonlyDoesNotReportWarning.cs");
            await VerifyCS.VerifyAnalyzerAsync(code);
        }

        [Test]
        public async Task SyncObjectAssignedInConstructorDoesNotReportWarning()
        {
            var code = VerifyCS.LoadTestData("SyncObjectReassignment/SyncObjectAssignedInConstructorDoesNotReportWarning.cs");
            await VerifyCS.VerifyAnalyzerAsync(code);
        }

        [Test]
        public async Task NonReadonlySyncObjectReportsError()
        {
            var code = VerifyCS.LoadTestData("SyncObjectReassignment/NonReadonlySyncObjectReportsError.cs");
            var expected = VerifyCS.Diagnostic("MIRAGE1004").WithLocation(0).WithArguments("mySyncList");
            await VerifyCS.VerifyAnalyzerAsync(code, expected);
        }

        [Test]
        public async Task SyncObjectReassignmentInMethodReportsError()
        {
            var code = VerifyCS.LoadTestData("SyncObjectReassignment/SyncObjectReassignmentInMethodReportsError.cs");
            var expected = VerifyCS.Diagnostic("MIRAGE1004").WithLocation(0).WithArguments("mySyncList");
            await VerifyCS.VerifyAnalyzerAsync(code, expected);
        }

        [Test]
        public async Task SyncObjectReassignmentInLocalFunctionInConstructorReportsError()
        {
            var code = VerifyCS.LoadTestData("SyncObjectReassignment/SyncObjectReassignmentInLocalFunctionInConstructorReportsError.cs");
            var expected = VerifyCS.Diagnostic("MIRAGE1004").WithLocation(0).WithArguments("mySyncList");
            await VerifyCS.VerifyAnalyzerAsync(code, expected);
        }

        [Test]
        public async Task SyncObjectReassignmentInLambdaInConstructorReportsError()
        {
            var code = VerifyCS.LoadTestData("SyncObjectReassignment/SyncObjectReassignmentInLambdaInConstructorReportsError.cs");
            var expected = VerifyCS.Diagnostic("MIRAGE1004").WithLocation(0).WithArguments("mySyncList");
            await VerifyCS.VerifyAnalyzerAsync(code, expected);
        }
    }
}
