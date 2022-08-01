using System;
using System.Linq;
using System.Reflection;
using Mirage.RemoteCalls;
using Mirage.Serialization;
using Mirage.Weaver.Serialization;
using Mono.Cecil;
using Mono.Cecil.Cil;
using MethodAttributes = Mono.Cecil.MethodAttributes;

namespace Mirage.Weaver
{
    /// <summary>
    /// Processes [ServerRpc] methods in NetworkBehaviour
    /// </summary>
    public class ServerRpcProcessor : RpcProcessor
    {
        public ServerRpcProcessor(ModuleDefinition module, Readers readers, Writers writers, IWeaverLogger logger) : base(module, readers, writers, logger)
        {
        }

        protected override Type AttributeType => typeof(ServerRpcAttribute);

        /// <summary>
        /// Replaces the user code with a stub.
        /// Moves the original code to a new method
        /// </summary>
        /// <param name="td">The class containing the method </param>
        /// <param name="md">The method to be stubbed </param>
        /// <param name="ServerRpcAttr">The attribute that made this an RPC</param>
        /// <returns>The method containing the original code</returns>
        /// <remarks>
        /// Generates code like this:
        /// <code>
        /// public void MyServerRpc(float thrusting, int spin)
        /// {
        ///     NetworkWriter networkWriter = new NetworkWriter();
        ///     networkWriter.Write(thrusting);
        ///     networkWriter.WritePackedUInt32((uint) spin);
        ///     base.SendServerRpcInternal(cmdName, networkWriter, cmdName);
        /// }
        ///
        /// public void UserCode_MyServerRpc(float thrusting, int spin)
        /// {
        ///     // whatever the user was doing before
        ///
        /// }
        /// </code>
        /// </remarks>
        private MethodDefinition GenerateStub(MethodDefinition md, CustomAttribute serverRpcAttr, int rpcIndex, ValueSerializer[] paramSerializers)
        {
            var channel = serverRpcAttr.GetField(nameof(ServerRpcAttribute.channel), 0);
            var requireAuthority = serverRpcAttr.GetField(nameof(ServerRpcAttribute.requireAuthority), true);

            var cmd = SubstituteMethod(md);

            var worker = md.Body.GetILProcessor();

            // if (IsServer)
            // {
            //    call the body
            //    return;
            // }
            CallBody(worker, cmd, requireAuthority);

            // NetworkWriter writer = NetworkWriterPool.GetWriter()
            var writer = md.AddLocal<PooledNetworkWriter>();
            worker.Append(worker.Create(OpCodes.Call, md.Module.ImportReference(() => NetworkWriterPool.GetWriter())));
            worker.Append(worker.Create(OpCodes.Stloc, writer));

            // write all the arguments that the user passed to the Cmd call
            WriteArguments(worker, md, writer, paramSerializers, RemoteCallType.ServerRpc);

            var cmdName = md.FullName;
            var sendMethod = GetSendMethod(md, worker);

            // ServerRpcSender.Send(this, 12345, writer, channel, requireAuthority)
            worker.Append(worker.Create(OpCodes.Ldarg_0));
            worker.Append(worker.Create(OpCodes.Ldc_I4, rpcIndex));
            worker.Append(worker.Create(OpCodes.Ldloc, writer));
            worker.Append(worker.Create(OpCodes.Ldc_I4, channel));
            worker.Append(worker.Create(requireAuthority.OpCode_Ldc()));
            worker.Append(worker.Create(OpCodes.Call, sendMethod));

            NetworkWriterHelper.CallRelease(module, worker, writer);

            worker.Append(worker.Create(OpCodes.Ret));

            return cmd;
        }

        public void InvokeLocally(ILProcessor worker, bool requiredAuthority, Action body)
        {
            // if (IsServer) {
            var endif = worker.Create(OpCodes.Nop);
            worker.Append(worker.Create(OpCodes.Ldarg_0));
            worker.Append(worker.Create(requiredAuthority.OpCode_Ldc()));
            worker.Append(worker.Create(OpCodes.Call, () => ServerRpcSender.ShouldInvokeLocally(default, default)));
            worker.Append(worker.Create(OpCodes.Brfalse, endif));

            body();

            // }
            worker.Append(endif);

        }

        private void CallBody(ILProcessor worker, MethodDefinition rpc, bool requiredAuthority)
        {
            InvokeLocally(worker, requiredAuthority, () =>
            {
                InvokeBody(worker, rpc);
                worker.Append(worker.Create(OpCodes.Ret));
            });
        }

        private MethodReference GetSendMethod(MethodDefinition md, ILProcessor worker)
        {
            if (md.ReturnType.Is(typeof(void)))
            {
                var sendMethod = md.Module.ImportReference(() => ServerRpcSender.Send(default, default, default, default, default));
                return sendMethod;
            }
            else
            {
                // call ServerRpcSender.SendWithReturn<T> and return the result
                var sendMethod = typeof(ServerRpcSender).GetMethods(BindingFlags.Public | BindingFlags.Static).First(m => m.Name == nameof(ServerRpcSender.SendWithReturn));
                var sendRef = md.Module.ImportReference(sendMethod);

                var returnType = md.ReturnType as GenericInstanceType;

                var genericMethod = new GenericInstanceMethod(sendRef);
                genericMethod.GenericArguments.Add(returnType.GenericArguments[0]);

                return genericMethod;
            }
        }

        /// <summary>
        /// Generates a skeleton for a ServerRpc
        /// </summary>
        /// <param name="td"></param>
        /// <param name="method"></param>
        /// <param name="userCodeFunc"></param>
        /// <returns>The newly created skeleton method</returns>
        /// <remarks>
        /// Generates code like this:
        /// <code>
        /// protected static void Skeleton_MyServerRpc(NetworkBehaviour obj, NetworkReader reader, NetworkConnection senderConnection)
        /// {
        ///     if (!obj.Identity.server.active)
        ///     {
        ///         return;
        ///     }
        ///     ((ShipControl) obj).UserCode_Thrust(reader.ReadSingle(), (int) reader.ReadPackedUInt32());
        /// }
        /// </code>
        /// </remarks>
        private MethodDefinition GenerateSkeleton(MethodDefinition method, MethodDefinition userCodeFunc, ValueSerializer[] paramSerializers)
        {
            var newName = SkeletonMethodName(method);
            var cmd = method.DeclaringType.AddMethod(newName,
                MethodAttributes.Family | MethodAttributes.HideBySig | MethodAttributes.Static,
                userCodeFunc.ReturnType);

            _ = cmd.AddParam<NetworkBehaviour>("behaviour");
            var readerParameter = cmd.AddParam<NetworkReader>("reader");
            var senderParameter = cmd.AddParam<INetworkPlayer>("senderConnection");
            _ = cmd.AddParam<int>("replyId");


            var worker = cmd.Body.GetILProcessor();

            // load `behaviour.`
            worker.Append(worker.Create(OpCodes.Ldarg_0));
            worker.Append(worker.Create(OpCodes.Castclass, method.DeclaringType.MakeSelfGeneric()));

            // read and load args
            ReadArguments(method, worker, readerParameter, senderParameter, false, paramSerializers);

            // invoke actual ServerRpc function
            worker.Append(worker.Create(OpCodes.Callvirt, userCodeFunc.MakeHostInstanceSelfGeneric()));
            worker.Append(worker.Create(OpCodes.Ret));

            return cmd;
        }

        public ServerRpcMethod ProcessRpc(MethodDefinition md, CustomAttribute serverRpcAttr, int rpcIndex)
        {
            ValidateMethod(md, RemoteCallType.ServerRpc);
            ValidateParameters(md, RemoteCallType.ServerRpc);
            ValidateReturnType(md, RemoteCallType.ServerRpc);

            // default vaue true for requireAuthority, or someone could force call these on server
            var requireAuthority = serverRpcAttr.GetField(nameof(ServerRpcAttribute.requireAuthority), true);

            var paramSerializers = GetValueSerializers(md);

            var userCodeFunc = GenerateStub(md, serverRpcAttr, rpcIndex, paramSerializers);

            var skeletonFunc = GenerateSkeleton(md, userCodeFunc, paramSerializers);
            return new ServerRpcMethod
            {
                Index = rpcIndex,
                stub = md,
                requireAuthority = requireAuthority,
                skeleton = skeletonFunc
            };
        }

        protected void InvokeBody(ILProcessor worker, MethodDefinition rpc)
        {
            // load this
            worker.Append(worker.Create(OpCodes.Ldarg_0));

            // load each param of rpc
            foreach (var param in rpc.Parameters)
            {
                // if param is network player, use Server's Local player instead
                //   in host mode this will be the Server's copy of the the player,
                //   in server mode this will be null
                if (IsNetworkPlayer(param.ParameterType))
                {
                    worker.Append(worker.Create(OpCodes.Ldarg_0));
                    worker.Append(worker.Create(OpCodes.Call, (NetworkBehaviour nb) => nb.Server));
                    worker.Append(worker.Create(OpCodes.Callvirt, (INetworkServer server) => server.LocalPlayer));
                }
                else
                {
                    worker.Append(worker.Create(OpCodes.Ldarg, param));
                }
            }
            worker.Append(worker.Create(OpCodes.Callvirt, rpc.MakeHostInstanceSelfGeneric()));
        }
    }
}
