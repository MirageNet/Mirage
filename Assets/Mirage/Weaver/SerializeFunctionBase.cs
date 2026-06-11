using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq.Expressions;
using Mirage.CodeGen;
using Mirage.Serialization;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace Mirage.Weaver
{
    public abstract class SerializeFunctionBase
    {
        [Conditional("WEAVER_DEBUG_LOGS")]
        private static void Log(string msg)
        {
            Console.Write($"[Weaver.SerializeFunction] {msg}\n");
        }
        /// <summary>Concrete type serialization function lookup.</summary>
        protected readonly Dictionary<TypeReference, MethodReference> functionLookup = new Dictionary<TypeReference, MethodReference>(new TypeReferenceComparer());
        /// <summary>Open generic serialization helper method lookup.</summary>
        protected readonly Dictionary<TypeDefinition, MethodReference> genericLookup = new Dictionary<TypeDefinition, MethodReference>(new TypeReferenceComparer());
        /// <summary>Concrete type length-limited serialization function lookup.</summary>
        protected readonly Dictionary<TypeReference, MethodReference> functionWithLengthLookup = new Dictionary<TypeReference, MethodReference>(new TypeReferenceComparer());
        /// <summary>Open generic length-limited serialization helper method lookup.</summary>
        protected readonly Dictionary<TypeDefinition, MethodReference> genericWithLengthLookup = new Dictionary<TypeDefinition, MethodReference>(new TypeReferenceComparer());
        protected readonly IWeaverLogger logger;
        protected readonly ModuleDefinition module;

        /// <summary>Count used to see if we generated any new ones, meaning the dll is changed and needs writing</summary>
        public int Count => functionLookup.Count;
        public int CountWithLength => functionWithLengthLookup.Count;

        /// <summary>
        /// Type used for logging, eg write or read
        /// </summary>
        protected abstract string FunctionTypeLog { get; }
        protected abstract string FunctionTypeWithLengthLog { get; }

        protected SerializeFunctionBase(ModuleDefinition module, IWeaverLogger logger)
        {
            this.logger = logger;
            this.module = module;
        }

        public void Register(TypeReference dataType, MethodReference methodReference) => RegisterInternal(dataType, methodReference, false);
        public void RegisterWithLength(TypeReference dataType, MethodReference methodReference) => RegisterInternal(dataType, methodReference, true);
        private void RegisterInternal(TypeReference dataType, MethodReference methodReference, bool withLength)
        {
            var lookup = withLength ? functionWithLengthLookup : functionLookup;
            var functionTypeLog = withLength ? FunctionTypeWithLengthLog : FunctionTypeLog;
            if (lookup.ContainsKey(dataType))
            {
                logger.Warning(
                    $"Registering a {functionTypeLog} for {dataType.FullName} when one already exists\n" +
                    $"  old:{lookup[dataType].FullName}\n" +
                    $"  new:{methodReference.FullName}",
                    methodReference.Resolve());
            }

            Log($"Register {functionTypeLog} for {dataType.FullName}, method:{methodReference.FullName}");

            // we need to import type when we Initialize Writers so import here in case it is used anywhere else
            var imported = module.ImportReference(dataType);
            lookup[imported] = methodReference;

            // mark type as generated,
            //MarkAsGenerated(dataType); <---  broken in unity2021
        }

        public void RegisterCollectionMethod(TypeDefinition dataType, MethodReference methodReference) => RegisterCollectionInternal(dataType, methodReference, withLength: false);
        public void RegisterCollectionMethodWithLength(TypeDefinition dataType, MethodReference methodReference) => RegisterCollectionInternal(dataType, methodReference, withLength: true);
        private void RegisterCollectionInternal(TypeDefinition dataType, MethodReference methodReference, bool withLength)
        {
            var lookup = withLength ? genericWithLengthLookup : genericLookup;
            var functionTypeLog = withLength ? FunctionTypeWithLengthLog : FunctionTypeLog;
            if (lookup.ContainsKey(dataType))
            {
                logger.Warning(
                    $"Registering a {functionTypeLog} for {dataType.FullName} when one already exists\n" +
                    $"  old:{lookup[dataType].FullName}\n" +
                    $"  new:{methodReference.FullName}",
                    methodReference.Resolve());
            }

            Log($"Register Collection Method {functionTypeLog} for {dataType.FullName}, method:{methodReference.FullName}");
            lookup[dataType] = methodReference;
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
        /// <returns>found method or null</returns>
        public MethodReference TryGetFunction(TypeReference typeReference, SequencePoint sequencePoint)
        {
            try
            {
                return GetFunction_Throws(typeReference);
            }
            catch (WeaverException e)
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
        public MethodReference GetFunction_Throws(TypeReference typeReference) => GetFunctionInternal(typeReference, withLength: false);

        /// <summary>
        /// checks if function exists for type, if it does not exist it trys to generate it with length limit
        /// </summary>
        /// <param name="typeReference"></param>
        /// <returns></returns>
        /// <exception cref="SerializeFunctionException">Throws if unable to find or create function</exception>
        // todo rename this to GetFunction once other classes are able to catch Exception
        public MethodReference GetFunctionWithLength_Throws(TypeReference typeReference) => GetFunctionInternal(typeReference, withLength: true);

        private MethodReference GetFunctionInternal(TypeReference typeReference, bool withLength)
        {
            // if is <T> then  just return generic write./read with T as the generic argument
            if (typeReference.IsGenericParameter || HasAsGenericAttribute(typeReference))
                return CreateGenericFunction(typeReference, withLength);

            // check if there is already a known function for type
            // this will find extension methods within this module
            var lookup = withLength ? functionWithLengthLookup : functionLookup;
            if (lookup.TryGetValue(typeReference, out var foundFunc))
                return foundFunc;

            //// before generating new function, check if one was generated for type in its own module
            //if (HasGeneratedFunctionInAnotherModule(typeReference)) < ---broken in unity2021
            //{
            //    return CreateGenericFunction(typeReference);
            //}

            var functionTypeLog = withLength ? FunctionTypeWithLengthLog : FunctionTypeLog;
            Log($"Trying to generate {functionTypeLog} for {typeReference.FullName}");
            return withLength
                ? GenerateFunctionWithLength(module.ImportReference(typeReference))
                : GenerateFunction(module.ImportReference(typeReference));
        }

        private bool HasAsGenericAttribute(TypeReference typeReference)
        {
            var typeDef = typeReference.Resolve();
            // system types can sometimes fail to resolve
            // but they will never have attribute so we can just return here
            if (typeDef == null)
                return false;

            return typeDef.HasCustomAttribute<WeaverWriteAsGenericAttribute>();
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

                TypeReference elementType;
                if (typeReference is ArrayType arrayType)
                    // need to use ArrayType to support jagged arrays
                    elementType = arrayType.ElementType;
                else
                    // fallback to GetElementType just incase
                    elementType = typeReference.GetElementType();

                var arrayMethod = module.ImportReference(ArrayExpression);
                return GenerateCollectionFunction(typeReference, new List<TypeReference> { elementType }, arrayMethod);
            }

            var typeDefinition = typeReference.Resolve();

            // check for collections
            if (genericLookup.TryGetValue(typeDefinition, out var collectionMethod))
            {
                var genericInstance = (GenericInstanceType)typeReference;
                var elementTypes = new List<TypeReference>();
                foreach (var type in genericInstance.GenericArguments)
                    elementTypes.Add(type);

                return GenerateCollectionFunction(typeReference, elementTypes, collectionMethod);
            }

            // check for invalid types
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

            // if it is genericInstance, then we can generate writer for it
            if (!typeReference.IsGenericInstance && typeDefinition.HasGenericParameters)
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
            var generated = GenerateClassOrStructFunction(typeReference);
            //MarkAsGenerated(typeDefinition); < ---broken in unity2021

            return generated;
        }

        private SerializeFunctionException ThrowCantGenerate(TypeReference typeReference, string typeDescription = null)
        {
            var reasonStr = string.IsNullOrEmpty(typeDescription) ? string.Empty : $"{typeDescription} ";
            return new SerializeFunctionException($"Cannot generate {FunctionTypeLog} for {reasonStr}{typeReference.Name}. Use a supported type or provide a custom {FunctionTypeLog}", typeReference);
        }

        private MethodReference GenerateFunctionWithLength(TypeReference typeReference)
        {
            // Unlike standard GenerateFunction which recursively compiles custom serializers,
            // This only generates function that will use the registered generic collection/array serializer (eg writers/readers that are tagged with WeaverSerializeCollectionAttribute).
            // we can't and dont need to generate recursively for and add withlength, fields/params that need MaxLength should be tagged with it themselves
            if (typeReference.IsByReference)
                throw new SerializeFunctionException($"Cannot pass {typeReference.Name} by reference", typeReference);

            if (typeReference.IsArray)
            {
                if (typeReference.IsMultidimensionalArray())
                    throw new SerializeFunctionException($"{typeReference.Name} is an unsupported type. Multidimensional arrays are not supported", typeReference);

                TypeReference elementType;
                if (typeReference is ArrayType arrayType)
                    elementType = arrayType.ElementType;
                else
                    elementType = typeReference.GetElementType();

                var arrayMethod = module.ImportReference(ArrayExpressionWithLength);
                return GenerateCollectionFunctionWithLength(typeReference, new List<TypeReference> { elementType }, arrayMethod);
            }

            var typeDefinition = typeReference.Resolve();

            if (typeDefinition != null && genericWithLengthLookup.TryGetValue(typeDefinition, out var collectionMethod))
            {
                var genericInstance = (GenericInstanceType)typeReference;
                var elementTypes = new List<TypeReference>();
                foreach (var type in genericInstance.GenericArguments)
                    elementTypes.Add(type);

                return GenerateCollectionFunctionWithLength(typeReference, elementTypes, collectionMethod);
            }

            throw new SerializeFunctionException($"Cannot generate {FunctionTypeWithLengthLog} for {typeReference.Name}. Limit attributes can only be used on types with a registered length-limited serializer.", typeReference);
        }

        /// <summary>
        /// Creates Generic instance for Write{T} or Read{T} with <paramref name="argument"/> as then generic argument
        /// <para>Can also create Write{int} if real type is given instead of generic argument</para>
        /// </summary>
        /// <param name="argument"></param>
        /// <returns></returns>
        private GenericInstanceMethod CreateGenericFunction(TypeReference argument, bool withLength)
        {
            // gets Writer<T>.Write method
            var method = withLength ? GetGenericFunctionWithLength() : GetGenericFunction();

            // makes method generic 
            var generic = new GenericInstanceMethod(method);
            generic.GenericArguments.Add(argument);

            return generic;
        }

        /// <summary>
        /// Gets generic Write{T} or Read{T}
        /// </summary>
        /// <returns></returns>
        protected abstract MethodReference GetGenericFunction();
        protected abstract MethodReference GetGenericFunctionWithLength();

        protected abstract MethodReference GetNetworkBehaviourFunction(TypeReference typeReference);

        protected abstract MethodReference GenerateEnumFunction(TypeReference typeReference);
        protected abstract MethodReference GenerateCollectionFunction(TypeReference typeReference, List<TypeReference> elementTypes, MethodReference collectionMethod);
        protected abstract MethodReference GenerateCollectionFunctionWithLength(TypeReference typeReference, List<TypeReference> elementTypes, MethodReference collectionMethod);

        protected abstract Expression<Action> ArrayExpression { get; }
        protected abstract Expression<Action> ArrayExpressionWithLength { get; }

        protected abstract MethodReference GenerateClassOrStructFunction(TypeReference typeReference);
    }
}
