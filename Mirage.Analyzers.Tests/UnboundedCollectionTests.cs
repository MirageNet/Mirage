using NUnit.Framework;
using System.Threading.Tasks;

namespace Mirage.Analyzers.Tests
{
    [TestFixture]
    public class UnboundedCollectionTests
    {
        private const string MockDefinitions = @"
namespace Mirage
{
    public class NetworkMessageAttribute : System.Attribute {}
    public class NetworkBehaviour {}
    public class ServerRpcAttribute : System.Attribute {}
    public class ClientRpcAttribute : System.Attribute {}
}
namespace Mirage.Serialization
{
    public class BitCountAttribute : System.Attribute
    {
        public BitCountAttribute(int bits) {}
    }
    public class VarIntAttribute : System.Attribute {}
    public class BitCountFromRangeAttribute : System.Attribute {}
    public class VarIntBlocksAttribute : System.Attribute {}
}
";

        [Test]
        public async Task Positive_BoundedStringAndCollection()
        {
            // Verify that strings and collections with a bit packing attribute do not trigger a warning
            var code = @"
using Mirage;
using Mirage.Serialization;
using System.Collections.Generic;

[NetworkMessage]
public struct ValidMessage
{
    [BitCount(8)]
    public string Name;

    [VarInt]
    public int[] Scores;

    [BitCount(10)]
    public List<float> Positions;
}
" + MockDefinitions;

            await VerifyCS.VerifyAnalyzerAsync(code);
        }

        [Test]
        public async Task Positive_NonNetworkContext()
        {
            // Verify that unbounded strings/collections outside of NetworkMessages or RPCs do not trigger a warning
            var code = @"
using System.Collections.Generic;

public class StandardClass
{
    public string UnboundedString;
    public List<int> UnboundedList;

    public void NormalMethod(string param) {}
}
";

            await VerifyCS.VerifyAnalyzerAsync(code);
        }

        [Test]
        public async Task Negative_UnboundedFieldAndPropertyInMessage()
        {
            // Verify that unbounded string/collection fields and properties in a NetworkMessage trigger warning
            var code = @"
using Mirage;
using System.Collections.Generic;

[NetworkMessage]
public struct InvalidMessage
{
    public string {|#0:Name|};

    public int[] {|#1:Scores|};

    public List<float> {|#2:Positions|} { get; set; }
}
" + MockDefinitions;

            var expectedField1 = VerifyCS.Diagnostic("MIRAGE1502").WithLocation(0).WithArguments("Name", "String");
            var expectedField2 = VerifyCS.Diagnostic("MIRAGE1502").WithLocation(1).WithArguments("Scores", "Int32[]");
            var expectedProp = VerifyCS.Diagnostic("MIRAGE1502").WithLocation(2).WithArguments("Positions", "List");

            await VerifyCS.VerifyAnalyzerAsync(code, expectedField1, expectedField2, expectedProp);
        }

        [Test]
        public async Task Negative_UnboundedParameterInRpc()
        {
            // Verify that unbounded parameters in ServerRpc and ClientRpc trigger warnings
            var code = @"
using Mirage;
using System.Collections.Generic;

public class PlayerBehaviour : NetworkBehaviour
{
    [ServerRpc]
    public void CmdSendText(string {|#0:text|}) {}

    [ClientRpc]
    public void RpcUpdateList(List<int> {|#1:items|}) {}
}
" + MockDefinitions;

            var expectedParam1 = VerifyCS.Diagnostic("MIRAGE1502").WithLocation(0).WithArguments("text", "String");
            var expectedParam2 = VerifyCS.Diagnostic("MIRAGE1502").WithLocation(1).WithArguments("items", "List");

            await VerifyCS.VerifyAnalyzerAsync(code, expectedParam1, expectedParam2);
        }
    }
}
