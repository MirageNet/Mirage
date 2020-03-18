using System;
using NUnit.Framework;
using UnityEngine;

namespace Mirror.Tests
{

    public class RpcComponent : NetworkBehaviour
    {
        public int cmdArg1;
        public string cmdArg2;

        [Command]
        public void CmdTest(int arg1, string arg2)
        {
            cmdArg1 = arg1;
            cmdArg2 = arg2;
        }

        public int rpcArg1;
        public string rpcArg2;

        [ClientRpc]
        public void RpcTest(int arg1, string arg2)
        {
            rpcArg1 = arg1;
            rpcArg2 = arg2;
        }

        public int targetRpcArg1;
        public string targetRpcArg2;

        [TargetRpc]
        public void TargetRpcTest(int arg1, string arg2)
        {
            targetRpcArg1 = arg1;
            targetRpcArg2 = arg2;
        }
    }

    public class RpcTests : HostTests<RpcComponent>
    {
        [Test]
        public void CommandWithoutAuthority()
        {
            var gameObject2 = new GameObject();
            NetworkIdentity identity2 = gameObject2.AddComponent<NetworkIdentity>();
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

        [Test]
        public void Command()
        {
            component.CmdTest(1, "hello");

            Assert.That(component.cmdArg1, Is.EqualTo(1));
            Assert.That(component.cmdArg2, Is.EqualTo("hello"));
        }

        [Test]
        public void ClientRpc()
        {
            component.RpcTest(1, "hello");
            // process spawn message from server
            client.Update();

            Assert.That(component.rpcArg1, Is.EqualTo(1));
            Assert.That(component.rpcArg2, Is.EqualTo("hello"));
        }

        [Test]
        public void TargetRpc()
        {

            component.TargetRpcTest(1, "hello");
            // process spawn message from server
            client.Update();

            Assert.That(component.targetRpcArg1, Is.EqualTo(1));
            Assert.That(component.targetRpcArg2, Is.EqualTo("hello"));
        }
    }
}
