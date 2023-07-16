using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Mirage.CodeGen;
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
                    CallRegister(worker, rpc, RpcInvokeType.ServerRpc, serverRpc.requireAuthority);
                }
                else if (rpc is ClientRpcMethod clientRpc)
                {
                    CallRegister(worker, rpc, RpcInvokeType.ClientRpc, false);
                }
            }
        }

        private static string HumanReadableName(MethodReference method)
        {
            var typeName = method.DeclaringType.FullName;
            var methodName = method.Name;

            return $"{typeName}.{methodName}";
        }

        /// <summary>
        /// Gets normal or Unitask register method
        /// </summary>
        private static MethodReference GetRegisterMethod(RpcMethod rpcMethod)
        {
            var skeleton = rpcMethod.skeleton;
            if (rpcMethod.ReturnType == ReturnType.Void)
            {
                return skeleton.Module.ImportReference((RemoteCallCollection c) => c.Register(default, default, default, default, default, default));
            }
            else
            {
                return CreateGenericRequestDelegate(skeleton);
            }
        }

        private static MethodReference CreateGenericRequestDelegate(MethodDefinition func)
        {
            var taskReturnType = func.ReturnType as GenericInstanceType;

            var returnType = taskReturnType.GenericArguments[0];

            var genericRegisterMethod = func.Module.ImportReference((RemoteCallCollection c) => c.RegisterRequest<object>(default, default, default, default, default, default)) as GenericInstanceMethod;

            var registerInstance = new GenericInstanceMethod(genericRegisterMethod.ElementMethod);
            registerInstance.GenericArguments.Add(returnType);
            return registerInstance;
        }


        private static void CallRegister(ILProcessor worker, RpcMethod rpcMethod, RpcInvokeType invokeType, bool requireAuthority)
        {
            var registerMethod = GetRegisterMethod(rpcMethod);

            var skeleton = rpcMethod.skeleton;
            var name = HumanReadableName(rpcMethod.stub);
            var index = rpcMethod.Index;

            // write collection.Register(index, name, invokerType, cmdRequireAuthority,...
            // arg1 is rpc collection
            worker.Append(worker.Create(OpCodes.Ldarg_1));
            worker.Append(worker.Create(OpCodes.Ldc_I4, index));
            worker.Append(worker.Create(OpCodes.Ldstr, name));
            worker.Append(worker.Create(requireAuthority.OpCode_Ldc()));
            worker.Append(worker.Create(OpCodes.Ldc_I4, (int)invokeType));

            // write behaviour
            worker.Append(worker.Create(OpCodes.Ldarg_0));

            // create delegate as last arg
            worker.Append(worker.Create(OpCodes.Ldnull));
            worker.Append(worker.Create(OpCodes.Ldftn, skeleton.MakeHostInstanceSelfGeneric()));
            var @delegate = CreateRpcDelegate(skeleton);
            worker.Append(worker.Create(OpCodes.Newobj, @delegate));

            // invoke register
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
