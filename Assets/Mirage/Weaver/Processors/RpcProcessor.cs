using System;
using System.Linq;
using Cysharp.Threading.Tasks;
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

        // helper functions to check if the method has a NetworkPlayer parameter
        protected static bool HasNetworkPlayerParameter(MethodDefinition md)
        {
            return md.Parameters.Count > 0 &&
                   IsNetworkPlayer(md.Parameters[0].ParameterType);
        }

        protected static bool IsNetworkPlayer(TypeReference type)
        {
            return type.Resolve().ImplementsInterface<INetworkPlayer>();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="method"></param>
        /// <returns></returns>
        /// <exception cref="RpcException">Throws when could not get Serializer for any parameter</exception>
        protected ValueSerializer[] GetValueSerializers(MethodDefinition method)
        {
            var serializers = new ValueSerializer[method.Parameters.Count];
            bool error = false;
            for (int i = 0; i < method.Parameters.Count; i++)
            {
                if (IsNetworkPlayer(method.Parameters[i].ParameterType))
                    continue;

                try
                {
                    serializers[i] = ValueSerializerFinder.GetSerializer(method, method.Parameters[i], writers, readers);
                }
                catch (SerializeFunctionException e)
                {
                    logger.Error(e, method.DebugInformation.SequencePoints.FirstOrDefault());
                    error = true;
                }
                catch (ValueSerializerException e)
                {
                    logger.Error(e.Message, method);
                    error = true;
                }
            }

            // check and log all bad params before throwing RPC
            if (error)
            {
                throw new RpcException($"Could not process Rpc because one or more of its parameter were invalid", method);
            }
            return serializers;
        }

        public void WriteArguments(ILProcessor worker, MethodDefinition method, VariableDefinition writer, ValueSerializer[] paramSerializers, RemoteCallType callType)
        {
            // write each argument
            // example result
            /*
            writer.WritePackedInt32(someNumber)
            writer.WriteNetworkIdentity(someTarget)
             */

            // NetworkConnection is not sent via the NetworkWriter so skip it here
            // skip first for NetworkConnection in TargetRpc
            bool skipFirst = ClientRpcWithTarget(method, callType);

            int startingArg = skipFirst ? 1 : 0;
            for (int i = startingArg; i < method.Parameters.Count; i++)
            {
                // try/catch for each arg so that it will give error for each
                ParameterDefinition param = method.Parameters[i];
                ValueSerializer serializer = paramSerializers[i];
                WriteArgument(worker, writer, param, serializer);
            }
        }

        static bool ClientRpcWithTarget(MethodDefinition method, RemoteCallType callType)
        {
            return (callType == RemoteCallType.ClientRpc)
                && HasNetworkPlayerParameter(method);
        }

        private void WriteArgument(ILProcessor worker, VariableDefinition writer, ParameterDefinition param, ValueSerializer serializer)
        {
            // dont write anything for INetworkPlayer, it is either target or sender
            if (IsNetworkPlayer(param.ParameterType))
                return;

            serializer.AppendWriteParameter(module, worker, writer, param);
        }

        public void ReadArguments(MethodDefinition method, ILProcessor worker, ParameterDefinition readerParameter, ParameterDefinition senderParameter, bool skipFirst, ValueSerializer[] paramSerializers)
        {
            // read each argument
            // example result
            /*
            CallCmdDoSomething(reader.ReadPackedInt32(), reader.ReadNetworkIdentity())
             */

            int startingArg = skipFirst ? 1 : 0;
            for (int i = startingArg; i < method.Parameters.Count; i++)
            {
                ParameterDefinition param = method.Parameters[i];
                ValueSerializer serializer = paramSerializers[i];
                ReadArgument(worker, readerParameter, senderParameter, param, serializer);
            }
        }

        private void ReadArgument(ILProcessor worker, ParameterDefinition readerParameter, ParameterDefinition senderParameter, ParameterDefinition param, ValueSerializer serializer)
        {
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

            serializer.AppendRead(module, worker, readerParameter, param.ParameterType);
        }

        /// <summary>
        /// check if a method is valid for rpc
        /// </summary>
        /// <exception cref="RpcException">Throws when method is invalid</exception>
        protected void ValidateMethod(MethodDefinition method, RemoteCallType callType)
        {
            if (method.IsAbstract)
            {
                throw new RpcException("Abstract Rpcs are currently not supported, use virtual method instead", method);
            }

            if (method.IsStatic)
            {
                throw new RpcException($"{method.Name} must not be static", method);
            }

            if (method.ReturnType.Is<System.Collections.IEnumerator>())
            {
                throw new RpcException($"{method.Name} cannot be a coroutine", method);
            }

            if (method.HasGenericParameters)
            {
                throw new RpcException($"{method.Name} cannot have generic parameters", method);
            }

        }

        /// <summary>
        /// checks if method parameters are valid for rpc
        /// </summary>
        /// <exception cref="RpcException">Throws when parameter are invalid</exception>
        protected void ValidateParameters(MethodReference method, RemoteCallType callType)
        {
            for (int i = 0; i < method.Parameters.Count; i++)
            {
                ParameterDefinition param = method.Parameters[i];
                ValidateParameter(method, param, callType, i == 0);
            }
        }

        /// <summary>
        /// checks if return type if valid for rpc
        /// </summary>
        /// <exception cref="RpcException">Throws when parameter are invalid</exception>
        protected void ValidateReturnType(MethodDefinition md, RemoteCallType callType)
        {
            TypeReference returnType = md.ReturnType;
            if (returnType.Is(typeof(void)))
                return;

            // only ServerRpc allow UniTask
            if (callType == RemoteCallType.ServerRpc)
            {
                Type unitaskType = typeof(UniTask<int>).GetGenericTypeDefinition();
                if (returnType.Is(unitaskType))
                    return;
            }


            if (callType == RemoteCallType.ServerRpc)
                throw new RpcException($"Use UniTask<{md.ReturnType}> to return values from [ServerRpc]", md);
            else
                throw new RpcException($"[ClientRpc] must return void", md);
        }

        /// <summary>
        /// checks if a parameter is valid for rpc
        /// </summary>
        /// <exception cref="RpcException">Throws when parameter are invalid</exception>
        void ValidateParameter(MethodReference method, ParameterDefinition param, RemoteCallType callType, bool firstParam)
        {
            if (param.IsOut)
            {
                throw new RpcException($"{method.Name} cannot have out parameters", method);
            }

            if (param.ParameterType.IsGenericParameter)
            {
                throw new RpcException($"{method.Name} cannot have generic parameters", method);
            }

            if (IsNetworkPlayer(param.ParameterType))
            {
                if (callType == RemoteCallType.ClientRpc && firstParam)
                {
                    return;
                }

                if (callType == RemoteCallType.ServerRpc)
                {
                    return;
                }

                throw new RpcException($"{method.Name} has invalid parameter {param}, Cannot pass NetworkConnections", method);
            }

            // check networkplayer before optional, because networkplayer can be optional
            if (param.IsOptional)
            {
                throw new RpcException($"{method.Name} cannot have optional parameters", method);
            }
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
