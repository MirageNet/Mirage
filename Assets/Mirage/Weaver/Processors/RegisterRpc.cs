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
    static class RegisterRpc
    {
        public static void RegisterAll(ILProcessor worker, List<RpcMethod> rpcs)
        {
            foreach (RpcMethod rpc in rpcs)
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

        static string HumanReadableName(MethodReference method)
        {
            string typeName = method.DeclaringType.FullName;
            string methodName = method.Name;

            return $"{typeName}.{methodName}";
        }

        static void RegisterServerRpc(ILProcessor worker, ServerRpcMethod rpcMethod)
        {
            MethodDefinition skeleton = rpcMethod.skeleton;
            string name = HumanReadableName(rpcMethod.stub);
            int hash = RpcProcessor.GetStableHash(rpcMethod.stub);
            bool requireAuthority = rpcMethod.requireAuthority;


            TypeDefinition netBehaviourSubclass = skeleton.DeclaringType;
            worker.Append(worker.Create(OpCodes.Ldtoken, netBehaviourSubclass.ConvertToGenericIfNeeded()));
            worker.Append(worker.Create(OpCodes.Call, () => Type.GetTypeFromHandle(default)));
            worker.Append(worker.Create(OpCodes.Ldstr, name));
            worker.Append(worker.Create(OpCodes.Ldc_I4, hash));
            // 0 for RemoteCalls.RpcInvokeType.ServerRpc
            worker.Append(worker.Create(OpCodes.Ldc_I4_0));
            worker.Append(worker.Create(OpCodes.Ldnull));
            worker.Append(worker.Create(OpCodes.Ldftn, skeleton));
            MethodReference @delegate = CreateRpcDelegate(skeleton);
            worker.Append(worker.Create(OpCodes.Newobj, @delegate));

            worker.Append(worker.Create(requireAuthority ? OpCodes.Ldc_I4_1 : OpCodes.Ldc_I4_0));

            MethodReference registerMethod = GetRegisterMethod(skeleton);
            worker.Append(worker.Create(OpCodes.Call, registerMethod));
        }

        /// <summary>
        /// Gets normal or Unitask register method
        /// </summary>
        static MethodReference GetRegisterMethod(MethodDefinition func)
        {
            if (func.ReturnType.Is(typeof(void)))
                return func.Module.ImportReference(() => RemoteCallHelper.Register(default, default, default, default, default, default));
            else
                return CreateGenericRequestDelegate(func);
        }

        static MethodReference CreateGenericRequestDelegate(MethodDefinition func)
        {
            var taskReturnType = func.ReturnType as GenericInstanceType;

            TypeReference returnType = taskReturnType.GenericArguments[0];

            var genericRegisterMethod = func.Module.ImportReference(() => RemoteCallHelper.RegisterRequest<object>(default, default, default, default, default)) as GenericInstanceMethod;

            var registerInstance = new GenericInstanceMethod(genericRegisterMethod.ElementMethod);
            registerInstance.GenericArguments.Add(returnType);
            return registerInstance;
        }

        /// <summary>
        /// This generates code like:
        /// NetworkBehaviour.RegisterServerRpcDelegate(base.GetType(), "CmdThrust", new NetworkBehaviour.CmdDelegate(ShipControl.InvokeCmdCmdThrust));
        /// </summary>
        /// <param name="worker"></param>
        /// <param name="rpcMethod"></param>
        static void RegisterClientRpc(ILProcessor worker, ClientRpcMethod rpcMethod)
        {
            MethodDefinition skeleton = rpcMethod.skeleton;
            string name = HumanReadableName(rpcMethod.stub);
            int hash = RpcProcessor.GetStableHash(rpcMethod.stub);

            TypeReference netBehaviourSubclass = skeleton.DeclaringType.ConvertToGenericIfNeeded();
            worker.Append(worker.Create(OpCodes.Ldtoken, netBehaviourSubclass));
            worker.Append(worker.Create(OpCodes.Call, () => Type.GetTypeFromHandle(default)));
            worker.Append(worker.Create(OpCodes.Ldstr, name));
            worker.Append(worker.Create(OpCodes.Ldc_I4, hash));
            // 1 for RemoteCalls.RpcInvokeType.ClientRpc
            worker.Append(worker.Create(OpCodes.Ldc_I4_1));
            worker.Append(worker.Create(OpCodes.Ldnull));
            worker.Append(worker.Create(OpCodes.Ldftn, skeleton));
            MethodReference @delegate = CreateRpcDelegate(skeleton);
            worker.Append(worker.Create(OpCodes.Newobj, @delegate));

            worker.Append(worker.Create(OpCodes.Call, () => RemoteCallHelper.Register(default, default, default, default, default, default)));
        }


        static MethodReference CreateRpcDelegate(MethodDefinition func)
        {
            if (func.ReturnType.Is(typeof(void)))
            {
                ConstructorInfo[] constructors = typeof(RpcDelegate).GetConstructors();
                return func.Module.ImportReference(constructors.First());
            }
            else if (func.ReturnType.Is(typeof(UniTask<int>).GetGenericTypeDefinition()))
            {
                var taskReturnType = func.ReturnType as GenericInstanceType;

                TypeReference returnType = taskReturnType.GenericArguments[0];
                TypeReference genericDelegate = func.Module.ImportReference(typeof(RequestDelegate<int>).GetGenericTypeDefinition());

                var delegateInstance = new GenericInstanceType(genericDelegate);
                delegateInstance.GenericArguments.Add(returnType);

                ConstructorInfo constructor = typeof(RequestDelegate<int>).GetConstructors().First();

                MethodReference constructorRef = func.Module.ImportReference(constructor);

                return constructorRef.MakeHostInstanceGeneric(delegateInstance);
            }
            else
            {
                throw new InvalidOperationException("Should not have got this far, weaver should have validated return type earlier");
            }
        }
    }
}
