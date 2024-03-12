using System;
using System.Linq;
using System.Reflection;
using Mirage.CodeGen;
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
        private MethodDefinition GenerateStub(MethodDefinition md, CustomAttribute clientRpcAttr, int rpcIndex, ValueSerializer[] paramSerializers, ReturnType returnType)
        {
            // get values from attribute
            var target = clientRpcAttr.GetField(nameof(ClientRpcAttribute.target), RpcTarget.Observers);
            var channel = clientRpcAttr.GetField(nameof(ClientRpcAttribute.channel), 0);
            var excludeOwner = clientRpcAttr.GetField(nameof(ClientRpcAttribute.excludeOwner), false);
            var excludeHost = clientRpcAttr.GetField(nameof(ClientRpcAttribute.excludeHost), false);

            var rpc = SubstituteMethod(md);

            var worker = md.Body.GetILProcessor();

            // if (IsClient)
            // {
            //    call the body
            // }
            if (!excludeHost)
                CallBody(worker, rpc, target, excludeOwner);

            // NetworkWriter writer = NetworkWriterPool.GetWriter()
            var writer = md.AddLocal<PooledNetworkWriter>();
            worker.Append(worker.Create(OpCodes.Call, () => NetworkWriterPool.GetWriter()));
            worker.Append(worker.Create(OpCodes.Stloc, writer));

            // write all the arguments that the user passed to the Rpc call
            WriteArguments(worker, md, writer, paramSerializers, RemoteCallType.ClientRpc);

            var rpcName = md.FullName;
            var sendMethod = GetSendMethod(md, target, returnType);

            // ClientRpcSender.Send(this, 12345, writer, channel, requireAuthority)
            worker.Append(worker.Create(OpCodes.Ldarg_0));
            worker.Append(worker.Create(OpCodes.Ldc_I4, rpcIndex));
            worker.Append(worker.Create(OpCodes.Ldloc, writer));
            // reply values always have reliable chnnael, so "SendWithReturn" does not take a channel parameter
            if (returnType == ReturnType.Void)
                worker.Append(worker.Create(OpCodes.Ldc_I4, channel));
            // last arg of send is either bool, or NetworkPlayer
            // see ClientRpcSender.Send methods
            if (target == RpcTarget.Observers)
                worker.Append(worker.Create(excludeOwner.OpCode_Ldc()));
            else if (target == RpcTarget.Player && HasFirstParameter<INetworkPlayer>(md))
                worker.Append(worker.Create(OpCodes.Ldarg_1));
            else // owner, or Player with no arg
                worker.Append(worker.Create(OpCodes.Ldnull));

            worker.Append(worker.Create(OpCodes.Call, sendMethod));

            NetworkWriterHelper.CallRelease(module, worker, writer);
            worker.Append(worker.Create(OpCodes.Ret));

            return rpc;
        }

        private static MethodReference GetSendMethod(MethodDefinition md, RpcTarget target, ReturnType returnType)
        {
            if (returnType == ReturnType.Void)
            {
                return target == RpcTarget.Observers
                    ? md.Module.ImportReference(() => ClientRpcSender.Send(default, default, default, default, default))
                    : md.Module.ImportReference(() => ClientRpcSender.SendTarget(default, default, default, default, default));
            }
            else
            {
                if (target == RpcTarget.Observers)
                    throw new InvalidOperationException("should have checked return type before this point");

                // call ClientRpcSender.SendWithReturn<T> and return the result
                var sendMethod = typeof(ClientRpcSender).GetMethods(BindingFlags.Public | BindingFlags.Static).First(m => m.Name == nameof(ClientRpcSender.SendTargetWithReturn));
                var sendRef = md.Module.ImportReference(sendMethod);

                var genericReturnType = md.ReturnType as GenericInstanceType;

                var genericMethod = new GenericInstanceMethod(sendRef);
                genericMethod.GenericArguments.Add(genericReturnType.GenericArguments[0]);

                return genericMethod;
            }
        }



        private void InvokeLocally(ILProcessor worker, RpcTarget target, bool excludeOwner, Action body)
        {
            // if (IsLocalClient) {
            var endif = worker.Create(OpCodes.Nop);

            // behaviour
            worker.Append(worker.Create(OpCodes.Ldarg_0));
            // rpcTarget
            worker.Append(worker.Create(OpCodes.Ldc_I4, (int)target));
            // networkPlayer (or null)
            if (target == RpcTarget.Player)
                // target will be arg1
                worker.Append(worker.Create(OpCodes.Ldarg_1));
            else
                worker.Append(worker.Create(OpCodes.Ldnull));

            worker.Append(worker.Create(excludeOwner.OpCode_Ldc()));
            // call function
            worker.Append(worker.Create(OpCodes.Call, () => ClientRpcSender.ShouldInvokeLocally(default, default, default, default)));
            worker.Append(worker.Create(OpCodes.Brfalse, endif));

            body();

            // }
            worker.Append(endif);

        }

        private void CallBody(ILProcessor worker, MethodDefinition rpc, RpcTarget target, bool excludeOwner)
        {
            InvokeLocally(worker, target, excludeOwner, () =>
            {
                InvokeBody(worker, rpc);
                // if target is owner or player we can return after invoking locally
                // this is because:
                // - there will be nothing to send, since the 1 target is the host
                // - if returnType is UniTask we need to return it here
                // if target is observers, then dont return, because we will be send to players other than just the host
                if (target == RpcTarget.Owner || target == RpcTarget.Player)
                    worker.Append(worker.Create(OpCodes.Ret));
            });
        }

        private void InvokeBody(ILProcessor worker, MethodDefinition rpc)
        {
            worker.Append(worker.Create(OpCodes.Ldarg_0));

            for (var i = 0; i < rpc.Parameters.Count; i++)
            {
                var parameter = rpc.Parameters[i];
                if (parameter.ParameterType.Is<INetworkPlayer>())
                {
                    // when a client rpc is invoked in host mode
                    // and it receives a INetworkPlayer,  we
                    // need to change the value we pass to the
                    // local connection to the server
                    worker.Append(worker.Create(OpCodes.Ldarg_0));
                    worker.Append(worker.Create(OpCodes.Call, (NetworkBehaviour nb) => nb.Client));
                    worker.Append(worker.Create(OpCodes.Callvirt, (NetworkClient nc) => nc.Player));
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
            ValidateMethod(md);
            ValidateParameters(md, RemoteCallType.ClientRpc);
            ValidateAttribute(md, clientRpcAttr);

            var target = clientRpcAttr.GetField(nameof(ClientRpcAttribute.target), RpcTarget.Observers);
            var excludeOwner = clientRpcAttr.GetField(nameof(ClientRpcAttribute.excludeOwner), false);

            var returnType = ValidateReturnType(md, RemoteCallType.ClientRpc, target);

            var paramSerializers = GetValueSerializers(md);

            var userCodeFunc = GenerateStub(md, clientRpcAttr, rpcIndex, paramSerializers, returnType);

            var skeletonFunc = GenerateSkeleton(md, userCodeFunc, clientRpcAttr, paramSerializers);

            return new ClientRpcMethod
            {
                Index = rpcIndex,
                stub = md,
                target = target,
                excludeOwner = excludeOwner,
                skeleton = skeletonFunc,
                ReturnType = returnType,
            };
        }

        /// <summary>
        /// checks ClientRpc Attribute values are valid
        /// </summary>
        /// <exception cref="RpcException">Throws when parameter are invalid</exception>
        private void ValidateAttribute(MethodDefinition md, CustomAttribute clientRpcAttr)
        {
            var target = clientRpcAttr.GetField(nameof(ClientRpcAttribute.target), RpcTarget.Observers);
            if (target == RpcTarget.Player && !HasFirstParameter<INetworkPlayer>(md))
            {
                throw new RpcException("ClientRpc with RpcTarget.Player needs a network player parameter", md);
            }

            var excludeOwner = clientRpcAttr.GetField(nameof(ClientRpcAttribute.excludeOwner), false);
            if (target == RpcTarget.Owner && excludeOwner)
            {
                throw new RpcException("ClientRpc with RpcTarget.Owner cannot have excludeOwner set as true", md);
            }
        }
    }
}
