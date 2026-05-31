using NUnit.Framework;
using System.Threading.Tasks;

namespace Mirage.Analyzers.Tests
{
    [TestFixture]
    public class DirectMutationTests
    {
        private const string MockDefinitions = @"
namespace Mirage
{
    public class NetworkBehaviour {}
    public class WeaverSafeClassAttribute : System.Attribute {}
}
namespace Mirage.Collections
{
    public interface ISyncObject {}
    public class SyncList<T> : ISyncObject
    {
        public T this[int index] { get => default; set {} }
    }
    public class SyncDictionary<TKey, TValue> : ISyncObject
    {
        public TValue this[TKey key] { get => default; set {} }
    }
}
";

        [Test]
        public async Task ModifyingLocalArrayDoesNotReportWarning()
        {
            // Verify that modifying standard local array elements does not trigger MIRAGE1003
            var code = @"
using Mirage;

public class MyClass
{
    public int Value;
}

public class MyBehaviour : NetworkBehaviour
{
    public void Modify()
    {
        var array = new MyClass[5];
        array[0].Value = 10;
    }
}
" + MockDefinitions;

            await VerifyCS.VerifyAnalyzerAsync(code);
        }

        [Test]
        public async Task ModifyingStandardListDoesNotReportWarning()
        {
            // Verify that modifying standard System.Collections.Generic.List elements does not trigger MIRAGE1003
            var code = @"
using System.Collections.Generic;
using Mirage;

public class MyClass
{
    public int Value;
}

public class MyBehaviour : NetworkBehaviour
{
    public void Modify()
    {
        var list = new List<MyClass>();
        list[0].Value = 10;
    }
}
" + MockDefinitions;

            await VerifyCS.VerifyAnalyzerAsync(code);
        }

        [Test]
        public async Task AssigningEntireElementDoesNotReportWarning()
        {
            // Verify that replacing the entire element in SyncList is allowed and does not trigger MIRAGE1003
            var code = @"
using Mirage;
using Mirage.Collections;

[WeaverSafeClass]
public struct MyStruct
{
    public int Value;
}

public class MyBehaviour : NetworkBehaviour
{
    public readonly SyncList<MyStruct> mySyncList = new SyncList<MyStruct>();

    public void Modify()
    {
        mySyncList[0] = new MyStruct { Value = 10 };
    }
}
" + MockDefinitions;

            await VerifyCS.VerifyAnalyzerAsync(code);
        }

        [Test]
        public async Task ReadingElementMemberDoesNotReportWarning()
        {
            // Verify that reading a member of an element does not trigger MIRAGE1003
            var code = @"
using Mirage;
using Mirage.Collections;

public class MyClass
{
    public int Value;
}

public class MyBehaviour : NetworkBehaviour
{
    public readonly SyncList<MyClass> mySyncList = new SyncList<MyClass>();

    public void Modify()
    {
        int val = mySyncList[0].Value;
    }
}
" + MockDefinitions;

            await VerifyCS.VerifyAnalyzerAsync(code);
        }

        [Test]
        public async Task DirectMutationOfSyncListElementReportsWarning()
        {
            // Verify that directly mutating a field of a SyncList element triggers MIRAGE1003
            var code = @"
using Mirage;
using Mirage.Collections;

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

        [Test]
        public async Task DirectMutationOfSyncDictionaryElementReportsWarning()
        {
            // Verify that directly mutating a field of a SyncDictionary element triggers MIRAGE1003
            var code = @"
using Mirage;
using Mirage.Collections;

public class MyClass
{
    public int Value;
}

public class MyBehaviour : NetworkBehaviour
{
    public readonly SyncDictionary<int, MyClass> mySyncDict = new SyncDictionary<int, MyClass>();

    public void Modify()
    {
        {|#0:mySyncDict[1]|}.Value = 10;
    }
}
" + MockDefinitions;

            var expected = VerifyCS.Diagnostic("MIRAGE1003").WithLocation(0).WithArguments("mySyncDict");
            await VerifyCS.VerifyAnalyzerAsync(code, expected);
        }

        [Test]
        public async Task DirectMutationWithCompoundAssignmentReportsWarning()
        {
            // Verify that compound assignments like += on elements trigger MIRAGE1003
            var code = @"
using Mirage;
using Mirage.Collections;

public class MyClass
{
    public int Value;
}

public class MyBehaviour : NetworkBehaviour
{
    public readonly SyncList<MyClass> mySyncList = new SyncList<MyClass>();

    public void Modify()
    {
        {|#0:mySyncList[0]|}.Value += 5;
    }
}
" + MockDefinitions;

            var expected = VerifyCS.Diagnostic("MIRAGE1003").WithLocation(0).WithArguments("mySyncList");
            await VerifyCS.VerifyAnalyzerAsync(code, expected);
        }

        [Test]
        public async Task DirectMutationWithUnaryExpressionReportsWarning()
        {
            // Verify that unary operators like ++ trigger MIRAGE1003
            var code = @"
using Mirage;
using Mirage.Collections;

public class MyClass
{
    public int Value;
}

public class MyBehaviour : NetworkBehaviour
{
    public readonly SyncList<MyClass> mySyncList = new SyncList<MyClass>();

    public void Modify()
    {
        {|#0:mySyncList[0]|}.Value++;
    }
}
" + MockDefinitions;

            var expected = VerifyCS.Diagnostic("MIRAGE1003").WithLocation(0).WithArguments("mySyncList");
            await VerifyCS.VerifyAnalyzerAsync(code, expected);
        }

        [Test]
        public async Task PassingElementMemberAsRefParamReportsWarning()
        {
            // Verify that passing an element member by reference triggers MIRAGE1003
            var code = @"
using Mirage;
using Mirage.Collections;

public class MyClass
{
    public int Value;
}

public class MyBehaviour : NetworkBehaviour
{
    public readonly SyncList<MyClass> mySyncList = new SyncList<MyClass>();

    public void Modify()
    {
        Helper(ref {|#0:mySyncList[0]|}.Value);
    }

    private void Helper(ref int val)
    {
        val = 5;
    }
}
" + MockDefinitions;

            var expected = VerifyCS.Diagnostic("MIRAGE1003").WithLocation(0).WithArguments("mySyncList");
            await VerifyCS.VerifyAnalyzerAsync(code, expected);
        }

        [Test]
        public async Task NestedMemberAccessMutationReportsWarning()
        {
            // Verify that deeply nested member access mutation also triggers MIRAGE1003
            var code = @"
using Mirage;
using Mirage.Collections;

public class NestedClass
{
    public int Value;
}

public class MyClass
{
    public NestedClass Nested = new NestedClass();
}

public class MyBehaviour : NetworkBehaviour
{
    public readonly SyncList<MyClass> mySyncList = new SyncList<MyClass>();

    public void Modify()
    {
        {|#0:mySyncList[0]|}.Nested.Value = 10;
    }
}
" + MockDefinitions;

            var expected = VerifyCS.Diagnostic("MIRAGE1003").WithLocation(0).WithArguments("mySyncList");
            await VerifyCS.VerifyAnalyzerAsync(code, expected);
        }
    }
}
