using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

namespace Mirage.Tests.Runtime.GuardTests
{
    public class ExampleGuards_NetworkMethod : NetworkBehaviour
    {
        public const int RETURN_VALUE = 10;
        public const int OUT_VALUE_1 = 20;
        public const int OUT_VALUE_2 = 20;

        // Define a list to keep track of all method calls
        public readonly List<string> Calls = new List<string>();

        [NetworkMethod(NetworkFlags.NotActive)]
        public void CallNotActive()
        {
            Calls.Add(nameof(CallNotActive));
        }
        [NetworkMethod(NetworkFlags.Server)]
        public void CallServer()
        {
            Calls.Add(nameof(CallServer));
        }
        [NetworkMethod(NetworkFlags.Server, error = false)]
        public void CallServerCallback()
        {
            Calls.Add(nameof(CallServerCallback));
        }
        [NetworkMethod(NetworkFlags.Client)]
        public void CallClient()
        {
            Calls.Add(nameof(CallClient));
        }

        [NetworkMethod(NetworkFlags.Active)]
        public void CallActive()
        {
            Calls.Add(nameof(CallActive));
        }

        [NetworkMethod(NetworkFlags.HasAuthority)]
        public void CallHasAuthority()
        {
            Calls.Add(nameof(CallHasAuthority));
        }

        [NetworkMethod(NetworkFlags.LocalOwner)]
        public void CallLocalOwner()
        {
            Calls.Add(nameof(CallLocalOwner));
        }

        [NetworkMethod(NetworkFlags.Server | NetworkFlags.HasAuthority)]
        public void CallServerOrHasAuthority()
        {
            Calls.Add(nameof(CallServerOrHasAuthority));
        }

        [NetworkMethod(NetworkFlags.Server | NetworkFlags.NotActive)]
        public void CallServerOrNotActive()
        {
            Calls.Add(nameof(CallServerOrNotActive));
        }
    }

    public class GuardsTests_NetworkMethod : ClientServerSetup<ExampleGuards_NetworkMethod>
    {
        [Test]
        public void CanCallServerAsServer()
        {
            serverComponent.CallServer();
            Assert.That(serverComponent.Calls, Has.Count.EqualTo(1));
            Assert.That(serverComponent.Calls, Does.Contain(nameof(ExampleGuards_NetworkMethod.CallServer)));
        }

        [Test]
        public void CanCallServerCallbackAsServer()
        {
            serverComponent.CallServerCallback();
            Assert.That(serverComponent.Calls, Has.Count.EqualTo(1));
            Assert.That(serverComponent.Calls, Does.Contain(nameof(ExampleGuards_NetworkMethod.CallServerCallback)));
        }

        [Test]
        public void CanCallActiveAsActive()
        {
            serverComponent.CallActive();
            Assert.That(serverComponent.Calls, Has.Count.EqualTo(1));
            Assert.That(serverComponent.Calls, Does.Contain(nameof(ExampleGuards_NetworkMethod.CallActive)));

            clientComponent.CallActive();
            Assert.That(clientComponent.Calls, Has.Count.EqualTo(1));
            Assert.That(clientComponent.Calls, Does.Contain(nameof(ExampleGuards_NetworkMethod.CallActive)));
        }

        [Test]
        public void CannotCallActiveAsActive()
        {
            var guardedComponent = CreateBehaviour<ExampleGuards_NetworkMethod>();
            Assert.Throws<MethodInvocationException>(() =>
            {
                guardedComponent.CallActive();
            });
            Assert.That(guardedComponent.Calls, Is.Empty);
        }

        [Test]
        public void CanCallAuthorityWithAuthority()
        {
            clientComponent.CallHasAuthority();
            Assert.That(clientComponent.Calls, Has.Count.EqualTo(1));
            Assert.That(clientComponent.Calls, Does.Contain(nameof(ExampleGuards_NetworkMethod.CallHasAuthority)));
        }

        [Test]
        public void CannotCallAuthorityWithoutAuthority()
        {
            Assert.Throws<MethodInvocationException>(() =>
            {
                serverComponent.CallHasAuthority();
            });
            Assert.That(serverComponent.Calls, Is.Empty);
        }

        [Test]
        public void CanCallLocalPlayerAsLocalPlayer()
        {
            clientComponent.CallLocalOwner();
            Assert.That(clientComponent.Calls, Has.Count.EqualTo(1));
            Assert.That(clientComponent.Calls, Does.Contain(nameof(ExampleGuards_NetworkMethod.CallLocalOwner)));
        }

        [Test]
        public void CannotCallLocalPlayerAsNonLocalPlayer()
        {
            Assert.Throws<MethodInvocationException>(() =>
            {
                serverComponent.CallLocalOwner();
            });
            Assert.That(serverComponent.Calls, Is.Empty);
        }

        [Test]
        public void CannotCallNotActiveAsServer()
        {
            Assert.Throws<MethodInvocationException>(() =>
            {
                serverComponent.CallNotActive();
            });
            Assert.That(serverComponent.Calls, Is.Empty);
        }

        [Test]
        public void CannotCallNotActiveAsClient()
        {
            Assert.Throws<MethodInvocationException>(() =>
            {
                clientComponent.CallNotActive();
            });
            Assert.That(clientComponent.Calls, Is.Empty);
        }

        [Test]
        public void CannotCallNotActiveAsUnspawned()
        {
            var guardedComponent = CreateBehaviour<ExampleGuards_NetworkMethod>();
            Debug.Assert(guardedComponent.NetId == 0);

            guardedComponent.CallNotActive();
            Assert.That(guardedComponent.Calls, Has.Count.EqualTo(1));
            Assert.That(guardedComponent.Calls, Does.Contain(nameof(ExampleGuards_NetworkMethod.CallNotActive)));
        }

        [Test]
        public void CanCallServerOrHasAuthority()
        {
            serverComponent.CallServerOrHasAuthority();
            Assert.That(serverComponent.Calls, Has.Count.EqualTo(1));
            Assert.That(serverComponent.Calls, Does.Contain(nameof(ExampleGuards_NetworkMethod.CallServerOrHasAuthority)));

            clientComponent.CallServerOrHasAuthority();
            Assert.That(clientComponent.Calls, Has.Count.EqualTo(1));
            Assert.That(clientComponent.Calls, Does.Contain(nameof(ExampleGuards_NetworkMethod.CallServerOrHasAuthority)));
        }

        [Test]
        public void CannotCallServerOrHasAuthorityAsUnspawned()
        {
            var guardedComponent = CreateBehaviour<ExampleGuards_NetworkMethod>();
            Assert.Throws<MethodInvocationException>(() =>
            {
                guardedComponent.CallServerOrHasAuthority();
            });
            Assert.That(guardedComponent.Calls, Is.Empty);
        }

        [Test]
        public void CanCallServerOrNotActive()
        {
            serverComponent.CallServerOrNotActive();
            Assert.That(serverComponent.Calls, Has.Count.EqualTo(1));
            Assert.That(serverComponent.Calls, Does.Contain(nameof(ExampleGuards_NetworkMethod.CallServerOrNotActive)));

            var guardedComponent = CreateBehaviour<ExampleGuards_NetworkMethod>();
            Debug.Assert(guardedComponent.NetId == 0);
            guardedComponent.CallServerOrNotActive();
            Assert.That(guardedComponent.Calls, Has.Count.EqualTo(1));
            Assert.That(guardedComponent.Calls, Does.Contain(nameof(ExampleGuards_NetworkMethod.CallServerOrNotActive)));
        }

        [Test]
        public void CannotCallServerOrNotActive()
        {
            Assert.Throws<MethodInvocationException>(() =>
            {
                clientComponent.CallServerOrNotActive();
            });
            Assert.That(clientComponent.Calls, Is.Empty);
        }
    }
}
