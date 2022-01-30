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
        ServerRpc,
        ClientRpc
    }

    /// <summary>
    /// Stub Skeleton for RPC
    /// </summary>
    class RpcMethod
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
            if (DeclaringType.IsInstanceOfType(invokingType))
            {
                function(invokingType, reader, senderPlayer, replyId);
                return;
            }
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

        static readonly Dictionary<int, RpcMethod> cmdHandlerDelegates = new Dictionary<int, RpcMethod>();

        /// <summary>
        /// Creates hash from Type and method name
        /// </summary>
        /// <param name="invokeClass"></param>
        /// <param name="methodName"></param>
        /// <returns></returns>
        [System.Obsolete("Generate this hash in weaver instead", true)]
        internal static int GetMethodHash(Type invokeClass, string methodName)
        {
            // (invokeClass + ":" + cmdName).GetStableHashCode() would cause allocations.
            // so hash1 + hash2 is better.
            unchecked
            {
                int hash = invokeClass.FullName.GetStableHashCode();
                return hash * 503 + methodName.GetStableHashCode();
            }
        }

        /// <summary>
        /// helper function register a ServerRpc/Rpc delegate
        /// </summary>
        /// <param name="invokeClass"></param>
        /// <param name="name"></param>
        /// <param name="invokerType"></param>
        /// <param name="func"></param>
        /// <param name="cmdRequireAuthority"></param>
        /// <returns>remote function hash</returns>
        public static int Register(Type invokeClass, string name, int hash, RpcInvokeType invokerType, RpcDelegate func, bool cmdRequireAuthority = true)
        {
            if (CheckIfDelegateExists(invokeClass, invokerType, func, hash))
                return hash;

            var invoker = new RpcMethod
            {
                name = name,
                InvokeType = invokerType,
                DeclaringType = invokeClass,
                function = func,
                RequireAuthority = cmdRequireAuthority,
            };

            cmdHandlerDelegates[hash] = invoker;

            if (logger.LogEnabled())
            {
                string requireAuthorityMessage = invokerType == RpcInvokeType.ServerRpc ? $" RequireAuthority:{cmdRequireAuthority}" : "";
                logger.Log($"RegisterDelegate hash: {hash} invokerType: {invokerType} method: {func.Method.Name}{requireAuthorityMessage}");
            }

            return hash;
        }

        public static void RegisterRequest<T>(Type invokeClass, string name, int hash, RequestDelegate<T> func, bool cmdRequireAuthority = true)
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

        static bool CheckIfDelegateExists(Type invokeClass, RpcInvokeType invokerType, RpcDelegate func, int cmdHash)
        {
            if (cmdHandlerDelegates.ContainsKey(cmdHash))
            {
                // something already registered this hash
                RpcMethod oldInvoker = cmdHandlerDelegates[cmdHash];
                if (oldInvoker.AreEqual(invokeClass, invokerType, func))
                {
                    // it's all right,  it was the same function
                    return true;
                }

                logger.LogError($"Function {oldInvoker.DeclaringType}.{oldInvoker.function.Method.Name} and {invokeClass}.{func.Method.Name} have the same hash.  Please rename one of them");
            }

            return false;
        }

        public static void RegisterServerRpc(Type invokeClass, string name, int hash, RpcDelegate func, bool requireAuthority)
        {
            Register(invokeClass, name, hash, RpcInvokeType.ServerRpc, func, requireAuthority);
        }

        // todo do we need these extra methods, weaver could just call RegisterDelegate
        public static void RegisterClientRpc(Type invokeClass, string name, int hash, RpcDelegate func)
        {
            Register(invokeClass, name, hash, RpcInvokeType.ClientRpc, func);
        }

        /// <summary>
        /// We need this in order to clean up tests
        /// </summary>
        internal static void RemoveDelegate(int hash)
        {
            cmdHandlerDelegates.Remove(hash);
        }

        internal static RpcMethod GetRpc(int cmdHash)
        {
            if (cmdHandlerDelegates.TryGetValue(cmdHash, out RpcMethod invoker))
            {
                return invoker;
            }

            throw new MethodInvocationException($"No RPC method found for hash {cmdHash}");
        }

        /// <summary>
        /// Gets the handler function for a given hash
        /// Can be used by profilers and debuggers
        /// </summary>
        /// <param name="hash">rpc function hash</param>
        /// <returns>The function delegate that will handle the ServerRpc</returns>
        public static RpcDelegate GetDelegate(int hash)
        {
            if (cmdHandlerDelegates.TryGetValue(hash, out RpcMethod invoker))
            {
                return invoker.function;
            }
            return null;
        }
    }
}

