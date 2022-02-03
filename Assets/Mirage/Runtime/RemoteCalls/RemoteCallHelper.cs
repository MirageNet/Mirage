using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Mirage.Logging;
using Mirage.Serialization;
using UnityEngine;

namespace Mirage.RemoteCalls
{
    /// <summary>
    /// Delegate for ServerRpc functions.
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="reader"></param>
    public delegate void RpcDelegate(NetworkBehaviour obj, NetworkReader reader, INetworkPlayer senderPlayer, int replyId);
    public delegate UniTask<T> RequestDelegate<T>(NetworkBehaviour obj, NetworkReader reader, INetworkPlayer senderPlayer, int replyId);

    // invoke type for Rpc
    public enum RpcInvokeType
    {
        ServerRpc = 0,
        ClientRpc = 1,
    }

    /// <summary>
    /// Used for invoking a RPC methods
    /// </summary>
    public class RemoteCall
    {
        /// <summary>
        /// Type that rpc was declared in
        /// </summary>
        public Type DeclaringType;
        /// <summary>
        /// Server rpc or client rpc
        /// </summary>
        public RpcInvokeType InvokeType;
        /// <summary>
        /// Function to be invoked when receiving message
        /// </summary>
        public RpcDelegate function;
        /// <summary>
        /// Used by ServerRpc
        /// </summary>
        public bool RequireAuthority;
        /// <summary>
        /// User friendly name
        /// </summary>
        public string name;

        public bool AreEqual(Type invokeClass, RpcInvokeType invokeType, RpcDelegate invokeFunction)
        {
            return DeclaringType == invokeClass &&
                    InvokeType == invokeType &&
                    function == invokeFunction;
        }

        internal void Invoke(NetworkReader reader, NetworkBehaviour invokingType, INetworkPlayer senderPlayer = null, int replyId = 0)
        {
            // todo do we need to do this check? it should never happen
            //if (DeclaringType.IsInstanceOfType(invokingType))
            //{
            function(invokingType, reader, senderPlayer, replyId);
            return;
            //}
            throw new MethodInvocationException($"Invalid Rpc call {function} for component {invokingType}");
        }

        /// <summary>
        /// User friendly name used for debug/error messages
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return name;
        }
    }

    /// <summary>
    /// Used to help manage remote calls for NetworkBehaviours
    /// </summary>
    public static class RemoteCallHelper
    {
        static readonly ILogger logger = LogFactory.GetLogger(typeof(RemoteCallHelper));

        static readonly Dictionary<int, RemoteCall> calls = new Dictionary<int, RemoteCall>();

        /// <summary>
        /// helper function register a ServerRpc/Rpc delegate
        /// </summary>
        /// <param name="invokeClass"></param>
        /// <param name="name"></param>
        /// <param name="invokerType"></param>
        /// <param name="func"></param>
        /// <param name="cmdRequireAuthority"></param>
        /// <returns>remote function hash</returns>
        public static void Register(Type invokeClass, string name, int hash, RpcInvokeType invokerType, RpcDelegate func, bool cmdRequireAuthority)
        {
            if (CheckDuplicate(invokeClass, invokerType, func, hash))
                return;

            var invoker = new RemoteCall
            {
                name = name,
                InvokeType = invokerType,
                DeclaringType = invokeClass,
                function = func,
                RequireAuthority = cmdRequireAuthority,
            };

            calls[hash] = invoker;

            if (logger.LogEnabled())
            {
                string requireAuthorityMessage = invokerType == RpcInvokeType.ServerRpc ? $" RequireAuthority:{cmdRequireAuthority}" : "";
                logger.Log($"RegisterDelegate hash: {hash} invokerType: {invokerType} method: {func.Method.Name}{requireAuthorityMessage}");
            }
        }

        public static void RegisterRequest<T>(Type invokeClass, string name, int hash, RequestDelegate<T> func, bool cmdRequireAuthority)
        {
            async UniTaskVoid Wrapper(NetworkBehaviour obj, NetworkReader reader, INetworkPlayer senderPlayer, int replyId)
            {
                /// invoke the serverRpc and send a reply message
                T result = await func(obj, reader, senderPlayer, replyId);

                using (PooledNetworkWriter writer = NetworkWriterPool.GetWriter())
                {
                    writer.Write(result);
                    var serverRpcReply = new ServerRpcReply
                    {
                        replyId = replyId,
                        payload = writer.ToArraySegment()
                    };

                    senderPlayer.Send(serverRpcReply);
                }
            }

            void CmdWrapper(NetworkBehaviour obj, NetworkReader reader, INetworkPlayer senderPlayer, int replyId)
            {
                Wrapper(obj, reader, senderPlayer, replyId).Forget();
            }

            Register(invokeClass, name, hash, RpcInvokeType.ServerRpc, CmdWrapper, cmdRequireAuthority);
        }

        static bool CheckDuplicate(Type invokeClass, RpcInvokeType invokerType, RpcDelegate func, int cmdHash)
        {
            if (calls.ContainsKey(cmdHash))
            {
                // something already registered this hash
                RemoteCall oldInvoker = calls[cmdHash];
                if (oldInvoker.AreEqual(invokeClass, invokerType, func))
                {
                    // it's all right,  it was the same function
                    return true;
                }

                logger.LogError($"Function {oldInvoker.DeclaringType}.{oldInvoker.function.Method.Name} and {invokeClass}.{func.Method.Name} have the same hash.  Please rename one of them");
            }

            return false;
        }

        public static RemoteCall GetCall(int hash)
        {
            if (calls.TryGetValue(hash, out RemoteCall invoker))
            {
                return invoker;
            }

            throw new MethodInvocationException($"No RPC method found for hash {hash}");
        }

        public static bool TryGetCall(int hash, out RemoteCall call)
        {
            return calls.TryGetValue(hash, out call);
        }
    }
}

