using System.Collections.Generic;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace Mirage.Weaver
{
    public abstract class SerializeFunctionBase
    {
        protected readonly Dictionary<TypeReference, MethodReference> funcs = new Dictionary<TypeReference, MethodReference>(new TypeReferenceComparer());
        protected readonly IWeaverLogger logger;
        protected readonly ModuleDefinition module;

        public int Count => funcs.Count;

        /// <summary>
        /// Type used for logging, eg write or read
        /// </summary>
        protected abstract string FunctionTypeLog { get; }

        protected SerializeFunctionBase(ModuleDefinition module, IWeaverLogger logger)
        {
            this.logger = logger;
            this.module = module;
        }


        public void Register(TypeReference dataType, MethodReference methodReference)
        {
            if (funcs.ContainsKey(dataType))
            {
                logger.Warning(
                    $"Registering a {FunctionTypeLog} method for {dataType.FullName} when one already exists\n" +
                    $"  old:{funcs[dataType].FullName}\n" +
                    $"  new:{methodReference.FullName}",
                    methodReference.Resolve());
            }

            // we need to import type when we Initialize Writers so import here in case it is used anywhere else
            TypeReference imported = module.ImportReference(dataType);
            funcs[imported] = methodReference;
        }

        public MethodReference GetFunction<T>(SequencePoint sequencePoint) =>
            GetFunction(module.ImportReference<T>(), sequencePoint);

        public MethodReference GetFunction(TypeReference typeReference, SequencePoint sequencePoint)
        {
            if (funcs.TryGetValue(typeReference, out MethodReference foundFunc))
            {
                return foundFunc;
            }
            else
            {
                try
                {
                    return GenerateFunction(module.ImportReference(typeReference), sequencePoint);
                }
                catch (SerializeFunctionException e)
                {
                    logger.Error(e);
                    return null;
                }
            }
        }

        protected abstract MethodReference GenerateFunction(TypeReference typeReference, SequencePoint sequencePoint);
    }
}
