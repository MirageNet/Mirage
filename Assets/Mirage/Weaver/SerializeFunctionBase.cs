using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace Mirage.Weaver
{
    public abstract class SerializeFunctionBase
    {
        protected readonly Dictionary<TypeReference, MethodReference> funcs = new Dictionary<TypeReference, MethodReference>(new TypeReferenceComparer());
        private readonly IWeaverLogger logger;
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
                    $"Registering a {FunctionTypeLog} for {dataType.FullName} when one already exists\n" +
                    $"  old:{funcs[dataType].FullName}\n" +
                    $"  new:{methodReference.FullName}",
                    methodReference.Resolve());
            }

            // we need to import type when we Initialize Writers so import here in case it is used anywhere else
            TypeReference imported = module.ImportReference(dataType);
            funcs[imported] = methodReference;
        }

        /// <summary>
        /// Trys to get writer for type, returns null if not found
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sequencePoint"></param>
        /// <returns>found methohd or null</returns>
        public MethodReference TryGetFunction<T>(SequencePoint sequencePoint) =>
            TryGetFunction(module.ImportReference<T>(), sequencePoint);

        /// <summary>
        /// Trys to get writer for type, returns null if not found
        /// </summary>
        /// <param name="typeReference"></param>
        /// <param name="sequencePoint"></param>
        /// <returns>found methohd or null</returns>
        public MethodReference TryGetFunction(TypeReference typeReference, SequencePoint sequencePoint)
        {
            try
            {
                return GetFunction_Thorws(typeReference);
            }
            catch (SerializeFunctionException e)
            {
                logger.Error(e, sequencePoint);
                return null;
            }
        }

        /// <summary>
        /// checks if function exists for type, if it does not exist it trys to generate it
        /// </summary>
        /// <param name="typeReference"></param>
        /// <param name="sequencePoint"></param>
        /// <returns></returns>
        /// <exception cref="SerializeFunctionException">Throws if unable to find or create function</exception>
        // todo rename this to GetFunction once other classes are able to catch Exception
        public MethodReference GetFunction_Thorws(TypeReference typeReference)
        {
            if (funcs.TryGetValue(typeReference, out MethodReference foundFunc))
            {
                return foundFunc;
            }
            else
            {
                return GenerateFunction(module.ImportReference(typeReference));
            }
        }

        private MethodReference GenerateFunction(TypeReference typeReference)
        {
            if (typeReference.IsByReference)
            {
                throw new SerializeFunctionException($"Cannot pass {typeReference.Name} by reference", typeReference);
            }

            // Arrays are special, if we resolve them, we get the element type,
            // eg int[] resolves to int
            // therefore process this before checks below
            if (typeReference.IsArray)
            {
                if (typeReference.IsMultidimensionalArray())
                {
                    throw new SerializeFunctionException($"{typeReference.Name} is an unsupported type. Multidimensional arrays are not supported", typeReference);
                }
                TypeReference elementType = typeReference.GetElementType();
                return GenerateCollectionFunction(typeReference, elementType, ArrayExpression);
            }

            // check for collections
            if (typeReference.Is(typeof(Nullable<>)))
            {
                var genericInstance = (GenericInstanceType)typeReference;
                TypeReference elementType = genericInstance.GenericArguments[0];

                return GenerateCollectionFunction(typeReference, elementType, NullableExpression);
            }
            if (typeReference.Is(typeof(ArraySegment<>)))
            {
                var genericInstance = (GenericInstanceType)typeReference;
                TypeReference elementType = genericInstance.GenericArguments[0];

                return GenerateSegmentFunction(typeReference, elementType);
            }
            if (typeReference.Is(typeof(List<>)))
            {
                var genericInstance = (GenericInstanceType)typeReference;
                TypeReference elementType = genericInstance.GenericArguments[0];

                return GenerateCollectionFunction(typeReference, elementType, ListExpression);
            }


            // check for invalid types
            TypeDefinition typeDefinition = typeReference.Resolve();
            if (typeDefinition == null)
            {
                throw ThrowCantGenerate(typeReference);
            }

            if (typeDefinition.IsEnum)
            {
                // serialize enum as their base type
                return GenerateEnumFunction(typeReference);
            }

            if (typeDefinition.IsDerivedFrom<NetworkBehaviour>())
            {
                return GetNetworkBehaviourFunction(typeReference);
            }

            // unity base types are invalid
            if (typeDefinition.IsDerivedFrom<UnityEngine.Component>())
            {
                throw ThrowCantGenerate(typeReference, "component type");
            }
            if (typeReference.Is<UnityEngine.Object>())
            {
                throw ThrowCantGenerate(typeReference);
            }
            if (typeReference.Is<UnityEngine.ScriptableObject>())
            {
                throw ThrowCantGenerate(typeReference);
            }


            if (typeDefinition.HasGenericParameters)
            {
                throw ThrowCantGenerate(typeReference, "generic type");
            }
            if (typeDefinition.IsInterface)
            {
                throw ThrowCantGenerate(typeReference, "interface");
            }
            if (typeDefinition.IsAbstract)
            {
                throw ThrowCantGenerate(typeReference, "abstract class");
            }

            // generate writer for class/struct
            return GenerateClassOrStructFunction(typeReference);
        }

        SerializeFunctionException ThrowCantGenerate(TypeReference typeReference, string typeDescription = null)
        {
            string reasonStr = string.IsNullOrEmpty(typeDescription) ? string.Empty : $"{typeDescription} ";
            return new SerializeFunctionException($"Cannot generate {FunctionTypeLog} for {reasonStr}{typeReference.Name}. Use a supported type or provide a custom {FunctionTypeLog}", typeReference);
        }

        protected abstract MethodReference GetNetworkBehaviourFunction(TypeReference typeReference);


        protected abstract MethodReference GenerateEnumFunction(TypeReference typeReference);
        protected abstract MethodReference GenerateCollectionFunction(TypeReference typeReference, TypeReference elementType, Expression<Action> genericExpression);
        protected abstract MethodReference GenerateSegmentFunction(TypeReference typeReference, TypeReference elementType);

        protected abstract Expression<Action> ArrayExpression { get; }
        protected abstract Expression<Action> ListExpression { get; }
        protected abstract Expression<Action> NullableExpression { get; }

        protected abstract MethodReference GenerateClassOrStructFunction(TypeReference typeReference);
    }
}
