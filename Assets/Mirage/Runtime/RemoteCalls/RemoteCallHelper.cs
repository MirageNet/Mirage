using System;
using Cysharp.Threading.Tasks;
using Mirage.Logging;
using Mirage.Serialization;
using UnityEngine;

namespace Mirage.RemoteCalls
{
    public class RemoteCallCollection
    {
        static readonly ILogger logger = LogFactory.GetLogger(typeof(RemoteCallCollection));

        public RemoteCall[] remoteCalls;

        public RemoteCallCollection(NetworkBehaviour behaviour)
        {
            remoteCalls = new RemoteCall[behaviour.GetRpcCount()];
        }

        public void Register(int index, Type invokeClass, string name, RpcInvokeType invokerType, RpcDelegate func, bool cmdRequireAuthority)
        {
            // weaver gives index, so should never give 2 indexes that are the same
            if (remoteCalls[index] != null)
                throw new InvalidOperationException("2 Rpc has same index");

            var call = new RemoteCall(invokeClass, invokerType, func, cmdRequireAuthority, name);
            remoteCalls[index] = call;

            if (logger.LogEnabled())
            {
                string requireAuthorityMessage = invokerType == RpcInvokeType.ServerRpc ? $" RequireAuthority:{cmdRequireAuthority}" : "";
                logger.Log($"RegisterDelegate invokerType: {invokerType} method: {func.Method.Name}{requireAuthorityMessage}");
            }
        }

        public void RegisterRequest<T>(int index, Type invokeClass, string name, RequestDelegate<T> func, bool cmdRequireAuthority)
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

            Register(index, invokeClass, name, RpcInvokeType.ServerRpc, CmdWrapper, cmdRequireAuthority);
        }

        public RemoteCall Get(int index)
        {
            return remoteCalls[index];
        }
    }
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
        public readonly Type DeclaringType;
        /// <summary>
        /// Server rpc or client rpc
        /// </summary>
        public readonly RpcInvokeType InvokeType;
        /// <summary>
        /// Function to be invoked when receiving message
        /// </summary>
        public readonly RpcDelegate function;
        /// <summary>
        /// Used by ServerRpc
        /// </summary>
        public readonly bool RequireAuthority;
        /// <summary>
        /// User friendly name
        /// </summary>
        public readonly string name;

        public RemoteCall(Type declaringType, RpcInvokeType invokeType, RpcDelegate function, bool requireAuthority, string name)
        {
            DeclaringType = declaringType;
            InvokeType = invokeType;
            this.function = function;
            RequireAuthority = requireAuthority;
            this.name = name;
        }

        public bool AreEqual(Type declaringType, RpcInvokeType invokeType, RpcDelegate function)
        {
            if (InvokeType != invokeType)
                return false;

            if (declaringType.IsGenericType)
                return AreEqualIgnoringGeneric(declaringType, function);

            return DeclaringType == declaringType
                && this.function == function;
        }

        bool AreEqualIgnoringGeneric(Type declaringType, RpcDelegate function)
        {
            // if this.type not generic, then not equal
            if (!DeclaringType.IsGenericType)
                return false;

            // types must be in same assembly to be equal
            if (DeclaringType.Assembly != declaringType.Assembly)
                return false;

            Debug.Assert(declaringType == function.Method.DeclaringType);
            Debug.Assert(DeclaringType == this.function.Method.DeclaringType);

            // we check Assembly above, so we know these 2 functions must be in same assmebly here
            // - we can check Namespace and Name to acount generic check
            // - weaver check to make sure method in type have unique hash
            // - weaver appends hash to names, so overloads will have different hash/names
            return DeclaringType.Namespace == declaringType.Namespace
                && DeclaringType.Name == declaringType.Name
                && this.function.Method.Name == function.Method.Name;
        }

        internal void Invoke(NetworkReader reader, NetworkBehaviour invokingType, INetworkPlayer senderPlayer = null, int replyId = 0)
        {
            function(invokingType, reader, senderPlayer, replyId);
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
}

