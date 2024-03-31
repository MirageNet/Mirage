using System.Collections.Generic;
using Mirage.CodeGen;
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
    internal class AttributeProcessor
    {
        private readonly IWeaverLogger logger;
        private readonly MethodReference IsServer;
        private readonly MethodReference IsClient;
        private readonly MethodReference HasAuthority;
        private readonly MethodReference IsLocalPlayer;
        private bool modified = false;

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
            foreach (var foundType in foundTypes)
            {
                ProcessType(foundType);
            }

            return modified;
        }

        private void ProcessType(FoundType foundType)
        {
            foreach (var md in foundType.TypeDefinition.Methods)
            {
                ProcessMethod(md, foundType);
            }

            if (!foundType.IsNetworkBehaviour)
            {
                foreach (var fd in foundType.TypeDefinition.Fields)
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
        private void ProcessFields(FieldDefinition fd, FoundType foundType)
        {
            if (fd.HasCustomAttribute<SyncVarAttribute>())
                logger.Error($"SyncVar {fd.Name} must be inside a NetworkBehaviour. {foundType.TypeDefinition.Name} is not a NetworkBehaviour", fd);

            // only check SyncObjects inside Monobehaviours
            if (foundType.IsMonoBehaviour && SyncObjectProcessor.ImplementsSyncObject(fd.FieldType))
            {
                logger.Error($"{fd.Name} is a SyncObject and can not be used inside Monobehaviour. {foundType.TypeDefinition.Name} is not a NetworkBehaviour", fd);
            }
        }

        private void ProcessMethod(MethodDefinition md, FoundType foundType)
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
        private static bool IgnoreMethod(MethodDefinition md)
        {
            return md.Name == ".cctor" ||
                md.Name == NetworkBehaviourProcessor.ProcessedFunctionName;
        }

        private void ProcessMethodAttributes(MethodDefinition md, FoundType foundType)
        {
            InjectGuard<ServerAttribute>(md, foundType, IsServer, "[Server] function '{0}' called when server not active");
            InjectGuard<ClientAttribute>(md, foundType, IsClient, "[Client] function '{0}' called when client not active");
            InjectGuard<HasAuthorityAttribute>(md, foundType, HasAuthority, "[Has Authority] function '{0}' called on player without authority");
            InjectGuard<LocalPlayerAttribute>(md, foundType, IsLocalPlayer, "[Local Player] function '{0}' called on nonlocal player");
            InjectNetworkMethodGuard(md, foundType);
            CheckAttribute<ServerRpcAttribute>(md, foundType);
            CheckAttribute<ClientRpcAttribute>(md, foundType);
        }

        private void CheckAttribute<TAttribute>(MethodDefinition md, FoundType foundType)
        {
            var attribute = md.GetCustomAttribute<TAttribute>();
            if (attribute == null)
                return;

            if (!foundType.IsNetworkBehaviour)
            {
                logger.Error($"{attribute.AttributeType.Name} method {md.Name} must be declared in a NetworkBehaviour", md);
            }
        }

        private bool TryGetAttribute<TAttribute>(MethodDefinition md, FoundType foundType, out CustomAttribute attribute)
        {
            attribute = md.GetCustomAttribute<TAttribute>();
            if (attribute == null)
                return false;

            if (md.IsAbstract)
            {
                logger.Error($"{typeof(TAttribute)} can't be applied to abstract method. Apply to override methods instead.", md);
                return false;
            }

            if (!foundType.IsNetworkBehaviour)
            {
                logger.Error($"{attribute.AttributeType.Name} method {md.Name} must be declared in a NetworkBehaviour", md);
                return false;
            }

            if (md.Name == "Awake" && !md.HasParameters)
            {
                logger.Error($"{attribute.AttributeType.Name} will not work on the Awake method.", md);
                return false;
            }

            if (md.IsStatic)
            {
                logger.Error($"{attribute.AttributeType.Name} will not work on static method.", md);
                return false;
            }

            // dont need to set modified for errors, so we set it here when we start doing ILProcessing
            modified = true;
            return true;
        }

        private void InjectGuard<TAttribute>(MethodDefinition md, FoundType foundType, MethodReference predicate, string format)
        {
            if (!TryGetAttribute<TAttribute>(md, foundType, out var attribute))
                return;

            var throwError = attribute.GetField("error", true);
            var worker = md.Body.GetILProcessor();
            var top = md.Body.Instructions[0];

            worker.InsertBefore(top, worker.Create(OpCodes.Ldarg_0));
            worker.InsertBefore(top, worker.Create(OpCodes.Call, predicate));
            worker.InsertBefore(top, worker.Create(OpCodes.Brtrue, top));

            if (throwError)
            {
                var message = string.Format(format, md.Name);
                worker.InsertBefore(top, worker.Create(OpCodes.Ldstr, message));
                worker.InsertBefore(top, worker.Create(OpCodes.Newobj, () => new MethodInvocationException("")));
                worker.InsertBefore(top, worker.Create(OpCodes.Throw));
            }
            else
            {
                // dont need to set param or return if we throw
                InjectGuardParameters(md, worker, top);
                InjectGuardReturnValue(md, worker, top);
                worker.InsertBefore(top, worker.Create(OpCodes.Ret));
            }
        }

        private void InjectNetworkMethodGuard(MethodDefinition md, FoundType foundType)
        {
            if (!TryGetAttribute<NetworkMethodAttribute>(md, foundType, out var attribute))
                return;

            // Get the required flags from the attribute constructor argument
            var requiredFlagsValue = (NetworkFlags)attribute.ConstructorArguments[0].Value;
            var throwError = attribute.GetField("error", true);
            var worker = md.Body.GetILProcessor();
            var top = md.Body.Instructions[0];

            // check for each flag
            // if true, then jump to start of code
            // this should act as an OR check
            if (requiredFlagsValue.HasFlag(NetworkFlags.Server))
            {
                worker.InsertBefore(top, worker.Create(OpCodes.Ldarg_0));
                worker.InsertBefore(top, worker.Create(OpCodes.Call, IsServer));
                worker.InsertBefore(top, worker.Create(OpCodes.Brtrue, top));
            }
            if (requiredFlagsValue.HasFlag(NetworkFlags.Client))
            {
                worker.InsertBefore(top, worker.Create(OpCodes.Ldarg_0));
                worker.InsertBefore(top, worker.Create(OpCodes.Call, IsClient));
                worker.InsertBefore(top, worker.Create(OpCodes.Brtrue, top));
            }
            if (requiredFlagsValue.HasFlag(NetworkFlags.HasAuthority))
            {
                worker.InsertBefore(top, worker.Create(OpCodes.Ldarg_0));
                worker.InsertBefore(top, worker.Create(OpCodes.Call, HasAuthority));
                worker.InsertBefore(top, worker.Create(OpCodes.Brtrue, top));
            }
            if (requiredFlagsValue.HasFlag(NetworkFlags.LocalOwner))
            {
                // Check if the object is the local player's
                worker.InsertBefore(top, worker.Create(OpCodes.Ldarg_0));
                worker.InsertBefore(top, worker.Create(OpCodes.Call, IsLocalPlayer));
                worker.InsertBefore(top, worker.Create(OpCodes.Brtrue, top));
            }

            if (requiredFlagsValue.HasFlag(NetworkFlags.NotActive))
            {
                // Check if neither Server nor Clients are active
                worker.InsertBefore(top, worker.Create(OpCodes.Ldarg_0));
                worker.InsertBefore(top, worker.Create(OpCodes.Call, IsServer));
                worker.InsertBefore(top, worker.Create(OpCodes.Ldarg_0));
                worker.InsertBefore(top, worker.Create(OpCodes.Call, IsClient));
                worker.InsertBefore(top, worker.Create(OpCodes.Or));
                worker.InsertBefore(top, worker.Create(OpCodes.Brfalse, top));
            }

            if (throwError)
            {
                var message = $"Method '{md.Name}' cannot be executed as {nameof(NetworkFlags)} condition is not met.";
                worker.InsertBefore(top, worker.Create(OpCodes.Ldstr, message));
                worker.InsertBefore(top, worker.Create(OpCodes.Newobj, () => new MethodInvocationException("")));
                worker.InsertBefore(top, worker.Create(OpCodes.Throw));
            }
            else
            {
                // dont need to set param or return if we throw
                InjectGuardParameters(md, worker, top);
                InjectGuardReturnValue(md, worker, top);
                worker.InsertBefore(top, worker.Create(OpCodes.Ret));
            }
        }


        // this is required to early-out from a function with "out" parameters
        private static void InjectGuardParameters(MethodDefinition md, ILProcessor worker, Instruction top)
        {
            var offset = md.Resolve().IsStatic ? 0 : 1;
            for (var index = 0; index < md.Parameters.Count; index++)
            {
                var param = md.Parameters[index];
                // IsOut will be TRUE for `out` but FALSE for `ref`
                // this is what we want, because we dont want to set `ref` values
                if (!param.IsOut)
                    continue;

                var byRefType = (Mono.Cecil.ByReferenceType)param.ParameterType;

                // need to use ElementType not GetElementType()
                //   GetElementType() will get the element type of the inner elementType
                //   which will return wrong type for arrays and generics
                var outType = byRefType.ElementType;

                worker.InsertBefore(top, worker.Create(OpCodes.Ldarg, index + offset));
                worker.InsertBefore(top, worker.Create(OpCodes.Initobj, outType));
            }
        }

        // this is required to early-out from a function with a return value.
        private static void InjectGuardReturnValue(MethodDefinition md, ILProcessor worker, Instruction top)
        {
            if (!md.ReturnType.Is(typeof(void)))
            {
                var returnLocal = md.AddLocal(md.ReturnType);
                worker.InsertBefore(top, worker.Create(OpCodes.Ldloca, returnLocal));
                worker.InsertBefore(top, worker.Create(OpCodes.Initobj, md.ReturnType));
                worker.InsertBefore(top, worker.Create(OpCodes.Ldloc, returnLocal));
            }
        }
    }
}
