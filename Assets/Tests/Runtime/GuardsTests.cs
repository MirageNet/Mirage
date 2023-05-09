using System.Collections.Generic;
using NUnit.Framework;

namespace Mirage.Tests.Runtime.GuardTests
{
    public class ExampleGuards : NetworkBehaviour
    {
        public const int RETURN_VALUE = 10;
        public const int OUT_VALUE_1 = 20;
        public const int OUT_VALUE_2 = 20;

        // Define a list to keep track of all method calls
        public readonly List<string> Calls = new List<string>();

        [Server]
        public void CallServerFunction()
        {
            Calls.Add(nameof(CallServerFunction));
        }

        [Server(error = false)]
        public void CallServerCallbackFunction()
        {
            Calls.Add(nameof(CallServerCallbackFunction));
        }

        [Client]
        public void CallClientFunction()
        {
            Calls.Add(nameof(CallClientFunction));
        }

        [Client(error = false)]
        public void CallClientCallbackFunction()
        {
            Calls.Add(nameof(CallClientCallbackFunction));
        }

        [HasAuthority]
        public void CallAuthorityFunction()
        {
            Calls.Add(nameof(CallAuthorityFunction));
        }

        [HasAuthority(error = false)]
        public void CallAuthorityNoErrorFunction()
        {
            Calls.Add(nameof(CallAuthorityNoErrorFunction));
        }

        [LocalPlayer]
        public void CallLocalPlayer()
        {
            Calls.Add(nameof(CallLocalPlayer));
        }

        [LocalPlayer(error = false)]
        public void CallLocalPlayerNoError()
        {
            Calls.Add(nameof(CallLocalPlayerNoError));
        }


        [Server]
        public int CallServerFunction_Return()
        {
            Calls.Add(nameof(CallServerFunction_Return));
            return RETURN_VALUE;
        }

        [Server(error = false)]
        public int CallServerCallbackFunction_Return()
        {
            Calls.Add(nameof(CallServerCallbackFunction_Return));
            return RETURN_VALUE;
        }

        [Server]
        public void CallServerFunction_Out(out int outValue)
        {
            Calls.Add(nameof(CallServerFunction_Out));
            outValue = OUT_VALUE_1;
        }

        [Server(error = false)]
        public void CallServerCallbackFunction_Out(out int outValue)
        {
            Calls.Add(nameof(CallServerCallbackFunction_Out));
            outValue = OUT_VALUE_1;
        }

        [Server]
        public void CallServerFunction_Ref(ref int outValue)
        {
            Calls.Add(nameof(CallServerFunction_Ref));
            outValue = OUT_VALUE_1;
        }

        [Server(error = false)]
        public void CallServerCallbackFunction_Ref(ref int outValue)
        {
            Calls.Add(nameof(CallServerCallbackFunction_Ref));
            outValue = OUT_VALUE_1;
        }

        [Server]
        public void CallServerFunction_RefArray(ref int[] outValue)
        {
            Calls.Add(nameof(CallServerFunction_RefArray));
            outValue = new int[1] { OUT_VALUE_1 };
        }

        [Server(error = false)]
        public void CallServerCallbackFunction_RefArray(ref int[] outValue)
        {
            Calls.Add(nameof(CallServerCallbackFunction_RefArray));
            outValue = new int[1] { OUT_VALUE_1 };
        }

        [Server]
        public void CallServerFunction_Out2(out int outValue1, out int outValue2)
        {
            Calls.Add(nameof(CallServerFunction_Out2));
            outValue1 = OUT_VALUE_1;
            outValue2 = OUT_VALUE_2;
        }

        [Server(error = false)]
        public void CallServerCallbackFunction_Out2(out int outValue1, out int outValue2)
        {
            Calls.Add(nameof(CallServerCallbackFunction_Out2));
            outValue1 = OUT_VALUE_1;
            outValue2 = OUT_VALUE_2;
        }

        [Server]
        public void CallServerFunction_OutExampleGuards(out ExampleGuards exampleGuards)
        {
            Calls.Add(nameof(CallServerFunction_OutExampleGuards));
            exampleGuards = this;
        }

        [Server(error = false)]
        public void CallServerCallbackFunction_OutExampleGuards(out ExampleGuards exampleGuards)
        {
            Calls.Add(nameof(CallServerCallbackFunction_OutExampleGuards));
            exampleGuards = null;
        }

        [Server(error = false)]
        public void CallServerCallbackFunction_Generic(out List<int> outValue)
        {
            Calls.Add(nameof(CallServerCallbackFunction_Generic));
            outValue = new List<int>();
        }
        [Server(error = false)]
        public void CallServerCallbackFunction_Array(out int[] outValue)
        {
            Calls.Add(nameof(CallServerCallbackFunction_Array));
            outValue = new int[10];
        }
        [Server(error = false)]
        public void CallServerCallbackFunction_GenericStruct<T>(out GenericStruct<T> outValue, T value)
        {
            Calls.Add(nameof(CallServerCallbackFunction_GenericStruct));
            outValue = new GenericStruct<T> { Value = value };
        }
    }
    public struct GenericStruct<T>
    {
        public T Value;
    }

    public class GuardsTests : ClientServerSetup<ExampleGuards>
    {
        [Test]
        public void CanCallServerFunctionAsServer()
        {
            serverComponent.CallServerFunction();
            Assert.That(serverComponent.Calls, Has.Count.EqualTo(1));
            Assert.That(serverComponent.Calls, Does.Contain(nameof(ExampleGuards.CallServerFunction)));
        }

        [Test]
        public void CanCallServerFunctionCallbackAsServer()
        {
            serverComponent.CallServerCallbackFunction();
            Assert.That(serverComponent.Calls, Has.Count.EqualTo(1));
            Assert.That(serverComponent.Calls, Does.Contain(nameof(ExampleGuards.CallServerCallbackFunction)));
        }

        [Test]
        public void CannotCallClientFunctionAsServer()
        {
            Assert.Throws<MethodInvocationException>(() =>
            {
                serverComponent.CallClientFunction();
            });
        }

        [Test]
        public void CannotCallClientCallbackFunctionAsServer()
        {
            serverComponent.CallClientCallbackFunction();
            Assert.That(serverComponent.Calls, Is.Empty);
        }

        [Test]
        public void CannotCallServerFunctionAsClient()
        {
            Assert.Throws<MethodInvocationException>(() =>
            {
                clientComponent.CallServerFunction();
            });
            Assert.That(clientComponent.Calls, Is.Empty);
        }

        [Test]
        public void CannotCallServerFunctionCallbackAsClient()
        {
            clientComponent.CallServerCallbackFunction();
            Assert.That(clientComponent.Calls, Is.Empty);
        }


        [Test]
        public void CannotCallServerFunctionAsClient_Return()
        {
            Assert.Throws<MethodInvocationException>(() =>
            {
                clientComponent.CallServerFunction_Return();
            });
            Assert.That(clientComponent.Calls, Is.Empty);
        }

        [Test]
        public void CannotCallServerCallbackFunctionAsClient_Return()
        {
            var returnValue = clientComponent.CallServerCallbackFunction_Return();
            Assert.That(clientComponent.Calls, Is.Empty);
            Assert.That(returnValue, Is.EqualTo(default(int)));
        }

        [Test]
        public void CannotCallServerFunctionAsClient_Out()
        {
            int outValue = default;
            Assert.Throws<MethodInvocationException>(() =>
            {
                clientComponent.CallServerFunction_Out(out outValue);
            });
            Assert.That(clientComponent.Calls, Is.Empty);
            Assert.That(outValue, Is.EqualTo(default(int)));
        }

        [Test]
        public void CannotCallServerCallbackFunctionAsClient_Out()
        {
            int outValue;
            clientComponent.CallServerCallbackFunction_Out(out outValue);
            Assert.That(clientComponent.Calls, Is.Empty);
            Assert.That(outValue, Is.EqualTo(default(int)));
        }

        [Test]
        public void CannotCallServerFunctionAsClient_Ref()
        {
            var outValue = 2;
            var startValue = outValue;
            Assert.Throws<MethodInvocationException>(() =>
            {
                clientComponent.CallServerFunction_Ref(ref outValue);
            });
            Assert.That(clientComponent.Calls, Is.Empty);
            // ref should not be changed
            Assert.That(outValue, Is.EqualTo(startValue));
        }

        [Test]
        public void CannotCallServerCallbackFunctionAsClient_Ref()
        {
            var outValue = 2;
            var startValue = outValue;
            clientComponent.CallServerCallbackFunction_Ref(ref outValue);
            Assert.That(clientComponent.Calls, Is.Empty);
            // ref should not be changed
            Assert.That(outValue, Is.EqualTo(startValue));// same ref
        }

        [Test]
        public void CannotCallServerFunctionAsClient_RefArray()
        {
            var outValue = new int[2];
            var startValue = outValue;
            Assert.Throws<MethodInvocationException>(() =>
            {
                clientComponent.CallServerFunction_RefArray(ref outValue);
            });
            Assert.That(clientComponent.Calls, Is.Empty);
            // ref should not be changed
            Assert.That(outValue, Is.EqualTo(startValue));// same ref
        }

        [Test]
        public void CannotCallServerCallbackFunctionAsClient_RefArray()
        {
            var outValue = new int[2];
            var startValue = outValue;

            clientComponent.CallServerCallbackFunction_RefArray(ref outValue);
            // ref should not be changed
            Assert.That(outValue, Is.EqualTo(startValue));// same ref
            Assert.That(outValue, Has.Length.EqualTo(2));
        }

        [Test]
        public void CannotCallServerFunctionAsClient_Out2()
        {
            int outValue1 = default, outValue2 = default;
            Assert.Throws<MethodInvocationException>(() =>
            {
                clientComponent.CallServerFunction_Out2(out outValue1, out outValue2);
            });
            Assert.That(clientComponent.Calls, Is.Empty);
            Assert.That(outValue1, Is.EqualTo(default(int)));
            Assert.That(outValue2, Is.EqualTo(default(int)));
        }

        [Test]
        public void CannotCallServerCallbackFunctionAsClient_Out2()
        {
            int outValue1, outValue2;
            clientComponent.CallServerCallbackFunction_Out2(out outValue1, out outValue2);
            Assert.That(clientComponent.Calls, Is.Empty);
            Assert.That(outValue1, Is.EqualTo(default(int)));
            Assert.That(outValue2, Is.EqualTo(default(int)));
        }

        [Test]
        public void CannotCallServerFunctionAsClient_OutExampleGuards()
        {
            ExampleGuards exampleGuards = default;
            Assert.Throws<MethodInvocationException>(() =>
            {
                clientComponent.CallServerFunction_OutExampleGuards(out exampleGuards);
            });
            Assert.That(clientComponent.Calls, Is.Empty);
            Assert.That(exampleGuards, Is.EqualTo(default(ExampleGuards)));
        }

        [Test]
        public void CannotCallServerCallbackFunctionAsClient_OutExampleGuards()
        {
            ExampleGuards exampleGuards;
            clientComponent.CallServerCallbackFunction_OutExampleGuards(out exampleGuards);
            Assert.That(clientComponent.Calls, Is.Empty);
            Assert.That(exampleGuards, Is.EqualTo(default(ExampleGuards)));
        }
        [Test]
        public void CannotCallServerCallbackFunctionAsClient_Generic()
        {
            clientComponent.CallServerCallbackFunction_Generic(out var outValue);
            Assert.That(clientComponent.Calls, Is.Empty);
            Assert.That(outValue, Is.EqualTo(null));
        }
        [Test]
        public void CannotCallServerCallbackFunctionAsClient_Array()
        {
            clientComponent.CallServerCallbackFunction_Array(out var outValue);
            Assert.That(clientComponent.Calls, Is.Empty);
            Assert.That(outValue, Is.EqualTo(null));
        }
        [Test]
        public void CannotCallServerCallbackFunctionAsClient_GenericStruct()
        {
            GenericStruct<int> outValue;
            clientComponent.CallServerCallbackFunction_GenericStruct<int>(out outValue, ExampleGuards.OUT_VALUE_1);
            Assert.That(clientComponent.Calls, Is.Empty);
            Assert.That(outValue, Is.EqualTo(default(GenericStruct<int>)));
        }

        [Test]
        public void CanCallClientFunctionAsClient()
        {
            clientComponent.CallClientFunction();
            Assert.That(clientComponent.Calls, Does.Contain(nameof(ExampleGuards.CallClientFunction)));
        }

        [Test]
        public void CanCallClientCallbackFunctionAsClient()
        {
            clientComponent.CallClientCallbackFunction();
            Assert.That(clientComponent.Calls, Does.Contain(nameof(ExampleGuards.CallClientCallbackFunction)));
        }

        [Test]
        public void CanCallHasAuthorityFunctionAsClient()
        {
            clientComponent.CallAuthorityFunction();
            Assert.That(clientComponent.Calls, Does.Contain(nameof(ExampleGuards.CallAuthorityFunction)));
        }

        [Test]
        public void CanCallHasAuthorityCallbackFunctionAsClient()
        {
            clientComponent.CallAuthorityNoErrorFunction();
            Assert.That(clientComponent.Calls, Does.Contain(nameof(ExampleGuards.CallAuthorityNoErrorFunction)));
        }

        [Test]
        public void GuardHasAuthorityError()
        {
            var guardedComponent = CreateBehaviour<ExampleGuards>();
            Assert.Throws<MethodInvocationException>(() =>
            {
                guardedComponent.CallAuthorityFunction();
            });
        }

        [Test]
        public void GuardHasAuthorityNoError()
        {
            var guardedComponent = CreateBehaviour<ExampleGuards>();
            guardedComponent.CallAuthorityNoErrorFunction();
            Assert.That(guardedComponent.Calls, Does.Not.Contain(nameof(ExampleGuards.CallAuthorityNoErrorFunction)));
        }

        [Test]
        public void CanCallLocalPlayer()
        {
            clientComponent.CallLocalPlayer();
            Assert.That(clientComponent.Calls, Does.Contain(nameof(ExampleGuards.CallLocalPlayer)));
        }

        [Test]
        public void CanCallLocalPlayerNoError()
        {
            clientComponent.CallLocalPlayerNoError();
            Assert.That(clientComponent.Calls, Does.Contain(nameof(ExampleGuards.CallLocalPlayerNoError)));
        }

        [Test]
        public void GuardLocalPlayer()
        {
            var guardedComponent = CreateBehaviour<ExampleGuards>();

            Assert.Throws<MethodInvocationException>(() =>
            {
                guardedComponent.CallLocalPlayer();

            });
        }

        [Test]
        public void GuardLocalPlayerNoError()
        {
            var guardedComponent = CreateBehaviour<ExampleGuards>();

            guardedComponent.CallLocalPlayerNoError();
            Assert.That(guardedComponent.Calls, Does.Not.Contain(nameof(ExampleGuards.CallLocalPlayerNoError)));
        }

    }
}
