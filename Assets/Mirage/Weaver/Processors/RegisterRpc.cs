using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Cysharp.Threading.Tasks;
using Mirage.RemoteCalls;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace Mirage.Weaver
{
    internal static class RegisterRpc
    {
        public static void RegisterAll(ILProcessor worker, List<RpcMethod> rpcs)
        {
            foreach (var rpc in rpcs)
            {
                if (rpc is ServerRpcMethod serverRpc)
                {
                    RegisterServerRpc(worker, serverRpc);
                }
                else if (rpc is ClientRpcMethod clientRpc)
                {
                    RegisterClientRpc(worker, clientRpc);
                }
            }
        }

        private static string HumanReadableName(MethodReference method)
        {
            var typeName = method.DeclaringType.FullName;
            var methodName = method.Name;

            return $"{typeName}.{methodName}";
        }

        private static void RegisterServerRpc(ILProcessor worker, ServerRpcMethod rpc)
        {
            var skeleton = rpc.skeleton;
            var requireAuthority = rpc.requireAuthority;

            var registerMethod = GetRegisterMethod(skeleton);
            var invokeType = GetServerInvokeType(rpc);
            CallRegister(worker, rpc, invokeType, registerMethod, requireAuthority);
        }

        private static RpcInvokeType? GetServerInvokeType(ServerRpcMethod rpcMethod)
        {
            var func = rpcMethod.skeleton;
            if (func.ReturnType.Is(typeof(void)))
                return RpcInvokeType.ServerRpc;
            else
                // Request RPC dont need type, so pass nullable so opcode is exlcuded from register
                return default(RpcInvokeType?);
        }

        /// <summary>
        /// Gets normal or Unitask register method
        /// </summary>
        private static MethodReference GetRegisterMethod(MethodDefinition func)
        {
            if (func.ReturnType.Is(typeof(void)))
                return func.Module.ImportReference((RemoteCallCollection c) => c.Register(default, default, default, default, default, default));
            else
                return CreateGenericRequestDelegate(func);
        }

        private static MethodReference CreateGenericRequestDelegate(MethodDefinition func)
        {
            var taskReturnType = func.ReturnType as GenericInstanceType;

            var returnType = taskReturnType.GenericArguments[0];

            var genericRegisterMethod = func.Module.ImportReference((RemoteCallCollection c) => c.RegisterRequest<object>(default, default, default, default, default)) as GenericInstanceMethod;

            var registerInstance = new GenericInstanceMethod(genericRegisterMethod.ElementMethod);
            registerInstance.GenericArguments.Add(returnType);
            return registerInstance;
        }

        /// <summary>
        /// This generates code like:
        /// NetworkBehaviour.RegisterServerRpcDelegate(base.GetType(), "CmdThrust", new NetworkBehaviour.CmdDelegate(ShipControl.InvokeCmdCmdThrust));
        /// </summary>
        /// <param name="worker"></param>
        /// <param name="rpc"></param>
        private static void RegisterClientRpc(ILProcessor worker, ClientRpcMethod rpc)
        {
            var registerMethod = GetRegisterMethod(rpc.skeleton);
            CallRegister(worker, rpc, RpcInvokeType.ClientRpc, registerMethod, false);
        }

        private static void CallRegister(ILProcessor worker, RpcMethod rpcMethod, RpcInvokeType? invokeType, MethodReference registerMethod, bool requireAuthority)
        {
            var skeleton = rpcMethod.skeleton;
            var name = HumanReadableName(rpcMethod.stub);
            var index = rpcMethod.Index;
            var module = rpcMethod.stub.Module;

            var collectionFieldInfo = typeof(NetworkBehaviour).GetField(nameof(NetworkBehaviour.RemoteCallCollection), BindingFlags.Public | BindingFlags.Instance);
            var collectionField = module.ImportReference(collectionFieldInfo);

            // arg0 is remote collection
            // this.remoteCallCollection
            worker.Append(worker.Create(OpCodes.Ldarg_0));
            worker.Append(worker.Create(OpCodes.Ldfld, collectionField));

            // arg1 is rpc index
            worker.Append(worker.Create(OpCodes.Ldc_I4, index));

            // typeof()
            var netBehaviourSubclass = skeleton.DeclaringType.ConvertToGenericIfNeeded();
            worker.Append(worker.Create(OpCodes.Ldtoken, netBehaviourSubclass));
            worker.Append(worker.Create(OpCodes.Call, () => Type.GetTypeFromHandle(default)));

            worker.Append(worker.Create(OpCodes.Ldstr, name));

            // RegisterRequest has no type, it is always serverRpc, so dont need to include arg
            if (invokeType.HasValue)
            {
                worker.Append(worker.Create(OpCodes.Ldc_I4, (int)invokeType.Value));
            }

            // new delegate
            worker.Append(worker.Create(OpCodes.Ldnull));
            worker.Append(worker.Create(OpCodes.Ldftn, skeleton.MakeHostInstanceSelfGeneric()));
            var @delegate = CreateRpcDelegate(skeleton);
            worker.Append(worker.Create(OpCodes.Newobj, @delegate));

            worker.Append(worker.Create(requireAuthority.OpCode_Ldc()));
            worker.Append(worker.Create(OpCodes.Call, registerMethod));
        }

        private static MethodReference CreateRpcDelegate(MethodDefinition func)
        {
            if (func.ReturnType.Is(typeof(void)))
            {
                var constructors = typeof(RpcDelegate).GetConstructors();
                return func.Module.ImportReference(constructors.First());
            }
            else if (func.ReturnType.Is(typeof(UniTask<int>).GetGenericTypeDefinition()))
            {
                var taskReturnType = func.ReturnType as GenericInstanceType;

                var returnType = taskReturnType.GenericArguments[0];
                var genericDelegate = func.Module.ImportReference(typeof(RequestDelegate<int>).GetGenericTypeDefinition());

                var delegateInstance = new GenericInstanceType(genericDelegate);
                delegateInstance.GenericArguments.Add(returnType);

                var constructor = typeof(RequestDelegate<int>).GetConstructors().First();

                var constructorRef = func.Module.ImportReference(constructor);

                return constructorRef.MakeHostInstanceGeneric(delegateInstance);
            }
            else
            {
                throw new InvalidOperationException("Should not have got this far, weaver should have validated return type earlier");
            }
        }
    }
}
