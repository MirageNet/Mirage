using System;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Mirage.Tests.Runtime.RpcTests.Async
{
    public class ReturnRpcComponent_int : NetworkBehaviour
    {
        public int rpcResult;

        [ServerRpc]
        public UniTask<int> GetResultServer()
        {
            return UniTask.FromResult(rpcResult);
        }

        [ClientRpc(target = RpcTarget.Player)]
        public UniTask<int> GetResultTarget(INetworkPlayer target)
        {
            return UniTask.FromResult(rpcResult);
        }

        [ClientRpc(target = RpcTarget.Owner)]
        public UniTask<int> GetResultOwner()
        {
            return UniTask.FromResult(rpcResult);
        }
    }
    public class ReturnRpcComponent_float : NetworkBehaviour
    {
        public float rpcResult;

        [ServerRpc]
        public UniTask<float> GetResultServer()
        {
            return UniTask.FromResult(rpcResult);
        }

        [ClientRpc(target = RpcTarget.Player)]
        public UniTask<float> GetResultTarget(INetworkPlayer target)
        {
            return UniTask.FromResult(rpcResult);
        }

        [ClientRpc(target = RpcTarget.Owner)]
        public UniTask<float> GetResultOwner()
        {
            return UniTask.FromResult(rpcResult);
        }
    }
    public class ReturnRpcComponent_throw : NetworkBehaviour
    {
        public static ArgumentException TestException => new System.ArgumentException("some bad thing happened");
        [ServerRpc]
        public UniTask<float> GetResultServer()
        {
            throw TestException;
        }

        [ClientRpc(target = RpcTarget.Player)]
        public UniTask<float> GetResultTarget(INetworkPlayer target)
        {
            throw TestException;
        }

        [ClientRpc(target = RpcTarget.Owner)]
        public UniTask<float> GetResultOwner()
        {
            throw TestException;
        }
    }
    public class ReturnRpcComponent_struct : NetworkBehaviour
    {
        public Vector3 rpcResult;

        [ServerRpc]
        public UniTask<Vector3> GetResultServer()
        {
            return UniTask.FromResult(rpcResult);
        }

        [ClientRpc(target = RpcTarget.Player)]
        public UniTask<Vector3> GetResultTarget(INetworkPlayer target)
        {
            return UniTask.FromResult(rpcResult);
        }

        [ClientRpc(target = RpcTarget.Owner)]
        public UniTask<Vector3> GetResultOwner()
        {
            return UniTask.FromResult(rpcResult);
        }
    }
    public class ReturnRpcComponent_Identity : NetworkBehaviour
    {
        public NetworkIdentity rpcResult;

        [ServerRpc]
        public UniTask<NetworkIdentity> GetResultServer()
        {
            return UniTask.FromResult(rpcResult);
        }

        [ClientRpc(target = RpcTarget.Player)]
        public UniTask<NetworkIdentity> GetResultTarget(INetworkPlayer target)
        {
            return UniTask.FromResult(rpcResult);
        }

        [ClientRpc(target = RpcTarget.Owner)]
        public UniTask<NetworkIdentity> GetResultOwner()
        {
            return UniTask.FromResult(rpcResult);
        }
    }
}
