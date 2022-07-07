using System;
using System.Linq;
using System.Linq.Expressions;
using Mirage.Weaver;
using Mono.Cecil.Cil;
using NUnit.Framework;

namespace Mirage.Tests.Weaver
{
    public class ClientServerAttributeTests : WeaverTestBase
    {
        [Test, BatchSafe(BatchType.Success)]
        public void NetworkBehaviourServer()
        {
            IsSuccess();
            CheckAddedCode(
                (NetworkBehaviour nb) => nb.IsServer,
                "ClientServerAttributeTests.NetworkBehaviourServer.NetworkBehaviourServer", "ServerOnlyMethod");

        }

        [Test]
        public void NetworkBehaviourServerOnAwake()
        {
            HasError("ServerAttribute will not work on the Awake method.",
                "System.Void ClientServerAttributeTests.NetworkBehaviourServer.NetworkBehaviourServerOnAwake::Awake()");
        }

        [Test, BatchSafe(BatchType.Success)]
        public void NetworkBehaviourServerOnAwakeWithParameters()
        {
            IsSuccess();
            CheckAddedCode(
                (NetworkBehaviour nb) => nb.IsServer,
                "ClientServerAttributeTests.NetworkBehaviourServer.NetworkBehaviourServerOnAwakeWithParameters", "Awake");

        }

        [Test, BatchSafe(BatchType.Success)]
        public void NetworkBehaviourClient()
        {
            IsSuccess();
            CheckAddedCode(
                (NetworkBehaviour nb) => nb.IsClient,
                "ClientServerAttributeTests.NetworkBehaviourClient.NetworkBehaviourClient", "ClientOnlyMethod");
        }

        [Test]
        public void NetworkBehaviourClientOnAwake()
        {
            HasError("ClientAttribute will not work on the Awake method.",
                "System.Void ClientServerAttributeTests.NetworkBehaviourClient.NetworkBehaviourClientOnAwake::Awake()");
        }

        [Test, BatchSafe(BatchType.Success)]
        public void NetworkBehaviourClientOnAwakeWithParameters()
        {
            IsSuccess();
            CheckAddedCode(
                (NetworkBehaviour nb) => nb.IsClient,
                "ClientServerAttributeTests.NetworkBehaviourClient.NetworkBehaviourClientOnAwakeWithParameters", "Awake");
        }

        [Test, BatchSafe(BatchType.Success)]
        public void NetworkBehaviourHasAuthority()
        {
            IsSuccess();
            CheckAddedCode(
                (NetworkBehaviour nb) => nb.HasAuthority,
                "ClientServerAttributeTests.NetworkBehaviourHasAuthority.NetworkBehaviourHasAuthority", "HasAuthorityMethod");
        }

        [Test]
        public void NetworkBehaviourHasAuthorityOnAwake()
        {
            HasError("HasAuthorityAttribute will not work on the Awake method.",
                "System.Void ClientServerAttributeTests.NetworkBehaviourHasAuthority.NetworkBehaviourHasAuthorityOnAwake::Awake()");
        }

        [Test, BatchSafe(BatchType.Success)]
        public void NetworkBehaviourHasAuthorityOnAwakeWithParameters()
        {
            IsSuccess();
            CheckAddedCode(
                (NetworkBehaviour nb) => nb.HasAuthority,
                "ClientServerAttributeTests.NetworkBehaviourHasAuthority.NetworkBehaviourHasAuthorityOnAwakeWithParameters", "Awake");
        }

        [Test, BatchSafe(BatchType.Success)]
        public void NetworkBehaviourLocalPlayer()
        {
            IsSuccess();
            CheckAddedCode(
                (NetworkBehaviour nb) => nb.IsLocalPlayer,
                "ClientServerAttributeTests.NetworkBehaviourLocalPlayer.NetworkBehaviourLocalPlayer", "LocalPlayerMethod");
        }

        [Test]
        public void NetworkBehaviourLocalPlayerOnAwake()
        {
            HasError("LocalPlayerAttribute will not work on the Awake method.",
                "System.Void ClientServerAttributeTests.NetworkBehaviourLocalPlayer.NetworkBehaviourLocalPlayerOnAwake::Awake()");
        }

        [Test, BatchSafe(BatchType.Success)]
        public void NetworkBehaviourLocalPlayerOnAwakeWithParameters()
        {
            IsSuccess();
            CheckAddedCode(
                (NetworkBehaviour nb) => nb.IsLocalPlayer,
                "ClientServerAttributeTests.NetworkBehaviourLocalPlayer.NetworkBehaviourLocalPlayerOnAwakeWithParameters", "Awake");
        }

        /// <summary>
        /// Checks that first Instructions in MethodBody is addedString
        /// </summary>
        /// <param name="addedString"></param>
        /// <param name="methodName"></param>
        private void CheckAddedCode(Expression<Func<NetworkBehaviour, bool>> pred, string className, string methodName)
        {
            var type = testResult.assembly.MainModule.GetType(className);
            var method = type.Methods.First(m => m.Name == methodName);
            var body = method.Body;

            var top = body.Instructions[0];
            Assert.That(top.OpCode, Is.EqualTo(OpCodes.Ldarg_0));

            var methodRef = testResult.assembly.MainModule.ImportReference(pred);

            var call = body.Instructions[1];

            Assert.That(call.OpCode, Is.EqualTo(OpCodes.Call));
            Assert.That(call.Operand.ToString(), Is.EqualTo(methodRef.ToString()));
        }
    }
}
