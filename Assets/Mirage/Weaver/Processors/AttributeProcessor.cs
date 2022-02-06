using System.Collections.Generic;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace Mirage.Weaver
{
    /// <summary>
    /// Processes methods and fields to check their attrbiutes to make sure they are allowed on the type
    /// <para>
    /// Injects server/client active checks for [Server/Client] attributes
    /// </para>
    /// </summary>
    class AttributeProcessor
    {
        private readonly IWeaverLogger logger;

        readonly MethodReference IsServer;
        readonly MethodReference IsClient;
        readonly MethodReference HasAuthority;
        readonly MethodReference IsLocalPlayer;

        bool modified = false;

        public AttributeProcessor(ModuleDefinition module, IWeaverLogger logger)
        {
            this.logger = logger;

            // Cache these so that we dont import them for each site we process
            IsServer = module.ImportReference((NetworkBehaviour nb) => nb.IsServer);
            IsClient = module.ImportReference((NetworkBehaviour nb) => nb.IsClient);
            HasAuthority = module.ImportReference((NetworkBehaviour nb) => nb.HasAuthority);
            IsLocalPlayer = module.ImportReference((NetworkBehaviour nb) => nb.IsLocalPlayer);
        }

        public bool ProcessTypes(IReadOnlyList<FoundType> foundTypes)
        {
            foreach (FoundType foundType in foundTypes)
            {
                ProcessType(foundType);
            }

            return modified;
        }

        void ProcessType(FoundType foundType)
        {
            foreach (MethodDefinition md in foundType.TypeDefinition.Methods)
            {
                ProcessMethod(md, foundType);
            }

            if (!foundType.IsNetworkBehaviour)
            {
                foreach (FieldDefinition fd in foundType.TypeDefinition.Fields)
                {
                    ProcessFields(fd, foundType);
                }
            }
        }

        /// <summary>
        /// Check if Syncvar or SyncObject are used outside of NetworkBehaviour
        /// </summary>
        /// <param name="fd"></param>
        /// <param name="foundType"></param>
        void ProcessFields(FieldDefinition fd, FoundType foundType)
        {
            if (fd.HasCustomAttribute<SyncVarAttribute>())
                logger.Error($"SyncVar {fd.Name} must be inside a NetworkBehaviour. {foundType.TypeDefinition.Name} is not a NetworkBehaviour", fd);

            // only check SyncObjects inside Monobehaviours
            if (foundType.IsMonoBehaviour && SyncObjectProcessor.ImplementsSyncObject(fd.FieldType))
            {
                logger.Error($"{fd.Name} is a SyncObject and can not be used inside Monobehaviour. {foundType.TypeDefinition.Name} is not a NetworkBehaviour", fd);
            }
        }

        void ProcessMethod(MethodDefinition md, FoundType foundType)
        {
            if (IgnoreMethod(md))
                return;

            ProcessMethodAttributes(md, foundType);
        }

        /// <summary>
        /// Ignore if it is static constructor, or a Weaver Generated function
        /// </summary>
        /// <param name="md"></param>
        /// <returns></returns>
        static bool IgnoreMethod(MethodDefinition md)
        {
            return md.Name == ".cctor" ||
                md.Name == NetworkBehaviourProcessor.ProcessedFunctionName;
        }

        void ProcessMethodAttributes(MethodDefinition md, FoundType foundType)
        {
            InjectGuard<ServerAttribute>(md, foundType, IsServer, "[Server] function '{0}' called when server not active");
            InjectGuard<ClientAttribute>(md, foundType, IsClient, "[Client] function '{0}' called when client not active");
            InjectGuard<HasAuthorityAttribute>(md, foundType, HasAuthority, "[Has Authority] function '{0}' called on player without authority");
            InjectGuard<LocalPlayerAttribute>(md, foundType, IsLocalPlayer, "[Local Player] function '{0}' called on nonlocal player");
            CheckAttribute<ServerRpcAttribute>(md, foundType);
            CheckAttribute<ClientRpcAttribute>(md, foundType);
        }

        void CheckAttribute<TAttribute>(MethodDefinition md, FoundType foundType)
        {
            CustomAttribute attribute = md.GetCustomAttribute<TAttribute>();
            if (attribute == null)
                return;

            if (!foundType.IsNetworkBehaviour)
            {
                logger.Error($"{attribute.AttributeType.Name} method {md.Name} must be declared in a NetworkBehaviour", md);
            }
        }

        void InjectGuard<TAttribute>(MethodDefinition md, FoundType foundType, MethodReference predicate, string format)
        {
            CustomAttribute attribute = md.GetCustomAttribute<TAttribute>();
            if (attribute == null)
                return;

            if (md.IsAbstract)
            {
                logger.Error($"{typeof(TAttribute)} can't be applied to abstract method. Apply to override methods instead.", md);
                return;
            }

            if (!foundType.IsNetworkBehaviour)
            {
                logger.Error($"{attribute.AttributeType.Name} method {md.Name} must be declared in a NetworkBehaviour", md);
                return;
            }

            if (md.Name == "Awake" && !md.HasParameters)
            {
                logger.Error($"{attribute.AttributeType.Name} will not work on the Awake method.", md);
                return;
            }

            // dont need to set modified for errors, so we set it here when we start doing ILProcessing
            modified = true;

            bool throwError = attribute.GetField("error", true);
            ILProcessor worker = md.Body.GetILProcessor();
            Instruction top = md.Body.Instructions[0];

            worker.InsertBefore(top, worker.Create(OpCodes.Ldarg_0));
            worker.InsertBefore(top, worker.Create(OpCodes.Call, predicate));
            worker.InsertBefore(top, worker.Create(OpCodes.Brtrue, top));
            if (throwError)
            {
                string message = string.Format(format, md.Name);
                worker.InsertBefore(top, worker.Create(OpCodes.Ldstr, message));
                worker.InsertBefore(top, worker.Create(OpCodes.Newobj, () => new MethodInvocationException("")));
                worker.InsertBefore(top, worker.Create(OpCodes.Throw));
            }
            InjectGuardParameters(md, worker, top);
            InjectGuardReturnValue(md, worker, top);
            worker.InsertBefore(top, worker.Create(OpCodes.Ret));
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
                    worker.InsertBefore(top, worker.Create(OpCodes.Ldloca, elementLocal));
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
                worker.InsertBefore(top, worker.Create(OpCodes.Ldloca, returnLocal));
                worker.InsertBefore(top, worker.Create(OpCodes.Initobj, md.ReturnType));
                worker.InsertBefore(top, worker.Create(OpCodes.Ldloc, returnLocal));
            }
        }
    }
}
