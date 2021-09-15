// Injects server/client active checks for [Server/Client] attributes
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace Mirage.Weaver
{
    class AttributeProcessor
    {
        private readonly IWeaverLogger logger;

        readonly MethodReference IsServer;
        readonly MethodReference IsClient;
        readonly MethodReference HasAuthority;
        readonly MethodReference IsLocalPlayer;

        public AttributeProcessor(ModuleDefinition module, IWeaverLogger logger)
        {
            this.logger = logger;

            // Cache these so that we dont import them for each site we process
            IsServer = module.ImportReference((NetworkBehaviour nb) => nb.IsServer);
            IsClient = module.ImportReference((NetworkBehaviour nb) => nb.IsClient);
            HasAuthority = module.ImportReference((NetworkBehaviour nb) => nb.HasAuthority);
            IsLocalPlayer = module.ImportReference((NetworkBehaviour nb) => nb.IsLocalPlayer);
        }

        public bool ProcessType(FoundType foundType)
        {
            bool modified = false;
            foreach (MethodDefinition md in foundType.TypeDefinition.Methods)
            {
                modified |= ProcessMethod(md, foundType);
            }

            if (!foundType.IsNetworkBehaviour)
            {
                foreach (FieldDefinition fd in foundType.TypeDefinition.Fields)
                {
                    ProcessFields(fd, foundType);
                }
            }

            return modified;
        }

        /// <summary>
        /// Check if Syncvar or SyncObject are used outside of NetworkBehaviour
        /// </summary>
        /// <param name="fd"></param>
        /// <param name="foundType"></param>
        void ProcessFields(FieldDefinition fd, FoundType foundType)
        {
            if (fd.HasCustomAttribute<SyncVarAttribute>())
                logger.Error($"SyncVar {fd.Name} must be inside a NetworkBehaviour.  {foundType.TypeDefinition.Name} is not a NetworkBehaviour", fd);

            // only check SyncObjects inside Monobehaviours
            if (foundType.IsMonoBehaviour && SyncObjectProcessor.ImplementsSyncObject(fd.FieldType))
            {
                logger.Error($"{fd.Name} is a SyncObject and must be inside a NetworkBehaviour.  {foundType.TypeDefinition.Name} is not a NetworkBehaviour", fd);
            }
        }

        bool ProcessMethod(MethodDefinition md, FoundType foundType)
        {
            if (IgnoreMethod(md))
                return false;

            return ProcessMethodAttributes(md, foundType);
        }

        /// <summary>
        /// Ignore if it is static constructor, or a Weaver Generated function
        /// </summary>
        /// <param name="md"></param>
        /// <returns></returns>
        private static bool IgnoreMethod(MethodDefinition md)
        {
            return md.Name == ".cctor" ||
                md.Name == NetworkBehaviourProcessor.ProcessedFunctionName ||
                md.Name.StartsWith(RpcProcessor.InvokeRpcPrefix);
        }

        bool ProcessMethodAttributes(MethodDefinition md, FoundType foundType)
        {
            bool modified;
            modified = InjectGuard<ServerAttribute>(md, foundType, IsServer, "[Server] function '" + md.FullName + "' called on client");
            modified |= InjectGuard<ClientAttribute>(md, foundType, IsClient, "[Client] function '" + md.FullName + "' called on server");
            modified |= InjectGuard<HasAuthorityAttribute>(md, foundType, HasAuthority, "[Has Authority] function '" + md.FullName + "' called on player without authority");
            modified |= InjectGuard<LocalPlayerAttribute>(md, foundType, IsLocalPlayer, "[Local Player] function '" + md.FullName + "' called on nonlocal player");
            CheckAttribute<ServerRpcAttribute>(md, foundType);
            CheckAttribute<ClientRpcAttribute>(md, foundType);


            return modified;
        }

        private void CheckAttribute<TAttribute>(MethodDefinition md, FoundType foundType)
        {
            CustomAttribute attribute = md.GetCustomAttribute<TAttribute>();
            if (attribute == null)
                return;

            if (!foundType.IsNetworkBehaviour)
            {
                logger.Error($"{attribute.AttributeType.Name} method {md.Name} must be declared in a NetworkBehaviour", md);
            }
        }

        bool InjectGuard<TAttribute>(MethodDefinition md, FoundType foundType, MethodReference predicate, string message)
        {
            CustomAttribute attribute = md.GetCustomAttribute<TAttribute>();
            if (attribute == null)
                return false;

            if (md.IsAbstract)
            {
                logger.Error($" {typeof(TAttribute)} can't be applied to abstract method. Apply to override methods instead.", md);
                return false;
            }

            bool throwError = attribute.GetField("error", true);

            if (!foundType.IsNetworkBehaviour)
            {
                logger.Error($"{attribute.AttributeType.Name} method {md.Name} must be declared in a NetworkBehaviour", md);
                return true;
            }
            ILProcessor worker = md.Body.GetILProcessor();
            Instruction top = md.Body.Instructions[0];

            worker.InsertBefore(top, worker.Create(OpCodes.Ldarg_0));
            worker.InsertBefore(top, worker.Create(OpCodes.Call, predicate));
            worker.InsertBefore(top, worker.Create(OpCodes.Brtrue, top));
            if (throwError)
            {
                worker.InsertBefore(top, worker.Create(OpCodes.Ldstr, message));
                worker.InsertBefore(top, worker.Create(OpCodes.Newobj, () => new MethodInvocationException("")));
                worker.InsertBefore(top, worker.Create(OpCodes.Throw));
            }
            InjectGuardParameters(md, worker, top);
            InjectGuardReturnValue(md, worker, top);
            worker.InsertBefore(top, worker.Create(OpCodes.Ret));
            return true;
        }

        // this is required to early-out from a function with "ref" or "out" parameters
        static void InjectGuardParameters(MethodDefinition md, ILProcessor worker, Instruction top)
        {
            int offset = md.Resolve().IsStatic ? 0 : 1;
            for (int index = 0; index < md.Parameters.Count; index++)
            {
                ParameterDefinition param = md.Parameters[index];
                if (param.IsOut)
                {
                    TypeReference elementType = param.ParameterType.GetElementType();

                    VariableDefinition elementLocal = md.AddLocal(elementType);

                    worker.InsertBefore(top, worker.Create(OpCodes.Ldarg, index + offset));
                    worker.InsertBefore(top, worker.Create(OpCodes.Ldloca_S, elementLocal));
                    worker.InsertBefore(top, worker.Create(OpCodes.Initobj, elementType));
                    worker.InsertBefore(top, worker.Create(OpCodes.Ldloc, elementLocal));
                    worker.InsertBefore(top, worker.Create(OpCodes.Stobj, elementType));
                }
            }
        }

        // this is required to early-out from a function with a return value.
        static void InjectGuardReturnValue(MethodDefinition md, ILProcessor worker, Instruction top)
        {
            if (!md.ReturnType.Is(typeof(void)))
            {
                VariableDefinition returnLocal = md.AddLocal(md.ReturnType);
                worker.InsertBefore(top, worker.Create(OpCodes.Ldloca_S, returnLocal));
                worker.InsertBefore(top, worker.Create(OpCodes.Initobj, md.ReturnType));
                worker.InsertBefore(top, worker.Create(OpCodes.Ldloc, returnLocal));
            }
        }
    }
}
