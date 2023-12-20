using System;
using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Mirage.Tests.Runtime.RpcTests
{
    public class ClientServerRPCTests : ClientServerSetup<MockRpcComponent>
    {
        [UnityTest]
        public IEnumerator ServerRpc()
        {
            clientComponent.Server2Args(1, "hello");

            yield return null;
            yield return null;

            Assert.That(serverComponent.Server2ArgsCalls.Count, Is.EqualTo(1));
            Assert.That(serverComponent.Server2ArgsCalls[0].arg1, Is.EqualTo(1));
            Assert.That(serverComponent.Server2ArgsCalls[0].arg2, Is.EqualTo("hello"));
        }

        [UnityTest]
        public IEnumerator ServerRpcWithSenderOnClient()
        {
            clientComponent.ServerWithSender(1);

            yield return null;
            yield return null;

            Assert.That(serverComponent.ServerWithSenderCalls.Count, Is.EqualTo(1));
            Assert.That(serverComponent.ServerWithSenderCalls[0].arg1, Is.EqualTo(1));
            Assert.That(serverComponent.ServerWithSenderCalls[0].sender, Is.EqualTo(serverPlayer));
        }

        [Test]
        public void ServerRpcWithSenderOnServer()
        {
            var exception = Assert.Throws<InvalidOperationException>(() =>
            {
                serverComponent.ServerWithSender(1);
            });

            Assert.That(exception, Has.Message.EqualTo("Server RPC can only be called when client is active"));
        }

        [UnityTest]
        public IEnumerator ServerRpcWithNetworkIdentity()
        {
            clientComponent.ServerWithNI(clientIdentity);

            yield return null;
            yield return null;

            Assert.That(serverComponent.ServerWithNICalls.Count, Is.EqualTo(1));
            Assert.That(serverComponent.ServerWithNICalls[0], Is.SameAs(serverIdentity));
        }

        [UnityTest]
        public IEnumerator ClientRpc()
        {
            serverComponent.Client2Args(1, "hello");

            yield return null;
            yield return null;

            Assert.That(clientComponent.Client2ArgsCalls.Count, Is.EqualTo(1));
            Assert.That(clientComponent.Client2ArgsCalls[0].arg1, Is.EqualTo(1));
            Assert.That(clientComponent.Client2ArgsCalls[0].arg2, Is.EqualTo("hello"));
        }

        [UnityTest]
        public IEnumerator ClientConnRpc()
        {
            serverComponent.ClientTarget(serverPlayer, 1, "hello");

            yield return null;
            yield return null;

            Assert.That(clientComponent.ClientTargetCalls.Count, Is.EqualTo(1));
            Assert.That(clientComponent.ClientTargetCalls[0].player, Is.EqualTo(clientPlayer));
            Assert.That(clientComponent.ClientTargetCalls[0].arg1, Is.EqualTo(1));
            Assert.That(clientComponent.ClientTargetCalls[0].arg2, Is.EqualTo("hello"));
        }

        [UnityTest]
        public IEnumerator ClientOwnerRpc()
        {
            serverComponent.ClientOwner(1, "hello");

            yield return null;
            yield return null;

            Assert.That(clientComponent.ClientOwnerCalls.Count, Is.EqualTo(1));
            Assert.That(clientComponent.ClientOwnerCalls[0].arg1, Is.EqualTo(1));
            Assert.That(clientComponent.ClientOwnerCalls[0].arg2, Is.EqualTo("hello"));
        }

        [UnityTest]
        public IEnumerator ClientExcludeOwner()
        {
            Debug.Assert(serverComponent.Owner != null);
            serverComponent.ClientExcludeOwner(1, "hello");
            // process spawn message from server
            yield return null;
            yield return null;

            Assert.That(clientComponent.ClientExcludeOwnerCalls.Count, Is.EqualTo(0), "owner should not get if excludeOwner is true");
        }
    }
}
