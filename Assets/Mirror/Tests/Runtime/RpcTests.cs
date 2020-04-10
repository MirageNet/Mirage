using System;
using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Mirror.Tests
{

    public class RpcComponent : NetworkBehaviour
    {
        public int cmdArg1;
        public string cmdArg2;

        [Command]
        public void CmdTest(int arg1, string arg2)
        {
            this.cmdArg1 = arg1;
            this.cmdArg2 = arg2;
        }

        public NetworkIdentity cmdNi;

        [Command]
        public void CmdNetworkIdentity(NetworkIdentity ni)
        {
            this.cmdNi = ni;
        }

        public int rpcArg1;
        public string rpcArg2;

        [ClientRpc]
        public void RpcTest(int arg1, string arg2)
        {
            this.rpcArg1 = arg1;
            this.rpcArg2 = arg2;
        }

        public int targetRpcArg1;
        public string targetRpcArg2;
        public INetworkConnection targetRpcConn;

        [TargetRpc]
        public void TargetRpcTest(INetworkConnection conn, int arg1, string arg2)
        {
            this.targetRpcConn = conn;
            this.targetRpcArg1 = arg1;
            this.targetRpcArg2 = arg2;
        }
    }

    public class RpcTests : HostSetup<RpcComponent>
    {
        [Test]
        public void CommandWithoutAuthority()
        {
            var gameObject2 = new GameObject();
            RpcComponent rpcComponent2 = gameObject2.AddComponent<RpcComponent>();

            // spawn it without client authority
            server.Spawn(gameObject2);

            // process spawn message from server
            client.Update();

            // only authorized clients can call command
            Assert.Throws<UnauthorizedAccessException>(() =>
           {
               rpcComponent2.CmdTest(1, "hello");
           });

        }

        [UnityTest]
        public IEnumerator Command()
        {
            component.CmdTest(1, "hello");
            yield return null;

            Assert.That(component.cmdArg1, Is.EqualTo(1));
            Assert.That(component.cmdArg2, Is.EqualTo("hello"));
        }

        [UnityTest]
        public IEnumerator CommandWithNetworkIdentity()
        {
            component.CmdNetworkIdentity(identity);

            yield return null;

            Assert.That(component.cmdNi, Is.SameAs(identity));
        }

        [UnityTest]
        public IEnumerator ClientRpc()
        {
            component.RpcTest(1, "hello");
            // process spawn message from server
            yield return null;

            Assert.That(component.rpcArg1, Is.EqualTo(1));
            Assert.That(component.rpcArg2, Is.EqualTo("hello"));
        }

        [UnityTest]
        public IEnumerator TargetRpc()
        {
            component.TargetRpcTest(manager.server.LocalConnection, 1, "hello");
            // process spawn message from server
            yield return null;

            Assert.That(component.targetRpcConn, Is.SameAs(manager.client.Connection));
            Assert.That(component.targetRpcArg1, Is.EqualTo(1));
            Assert.That(component.targetRpcArg2, Is.EqualTo("hello"));
        }

        [UnityTest]
        public IEnumerator DisconnectHostTest()
        {
            // set local connection
            Assert.That(server.LocalClientActive, Is.True);
            Assert.That(server.connections, Has.Count.EqualTo(1));

            server.Disconnect();

            // wait for messages to get dispatched
            yield return null;

            // state cleared?
            Assert.That(server.connections, Is.Empty);
            Assert.That(server.Active, Is.False);
            Assert.That(server.LocalConnection, Is.Null);
            Assert.That(server.LocalClientActive, Is.False);
        }

    }
}
