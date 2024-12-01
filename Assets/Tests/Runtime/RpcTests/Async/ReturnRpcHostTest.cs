using System;
using System.Collections;
using Cysharp.Threading.Tasks;
using Mirage.Tests.Runtime.Host;
using NUnit.Framework;
using UnityEngine.TestTools;
using Random = UnityEngine.Random;

namespace Mirage.Tests.Runtime.RpcTests.Async
{
    public class ReturnRpcHostTest_int : HostSetup<ReturnRpcComponent_int>
    {
        [UnityTest]
        public IEnumerator ServerRpcReturn() => UniTask.ToCoroutine(async () =>
        {
            var random = Random.Range(1, 100);
            hostComponent.rpcResult = random;
            var result = await hostComponent.GetResultServer();
            Assert.That(result, Is.EqualTo(random));
        });

        [UnityTest]
        public IEnumerator ClientRpcTargetReturn() => UniTask.ToCoroutine(async () =>
        {
            var random = Random.Range(1, 100);
            hostComponent.rpcResult = random;
            var result = await hostComponent.GetResultTarget(hostServerPlayer);
            Assert.That(result, Is.EqualTo(random));
        });

        [UnityTest]
        public IEnumerator ClientRpcOwnerReturn() => UniTask.ToCoroutine(async () =>
        {
            var random = Random.Range(1, 100);
            hostComponent.rpcResult = random;
            var result = await hostComponent.GetResultOwner();
            Assert.That(result, Is.EqualTo(random));
        });
    }
    public class ReturnRpcHostTest_float : HostSetup<ReturnRpcComponent_float>
    {
        [UnityTest]
        public IEnumerator ServerRpcReturn() => UniTask.ToCoroutine(async () =>
        {
            var random = (Random.value - .5f) * 200;
            hostComponent.rpcResult = random;
            var result = await hostComponent.GetResultServer();
            Assert.That(result, Is.EqualTo(random));
        });

        [UnityTest]
        public IEnumerator ClientRpcTargetReturn() => UniTask.ToCoroutine(async () =>
        {
            var random = (Random.value - .5f) * 200;
            hostComponent.rpcResult = random;
            var result = await hostComponent.GetResultTarget(hostServerPlayer);
            Assert.That(result, Is.EqualTo(random));
        });

        [UnityTest]
        public IEnumerator ClientRpcOwnerReturn() => UniTask.ToCoroutine(async () =>
        {
            var random = (Random.value - .5f) * 200;
            hostComponent.rpcResult = random;
            var result = await hostComponent.GetResultOwner();
            Assert.That(result, Is.EqualTo(random));
        });
    }

    public class ReturnRpcHostTest_Throw : HostSetup<ReturnRpcComponent_throw>
    {
        [UnityTest]
        public IEnumerator ServerRpcReturn() => UniTask.ToCoroutine(async () =>
        {
            try
            {
                _ = await hostComponent.GetResultServer();
                Assert.Fail();
            }
            catch (ArgumentException e) // host invokes direction, so we can catch exception
            {
                Assert.That(e, Has.Message.EqualTo(ReturnRpcComponent_throw.TestException.Message));
            }
        });

        [UnityTest]
        public IEnumerator ClientRpcTargetReturn() => UniTask.ToCoroutine(async () =>
        {
            try
            {
                _ = await hostComponent.GetResultTarget(hostServerPlayer);
                Assert.Fail();
            }
            catch (ArgumentException e) // host invokes direction, so we can catch exception
            {
                Assert.That(e, Has.Message.EqualTo(ReturnRpcComponent_throw.TestException.Message));
            }
        });

        [UnityTest]
        public IEnumerator ClientRpcOwnerReturn() => UniTask.ToCoroutine(async () =>
        {
            try
            {
                _ = await hostComponent.GetResultOwner();
                Assert.Fail();
            }
            catch (ArgumentException e) // host invokes direction, so we can catch exception
            {
                Assert.That(e, Has.Message.EqualTo(ReturnRpcComponent_throw.TestException.Message));
            }
        });
    }

    public class ReturnRpcHostTest_struct : HostSetup<ReturnRpcComponent_struct>
    {
        [UnityTest]
        public IEnumerator ServerRpcReturn() => UniTask.ToCoroutine(async () =>
        {
            var random = Random.insideUnitSphere;
            hostComponent.rpcResult = random;
            var result = await hostComponent.GetResultServer();
            Assert.That(result, Is.EqualTo(random));
        });

        [UnityTest]
        public IEnumerator ClientRpcTargetReturn() => UniTask.ToCoroutine(async () =>
        {
            var random = Random.insideUnitSphere;
            hostComponent.rpcResult = random;
            var result = await hostComponent.GetResultTarget(hostServerPlayer);
            Assert.That(result, Is.EqualTo(random));
        });

        [UnityTest]
        public IEnumerator ClientRpcOwnerReturn() => UniTask.ToCoroutine(async () =>
        {
            var random = Random.insideUnitSphere;
            hostComponent.rpcResult = random;
            var result = await hostComponent.GetResultOwner();
            Assert.That(result, Is.EqualTo(random));
        });
    }
    public class ReturnRpcHostTest_Identity : HostSetup<ReturnRpcComponent_Identity>
    {
        [UnityTest]
        public IEnumerator ServerRpcReturn() => UniTask.ToCoroutine(async () =>
        {
            hostComponent.rpcResult = hostIdentity;
            var result = await hostComponent.GetResultServer();
            // server returning its version of Identity should cause client to get reference to clients version
            Assert.That(result, Is.EqualTo(hostIdentity));
        });

        [UnityTest]
        public IEnumerator ClientRpcTargetReturn() => UniTask.ToCoroutine(async () =>
        {
            hostComponent.rpcResult = hostIdentity;
            var result = await hostComponent.GetResultTarget(hostServerPlayer);
            Assert.That(result, Is.EqualTo(hostIdentity));
        });

        [UnityTest]
        public IEnumerator ClientRpcOwnerReturn() => UniTask.ToCoroutine(async () =>
        {
            hostComponent.rpcResult = hostIdentity;
            var result = await hostComponent.GetResultOwner();
            Assert.That(result, Is.EqualTo(hostIdentity));
        });
    }
}

