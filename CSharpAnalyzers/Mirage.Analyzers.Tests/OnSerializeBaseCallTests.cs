using NUnit.Framework;
using System.Threading.Tasks;

namespace Mirage.Analyzers.Tests
{
    [TestFixture]
    public class OnSerializeBaseCallTests
    {
        [Test]
        public async Task Positive_BaseCallIncluded()
        {
            var code = VerifyCS.LoadTestData("OnSerializeBaseCall/Positive_BaseCallIncluded.cs");
            await VerifyCS.VerifyAnalyzerAsync(code);
        }

        [Test]
        public async Task Positive_NoBaseSyncState()
        {
            var code = VerifyCS.LoadTestData("OnSerializeBaseCall/Positive_NoBaseSyncState.cs");
            await VerifyCS.VerifyAnalyzerAsync(code);
        }

        [Test]
        public async Task Negative_MissingBaseOnSerialize()
        {
            var code = VerifyCS.LoadTestData("OnSerializeBaseCall/Negative_MissingBaseOnSerialize.cs");
            var expected = VerifyCS.Diagnostic("MIRAGE1402")
                .WithLocation(0)
                .WithArguments("OnSerialize");

            await VerifyCS.VerifyAnalyzerAsync(code, expected);
        }

        [Test]
        public async Task Negative_MissingBaseOnDeserialize()
        {
            var code = VerifyCS.LoadTestData("OnSerializeBaseCall/Negative_MissingBaseOnDeserialize.cs");
            var expected = VerifyCS.Diagnostic("MIRAGE1402")
                .WithLocation(0)
                .WithArguments("OnDeserialize");

            await VerifyCS.VerifyAnalyzerAsync(code, expected);
        }

        [Test]
        public async Task Edge_BaseSyncStateFromISyncObject()
        {
            var code = VerifyCS.LoadTestData("OnSerializeBaseCall/Edge_BaseSyncStateFromISyncObject.cs");
            var expected = VerifyCS.Diagnostic("MIRAGE1402")
                .WithLocation(0)
                .WithArguments("OnSerialize");

            await VerifyCS.VerifyAnalyzerAsync(code, expected);
        }
    }
}
