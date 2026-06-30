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
            var code = VerifyCS.LoadTestData("Mirage1003Tests/Valid_ReadonlySyncObjectDoesNotReportWarning.cs");
            await VerifyCS.VerifyAnalyzerAsync(code);
        }

        [Test]
        public async Task NonSyncObjectFieldNotReadonlyDoesNotReportWarning()
        {
            var code = VerifyCS.LoadTestData("Mirage1003Tests/Valid_NonSyncObjectFieldNotReadonlyDoesNotReportWarning.cs");
            await VerifyCS.VerifyAnalyzerAsync(code);
        }

        [Test]
        public async Task SyncObjectAssignedInConstructorDoesNotReportWarning()
        {
            var code = VerifyCS.LoadTestData("Mirage1003Tests/Valid_SyncObjectAssignedInConstructorDoesNotReportWarning.cs");
            await VerifyCS.VerifyAnalyzerAsync(code);
        }

        [Test]
        public async Task NonReadonlySyncObjectReportsError()
        {
            var code = VerifyCS.LoadTestData("Mirage1003Tests/Invalid_NonReadonlySyncObjectReportsError.cs");
            var expected = VerifyCS.Diagnostic("MIRAGE1003").WithLocation(0).WithArguments("mySyncList");
            await VerifyCS.VerifyAnalyzerAsync(code, expected);
        }

        [Test]
        public async Task SyncObjectReassignmentInMethodReportsError()
        {
            var code = VerifyCS.LoadTestData("Mirage1003Tests/Invalid_SyncObjectReassignmentInMethodReportsError.cs");
            var expectedField = VerifyCS.Diagnostic("MIRAGE1003").WithLocation(0).WithArguments("mySyncList");
            var expectedReassignment = VerifyCS.Diagnostic("MIRAGE1003").WithLocation(1).WithArguments("mySyncList");
            await VerifyCS.VerifyAnalyzerAsync(code, expectedField, expectedReassignment);
        }

        [Test]
        public async Task SyncObjectReassignmentInLocalFunctionInConstructorReportsError()
        {
            var code = VerifyCS.LoadTestData("Mirage1003Tests/Invalid_SyncObjectReassignmentInLocalFunctionInConstructorReportsError.cs");
            var expectedField = VerifyCS.Diagnostic("MIRAGE1003").WithLocation(0).WithArguments("mySyncList");
            var expectedReassignment = VerifyCS.Diagnostic("MIRAGE1003").WithLocation(1).WithArguments("mySyncList");
            await VerifyCS.VerifyAnalyzerAsync(code, expectedField, expectedReassignment);
        }

        [Test]
        public async Task SyncObjectReassignmentInLambdaInConstructorReportsError()
        {
            var code = VerifyCS.LoadTestData("Mirage1003Tests/Invalid_SyncObjectReassignmentInLambdaInConstructorReportsError.cs");
            var expectedField = VerifyCS.Diagnostic("MIRAGE1003").WithLocation(0).WithArguments("mySyncList");
            var expectedReassignment = VerifyCS.Diagnostic("MIRAGE1003").WithLocation(1).WithArguments("mySyncList");
            await VerifyCS.VerifyAnalyzerAsync(code, expectedField, expectedReassignment);
        }

        [Test]
        public async Task SyncObjectNotReadonlyAndReassignedInMethodReportsMultipleErrors()
        {
            var code = VerifyCS.LoadTestData("Mirage1003Tests/Invalid_SyncObjectNotReadonlyAndReassignedInMethodReportsMultipleErrors.cs");
            var expectedFieldWarning = VerifyCS.Diagnostic("MIRAGE1003").WithLocation(0).WithArguments("mySyncList");
            var expectedReassignmentError = VerifyCS.Diagnostic("MIRAGE1003").WithLocation(1).WithArguments("mySyncList");
            await VerifyCS.VerifyAnalyzerAsync(code, expectedFieldWarning, expectedReassignmentError);
        }
    }
}
