using System.Collections.Generic;
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
        /// <param name="td"></param>
        /// <returns></returns>
        public void ProcessSyncObjects(TypeDefinition td)
        {
            foreach (var fd in td.Fields)
            {
                if (fd.FieldType.IsGenericParameter || fd.ContainsGenericParameter) // Just ignore all generic objects.
                {
                    continue;
                }

                var tf = fd.FieldType.Resolve();
                if (tf == null)
                {
                    continue;
                }

                if (tf.ImplementsInterface<ISyncObject>())
                {
                    if (fd.IsStatic)
                    {
                        logger.Error($"{fd.Name} cannot be static", fd);
                        continue;
                    }

                    GenerateReadersAndWriters(fd.FieldType);

                    syncObjects.Add(fd);
                }
            }

            RegisterSyncObjects(td);
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

            if (tr != null)
            {
                GenerateReadersAndWriters(tr.Resolve().BaseType);
            }
        }

        private void RegisterSyncObjects(TypeDefinition netBehaviourSubclass)
        {
            Weaver.DebugLog(netBehaviourSubclass, "  GenerateConstants ");

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

                return typeRef.Resolve().ImplementsInterface<ISyncObject>();
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
    }
}
