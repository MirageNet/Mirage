using System;
using System.Runtime.CompilerServices;
using Cysharp.Threading.Tasks;
using Mirage.Logging;
using Mirage.Serialization;
using UnityEngine;

namespace Mirage.RemoteCalls
{
    public class RemoteCallCollection
    {
        private static readonly ILogger logger = LogFactory.GetLogger(typeof(RemoteCallCollection));

        /// <summary>
        /// This is set by NetworkIdentity when we register each NetworkBehaviour so that they can pass their own idnex in
        /// </summary>
        public int[] IndexOffset;
        public RemoteCall[] RemoteCalls;

        public unsafe void RegisterAll(NetworkBehaviour[] behaviours)
        {
            var behaviourCount = behaviours.Length;
            var totalCount = 0;
            var counts = stackalloc int[behaviourCount];
            IndexOffset = new int[behaviourCount];
            for (var i = 0; i < behaviourCount; i++)
            {
                counts[i] = behaviours[i].GetRpcCount();
                totalCount += counts[i];

                if (i > 0)
                    IndexOffset[i] = IndexOffset[i - 1] + counts[i - 1];
            }

            RemoteCalls = new RemoteCall[totalCount];
            for (var i = 0; i < behaviourCount; i++)
            {
                behaviours[i].RegisterRpc(this);
            }
        }

        public void Register(int index, string name, bool cmdRequireAuthority, RpcInvokeType invokerType, NetworkBehaviour behaviour, RpcDelegate func)
        {
            var indexOffset = GetIndexOffset(behaviour);
            // weaver gives index, so should never give 2 indexes that are the same
            if (RemoteCalls[indexOffset + index] != null)
                throw new InvalidOperationException("2 Rpc has same index");

            var call = new RemoteCall(behaviour, invokerType, func, cmdRequireAuthority, name);
            RemoteCalls[indexOffset + index] = call;

            if (logger.LogEnabled())
            {
                var requireAuthorityMessage = invokerType == RpcInvokeType.ServerRpc ? $" RequireAuthority:{cmdRequireAuthority}" : "";
                logger.Log($"RegisterDelegate invokerType: {invokerType} method: {func.Method.Name}{requireAuthorityMessage}");
            }
        }

        public void RegisterRequest<T>(int index, string name, bool cmdRequireAuthority, RpcInvokeType invokerType, NetworkBehaviour behaviour, RequestDelegate<T> func)
        {
            async UniTaskVoid Wrapper(NetworkBehaviour obj, NetworkReader reader, INetworkPlayer senderPlayer, int replyId)
            {
                /// invoke the serverRpc and send a reply message
                var result = await func(obj, reader, senderPlayer, replyId);

                using (var writer = NetworkWriterPool.GetWriter())
                {
                    writer.Write(result);
                    var serverRpcReply = new RpcReply
                    {
                        ReplyId = replyId,
                        Payload = writer.ToArraySegment()
                    };

                    senderPlayer.Send(serverRpcReply);
                }
            }

            void CmdWrapper(NetworkBehaviour obj, NetworkReader reader, INetworkPlayer senderPlayer, int replyId)
            {
                Wrapper(obj, reader, senderPlayer, replyId).Forget();
            }

            Register(index, name, cmdRequireAuthority, invokerType, behaviour, CmdWrapper);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetIndexOffset(NetworkBehaviour behaviour)
        {
            return IndexOffset[behaviour.ComponentIndex];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public RemoteCall GetRelative(NetworkBehaviour behaviour, int index)
        {
            return RemoteCalls[GetIndexOffset(behaviour) + index];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public RemoteCall GetAbsolute(int index)
        {
            return RemoteCalls[index];
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
        public readonly RpcDelegate Function;
        /// <summary>
        /// Used by ServerRpc
        /// </summary>
        public readonly bool RequireAuthority;
        /// <summary>
        /// User friendly name
        /// </summary>
        public readonly string Name;

        public readonly NetworkBehaviour Behaviour;

        public RemoteCall(NetworkBehaviour behaviour, RpcInvokeType invokeType, RpcDelegate function, bool requireAuthority, string name)
        {
            Behaviour = behaviour;
            InvokeType = invokeType;
            Function = function;
            RequireAuthority = requireAuthority;
            Name = name;
        }

        internal void Invoke(NetworkReader reader, INetworkPlayer senderPlayer = null, int replyId = 0)
        {
            Function(Behaviour, reader, senderPlayer, replyId);
        }

        /// <summary>
        /// User friendly name used for debug/error messages
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return Name;
        }
    }
}

