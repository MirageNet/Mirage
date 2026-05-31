using NUnit.Framework;
using System.Threading.Tasks;

namespace Mirage.Analyzers.Tests
{
    [TestFixture]
    public class AwakeStartNetworkStateTests
    {
        private const string MockDefinitions = @"
namespace Mirage
{
    public class NetworkBehaviour
    {
        public bool IsServer { get; }
        public bool IsClient { get; }
        public bool HasAuthority { get; }
        public bool IsLocalPlayer { get; }
        public bool IsOwner { get; }
        public bool IsHost { get; }
    }
    public class SyncVarAttribute : System.Attribute {}
}
";

        [Test]
        public async Task Positive_AccessInAllowedMethods()
        {
            // Verify that accessing network state in methods other than Awake/Start does not trigger warning
            var code = @"
using Mirage;

public class ValidBehaviour : NetworkBehaviour
{
    [SyncVar]
    public int Health { get; set; }

    [SyncVar]
    public int Points;

    public void Update()
    {
        if (IsServer)
        {
            Health = 100;
            Points = 10;
        }
    }

    public override void OnStartServer()
    {
        if (IsClient)
        {
            var h = Health;
        }
    }
}
" + MockDefinitions;

            await VerifyCS.VerifyAnalyzerAsync(code);
        }

        [Test]
        public async Task Positive_NonNetworkBehaviourClass()
        {
            // Verify that a class not inheriting from NetworkBehaviour can use Awake/Start with fields/properties of similar names
            var code = @"
public class NonNetworkClass
{
    public bool IsServer { get; set; }
    public int Health { get; set; }

    private void Awake()
    {
        if (IsServer)
        {
            Health = 100;
        }
    }
}
";

            await VerifyCS.VerifyAnalyzerAsync(code);
        }

        [Test]
        public async Task Negative_AccessIsServerInAwake()
        {
            // Verify accessing IsServer in Awake triggers MIRAGE1401 warning
            var code = @"
using Mirage;

public class MyBehaviour : NetworkBehaviour
{
    private void Awake()
    {
        if ({|#0:IsServer|})
        {
        }
    }
}
" + MockDefinitions;

            var expected = VerifyCS.Diagnostic("MIRAGE1401")
                .WithLocation(0)
                .WithArguments("IsServer", "Awake");

            await VerifyCS.VerifyAnalyzerAsync(code, expected);
        }

        [Test]
        public async Task Negative_AccessSyncVarPropertyInStart()
        {
            // Verify accessing SyncVar property in Start triggers MIRAGE1401 warning
            var code = @"
using Mirage;

public class MyBehaviour : NetworkBehaviour
{
    [SyncVar]
    public int Health { get; set; }

    private void Start()
    {
        {|#0:Health|} = 100;
    }
}
" + MockDefinitions;

            var expected = VerifyCS.Diagnostic("MIRAGE1401")
                .WithLocation(0)
                .WithArguments("Health", "Start");

            await VerifyCS.VerifyAnalyzerAsync(code, expected);
        }

        [Test]
        public async Task Negative_AccessSyncVarFieldInAwake()
        {
            // Verify accessing SyncVar field in Awake triggers MIRAGE1401 warning
            var code = @"
using Mirage;

public class MyBehaviour : NetworkBehaviour
{
    [SyncVar]
    public int points;

    private void Awake()
    {
        var p = {|#0:points|};
    }
}
" + MockDefinitions;

            var expected = VerifyCS.Diagnostic("MIRAGE1401")
                .WithLocation(0)
                .WithArguments("points", "Awake");

            await VerifyCS.VerifyAnalyzerAsync(code, expected);
        }

        [Test]
        public async Task Edge_NonSyncVarAccessInAwakeStart()
        {
            // Verify that non-SyncVar fields and properties can be accessed in Awake/Start
            var code = @"
using Mirage;

public class MyBehaviour : NetworkBehaviour
{
    public int NonSyncField;
    public int NonSyncProp { get; set; }

    private void Awake()
    {
        NonSyncField = 5;
        NonSyncProp = 10;
    }
}
" + MockDefinitions;

            await VerifyCS.VerifyAnalyzerAsync(code);
        }
    }
}
