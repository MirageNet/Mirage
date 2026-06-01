using NUnit.Framework;
using System.Threading.Tasks;

namespace Mirage.Analyzers.Tests
{
    [TestFixture]
    public class SyncVarAutoPropertyTests
    {
        [Test]
        public async Task ValidAutoPropertyDoesNotReportError()
        {
            var code = VerifyCS.LoadTestData("SyncVars/ValidAutoProperty.cs");
            await VerifyCS.VerifyAnalyzerAsync(code);
        }

        [Test]
        public async Task NonAutoPropertyReportsError()
        {
            var code = VerifyCS.LoadTestData("SyncVars/NonAutoProperty.cs");
            var expected = VerifyCS.Diagnostic("MIRAGE1002").WithLocation(0).WithArguments("MySyncVar");
            await VerifyCS.VerifyAnalyzerAsync(code, expected);
        }

        [Test]
        public async Task StaticPropertyReportsError()
        {
            var code = VerifyCS.LoadTestData("SyncVars/StaticProperty.cs");
            var expected = VerifyCS.Diagnostic("MIRAGE1002").WithLocation(0).WithArguments("MySyncVar");
            await VerifyCS.VerifyAnalyzerAsync(code, expected);
        }

        [Test]
        public async Task MissingSetterPropertyReportsError()
        {
            var code = VerifyCS.LoadTestData("SyncVars/MissingSetterProperty.cs");
            var expected = VerifyCS.Diagnostic("MIRAGE1002").WithLocation(0).WithArguments("MySyncVar");
            await VerifyCS.VerifyAnalyzerAsync(code, expected);
        }

        [Test]
        public async Task MissingGetterPropertyReportsError()
        {
            var code = VerifyCS.LoadTestData("SyncVars/MissingGetterProperty.cs");
            var expected = VerifyCS.Diagnostic("MIRAGE1002").WithLocation(0).WithArguments("MySyncVar");
            await VerifyCS.VerifyAnalyzerAsync(code, expected);
        }
    }
}
