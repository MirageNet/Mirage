using NUnit.Framework;
using System.Threading.Tasks;

namespace Mirage.Analyzers.Tests
{
    [TestFixture]
    public class FieldSerializationTests
    {
        private const string MockDefinitions = @"
namespace Mirage
{
    public class NetworkMessageAttribute : System.Attribute {}
    public class WeaverSafeClassAttribute : System.Attribute {}
    public class ServerRpcAttribute : System.Attribute {}
    public class ClientRpcAttribute : System.Attribute {}
    public class NetworkBehaviour {}
}

namespace Mirage.Serialization
{
    public class NetworkWriter {}
    public class NetworkReader {}
}

namespace Cysharp.Threading.Tasks
{
    public struct UniTask {}
    public struct UniTask<T> {}
}

namespace UnityEngine
{
    public class GameObject {}
    public struct Vector3 {}
}
";

        [Test]
        public async Task PrimitiveAndSupportedTypesDoNotReportError()
        {
            var code = @"
using Mirage;
using UnityEngine;
using System;

[NetworkMessage]
public struct ValidMessage
{
    public int myInt;
    public string myString;
    public Vector3 myVector;
    public Guid myGuid;
    public DateTime myDateTime;
    public byte[] myByteArray;
}
" + MockDefinitions;

            await VerifyCS.VerifyAnalyzerAsync(code);
        }

        [Test]
        public async Task CustomTypeWithSerializerDoesNotReportError()
        {
            var code = @"
using Mirage;
using Mirage.Serialization;

public struct CustomType
{
    public int value;
}

public static class CustomSerialization
{
    public static void WriteCustomType(this NetworkWriter writer, CustomType value) {}
    public static CustomType ReadCustomType(this NetworkReader reader) => default;
}

[NetworkMessage]
public struct ValidMessage
{
    public CustomType customValue;
}
" + MockDefinitions;

            await VerifyCS.VerifyAnalyzerAsync(code);
        }

        [Test]
        public async Task PrivateFieldsAreIgnored()
        {
            var code = @"
using Mirage;
using System.Threading;

[NetworkMessage]
public struct MessageWithPrivateField
{
    private Thread executionThread;
}
" + MockDefinitions;

            await VerifyCS.VerifyAnalyzerAsync(code);
        }

        [Test]
        public async Task UnserializableFieldReportsError()
        {
            var code = @"
using Mirage;
using System.Threading;

[NetworkMessage]
public struct StartSessionMessage
{
    public Thread {|#0:executionThread|};
}
" + MockDefinitions;

            var expected = VerifyCS.Diagnostic("MIRAGE1302")
                .WithLocation(0)
                .WithArguments("Thread", "NetworkMessage field");

            // Note: Since Thread is a class type without WeaverSafeClass attribute, it also triggers MIRAGE1301.
            var expectedClassWarning = VerifyCS.Diagnostic("MIRAGE1301")
                .WithLocation(0)
                .WithArguments("NetworkMessage field", "executionThread", "Thread");

            await VerifyCS.VerifyAnalyzerAsync(code, expected, expectedClassWarning);
        }

        [Test]
        public async Task UnserializablePropertyReportsError()
        {
            var code = @"
using Mirage;
using System.Threading;

[NetworkMessage]
public struct StartSessionMessage
{
    public Thread {|#0:ExecutionThread|} { get; set; }
}
" + MockDefinitions;

            var expected = VerifyCS.Diagnostic("MIRAGE1302")
                .WithLocation(0)
                .WithArguments("Thread", "NetworkMessage property");

            var expectedClassWarning = VerifyCS.Diagnostic("MIRAGE1301")
                .WithLocation(0)
                .WithArguments("NetworkMessage property", "ExecutionThread", "Thread");

            await VerifyCS.VerifyAnalyzerAsync(code, expected, expectedClassWarning);
        }

        [Test]
        public async Task UnserializableRpcParameterReportsError()
        {
            var code = @"
using Mirage;
using System.Threading;

public class MyBehaviour : NetworkBehaviour
{
    [ServerRpc]
    public void CmdStartSession(Thread {|#0:executionThread|}) {}
}
" + MockDefinitions;

            var expected = VerifyCS.Diagnostic("MIRAGE1302")
                .WithLocation(0)
                .WithArguments("Thread", "RPC parameter");

            var expectedClassWarning = VerifyCS.Diagnostic("MIRAGE1301")
                .WithLocation(0)
                .WithArguments("RPC parameter", "executionThread", "Thread");

            await VerifyCS.VerifyAnalyzerAsync(code, expected, expectedClassWarning);
        }

        [Test]
        public async Task UnserializableRpcReturnTypeReportsError()
        {
            var code = @"
using Mirage;
using System.Threading;
using Cysharp.Threading.Tasks;

public class MyBehaviour : NetworkBehaviour
{
    [ServerRpc]
    public UniTask<Thread> {|#0:CmdGetSession|}() => default;
}
" + MockDefinitions;

            var expected = VerifyCS.Diagnostic("MIRAGE1302")
                .WithLocation(0)
                .WithArguments("Thread", "RPC return type");

            var expectedClassWarning = VerifyCS.Diagnostic("MIRAGE1301")
                .WithLocation(0)
                .WithArguments("RPC return type", "CmdGetSession", "Thread");

            await VerifyCS.VerifyAnalyzerAsync(code, expected, expectedClassWarning);
        }

        [Test]
        public async Task StructWithUnserializableFieldReportsError()
        {
            var code = @"
using Mirage;
using System.Threading;

public struct NestedUnserializable
{
    public Thread threadField;
}

[NetworkMessage]
public struct MainMessage
{
    public NestedUnserializable {|#0:nestedField|};
}
" + MockDefinitions;

            var expected = VerifyCS.Diagnostic("MIRAGE1302")
                .WithLocation(0)
                .WithArguments("NestedUnserializable", "NetworkMessage field");

            await VerifyCS.VerifyAnalyzerAsync(code, expected);
        }

        [Test]
        public async Task RecursiveTypeReportsError()
        {
            var code = @"
using Mirage;

[WeaverSafeClass]
public class RecursiveClass
{
    public RecursiveClass {|#0:self|};
}

[NetworkMessage]
public struct Message
{
    public RecursiveClass {|#1:recursiveField|};
}
" + MockDefinitions;

            var expected1 = VerifyCS.Diagnostic("MIRAGE1302")
                .WithLocation(0)
                .WithArguments("RecursiveClass", "NetworkMessage field");

            var expected2 = VerifyCS.Diagnostic("MIRAGE1302")
                .WithLocation(1)
                .WithArguments("RecursiveClass", "NetworkMessage field");

            await VerifyCS.VerifyAnalyzerAsync(code, expected1, expected2);
        }

        [Test]
        public async Task MultiDimensionalArrayReportsError()
        {
            var code = @"
using Mirage;

[NetworkMessage]
public struct MessageWithMultiArray
{
    public int[,] {|#0:multiArray|};
}
" + MockDefinitions;

            var expected = VerifyCS.Diagnostic("MIRAGE1302")
                .WithLocation(0)
                .WithArguments("Int32", "NetworkMessage field");

            await VerifyCS.VerifyAnalyzerAsync(code, expected);
        }

        [Test]
        public async Task WeaverSafeClassWithUnserializableFieldReportsError()
        {
            var code = @"
using Mirage;
using System.Threading;

[WeaverSafeClass]
public class SafeClassWithThread
{
    public Thread threadField;
}

[NetworkMessage]
public struct Message
{
    public SafeClassWithThread {|#0:safeClassField|};
}
" + MockDefinitions;

            var expected = VerifyCS.Diagnostic("MIRAGE1302")
                .WithLocation(0)
                .WithArguments("SafeClassWithThread", "NetworkMessage field");

            await VerifyCS.VerifyAnalyzerAsync(code, expected);
        }
    }
}
