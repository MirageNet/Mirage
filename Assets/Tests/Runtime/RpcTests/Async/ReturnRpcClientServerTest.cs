using System.Collections;
using Cysharp.Threading.Tasks;
using Mirage.Tests.Runtime.Host;
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
}
