using System.Collections;
using System.Text.RegularExpressions;
using Cysharp.Threading.Tasks;
using Mirage.RemoteCalls;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Mirage.Tests.Runtime.RpcTests.Async
{
    public class ReturnRpcClientServerTest_int : ClientServerSetup<ReturnRpcComponent_int>
    {
        [UnityTest]
        public IEnumerator ServerRpcReturn() => UniTask.ToCoroutine(async () =>
        {
            var random = Random.Range(1, 100);
            serverComponent.rpcResult = random;
            var result = await clientComponent.GetResultServer();
            Assert.That(result, Is.EqualTo(random));
        });

        [UnityTest]
        public IEnumerator ClientRpcTargetReturn() => UniTask.ToCoroutine(async () =>
        {
            var random = Random.Range(1, 100);
            clientComponent.rpcResult = random;
            var result = await serverComponent.GetResultTarget(serverPlayer);
            Assert.That(result, Is.EqualTo(random));
        });

        [UnityTest]
        public IEnumerator ClientRpcOwnerReturn() => UniTask.ToCoroutine(async () =>
        {
            var random = Random.Range(1, 100);
            clientComponent.rpcResult = random;
            var result = await serverComponent.GetResultOwner();
            Assert.That(result, Is.EqualTo(random));
        });
    }

    public class ReturnRpcClientServerTest_float : ClientServerSetup<ReturnRpcComponent_float>
    {
        [UnityTest]
        public IEnumerator ServerRpcReturn() => UniTask.ToCoroutine(async () =>
        {
            var random = (Random.value - .5f) * 200;
            serverComponent.rpcResult = random;
            var result = await clientComponent.GetResultServer();
            Assert.That(result, Is.EqualTo(random));
        });

        [UnityTest]
        public IEnumerator ClientRpcTargetReturn() => UniTask.ToCoroutine(async () =>
        {
            var random = (Random.value - .5f) * 200;
            clientComponent.rpcResult = random;
            var result = await serverComponent.GetResultTarget(serverPlayer);
            Assert.That(result, Is.EqualTo(random));
        });

        [UnityTest]
        public IEnumerator ClientRpcOwnerReturn() => UniTask.ToCoroutine(async () =>
        {
            var random = (Random.value - .5f) * 200;
            clientComponent.rpcResult = random;
            var result = await serverComponent.GetResultOwner();
            Assert.That(result, Is.EqualTo(random));
        });
    }

    public class ReturnRpcClientServerTest_throw : ClientServerSetup<ReturnRpcComponent_throw>
    {
        [UnityTest]
        public IEnumerator ServerRpcReturn() => UniTask.ToCoroutine(async () =>
        {
            LogAssert.Expect(LogType.Error, new Regex(".*Return RPC threw an Exception:.*", RegexOptions.Multiline));
            try
            {
                _ = await clientComponent.GetResultServer();
                Assert.Fail();
            }
            catch (ReturnRpcException e)
            {
                var fullName = "Mirage.Tests.Runtime.RpcTests.Async.ReturnRpcComponent_throw.GetResultServer";
                var message = $"Exception thrown from return RPC. {fullName} on netId={clientComponent.NetId} {clientComponent.name}";
                Assert.That(e, Has.Message.EqualTo(message));
            }
        });

        [UnityTest]
        public IEnumerator ClientRpcTargetReturn() => UniTask.ToCoroutine(async () =>
        {
            LogAssert.Expect(LogType.Error, new Regex(".*Return RPC threw an Exception:.*", RegexOptions.Multiline));
            try
            {
                _ = await serverComponent.GetResultTarget(serverPlayer);
                Assert.Fail();
            }
            catch (ReturnRpcException e)
            {
                var fullName = "Mirage.Tests.Runtime.RpcTests.Async.ReturnRpcComponent_throw.GetResultTarget";
                var message = $"Exception thrown from return RPC. {fullName} on netId={serverComponent.NetId} {serverComponent.name}";
                Assert.That(e, Has.Message.EqualTo(message));
            }
        });

        [UnityTest]
        public IEnumerator ClientRpcOwnerReturn() => UniTask.ToCoroutine(async () =>
        {
            LogAssert.Expect(LogType.Error, new Regex(".*Return RPC threw an Exception:.*", RegexOptions.Multiline));
            try
            {
                _ = await serverComponent.GetResultOwner();
                Assert.Fail();
            }
            catch (ReturnRpcException e)
            {
                var fullName = "Mirage.Tests.Runtime.RpcTests.Async.ReturnRpcComponent_throw.GetResultOwner";
                var message = $"Exception thrown from return RPC. {fullName} on netId={serverComponent.NetId} {serverComponent.name}";
                Assert.That(e, Has.Message.EqualTo(message));
            }
        });
    }
    public class ReturnRpcClientServerTest_struct : ClientServerSetup<ReturnRpcComponent_struct>
    {
        [UnityTest]
        public IEnumerator ServerRpcReturn() => UniTask.ToCoroutine(async () =>
        {
            var random = Random.insideUnitSphere;
            serverComponent.rpcResult = random;
            var result = await clientComponent.GetResultServer();
            Assert.That(result, Is.EqualTo(random));
        });

        [UnityTest]
        public IEnumerator ClientRpcTargetReturn() => UniTask.ToCoroutine(async () =>
        {
            var random = Random.insideUnitSphere;
            clientComponent.rpcResult = random;
            var result = await serverComponent.GetResultTarget(serverPlayer);
            Assert.That(result, Is.EqualTo(random));
        });

        [UnityTest]
        public IEnumerator ClientRpcOwnerReturn() => UniTask.ToCoroutine(async () =>
        {
            var random = Random.insideUnitSphere;
            clientComponent.rpcResult = random;
            var result = await serverComponent.GetResultOwner();
            Assert.That(result, Is.EqualTo(random));
        });
    }
    public class ReturnRpcClientServerTest_Identity : ClientServerSetup<ReturnRpcComponent_Identity>
    {
        [UnityTest]
        public IEnumerator ServerRpcReturn() => UniTask.ToCoroutine(async () =>
        {
            serverComponent.rpcResult = serverIdentity;
            var result = await clientComponent.GetResultServer();
            // server returning its version of Identity should cause client to get reference to clients version
            Assert.That(result, Is.EqualTo(clientIdentity));
        });

        [UnityTest]
        public IEnumerator ClientRpcTargetReturn() => UniTask.ToCoroutine(async () =>
        {
            clientComponent.rpcResult = clientIdentity;
            var result = await serverComponent.GetResultTarget(serverPlayer);
            Assert.That(result, Is.EqualTo(serverIdentity));
        });

        [UnityTest]
        public IEnumerator ClientRpcOwnerReturn() => UniTask.ToCoroutine(async () =>
        {
            clientComponent.rpcResult = clientIdentity;
            var result = await serverComponent.GetResultOwner();
            Assert.That(result, Is.EqualTo(serverIdentity));
        });
    }

    public class ReturnRpcClientServerTest_DropExceptions : ClientServerSetup<ReturnRpcComponent_int>
    {
        [UnityTest]
        public IEnumerator ThrowsWhenObjectDestroyed() => UniTask.ToCoroutine(async () =>
        {
            // We destroy the object on the server
            // Client still attempts to make the RPC before processing the destroy message
            serverObjectManager.Destroy(serverComponent.gameObject);

            // Expect the grace period warning because the object was just destroyed
            LogAssert.Expect(LogType.Warning, new Regex(".*Spawned object not found, but was recently destroyed.*", RegexOptions.IgnoreCase));

            try
            {
                _ = await clientComponent.GetResultServer();
                Assert.Fail("Should have thrown ReturnRpcException");
            }
            catch (ReturnRpcException e)
            {
                // Verify it actually is a ReturnRpcException sent by the handler!
                Assert.That(e.Message, Does.Contain("Exception thrown from return RPC"));
            }
        });

        [UnityTest]
        public IEnumerator ThrowsWhenNoAuthority() => UniTask.ToCoroutine(async () =>
        {
            // Spawn a new object with authority so the client gets it as HasAuthority = true
            var newObject = InstantiateForTest(_characterPrefabGo);
            var comp = newObject.GetComponent<ReturnRpcComponent_int>();
            var identity = comp.Identity;
            serverObjectManager.Spawn(identity, serverPlayer);

            // Wait until it spawns on client
            await UniTask.Yield();
            await UniTask.Yield();
            await UniTask.Yield();

            // Get client version
            var clientComp = _remoteClients[0].Get(comp);

            // Note: ServerRpc requires authority by default
            // Remove authority from the client on the server
            identity.RemoveClientAuthority();

            // Client attempts to make the RPC because it still assumes it has authority for this frame
            // Expect the grace period warning because authority was removed very recently
            LogAssert.Expect(LogType.Warning, new Regex(".*ServerRpc for object without authority.*but within grace period.*", RegexOptions.IgnoreCase));

            try
            {
                _ = await clientComp.GetResultServer();
                Assert.Fail("Should have thrown ReturnRpcException");
            }
            catch (ReturnRpcException e)
            {
                // Verify it actually is a ReturnRpcException sent by the handler!
                Assert.That(e.Message, Does.Contain("Exception thrown from return RPC"));
            }
        });
    }
}
