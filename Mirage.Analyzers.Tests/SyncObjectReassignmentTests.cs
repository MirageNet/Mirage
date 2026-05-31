using NUnit.Framework;
using System.Threading.Tasks;

namespace Mirage.Analyzers.Tests
{
    [TestFixture]
    public class SyncObjectReassignmentTests
    {
        private const string MockDefinitions = @"
namespace Mirage
{
    public class NetworkBehaviour {}
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
        public async Task ReadonlySyncObjectDoesNotReportWarning()
        {
            // Verify that marking an ISyncObject field as readonly does not trigger MIRAGE1004
            var code = @"
using Mirage;
using Mirage.Collections;

public class MyBehaviour : NetworkBehaviour
{
    public readonly SyncList<int> mySyncList = new SyncList<int>();
}
" + MockDefinitions;

            await VerifyCS.VerifyAnalyzerAsync(code);
        }

        [Test]
        public async Task NonSyncObjectFieldNotReadonlyDoesNotReportWarning()
        {
            // Verify that a regular field not implementing ISyncObject is allowed to not be readonly
            var code = @"
using Mirage;

public class MyBehaviour : NetworkBehaviour
{
    public int myNormalField = 0;

    public void Modify()
    {
        myNormalField = 10;
    }
}
" + MockDefinitions;

            await VerifyCS.VerifyAnalyzerAsync(code);
        }

        [Test]
        public async Task SyncObjectAssignedInConstructorDoesNotReportWarning()
        {
            // Verify that assigning/initializing ISyncObject in constructor does not trigger MIRAGE1004
            var code = @"
using Mirage;
using Mirage.Collections;

public class MyBehaviour : NetworkBehaviour
{
    public readonly SyncList<int> mySyncList;

    public MyBehaviour()
    {
        mySyncList = new SyncList<int>();
    }
}
" + MockDefinitions;

            await VerifyCS.VerifyAnalyzerAsync(code);
        }

        [Test]
        public async Task NonReadonlySyncObjectReportsError()
        {
            // Verify that an ISyncObject field not marked readonly triggers MIRAGE1004 on field declaration
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
        public async Task SyncObjectReassignmentInMethodReportsError()
        {
            // Verify that reassigning an ISyncObject field outside constructor triggers MIRAGE1004
            var code = @"
using Mirage;
using Mirage.Collections;

public class MyBehaviour : NetworkBehaviour
{
    public readonly SyncList<int> mySyncList = new SyncList<int>();

    public void ResetList()
    {
        {|#0:mySyncList|} = new SyncList<int>();
    }
}
" + MockDefinitions;

            var expected = VerifyCS.Diagnostic("MIRAGE1004").WithLocation(0).WithArguments("mySyncList");
            await VerifyCS.VerifyAnalyzerAsync(code, expected);
        }

        [Test]
        public async Task SyncObjectReassignmentInLocalFunctionInConstructorReportsError()
        {
            // Verify that reassigning an ISyncObject inside a local function inside constructor triggers MIRAGE1004
            var code = @"
using Mirage;
using Mirage.Collections;

public class MyBehaviour : NetworkBehaviour
{
    public readonly SyncList<int> mySyncList = new SyncList<int>();

    public MyBehaviour()
    {
        void LocalMethod()
        {
            {|#0:mySyncList|} = new SyncList<int>();
        }
        LocalMethod();
    }
}
" + MockDefinitions;

            var expected = VerifyCS.Diagnostic("MIRAGE1004").WithLocation(0).WithArguments("mySyncList");
            await VerifyCS.VerifyAnalyzerAsync(code, expected);
        }

        [Test]
        public async Task SyncObjectReassignmentInLambdaInConstructorReportsError()
        {
            // Verify that reassigning an ISyncObject inside a lambda expression inside constructor triggers MIRAGE1004
            var code = @"
using System;
using Mirage;
using Mirage.Collections;

public class MyBehaviour : NetworkBehaviour
{
    public readonly SyncList<int> mySyncList = new SyncList<int>();

    public MyBehaviour()
    {
        Action act = () => {
            {|#0:mySyncList|} = new SyncList<int>();
        };
        act();
    }
}
" + MockDefinitions;

            var expected = VerifyCS.Diagnostic("MIRAGE1004").WithLocation(0).WithArguments("mySyncList");
            await VerifyCS.VerifyAnalyzerAsync(code, expected);
        }
    }
}
