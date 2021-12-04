using System.Collections;
using Cysharp.Threading.Tasks;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Mirage.Tests.Runtime.ClientServer
{
    public class ReturnRpcComponent_int : NetworkBehaviour
    {
        public int rpcResult;

        [ServerRpc]
        public UniTask<int> GetResult()
        {
            return UniTask.FromResult(rpcResult);
        }
    }
    public class ReturnRpcComponent_float : NetworkBehaviour
    {
        public float rpcResult;

        [ServerRpc]
        public UniTask<float> GetResult()
        {
            return UniTask.FromResult(rpcResult);
        }
    }
    public class ReturnRpcComponent_struct : NetworkBehaviour
    {
        public Vector3 rpcResult;

        [ServerRpc]
        public UniTask<Vector3> GetResult()
        {
            return UniTask.FromResult(rpcResult);
        }
    }
    public class ReturnRpcComponent_Identity : NetworkBehaviour
    {
        public NetworkIdentity rpcResult;

        [ServerRpc]
        public UniTask<NetworkIdentity> GetResult()
        {
            return UniTask.FromResult(rpcResult);
        }
    }


    public class ReturnRpcTest_int : ClientServerSetup<ReturnRpcComponent_int>
    {
        [UnityTest]
        public IEnumerator ServerRpcReturn() => UniTask.ToCoroutine(async () =>
        {
            int random = Random.Range(1, 100);
            serverComponent.rpcResult = random;
            int result = await clientComponent.GetResult();
            Assert.That(result, Is.EqualTo(random));
        });
    }
    public class ReturnRpcTest_float : ClientServerSetup<ReturnRpcComponent_float>
    {
        [UnityTest]
        public IEnumerator ServerRpcReturn() => UniTask.ToCoroutine(async () =>
        {
            float random = (Random.value - .5f) * 200;
            serverComponent.rpcResult = random;
            float result = await clientComponent.GetResult();
            Assert.That(result, Is.EqualTo(random));
        });
    }
    public class ReturnRpcTest_struct : ClientServerSetup<ReturnRpcComponent_struct>
    {
        [UnityTest]
        public IEnumerator ServerRpcReturn() => UniTask.ToCoroutine(async () =>
        {
            Vector3 random = Random.insideUnitSphere;
            serverComponent.rpcResult = random;
            Vector3 result = await clientComponent.GetResult();
            Assert.That(result, Is.EqualTo(random));
        });
    }
    public class ReturnRpcTest_Identity : ClientServerSetup<ReturnRpcComponent_Identity>
    {
        [UnityTest]
        public IEnumerator ServerRpcReturn() => UniTask.ToCoroutine(async () =>
        {
            serverComponent.rpcResult = serverIdentity;
            NetworkIdentity result = await clientComponent.GetResult();
            // server returning its version of Identity should cause client to get reference to clients version
            Assert.That(result, Is.EqualTo(clientIdentity));
        });
    }
}
