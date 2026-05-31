using NUnit.Framework;
using System.Threading.Tasks;

namespace Mirage.Analyzers.Tests
{
    [TestFixture]
    public class OnSerializeBaseCallTests
    {
        private const string MockDefinitions = @"
namespace Mirage
{
    public class NetworkBehaviour
    {
        public virtual bool OnSerialize(Mirage.Serialization.NetworkWriter writer, bool initialState) => true;
        public virtual void OnDeserialize(Mirage.Serialization.NetworkReader reader, bool initialState) {}
    }
    public class SyncVarAttribute : System.Attribute {}
}
namespace Mirage.Serialization
{
    public class NetworkWriter
    {
        public void WritePackedInt32(int value) {}
    }
    public class NetworkReader {}
}
namespace Mirage.Collections
{
    public interface ISyncObject {}
    public class SyncList<T> : ISyncObject {}
}
";

        [Test]
        public async Task Positive_BaseCallIncluded()
        {
            // Verify that calling base.OnSerialize and base.OnDeserialize when base has sync state compiles and triggers no warning
            var code = @"
using Mirage;
using Mirage.Serialization;

public class BasePlayer : NetworkBehaviour
{
    [SyncVar]
    public string PlayerName { get; set; }
}

public class HeroPlayer : BasePlayer
{
    [SyncVar]
    public int HeroId { get; set; }

    public override bool OnSerialize(NetworkWriter writer, bool initialState)
    {
        bool baseDirty = base.OnSerialize(writer, initialState);
        writer.WritePackedInt32(HeroId);
        return baseDirty || true;
    }

    public override void OnDeserialize(NetworkReader reader, bool initialState)
    {
        base.OnDeserialize(reader, initialState);
    }
}
" + MockDefinitions;

            await VerifyCS.VerifyAnalyzerAsync(code);
        }

        [Test]
        public async Task Positive_NoBaseSyncState()
        {
            // Verify that overriding OnSerialize/OnDeserialize without calling base does not trigger warning if base has no sync state
            var code = @"
using Mirage;
using Mirage.Serialization;

public class EmptyBase : NetworkBehaviour
{
    // No SyncVars or ISyncObjects here
}

public class DerivedPlayer : EmptyBase
{
    [SyncVar]
    public int HeroId { get; set; }

    public override bool OnSerialize(NetworkWriter writer, bool initialState)
    {
        writer.WritePackedInt32(HeroId);
        return true;
    }

    public override void OnDeserialize(NetworkReader reader, bool initialState)
    {
    }
}
" + MockDefinitions;

            await VerifyCS.VerifyAnalyzerAsync(code);
        }

        [Test]
        public async Task Negative_MissingBaseOnSerialize()
        {
            // Verify that missing base.OnSerialize when base has sync state triggers MIRAGE1402 warning
            var code = @"
using Mirage;
using Mirage.Serialization;

public class BasePlayer : NetworkBehaviour
{
    [SyncVar]
    public string PlayerName { get; set; }
}

public class HeroPlayer : BasePlayer
{
    [SyncVar]
    public int HeroId { get; set; }

    public override bool {|#0:OnSerialize|}(NetworkWriter writer, bool initialState)
    {
        writer.WritePackedInt32(HeroId);
        return true;
    }
}
" + MockDefinitions;

            var expected = VerifyCS.Diagnostic("MIRAGE1402")
                .WithLocation(0)
                .WithArguments("OnSerialize");

            await VerifyCS.VerifyAnalyzerAsync(code, expected);
        }

        [Test]
        public async Task Negative_MissingBaseOnDeserialize()
        {
            // Verify that missing base.OnDeserialize when base has sync state triggers MIRAGE1402 warning
            var code = @"
using Mirage;
using Mirage.Serialization;

public class BasePlayer : NetworkBehaviour
{
    [SyncVar]
    public string PlayerName { get; set; }
}

public class HeroPlayer : BasePlayer
{
    [SyncVar]
    public int HeroId { get; set; }

    public override void {|#0:OnDeserialize|}(NetworkReader reader, bool initialState)
    {
    }
}
" + MockDefinitions;

            var expected = VerifyCS.Diagnostic("MIRAGE1402")
                .WithLocation(0)
                .WithArguments("OnDeserialize");

            await VerifyCS.VerifyAnalyzerAsync(code, expected);
        }

        [Test]
        public async Task Edge_BaseSyncStateFromISyncObject()
        {
            // Verify warning is triggered if base class only has ISyncObject (like SyncList) and base call is missing
            var code = @"
using Mirage;
using Mirage.Serialization;
using Mirage.Collections;

public class BasePlayer : NetworkBehaviour
{
    public SyncList<int> Scores = new SyncList<int>();
}

public class HeroPlayer : BasePlayer
{
    public override bool {|#0:OnSerialize|}(NetworkWriter writer, bool initialState)
    {
        return true;
    }
}
" + MockDefinitions;

            var expected = VerifyCS.Diagnostic("MIRAGE1402")
                .WithLocation(0)
                .WithArguments("OnSerialize");

            await VerifyCS.VerifyAnalyzerAsync(code, expected);
        }
    }
}
