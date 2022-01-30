using System;
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
            MethodDefinition rpc = md.DeclaringType.AddMethod(
                SkeletonPrefix + md.Name,
                MethodAttributes.Family | MethodAttributes.HideBySig);

            ParameterDefinition readerParameter = rpc.AddParam<NetworkReader>("reader");
            _ = rpc.AddParam<INetworkPlayer>("senderConnection");
            _ = rpc.AddParam<int>("replyId");

            ILProcessor worker = rpc.Body.GetILProcessor();

            worker.Append(worker.Create(OpCodes.Ldarg_0));

            // NetworkConnection parameter is only required for RpcTarget.Player
            RpcTarget target = clientRpcAttr.GetField("target", RpcTarget.Observers);
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
            worker.Append(worker.Create(OpCodes.Callvirt, userCodeFunc));
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
        MethodDefinition GenerateStub(MethodDefinition md, CustomAttribute clientRpcAttr, ValueSerializer[] paramSerializers)
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

            string rpcName = md.Name;

            RpcTarget target = clientRpcAttr.GetField("target", RpcTarget.Observers);
            int channel = clientRpcAttr.GetField("channel", 0);
            bool excludeOwner = clientRpcAttr.GetField("excludeOwner", false);

            // invoke SendInternal and return
            // this
            worker.Append(worker.Create(OpCodes.Ldarg_0));

            if (target == RpcTarget.Player && HasNetworkPlayerParameter(md))
                worker.Append(worker.Create(OpCodes.Ldarg_1));
            else if (target == RpcTarget.Owner)
                worker.Append(worker.Create(OpCodes.Ldnull));

            worker.Append(worker.Create(OpCodes.Ldtoken, md.DeclaringType.ConvertToGenericIfNeeded()));
            // invokerClass
            worker.Append(worker.Create(OpCodes.Call, () => Type.GetTypeFromHandle(default)));
            worker.Append(worker.Create(OpCodes.Ldstr, rpcName));
            // writer
            worker.Append(worker.Create(OpCodes.Ldloc, writer));
            worker.Append(worker.Create(OpCodes.Ldc_I4, channel));

            if (target == RpcTarget.Observers)
            {
                worker.Append(worker.Create(excludeOwner ? OpCodes.Ldc_I4_1 : OpCodes.Ldc_I4_0));
                MethodReference sendRpcRef = md.Module.ImportReference<NetworkBehaviour>(nb => nb.SendRpcInternal(default, default, default, default, default));
                worker.Append(worker.Create(OpCodes.Callvirt, sendRpcRef));
            }
            else
            {
                MethodReference sendTargetRpcRef = md.Module.ImportReference<NetworkBehaviour>(nb => nb.SendTargetRpcInternal(default, default, default, default, default));
                worker.Append(worker.Create(OpCodes.Callvirt, sendTargetRpcRef));
            }

            NetworkWriterHelper.CallRelease(module, worker, writer);

            worker.Append(worker.Create(OpCodes.Ret));

            return rpc;
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
            worker.Append(worker.Create(OpCodes.Callvirt, rpc));
        }

        public ClientRpcMethod ProcessRpc(MethodDefinition md, CustomAttribute clientRpcAttr)
        {
            ValidateMethod(md, RemoteCallType.ClientRpc);
            ValidateParameters(md, RemoteCallType.ClientRpc);
            ValidateReturnType(md, RemoteCallType.ClientRpc);
            ValidateAttribute(md, clientRpcAttr);

            RpcTarget clientTarget = clientRpcAttr.GetField("target", RpcTarget.Observers);
            bool excludeOwner = clientRpcAttr.GetField("excludeOwner", false);

            ValueSerializer[] paramSerializers = GetValueSerializers(md);

            MethodDefinition userCodeFunc = GenerateStub(md, clientRpcAttr, paramSerializers);

            MethodDefinition skeletonFunc = GenerateSkeleton(md, userCodeFunc, clientRpcAttr, paramSerializers);

            return new ClientRpcMethod
            {
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
            RpcTarget target = clientRpcAttr.GetField("target", RpcTarget.Observers);
            if (target == RpcTarget.Player && !HasNetworkPlayerParameter(md))
            {
                throw new RpcException("ClientRpc with RpcTarget.Player needs a network player parameter", md);
            }

            bool excludeOwner = clientRpcAttr.GetField("excludeOwner", false);
            if (target == RpcTarget.Owner && excludeOwner)
            {
                throw new RpcException("ClientRpc with RpcTarget.Owner cannot have excludeOwner set as true", md);
            }
        }
    }
}
