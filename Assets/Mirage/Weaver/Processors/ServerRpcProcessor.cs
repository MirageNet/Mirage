// all the [ServerRpc] code from NetworkBehaviourProcessor in one place
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Cysharp.Threading.Tasks;
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
        struct ServerRpcMethod
        {
            public MethodDefinition stub;
            public bool requireAuthority;
            public MethodDefinition skeleton;
        }

        readonly List<ServerRpcMethod> serverRpcs = new List<ServerRpcMethod>();

        public ServerRpcProcessor(ModuleDefinition module, Readers readers, Writers writers, IWeaverLogger logger) : base(module, readers, writers, logger)
        {
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
        MethodDefinition GenerateStub(MethodDefinition md, CustomAttribute serverRpcAttr, ValueSerializer[] paramSerializers)
        {
            MethodDefinition cmd = SubstituteMethod(md);

            ILProcessor worker = md.Body.GetILProcessor();

            // if (IsServer)
            // {
            //    call the body
            //    return;
            // }
            CallBody(worker, cmd);

            // NetworkWriter writer = NetworkWriterPool.GetWriter()
            VariableDefinition writer = md.AddLocal<PooledNetworkWriter>();
            worker.Append(worker.Create(OpCodes.Call, md.Module.ImportReference(() => NetworkWriterPool.GetWriter())));
            worker.Append(worker.Create(OpCodes.Stloc, writer));

            // write all the arguments that the user passed to the Cmd call
            WriteArguments(worker, md, writer, paramSerializers, RemoteCallType.ServerRpc);

            string cmdName = md.Name;

            int channel = serverRpcAttr.GetField("channel", 0);
            bool requireAuthority = serverRpcAttr.GetField("requireAuthority", true);


            // invoke internal send and return
            // load 'base.' to call the SendServerRpc function with
            worker.Append(worker.Create(OpCodes.Ldarg_0));
            worker.Append(worker.Create(OpCodes.Ldtoken, md.DeclaringType.ConvertToGenericIfNeeded()));
            // invokerClass
            worker.Append(worker.Create(OpCodes.Call, () => Type.GetTypeFromHandle(default)));
            worker.Append(worker.Create(OpCodes.Ldstr, cmdName));
            // writer
            worker.Append(worker.Create(OpCodes.Ldloc, writer));
            worker.Append(worker.Create(OpCodes.Ldc_I4, channel));
            worker.Append(worker.Create(requireAuthority ? OpCodes.Ldc_I4_1 : OpCodes.Ldc_I4_0));
            CallSendServerRpc(md, worker);

            NetworkWriterHelper.CallRelease(module, worker, writer);

            worker.Append(worker.Create(OpCodes.Ret));

            return cmd;
        }

        public void IsServer(ILProcessor worker, Action body)
        {
            // if (IsServer) {
            Instruction endif = worker.Create(OpCodes.Nop);
            worker.Append(worker.Create(OpCodes.Ldarg_0));
            worker.Append(worker.Create(OpCodes.Call, (NetworkBehaviour nb) => nb.IsServer));
            worker.Append(worker.Create(OpCodes.Brfalse, endif));

            body();

            // }
            worker.Append(endif);

        }

        private void CallBody(ILProcessor worker, MethodDefinition rpc)
        {
            IsServer(worker, () =>
            {
                InvokeBody(worker, rpc);
                worker.Append(worker.Create(OpCodes.Ret));
            });
        }

        private void CallSendServerRpc(MethodDefinition md, ILProcessor worker)
        {
            if (md.ReturnType.Is(typeof(void)))
            {
                MethodReference sendServerRpcRef = md.Module.ImportReference<NetworkBehaviour>(nb => nb.SendServerRpcInternal(default, default, default, default, default));
                worker.Append(worker.Create(OpCodes.Call, sendServerRpcRef));
            }
            else
            {
                // call SendServerRpcWithReturn<T> and return the result
                Type netBehaviour = typeof(NetworkBehaviour);

                MethodInfo sendMethod = netBehaviour.GetMethods(BindingFlags.NonPublic | BindingFlags.Instance).First(m => m.Name == nameof(NetworkBehaviour.SendServerRpcWithReturn));
                MethodReference sendRef = md.Module.ImportReference(sendMethod);

                var returnType = md.ReturnType as GenericInstanceType;

                var instanceMethod = new GenericInstanceMethod(sendRef);
                instanceMethod.GenericArguments.Add(returnType.GenericArguments[0]);

                worker.Append(worker.Create(OpCodes.Call, instanceMethod));

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
        MethodDefinition GenerateSkeleton(MethodDefinition method, MethodDefinition userCodeFunc, ValueSerializer[] paramSerializers)
        {
            MethodDefinition cmd = method.DeclaringType.AddMethod(SkeletonPrefix + method.Name,
                MethodAttributes.Family | MethodAttributes.HideBySig | MethodAttributes.Static,
                userCodeFunc.ReturnType);

            _ = cmd.AddParam(method.DeclaringType, "behaviour");
            ParameterDefinition readerParameter = cmd.AddParam<NetworkReader>("reader");
            ParameterDefinition senderParameter = cmd.AddParam<INetworkPlayer>("senderConnection");
            _ = cmd.AddParam<int>("replyId");


            ILProcessor worker = cmd.Body.GetILProcessor();

            // load `behaviour.`
            worker.Append(worker.Create(OpCodes.Ldarg_0));

            // read and load args
            ReadArguments(method, worker, readerParameter, senderParameter, false, paramSerializers);

            // invoke actual ServerRpc function
            worker.Append(worker.Create(OpCodes.Callvirt, userCodeFunc));
            worker.Append(worker.Create(OpCodes.Ret));

            return cmd;
        }

        internal bool Validate(MethodDefinition md)
        {
            Type unitaskType = typeof(UniTask<int>).GetGenericTypeDefinition();
            if (!md.ReturnType.Is(typeof(void)) && !md.ReturnType.Is(unitaskType))
            {
                logger.Error($"Use UniTask<{ md.ReturnType}> to return values from [ServerRpc]", md);
                return false;
            }

            return true;
        }

        public void RegisterServerRpcs(ILProcessor cctorWorker)
        {
            foreach (ServerRpcMethod cmdResult in serverRpcs)
            {
                GenerateRegisterServerRpcDelegate(cctorWorker, cmdResult);
            }
        }

        void GenerateRegisterServerRpcDelegate(ILProcessor worker, ServerRpcMethod cmdResult)
        {
            MethodDefinition skeleton = cmdResult.skeleton;
            MethodReference registerMethod = GetRegisterMethod(skeleton);
            string cmdName = cmdResult.stub.Name;
            bool requireAuthority = cmdResult.requireAuthority;

            TypeDefinition netBehaviourSubclass = skeleton.DeclaringType;
            worker.Append(worker.Create(OpCodes.Ldtoken, netBehaviourSubclass.ConvertToGenericIfNeeded()));
            worker.Append(worker.Create(OpCodes.Call, () => Type.GetTypeFromHandle(default)));
            worker.Append(worker.Create(OpCodes.Ldstr, cmdName));
            worker.Append(worker.Create(OpCodes.Ldnull));
            CreateRpcDelegate(worker, skeleton);

            worker.Append(worker.Create(requireAuthority ? OpCodes.Ldc_I4_1 : OpCodes.Ldc_I4_0));

            worker.Append(worker.Create(OpCodes.Call, registerMethod));
        }

        private static MethodReference GetRegisterMethod(MethodDefinition func)
        {
            if (func.ReturnType.Is(typeof(void)))
                return func.Module.ImportReference(() => RemoteCallHelper.RegisterServerRpcDelegate(default, default, default, default));

            var taskReturnType = func.ReturnType as GenericInstanceType;

            TypeReference returnType = taskReturnType.GenericArguments[0];

            var genericRegisterMethod = func.Module.ImportReference(() => RemoteCallHelper.RegisterRequestDelegate<object>(default, default, default, default)) as GenericInstanceMethod;

            var registerInstance = new GenericInstanceMethod(genericRegisterMethod.ElementMethod);
            registerInstance.GenericArguments.Add(returnType);
            return registerInstance;
        }

        public void ProcessRpc(MethodDefinition md, CustomAttribute serverRpcAttr)
        {
            if (!ValidateRemoteCallAndParameters(md, RemoteCallType.ServerRpc))
                return;

            if (!Validate(md))
                return;

            bool requireAuthority = serverRpcAttr.GetField("requireAuthority", false);

            ValueSerializer[] paramSerializers = GetValueSerializers(md);

            MethodDefinition userCodeFunc = GenerateStub(md, serverRpcAttr, paramSerializers);

            MethodDefinition skeletonFunc = GenerateSkeleton(md, userCodeFunc, paramSerializers);
            serverRpcs.Add(new ServerRpcMethod
            {
                stub = md,
                requireAuthority = requireAuthority,
                skeleton = skeletonFunc
            });
        }

        protected void InvokeBody(ILProcessor worker, MethodDefinition rpc)
        {
            // load this
            worker.Append(worker.Create(OpCodes.Ldarg_0));

            // load each param of rpc
            foreach (ParameterDefinition param in rpc.Parameters)
            {
                // if param is network player, use Server's Local player instead
                //   in host mode this will be the Server's copy of the the player,
                //   in server mode this will be null
                if (IsNetworkPlayer(param))
                {
                    worker.Append(worker.Create(OpCodes.Ldarg_0));
                    worker.Append(worker.Create(OpCodes.Call, (NetworkBehaviour nb) => nb.Server));
                    worker.Append(worker.Create(OpCodes.Call, (NetworkServer server) => server.LocalPlayer));
                }
                else
                {
                    worker.Append(worker.Create(OpCodes.Ldarg, param));
                }
            }
            worker.Append(worker.Create(OpCodes.Call, rpc));

            bool IsNetworkPlayer(ParameterDefinition param)
            {
                return param.ParameterType.Resolve().ImplementsInterface<INetworkPlayer>();
            }
        }
    }
}
