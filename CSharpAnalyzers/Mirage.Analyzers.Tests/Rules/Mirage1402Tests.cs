using NUnit.Framework;
using System.Threading.Tasks;

namespace Mirage.Analyzers.Tests
{
    [TestFixture]
    public class Mirage1402Tests
    {
        [Test]
        public async Task Invalid_BaseCallIncluded()
        {
            var code = VerifyCS.LoadTestData("Mirage1402Tests/Invalid_BaseCallIncluded.cs");
            await VerifyCS.VerifyAnalyzerAsync(code);
        }

        [Test]
        public async Task Invalid_NoBaseSyncState()
        {
            var code = VerifyCS.LoadTestData("Mirage1402Tests/Invalid_NoBaseSyncState.cs");
            await VerifyCS.VerifyAnalyzerAsync(code);
        }

        [Test]
        public async Task Valid_MissingBaseOnSerialize()
        {
            var code = VerifyCS.LoadTestData("Mirage1402Tests/Valid_MissingBaseOnSerialize.cs");
            var expected = VerifyCS.Diagnostic("MIRAGE1402")
                .WithLocation(0)
                .WithArguments("OnSerialize");

            await VerifyCS.VerifyAnalyzerAsync(code, expected);
        }

        [Test]
        public async Task Valid_MissingBaseOnDeserialize()
        {
            var code = VerifyCS.LoadTestData("Mirage1402Tests/Valid_MissingBaseOnDeserialize.cs");
            var expected = VerifyCS.Diagnostic("MIRAGE1402")
                .WithLocation(0)
                .WithArguments("OnDeserialize");

            await VerifyCS.VerifyAnalyzerAsync(code, expected);
        }

        [Test]
        public async Task Edge_BaseSyncStateFromISyncObject()
        {
            var code = VerifyCS.LoadTestData("Mirage1402Tests/Invalid_Edge_BaseSyncStateFromISyncObject.cs");
            var expected = VerifyCS.Diagnostic("MIRAGE1402")
                .WithLocation(0)
                .WithArguments("OnSerialize");

            await VerifyCS.VerifyAnalyzerAsync(code, expected);
        }
    }
}
