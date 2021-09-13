using Mono.Cecil;
using Mono.Cecil.Cil;
using UnityEngine;

namespace Mirage.Weaver
{
    /// <summary>
    /// Processes All methods and fields and checks if they are valid and Injects any code (for [Server/Client] attributes)
    /// </summary>
    class AttributeProcessor
    {
        private readonly IWeaverLogger logger;
        private readonly ModuleImportCache moduleCache;

        readonly MethodReference IsServer;
        readonly MethodReference IsClient;
        readonly MethodReference HasAuthority;
        readonly MethodReference IsLocalPlayer;

        bool modified;

        public AttributeProcessor(ModuleImportCache moduleCache, IWeaverLogger logger)
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
        /// <returns>True if IL code was added</returns>
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

            bool isMonoBehaviour = typeDefinition.IsDerivedFrom<MonoBehaviour>();
            bool isNetworkBehaviour = typeDefinition.IsDerivedFrom<NetworkBehaviour>();
            foreach (FieldDefinition fd in typeDefinition.Fields)
            {
                // SyncObjects are not allowed in MonoBehaviour, Unless it is also NetworkBehaviour
                bool checkForSyncObjects = isMonoBehaviour && !isNetworkBehaviour;
                ProcessField(fd, checkForSyncObjects);
            }

            foreach (TypeDefinition nested in typeDefinition.NestedTypes)
            {
                ProcessType(nested);
            }
        }

        void ProcessField(FieldDefinition fd, bool checkForSyncObjects)
        {
            CheckUsage<SyncVarAttribute>(fd);
            if (checkForSyncObjects)
                CheckSyncObject(fd);
        }

        void CheckUsage<TAttribute>(IMemberDefinition md)
        {
            CustomAttribute attribute = md.GetCustomAttribute<TAttribute>();
            if (attribute == null)
                return;

            if (!md.DeclaringType.IsDerivedFrom<NetworkBehaviour>())
            {
                logger.Error($"{attribute.AttributeType.Name} can only be used inside a NetworkBehaviour", md);
            }
        }

        void CheckSyncObject(FieldDefinition fd)
        {
            // check if SyncObject is used inside a monobehaviour
            if (SyncObjectProcessor.ImplementsSyncObject(fd.FieldType))
            {
                logger.Error($"{fd.Name} is a SyncObject and can only be used inside a NetworkBehaviour.", fd);
            }
        }

        void ProcessMethod(MethodDefinition md)
        {
            // skip these
            if (md.Name == ".cctor" ||
                md.Name == NetworkBehaviourProcessor.ProcessedFunctionName ||
                md.Name.StartsWith(RpcProcessor.InvokeRpcPrefix))
                return;

            InjectGuard<ServerAttribute>(md, IsServer, "[Server] function '{0}' called on client");
            InjectGuard<ClientAttribute>(md, IsClient, "[Client] function '{0}' called on server");
            InjectGuard<HasAuthorityAttribute>(md, HasAuthority, "[Has Authority] function '{0}' called on player without authority");
            InjectGuard<LocalPlayerAttribute>(md, IsLocalPlayer, "[Local Player] function '{0}' called on nonlocal player");
            CheckUsage<ServerRpcAttribute>(md);
            CheckUsage<ClientRpcAttribute>(md);
        }

        void InjectGuard<TAttribute>(MethodDefinition md, MethodReference predicate, string messageFormat)
        {
            CustomAttribute attribute = md.GetCustomAttribute<TAttribute>();
            if (attribute == null)
                return;


            if (md.IsAbstract)
            {
                logger.Error($" {typeof(TAttribute)} can't be applied to abstract method. Apply to override methods instead.", md);
                return;
            }

            if (!md.DeclaringType.IsDerivedFrom<NetworkBehaviour>())
            {
                logger.Error($"{attribute.AttributeType.Name} can only be used inside a NetworkBehaviour", md);
                return;
            }

            // set modified here, it means an Attribute was found
            // we dont need to set modified for errors above
            modified = true;

            bool throwError = attribute.GetField("error", true);

            ILProcessor worker = md.Body.GetILProcessor();
            Instruction top = md.Body.Instructions[0];

            worker.InsertBefore(top, worker.Create(OpCodes.Ldarg_0));
            worker.InsertBefore(top, worker.Create(OpCodes.Call, predicate));
            worker.InsertBefore(top, worker.Create(OpCodes.Brtrue, top));
            if (throwError)
            {
                string message = string.Format(messageFormat, md.FullName);
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
