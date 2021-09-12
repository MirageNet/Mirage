using System;
using System.Linq;
using System.Reflection;
using Cysharp.Threading.Tasks;
using Mirage.RemoteCalls;
using Mirage.Weaver.Serialization;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace Mirage.Weaver
{
    public abstract class RpcProcessor
    {
        public const string SkeletonPrefix = "Skeleton_";
        public const string UserCodePrefix = "UserCode_";

        protected readonly ModuleDefinition module;
        protected readonly Readers readers;
        protected readonly Writers writers;
        protected readonly IWeaverLogger logger;

        public static string InvokeRpcPrefix => "InvokeUserCode_";

        protected RpcProcessor(ModuleDefinition module, Readers readers, Writers writers, IWeaverLogger logger)
        {
            this.module = module;
            this.readers = readers;
            this.writers = writers;
            this.logger = logger;
        }

        // helper functions to check if the method has a NetworkConnection parameter
        public bool HasNetworkConnectionParameter(MethodDefinition md)
        {
            return md.Parameters.Count > 0 &&
                   md.Parameters[0].ParameterType.Is<INetworkPlayer>();
        }

        public static bool IsNetworkPlayer(TypeReference type)
        {
            return type.Resolve().ImplementsInterface<INetworkPlayer>();
        }

        public void WriteArguments(ILProcessor worker, MethodDefinition method, VariableDefinition writer, RemoteCallType callType)
        {
            // write each argument
            // example result
            /*
            writer.WritePackedInt32(someNumber);
            writer.WriteNetworkIdentity(someTarget);
             */

            // NetworkConnection is not sent via the NetworkWriter so skip it here
            // skip first for NetworkConnection in TargetRpc
            bool skipFirst = callType == RemoteCallType.ClientRpc
                && HasNetworkConnectionParameter(method);

            int startingArg = skipFirst ? 1 : 0;
            for (int argIndex = startingArg; argIndex < method.Parameters.Count; argIndex++)
            {
                // try/catch for each arg so that it will give error for each
                try
                {
                    WriteArgument(worker, method, writer, argIndex);
                }
                catch (SerializeFunctionException e)
                {
                    logger.Error(e, method.DebugInformation.SequencePoints.FirstOrDefault());
                }
                catch (ValueSerializerException e)
                {
                    logger.Error(e.Message, method);
                }
            }
        }

        private void WriteArgument(ILProcessor worker, MethodDefinition method, VariableDefinition writer, int argIndex)
        {
            ParameterDefinition param = method.Parameters[argIndex];

            // dont write anything for INetworkPlayer, it is either target or sender
            if (IsNetworkPlayer(param.ParameterType))
                return;

            ValueSerializer valueSerializer = ValueSerializerFinder.GetSerializer(method, param, writers, null);
            // todo remove this function when other Serializer works
            if (valueSerializer is FunctionSerializer functionSerializer)
            {
                // arg+1 because arg0 is "this"
                functionSerializer.AppendWriteRpc(worker, writer, argIndex + 1);
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        public void ReadArguments(MethodDefinition method, ILProcessor worker, ParameterDefinition readerParameter, ParameterDefinition senderParameter, bool skipFirst)
        {
            // read each argument
            // example result
            /*
            CallCmdDoSomething(reader.ReadPackedInt32(), reader.ReadNetworkIdentity());
             */

            int startingArg = skipFirst ? 1 : 0;
            for (int argIndex = startingArg; argIndex < method.Parameters.Count; argIndex++)
            {
                // try/catch for each arg so that it will give error for each
                try
                {
                    ReadArgument(method, worker, readerParameter, senderParameter, argIndex);
                }
                catch (SerializeFunctionException e)
                {
                    logger.Error(e, method.DebugInformation.SequencePoints.FirstOrDefault());
                }
                catch (ValueSerializerException e)
                {
                    logger.Error(e.Message, method);
                }
            }
        }

        private void ReadArgument(MethodDefinition method, ILProcessor worker, ParameterDefinition readerParameter, ParameterDefinition senderParameter, int argIndex)
        {
            ParameterDefinition param = method.Parameters[argIndex];

            if (IsNetworkPlayer(param.ParameterType))
            {
                if (senderParameter != null)
                {
                    worker.Append(worker.Create(OpCodes.Ldarg, senderParameter));
                }
                else
                {
                    worker.Append(worker.Create(OpCodes.Ldnull));
                }

                return;
            }

            ValueSerializer valueSerializer = ValueSerializerFinder.GetSerializer(method, param, null, readers);

            // todo make sure this works for all ValueSerializer
            valueSerializer.AppendRead(null, worker, readerParameter, null);
        }

        // check if a Command/TargetRpc/Rpc function & parameters are valid for weaving
        public bool ValidateRemoteCallAndParameters(MethodDefinition method, RemoteCallType callType)
        {
            if (method.IsAbstract)
            {
                logger.Error("Abstract Rpcs are currently not supported, use virtual method instead", method);
                return false;
            }

            if (method.IsStatic)
            {
                logger.Error($"{method.Name} must not be static", method);
                return false;
            }

            if (method.ReturnType.Is<System.Collections.IEnumerator>())
            {
                logger.Error($"{method.Name} cannot be a coroutine", method);
                return false;
            }

            if (method.HasGenericParameters)
            {
                logger.Error($"{method.Name} cannot have generic parameters", method);
                return false;
            }

            return ValidateParameters(method, callType);
        }

        // check if all Command/TargetRpc/Rpc function's parameters are valid for weaving
        bool ValidateParameters(MethodReference method, RemoteCallType callType)
        {
            for (int i = 0; i < method.Parameters.Count; ++i)
            {
                ParameterDefinition param = method.Parameters[i];
                if (!ValidateParameter(method, param, callType, i == 0))
                {
                    return false;
                }
            }
            return true;
        }

        // validate parameters for a remote function call like Rpc/Cmd
        bool ValidateParameter(MethodReference method, ParameterDefinition param, RemoteCallType callType, bool firstParam)
        {
            if (param.IsOut)
            {
                logger.Error($"{method.Name} cannot have out parameters", method);
                return false;
            }

            if (param.ParameterType.IsGenericParameter)
            {
                logger.Error($"{method.Name} cannot have generic parameters", method);
                return false;
            }

            if (IsNetworkPlayer(param.ParameterType))
            {
                if (callType == RemoteCallType.ClientRpc && firstParam)
                {
                    // perfectly fine,  target rpc can receive a network connection as first parameter
                    return true;
                }

                if (callType == RemoteCallType.ServerRpc)
                {
                    return true;
                }

                logger.Error($"{method.Name} has invalid parameter {param}, Cannot pass NetworkConnections", method);
                return false;
            }

            if (param.IsOptional)
            {
                logger.Error($"{method.Name} cannot have optional parameters", method);
                return false;
            }

            return true;
        }

        public void CreateRpcDelegate(ILProcessor worker, MethodDefinition func)
        {
            MethodReference CmdDelegateConstructor;

            if (func.ReturnType.Is(typeof(void)))
            {
                ConstructorInfo[] constructors = typeof(CmdDelegate).GetConstructors();
                CmdDelegateConstructor = func.Module.ImportReference(constructors.First());
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

                CmdDelegateConstructor = constructorRef.MakeHostInstanceGeneric(delegateInstance);
            }
            else
            {
                logger.Error("Use UniTask<x> to return a value from ServerRpc in" + func);
                return;
            }

            worker.Append(worker.Create(OpCodes.Ldftn, func));
            worker.Append(worker.Create(OpCodes.Newobj, CmdDelegateConstructor));
        }

        // creates a method substitute
        // For example, if we have this:
        //  public void CmdThrust(float thrusting, int spin)
        //  {
        //      xxxxx   
        //  }
        //
        //  it will substitute the method and move the code to a new method with a provided name
        //  for example:
        //
        //  public void CmdTrust(float thrusting, int spin)
        //  {
        //  }
        //
        //  public void <newName>(float thrusting, int spin)
        //  {
        //      xxxxx
        //  }
        //
        //  Note that all the calls to the method remain untouched
        //
        //  the original method definition loses all code
        //  this returns the newly created method with all the user provided code
        public MethodDefinition SubstituteMethod(MethodDefinition md)
        {
            string newName = UserCodePrefix + md.Name;
            MethodDefinition cmd = md.DeclaringType.AddMethod(newName, md.Attributes, md.ReturnType);

            // add parameters
            foreach (ParameterDefinition pd in md.Parameters)
            {
                _ = cmd.AddParam(pd.ParameterType, pd.Name);
            }

            // swap bodies
            (cmd.Body, md.Body) = (md.Body, cmd.Body);

            // Move over all the debugging information
            foreach (SequencePoint sequencePoint in md.DebugInformation.SequencePoints)
                cmd.DebugInformation.SequencePoints.Add(sequencePoint);
            md.DebugInformation.SequencePoints.Clear();

            foreach (CustomDebugInformation customInfo in md.CustomDebugInformations)
                cmd.CustomDebugInformations.Add(customInfo);
            md.CustomDebugInformations.Clear();

            (md.DebugInformation.Scope, cmd.DebugInformation.Scope) = (cmd.DebugInformation.Scope, md.DebugInformation.Scope);

            FixRemoteCallToBaseMethod(md.DeclaringType, cmd);
            return cmd;
        }


        /// <summary>
        /// Finds and fixes call to base methods within remote calls
        /// <para>For example, changes `base.CmdDoSomething` to `base.UserCode_CmdDoSomething` within `this.UserCode_CmdDoSomething`</para>
        /// </summary>
        /// <param name="type"></param>
        /// <param name="method"></param>
        public void FixRemoteCallToBaseMethod(TypeDefinition type, MethodDefinition method)
        {
            string callName = method.Name;

            // all ServerRpcs/Rpc start with "UserCode_"
            // eg CallCmdDoSomething
            if (!callName.StartsWith(UserCodePrefix))
                return;

            // eg CmdDoSomething
            string baseRemoteCallName = method.Name.Substring(UserCodePrefix.Length);

            foreach (Instruction instruction in method.Body.Instructions)
            {
                // if call to base.CmdDoSomething within this.CallCmdDoSomething
                if (IsCallToMethod(instruction, out MethodDefinition calledMethod) &&
                    calledMethod.Name == baseRemoteCallName)
                {
                    TypeDefinition baseType = type.BaseType.Resolve();
                    MethodReference baseMethod = baseType.GetMethodInBaseType(callName);

                    if (baseMethod == null)
                    {
                        logger.Error($"Could not find base method for {callName}", method);
                        return;
                    }

                    if (!baseMethod.Resolve().IsVirtual)
                    {
                        logger.Error($"Could not find base method that was virtual {callName}", method);
                        return;
                    }

                    instruction.Operand = method.Module.ImportReference(baseMethod);

                    Weaver.DebugLog(type, $"Replacing call to '{calledMethod.FullName}' with '{baseMethod.FullName}' inside '{ method.FullName}'");
                }
            }
        }

        static bool IsCallToMethod(Instruction instruction, out MethodDefinition calledMethod)
        {
            if (instruction.OpCode == OpCodes.Call &&
                instruction.Operand is MethodDefinition method)
            {
                calledMethod = method;
                return true;
            }
            else
            {
                calledMethod = null;
                return false;
            }
        }

    }
}
