using NUnit.Framework;
using System.Threading.Tasks;

namespace Mirage.Analyzers.Tests
{
    [TestFixture]
    public class SyncVarClassTests
    {
        private const string MockDefinitions = @"
namespace Mirage
{
    public class NetworkBehaviour {}
    public class SyncVarAttribute : System.Attribute {}
    public class WeaverSafeClassAttribute : System.Attribute {}
}
namespace Mirage.Collections
{
    public interface ISyncObject {}
    public class SyncList<T> : ISyncObject
    {
        public T this[int index] { get => default; set {} }
    }
}
";

        [Test]
        public async Task SafeTypeDoesNotReportWarning()
        {
            // Verify that primitive type syncvars do not trigger MIRAGE1001 warning
            var code = @"
using Mirage;

public class MyBehaviour : NetworkBehaviour
{
    [SyncVar]
    public int mySyncVar;
}
" + MockDefinitions;

            await VerifyCS.VerifyAnalyzerAsync(code);
        }

        [Test]
        public async Task ClassTypeReportsWarning()
        {
            // Verify that class type syncvars trigger MIRAGE1001 warning with the correct argument description
            var code = @"
using Mirage;

public class MyClass {}

public class MyBehaviour : NetworkBehaviour
{
    [SyncVar]
    public MyClass {|#0:mySyncVar|};
}
" + MockDefinitions;

            var expected = VerifyCS.Diagnostic("MIRAGE1001").WithLocation(0).WithArguments("mySyncVar", "MyClass");
            await VerifyCS.VerifyAnalyzerAsync(code, expected);
        }

        [Test]
        public async Task SyncObjectNotReadonlyReportsError()
        {
            var code = @"
using Mirage;
using Mirage.Collections;

public class MyBehaviour : NetworkBehaviour
{
    public SyncList<int> {|#0:mySyncList|};
}
" + MockDefinitions;

            var expected = VerifyCS.Diagnostic("MIRAGE1004").WithLocation(0).WithArguments("mySyncList");
            await VerifyCS.VerifyAnalyzerAsync(code, expected);
        }

        [Test]
        public async Task SyncObjectReassignmentReportsError()
        {
            var code = @"
using Mirage;
using Mirage.Collections;

public class MyBehaviour : NetworkBehaviour
{
    public SyncList<int> {|#0:mySyncList|} = new SyncList<int>();

    public void Modify()
    {
        {|#1:mySyncList|} = new SyncList<int>();
    }
}
" + MockDefinitions;

            var expectedField = VerifyCS.Diagnostic("MIRAGE1004").WithLocation(0).WithArguments("mySyncList");
            var expectedAssignment = VerifyCS.Diagnostic("MIRAGE1004").WithLocation(1).WithArguments("mySyncList");
            await VerifyCS.VerifyAnalyzerAsync(code, expectedField, expectedAssignment);
        }

        [Test]
        public async Task DirectMutationOfSyncListElementReportsWarning()
        {
            var code = @"
using Mirage;
using Mirage.Collections;

[WeaverSafeClass]
public class MyClass
{
    public int Value;
}

public class MyBehaviour : NetworkBehaviour
{
    public readonly SyncList<MyClass> mySyncList = new SyncList<MyClass>();

    public void Modify()
    {
        {|#0:mySyncList[0]|}.Value = 10;
    }
}
" + MockDefinitions;

            var expected = VerifyCS.Diagnostic("MIRAGE1003").WithLocation(0).WithArguments("mySyncList");
            await VerifyCS.VerifyAnalyzerAsync(code, expected);
        }
    }
}
