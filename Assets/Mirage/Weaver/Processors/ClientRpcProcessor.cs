using System;
using Mirage.RemoteCalls;
using Mirage.Serialization;
using Mirage.Weaver.Serialization;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace Mirage.Weaver
{
    /// <summary>
    /// Processes [Rpc] methods in NetworkBehaviour
    /// </summary>
    public class ClientRpcProcessor : RpcProcessor
    {
        public ClientRpcProcessor(ModuleDefinition module, Readers readers, Writers writers, IWeaverLogger logger) : base(module, readers, writers, logger)
        {
        }

        protected override Type AttributeType => typeof(ClientRpcAttribute);

        /// <summary>
        /// Generates a skeleton for an RPC
        /// </summary>
        /// <param name="td"></param>
        /// <param name="method"></param>
        /// <param name="cmdCallFunc"></param>
        /// <returns>The newly created skeleton method</returns>
        /// <remarks>
        /// Generates code like this:
        /// <code>
        /// protected static void Skeleton_Test(NetworkBehaviour obj, NetworkReader reader, NetworkConnection senderConnection)
        /// {
        ///     if (!obj.Identity.server.active)
        ///     {
        ///         return;
        ///     }
        ///     ((ShipControl) obj).UserCode_Test(reader.ReadSingle(), (int) reader.ReadPackedUInt32());
        /// }
        /// </code>
        /// </remarks>
        MethodDefinition GenerateSkeleton(MethodDefinition md, MethodDefinition userCodeFunc, CustomAttribute clientRpcAttr, ValueSerializer[] paramSerializers)
        {
            string newName = SkeletonMethodName(md);
            MethodDefinition rpc = md.DeclaringType.AddMethod(
                newName,
                MethodAttributes.Family | MethodAttributes.HideBySig | MethodAttributes.Static);

            _ = rpc.AddParam<NetworkBehaviour>("behaviour");
            ParameterDefinition readerParameter = rpc.AddParam<NetworkReader>("reader");
            _ = rpc.AddParam<INetworkPlayer>("senderConnection");
            _ = rpc.AddParam<int>("replyId");

            ILProcessor worker = rpc.Body.GetILProcessor();

            worker.Append(worker.Create(OpCodes.Ldarg_0));
            worker.Append(worker.Create(OpCodes.Castclass, md.DeclaringType.MakeSelfGeneric()));

            // NetworkConnection parameter is only required for RpcTarget.Player
            RpcTarget target = clientRpcAttr.GetField(nameof(ClientRpcAttribute.target), RpcTarget.Observers);
            bool hasNetworkConnection = target == RpcTarget.Player && HasNetworkPlayerParameter(md);

            if (hasNetworkConnection)
            {
                // this is called in the skeleton (the client)
                // the client should just get the connection to the server and pass that in
                worker.Append(worker.Create(OpCodes.Ldarg_0));
                worker.Append(worker.Create(OpCodes.Call, (NetworkBehaviour nb) => nb.Client));
                worker.Append(worker.Create(OpCodes.Callvirt, (INetworkClient nb) => nb.Player));
            }

            ReadArguments(md, worker, readerParameter, senderParameter: null, hasNetworkConnection, paramSerializers);

            // invoke actual ServerRpc function
            worker.Append(worker.Create(OpCodes.Callvirt, userCodeFunc.MakeHostInstanceSelfGeneric()));
            worker.Append(worker.Create(OpCodes.Ret));

            return rpc;
        }

        /// <summary>
        /// Replaces the user code with a stub.
        /// Moves the original code to a new method
        /// </summary>
        /// <param name="td">The class containing the method </param>
        /// <param name="md">The method to be stubbed </param>
        /// <param name="ServerRpcAttr">The attribute that made this an RPC</param>
        /// <returns>The method containing the original code</returns>
        /// <remarks>
        /// Generates code like this: (Observers case)
        /// <code>
        /// public void Test (int param)
        /// {
        ///     NetworkWriter writer = new NetworkWriter();
        ///     writer.WritePackedUInt32((uint) param);
        ///     base.SendRpcInternal(typeof(class),"RpcTest", writer, 0);
        /// }
        /// public void UserCode_Test(int param)
        /// {
        ///     // whatever the user did before
        /// }
        /// </code>
        ///
        /// Generates code like this: (Owner/Connection case)
        /// <code>
        /// public void TargetTest(NetworkConnection conn, int param)
        /// {
        ///     NetworkWriter writer = new NetworkWriter();
        ///     writer.WritePackedUInt32((uint)param);
        ///     base.SendTargetRpcInternal(conn, typeof(class), "TargetTest", val);
        /// }
        /// 
        /// public void UserCode_TargetTest(NetworkConnection conn, int param)
        /// {
        ///     // whatever the user did before
        /// }
        /// </code>
        /// or if no connection is specified
        ///
        /// <code>
        /// public void TargetTest (int param)
        /// {
        ///     NetworkWriter writer = new NetworkWriter();
        ///     writer.WritePackedUInt32((uint) param);
        ///     base.SendTargetRpcInternal(null, typeof(class), "TargetTest", val);
        /// }
        /// 
        /// public void UserCode_TargetTest(int param)
        /// {
        ///     // whatever the user did before
        /// }
        /// </code>
        /// </remarks>
        MethodDefinition GenerateStub(MethodDefinition md, CustomAttribute clientRpcAttr, int rpcIndex, ValueSerializer[] paramSerializers)
        {
            MethodDefinition rpc = SubstituteMethod(md);

            ILProcessor worker = md.Body.GetILProcessor();

            // if (IsClient)
            // {
            //    call the body
            // }
            CallBody(worker, rpc);

            // NetworkWriter writer = NetworkWriterPool.GetWriter()
            VariableDefinition writer = md.AddLocal<PooledNetworkWriter>();
            worker.Append(worker.Create(OpCodes.Call, () => NetworkWriterPool.GetWriter()));
            worker.Append(worker.Create(OpCodes.Stloc, writer));

            // write all the arguments that the user passed to the Rpc call
            WriteArguments(worker, md, writer, paramSerializers, RemoteCallType.ClientRpc);

            string rpcName = md.FullName;

            RpcTarget target = clientRpcAttr.GetField(nameof(ClientRpcAttribute.target), RpcTarget.Observers);
            int channel = clientRpcAttr.GetField(nameof(ClientRpcAttribute.channel), 0);
            bool excludeOwner = clientRpcAttr.GetField(nameof(ClientRpcAttribute.excludeOwner), false);

            MethodReference sendMethod = GetSendMethod(md, target);

            // ClientRpcSender.Send(this, 12345, writer, channel, requireAuthority)
            worker.Append(worker.Create(OpCodes.Ldarg_0));
            worker.Append(worker.Create(OpCodes.Ldc_I4, rpcIndex));
            worker.Append(worker.Create(OpCodes.Ldloc, writer));
            worker.Append(worker.Create(OpCodes.Ldc_I4, channel));
            // last arg of send is either bool, or NetworkPlayer
            // see ClientRpcSender.Send methods
            if (target == RpcTarget.Observers)
                worker.Append(worker.Create(excludeOwner ? OpCodes.Ldc_I4_1 : OpCodes.Ldc_I4_0));
            else if (target == RpcTarget.Player && HasNetworkPlayerParameter(md))
                worker.Append(worker.Create(OpCodes.Ldarg_1));
            else // owner, or Player with no arg
                worker.Append(worker.Create(OpCodes.Ldnull));


            worker.Append(worker.Create(OpCodes.Call, sendMethod));

            NetworkWriterHelper.CallRelease(module, worker, writer);

            worker.Append(worker.Create(OpCodes.Ret));

            return rpc;
        }

        private static MethodReference GetSendMethod(MethodDefinition md, RpcTarget target)
        {
            return target == RpcTarget.Observers
                          ? md.Module.ImportReference(() => ClientRpcSender.Send(default, default, default, default, default))
                          : md.Module.ImportReference(() => ClientRpcSender.SendTarget(default, default, default, default, default));
        }

        void IsClient(ILProcessor worker, Action body)
        {
            // if (IsLocalClient) {
            Instruction endif = worker.Create(OpCodes.Nop);
            worker.Append(worker.Create(OpCodes.Ldarg_0));
            worker.Append(worker.Create(OpCodes.Call, (NetworkBehaviour nb) => nb.IsClient));
            worker.Append(worker.Create(OpCodes.Brfalse, endif));

            body();

            // }
            worker.Append(endif);

        }

        void CallBody(ILProcessor worker, MethodDefinition rpc)
        {
            IsClient(worker, () =>
            {
                InvokeBody(worker, rpc);
            });
        }

        void InvokeBody(ILProcessor worker, MethodDefinition rpc)
        {
            worker.Append(worker.Create(OpCodes.Ldarg_0));

            for (int i = 0; i < rpc.Parameters.Count; i++)
            {
                ParameterDefinition parameter = rpc.Parameters[i];
                if (parameter.ParameterType.Is<INetworkPlayer>())
                {
                    // when a client rpc is invoked in host mode
                    // and it receives a INetworkPlayer,  we
                    // need to change the value we pass to the
                    // local connection to the server
                    worker.Append(worker.Create(OpCodes.Ldarg_0));
                    worker.Append(worker.Create(OpCodes.Call, (NetworkBehaviour nb) => nb.Client));
                    worker.Append(worker.Create(OpCodes.Callvirt, (INetworkClient nc) => nc.Player));
                }
                else
                {
                    worker.Append(worker.Create(OpCodes.Ldarg, i + 1));
                }
            }
            worker.Append(worker.Create(OpCodes.Callvirt, rpc.MakeHostInstanceSelfGeneric()));
        }

        public ClientRpcMethod ProcessRpc(MethodDefinition md, CustomAttribute clientRpcAttr, int rpcIndex)
        {
            ValidateMethod(md, RemoteCallType.ClientRpc);
            ValidateParameters(md, RemoteCallType.ClientRpc);
            ValidateReturnType(md, RemoteCallType.ClientRpc);
            ValidateAttribute(md, clientRpcAttr);

            RpcTarget clientTarget = clientRpcAttr.GetField(nameof(ClientRpcAttribute.target), RpcTarget.Observers);
            bool excludeOwner = clientRpcAttr.GetField(nameof(ClientRpcAttribute.excludeOwner), false);

            ValueSerializer[] paramSerializers = GetValueSerializers(md);

            MethodDefinition userCodeFunc = GenerateStub(md, clientRpcAttr, rpcIndex, paramSerializers);

            MethodDefinition skeletonFunc = GenerateSkeleton(md, userCodeFunc, clientRpcAttr, paramSerializers);

            return new ClientRpcMethod
            {
                Index = rpcIndex,
                stub = md,
                target = clientTarget,
                excludeOwner = excludeOwner,
                skeleton = skeletonFunc
            };
        }

        /// <summary>
        /// checks ClientRpc Attribute values are valid
        /// </summary>
        /// <exception cref="RpcException">Throws when parameter are invalid</exception>
        void ValidateAttribute(MethodDefinition md, CustomAttribute clientRpcAttr)
        {
            RpcTarget target = clientRpcAttr.GetField(nameof(ClientRpcAttribute.target), RpcTarget.Observers);
            if (target == RpcTarget.Player && !HasNetworkPlayerParameter(md))
            {
                throw new RpcException("ClientRpc with RpcTarget.Player needs a network player parameter", md);
            }

            bool excludeOwner = clientRpcAttr.GetField(nameof(ClientRpcAttribute.excludeOwner), false);
            if (target == RpcTarget.Owner && excludeOwner)
            {
                throw new RpcException("ClientRpc with RpcTarget.Owner cannot have excludeOwner set as true", md);
            }
        }
    }
}
