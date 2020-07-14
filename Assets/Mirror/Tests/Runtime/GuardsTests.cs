using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Mirror.Tests
{
    public class ExampleGuards : NetworkBehaviour
    {
        public bool serverFunctionCalled;
        public bool serverCallbackFunctionCalled;
        public bool clientFunctionCalled;
        public bool clientCallbackFunctionCalled;
        public bool localPlayerCalled;
        public bool localPlayerNoErrorCalled;

        [Server]
        public void CallServerFunction()
        {
            serverFunctionCalled = true;
        }

        [Server(error=false)]
        public void CallServerCallbackFunction()
        {
            serverCallbackFunctionCalled = true;
        }

        [Client]
        public void CallClientFunction()
        {
            clientFunctionCalled = true;
        }

        [ClientCallback]
        public void CallClientCallbackFunction()
        {
            clientCallbackFunctionCalled = true;
        }

        [LocalPlayer]
        public void CallLocalPlayer()
        {
            localPlayerCalled = true;
        }

        [LocalPlayer(error = false)]
        public void CallLocalPlayerNoError()
        {
            localPlayerNoErrorCalled = true;
        }

    }

    public class GuardsTests : ClientServerSetup<ExampleGuards>
    {

        [Test]
        public void CanCallServerFunctionAsServer()
        {
            serverComponent.CallServerFunction();
            Assert.That(serverComponent.serverFunctionCalled, Is.True);
        }

        [Test]
        public void CanCallServerFunctionCallbackAsServer()
        {
            serverComponent.CallServerCallbackFunction();
            Assert.That(serverComponent.serverCallbackFunctionCalled, Is.True);
        }

        [Test]
        public void CannotCallClientFunctionAsServer()
        {
            serverComponent.CallClientFunction();
            LogAssert.Expect(UnityEngine.LogType.Warning, "[Client] function 'System.Void Mirror.Tests.ExampleGuards::CallClientFunction()' called on server");
            Assert.That(serverComponent.clientFunctionCalled, Is.False);
        }

        [Test]
        public void CannotCallClientCallbackFunctionAsServer()
        {
            serverComponent.CallClientCallbackFunction();
            Assert.That(serverComponent.clientCallbackFunctionCalled, Is.False);
        }

        [Test]
        public void CannotCallServerFunctionAsClient()
        {
            Assert.Throws<MethodInvocationException>(() =>
            {
               clientComponent.CallServerFunction();
            });
        }

        [Test]
        public void CannotCallServerFunctionCallbackAsClient()
        {
            clientComponent.CallServerCallbackFunction();
            Assert.That(clientComponent.serverCallbackFunctionCalled, Is.False);
        }

        [Test]
        public void CanCallClientFunctionAsClient()
        {
            clientComponent.CallClientFunction();
            Assert.That(clientComponent.clientFunctionCalled, Is.True);
        }

        [Test]
        public void CanCallClientCallbackFunctionAsClient()
        {
            clientComponent.CallClientCallbackFunction();
            Assert.That(clientComponent.clientCallbackFunctionCalled, Is.True);
        }

        [Test]
        public void CanCallLocalPlayer()
        {
            clientComponent.CallLocalPlayer();
            Assert.That(clientComponent.localPlayerCalled, Is.True);
        }

        [Test]
        public void CanCallLocalPlayerNoError()
        {
            clientComponent.CallLocalPlayerNoError();
            Assert.That(clientComponent.localPlayerNoErrorCalled, Is.True);
        }

        [Test]
        public void GuardLocalPlayer()
        {
            var obj = new GameObject("randomObject", typeof(NetworkIdentity), typeof(ExampleGuards));
            ExampleGuards guardedComponent = obj.GetComponent<ExampleGuards>();

            Assert.Throws<MethodInvocationException>(() =>
            {
                guardedComponent.CallLocalPlayer();
            });

            Object.Destroy(obj);
        }

        [Test]
        public void GuardLocalPlayerNoError()
        {
            var obj = new GameObject("randomObject", typeof(NetworkIdentity), typeof(ExampleGuards));
            ExampleGuards guardedComponent = obj.GetComponent<ExampleGuards>();

            guardedComponent.CallLocalPlayerNoError();
            Assert.That(guardedComponent.localPlayerNoErrorCalled, Is.False);

            Object.Destroy(obj);
        }
    }
}
