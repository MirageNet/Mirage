using System.Collections.Generic;
using Mirage.CodeGen;
using Mirage.Collections;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace Mirage.Weaver
{
    public class SyncObjectProcessor
    {
        private readonly List<FieldDefinition> syncObjects = new List<FieldDefinition>();

        private readonly Readers readers;
        private readonly Writers writers;
        private readonly IWeaverLogger logger;

        public SyncObjectProcessor(Readers readers, Writers writers, IWeaverLogger logger)
        {
            this.readers = readers;
            this.writers = writers;
            this.logger = logger;
        }

        /// <summary>
        /// Finds SyncObjects fields in a type
        /// <para>Type should be a NetworkBehaviour</para>
        /// </summary>
        /// <param name="netBehaviourType"></param>
        /// <returns></returns>
        public void ProcessSyncObjects(TypeDefinition netBehaviourType)
        {
            foreach (var fieldDef in netBehaviourType.Fields)
            {
                if (fieldDef.FieldType.IsGenericParameter || fieldDef.ContainsGenericParameter) // Just ignore all generic objects.
                {
                    continue;
                }

                var fieldType = fieldDef.FieldType.Resolve();
                if (fieldType == null)
                {
                    continue;
                }

                if (fieldType.Implements<ISyncObject>())
                {
                    if (fieldDef.IsStatic)
                    {
                        logger.Error($"{fieldDef.Name} cannot be static", fieldDef);
                        continue;
                    }

                    // SyncObjects must be instantiated before NetworkBehaviour runs its initialization,
                    // otherwise the Weaver-generated registration will invoke methods on a null reference at runtime.
                    if (!IsInitialized(netBehaviourType, fieldDef))
                    {
                        logger.Error($"SyncObject {fieldDef.Name} must be initialized. Please assign a value where the field is declared or in the constructor.", fieldDef);
                        continue;
                    }

                    GenerateReadersAndWriters(fieldDef.FieldType);

                    syncObjects.Add(fieldDef);
                }
            }

            RegisterSyncObjects(netBehaviourType);
        }

        /// <summary>
        /// Generates serialization methods for synclists
        /// </summary>
        /// <param name="td">The synclist class</param>
        /// <param name="mirrorBaseType">the base SyncObject td inherits from</param>
        private void GenerateReadersAndWriters(TypeReference tr)
        {
            if (tr is GenericInstanceType genericInstance)
            {
                foreach (var argument in genericInstance.GenericArguments)
                {
                    if (!argument.IsGenericParameter)
                    {
                        readers.TryGetFunction(argument, null);
                        writers.TryGetFunction(argument, null);
                    }
                }
            }

            var baseType = tr?.Resolve()?.BaseType;
            if (baseType != null)
                GenerateReadersAndWriters(baseType);
        }

        private void RegisterSyncObjects(TypeDefinition netBehaviourSubclass)
        {
            Weaver.DebugLog(netBehaviourSubclass, "GenerateConstants ");

            netBehaviourSubclass.AddToConstructor(logger, (worker) =>
            {
                foreach (var fd in syncObjects)
                {
                    GenerateSyncObjectRegistration(worker, fd);
                }
            });
        }

        public static bool ImplementsSyncObject(TypeReference typeRef)
        {
            try
            {
                // value types cant inherit from SyncObject
                if (typeRef.IsValueType)
                {
                    return false;
                }

                return typeRef.Resolve().Implements<ISyncObject>();
            }
            catch
            {
                // sometimes this will fail if we reference a weird library that can't be resolved, so we just swallow that exception and return false
            }

            return false;
        }

        /*
            // generates code like:
            this.InitSyncObject(m_sizes);
        */
        private static void GenerateSyncObjectRegistration(ILProcessor worker, FieldDefinition fd)
        {
            worker.Append(worker.Create(OpCodes.Ldarg_0));
            worker.Append(worker.Create(OpCodes.Ldarg_0));
            worker.Append(worker.Create(OpCodes.Ldfld, fd));

            var initSyncObjectRef = worker.Body.Method.Module.ImportReference<NetworkBehaviour>(nb => nb.InitSyncObject(default));
            worker.Append(worker.Create(OpCodes.Call, initSyncObjectRef));
        }

        private bool IsInitialized(TypeDefinition netBehaviourType, FieldDefinition fieldDef)
        {
            var hasConstructors = false;

            foreach (var method in netBehaviourType.Methods)
            {
                // skip non-constructor
                if (!method.IsConstructor || method.IsStatic)
                    continue;

                hasConstructors = true;
                // skip Delegating constructors (eg a constructor calling another constructor)
                // they defer field initialization to the target constructor, so we skip checking them to avoid false negatives.
                if (IsDelegatingConstructor(method, netBehaviourType))
                    continue;

                // if field is not assigned, then return false
                // note: we will need to check all constructors. but for Monobehaviour this will almost always be the default constructor
                if (!ConstructorAssignsField(method, fieldDef))
                    return false;
            }

            // all constructors valid, or we did not find any
            return hasConstructors;
        }

        private bool IsDelegatingConstructor(MethodDefinition ctor, TypeDefinition td)
        {
            if (!ctor.HasBody)
                return false;

            foreach (var instruction in ctor.Body.Instructions)
            {
                if (instruction.OpCode == OpCodes.Call && instruction.Operand is MethodReference mr && mr.Name == ".ctor" && mr.DeclaringType.Resolve() == td)
                    return true;
            }

            return false;
        }

        private bool ConstructorAssignsField(MethodDefinition ctor, FieldDefinition fd)
        {
            if (!ctor.HasBody)
                return false;

            foreach (var instruction in ctor.Body.Instructions)
            {
                // check if field as a store, if it does it almost certianly is assigned
                if (instruction.OpCode == OpCodes.Stfld && instruction.Operand is FieldReference fr && fr.Resolve() == fd)
                    return true;
            }

            return false;
        }
    }
}
