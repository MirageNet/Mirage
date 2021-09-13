using Mono.Cecil;
using Mono.Cecil.Cil;

namespace Mirage.Weaver
{
    // Injects server/client active checks for [Server/Client] attributes
    class ServerClientAttributeProcessor
    {
        private readonly IWeaverLogger logger;
        private readonly ModuleImportCache moduleCache;

        readonly MethodReference IsServer;
        readonly MethodReference IsClient;
        readonly MethodReference HasAuthority;
        readonly MethodReference IsLocalPlayer;

        bool modified;

        public ServerClientAttributeProcessor(ModuleImportCache moduleCache, IWeaverLogger logger)
        {
            this.logger = logger;
            this.moduleCache = moduleCache;

            // Cache these so that we dont import them for each site we process
            IsServer = moduleCache.ImportReference((NetworkBehaviour nb) => nb.IsServer);
            IsClient = moduleCache.ImportReference((NetworkBehaviour nb) => nb.IsClient);
            HasAuthority = moduleCache.ImportReference((NetworkBehaviour nb) => nb.HasAuthority);
            IsLocalPlayer = moduleCache.ImportReference((NetworkBehaviour nb) => nb.IsLocalPlayer);
        }

        /// <summary>
        /// Loops through all methods in module and checks them for Mirage Attributes
        /// <para>Checks for:<br/>
        /// - <see cref="ServerAttribute"/><br/>
        /// - <see cref="ClientAttribute"/><br/>
        /// - <see cref="HasAuthorityAttribute"/><br/>
        /// - <see cref="LocalPlayerAttribute"/>
        /// </para>
        /// </summary>
        /// <returns></returns>
        public bool ProcessModule()
        {
            Mono.Collections.Generic.Collection<TypeDefinition> types = moduleCache.Module.Types;
            foreach (TypeDefinition type in types)
            {
                ProcessType(type);
            }
            return modified;
        }
        private void ProcessType(TypeDefinition typeDefinition)
        {
            foreach (MethodDefinition md in typeDefinition.Methods)
            {
                ProcessMethod(md);
            }

            foreach (TypeDefinition nested in typeDefinition.NestedTypes)
            {
                ProcessType(nested);
            }
        }

        void ProcessMethod(MethodDefinition md)
        {
            // skip these
            if (md.Name == ".cctor" ||
                md.Name == NetworkBehaviourProcessor.ProcessedFunctionName ||
                md.Name.StartsWith(RpcProcessor.InvokeRpcPrefix))
                return;

            // check HasCustomAttribute here so avoid string allocation if it does not have an attribute

            if (md.HasCustomAttribute<ServerAttribute>())
                InjectGuard<ServerAttribute>(md, IsServer, "[Server] function '" + md.FullName + "' called on client");

            if (md.HasCustomAttribute<ClientAttribute>())
                InjectGuard<ClientAttribute>(md, IsClient, "[Client] function '" + md.FullName + "' called on server");

            if (md.HasCustomAttribute<HasAuthorityAttribute>())
                InjectGuard<HasAuthorityAttribute>(md, HasAuthority, "[Has Authority] function '" + md.FullName + "' called on player without authority");

            if (md.HasCustomAttribute<LocalPlayerAttribute>())
                InjectGuard<LocalPlayerAttribute>(md, IsLocalPlayer, "[Local Player] function '" + md.FullName + "' called on nonlocal player");
        }

        void InjectGuard<TAttribute>(MethodDefinition md, MethodReference predicate, string message)
        {
            CustomAttribute attribute = md.GetCustomAttribute<TAttribute>();
            if (attribute == null)
                return;

            // set modified here, it means an Attribute was found
            // and it will give add code, or give error for bad use
            modified = true;

            if (md.IsAbstract)
            {
                logger.Error($" {typeof(TAttribute)} can't be applied to abstract method. Apply to override methods instead.", md);
                return;
            }

            if (!md.DeclaringType.IsDerivedFrom<NetworkBehaviour>())
            {
                logger.Error($"{attribute.AttributeType.Name} method {md.Name} must be declared in a NetworkBehaviour", md);
                return;
            }

            bool throwError = attribute.GetField("error", true);

            ILProcessor worker = md.Body.GetILProcessor();
            Instruction top = md.Body.Instructions[0];

            worker.InsertBefore(top, worker.Create(OpCodes.Ldarg_0));
            worker.InsertBefore(top, worker.Create(OpCodes.Call, predicate));
            worker.InsertBefore(top, worker.Create(OpCodes.Brtrue, top));
            if (throwError)
            {
                worker.InsertBefore(top, worker.Create(OpCodes.Ldstr, message));
                worker.InsertBefore(top, worker.Create(OpCodes.Newobj, moduleCache.ImportReference(() => new MethodInvocationException(""))));
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
