using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Mirage.Serialization;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Cecil.Rocks;

namespace Mirage.Weaver
{
    internal class SerializeMethod
    {
        public readonly MethodReference reference;
        public readonly int priority;

        public SerializeMethod(MethodReference reference, int priority)
        {
            this.reference = reference ?? throw new ArgumentNullException(nameof(reference));
            this.priority = priority;
        }
    }
    public class Writers
    {
        readonly Dictionary<TypeReference, SerializeMethod> writeFuncs = new Dictionary<TypeReference, SerializeMethod>(new TypeReferenceComparer());

        public int Count => writeFuncs.Count;

        private readonly IWeaverLogger logger;
        private readonly ModuleDefinition module;

        public Writers(ModuleDefinition module, IWeaverLogger logger)
        {
            this.logger = logger;
            this.module = module;
        }

        public void Register(TypeReference dataType, MethodReference methodReference)
        {
            int newPriority = getSerializePriority(methodReference);
            if (writeFuncs.ContainsKey(dataType))
            {
                SerializeMethod oldWriter = writeFuncs[dataType];

                // if old is higher, just return
                if (oldWriter.priority > newPriority) { return; }

                // if same, then warn
                if (oldWriter.priority == newPriority)
                {
                    logger.Warning(
                        $"Registering a Write method for {dataType.FullName} when one already exists\n" +
                        $"  old:{oldWriter.reference.FullName}\n" +
                        $"  new:{methodReference.FullName}",
                        methodReference);
                }

                // if new is higher, then add new
            }

            // we need to import type when we Initialize Writers so import here in case it is used anywhere else
            TypeReference imported = module.ImportReference(dataType);
            writeFuncs[imported] = new SerializeMethod(methodReference, newPriority);
        }

        static int getSerializePriority(MethodReference methodReference)
        {
            CustomAttribute attribute = methodReference.Resolve().GetCustomAttribute<SerializeExtensionAttribute>();

            int priority = 0;

            if (attribute != null)
            {
                priority = attribute.GetField<int>(nameof(SerializeExtensionAttribute.Priority), 0);
            }

            return priority;
        }

        public MethodReference GetWriteFunc<T>(SequencePoint sequencePoint) =>
            GetWriteFunc(module.ImportReference<T>(), sequencePoint);

        /// <summary>
        /// Finds existing writer for type, if non exists trys to create one
        /// <para>This method is recursive</para>
        /// </summary>
        /// <param name="typeReference"></param>
        /// <param name="recursionCount"></param>
        /// <returns>Returns <see cref="MethodReference"/> or null</returns>
        public MethodReference GetWriteFunc(TypeReference typeReference, SequencePoint sequencePoint)
        {
            if (writeFuncs.TryGetValue(typeReference, out SerializeMethod foundFunc))
            {
                return foundFunc.reference;
            }
            return GenerateWriter(module.ImportReference(typeReference), sequencePoint);
        }

        /// <exception cref="GenerateWriterException">Throws when writer could not be generated for type</exception>
        MethodReference GenerateWriter(TypeReference typeReference, SequencePoint sequencePoint)
        {
            if (typeReference.IsByReference)
            {
                logger.Error($"Cannot pass {typeReference.Name} by reference", typeReference, sequencePoint);
                return null;
            }

            // Arrays are special, if we resolve them, we get the element type,
            // eg int[] resolves to int
            // therefore process this before checks below
            if (typeReference.IsArray)
            {
                if (typeReference.IsMultidimensionalArray())
                {
                    logger.Error($"{typeReference.Name} is an unsupported type. Multidimensional arrays are not supported", typeReference, sequencePoint);
                }
                TypeReference elementType = typeReference.GetElementType();
                return GenerateCollectionWriter(typeReference, elementType, () => CollectionExtensions.WriteArray<byte>(default, default), sequencePoint);
            }

            if (typeReference.Resolve()?.IsEnum ?? false)
            {
                // serialize enum as their base type
                return GenerateEnumWriteFunc(typeReference, sequencePoint);
            }

            // check for collections
            if (typeReference.Is(typeof(Nullable<>)))
            {
                var genericInstance = (GenericInstanceType)typeReference;
                TypeReference elementType = genericInstance.GenericArguments[0];

                return GenerateCollectionWriter(typeReference, elementType, () => SystemTypesExtensions.WriteNullable<byte>(default, default), sequencePoint);
            }
            if (typeReference.Is(typeof(ArraySegment<>)))
            {
                var genericInstance = (GenericInstanceType)typeReference;
                TypeReference elementType = genericInstance.GenericArguments[0];

                return GenerateCollectionWriter(typeReference, elementType, () => CollectionExtensions.WriteArraySegment<byte>(default, default), sequencePoint);
            }
            if (typeReference.Is(typeof(List<>)))
            {
                var genericInstance = (GenericInstanceType)typeReference;
                TypeReference elementType = genericInstance.GenericArguments[0];

                return GenerateCollectionWriter(typeReference, elementType, () => CollectionExtensions.WriteList<byte>(default, default), sequencePoint);
            }

            // check for invalid types
            TypeDefinition typeDefinition = typeReference.Resolve();
            if (typeDefinition == null)
            {
                logger.Error($"{typeReference.Name} is not a supported type. Use a supported type or provide a custom writer", typeReference, sequencePoint);
                return null;
            }
            if (typeDefinition.IsDerivedFrom<NetworkBehaviour>())
            {
                return GetNetworkBehaviourWriter(typeReference);
            }
            if (typeDefinition.IsDerivedFrom<UnityEngine.Component>())
            {
                logger.Error($"Cannot generate writer for component type {typeReference.Name}. Use a supported type or provide a custom writer", typeReference, sequencePoint);
                return null;
            }
            if (typeReference.Is<UnityEngine.Object>())
            {
                logger.Error($"Cannot generate writer for {typeReference.Name}. Use a supported type or provide a custom writer", typeReference, sequencePoint);
                return null;
            }
            if (typeReference.Is<UnityEngine.ScriptableObject>())
            {
                logger.Error($"Cannot generate writer for {typeReference.Name}. Use a supported type or provide a custom writer", typeReference, sequencePoint);
                return null;
            }
            if (typeDefinition.HasGenericParameters)
            {
                logger.Error($"Cannot generate writer for generic type {typeReference.Name}. Use a supported type or provide a custom writer", typeReference, sequencePoint);
                return null;
            }
            if (typeDefinition.IsInterface)
            {
                logger.Error($"Cannot generate writer for interface {typeReference.Name}. Use a supported type or provide a custom writer", typeReference, sequencePoint);
                return null;
            }
            if (typeDefinition.IsAbstract)
            {
                logger.Error($"Cannot generate writer for abstract class {typeReference.Name}. Use a supported type or provide a custom writer", typeReference, sequencePoint);
                return null;
            }

            // generate writer for class/struct
            return GenerateClassOrStructWriterFunction(typeReference, sequencePoint);
        }

        private MethodReference GetNetworkBehaviourWriter(TypeReference typeReference)
        {
            MethodReference writeFunc = module.ImportReference<NetworkWriter>((nw) => nw.WriteNetworkBehaviour(default));
            Register(typeReference, writeFunc);
            return writeFunc;
        }

        private MethodDefinition GenerateEnumWriteFunc(TypeReference typeReference, SequencePoint sequencePoint)
        {
            MethodDefinition writerFunc = GenerateWriterFunc(typeReference);

            ILProcessor worker = writerFunc.Body.GetILProcessor();

            MethodReference underlyingWriter = GetWriteFunc(typeReference.Resolve().GetEnumUnderlyingType(), sequencePoint);

            worker.Append(worker.Create(OpCodes.Ldarg_0));
            worker.Append(worker.Create(OpCodes.Ldarg_1));
            worker.Append(worker.Create(OpCodes.Call, underlyingWriter));

            worker.Append(worker.Create(OpCodes.Ret));
            return writerFunc;
        }

        private MethodDefinition GenerateWriterFunc(TypeReference typeReference)
        {
            string functionName = "_Write_" + typeReference.FullName;
            // create new writer for this type
            MethodDefinition writerFunc = module.GeneratedClass().AddMethod(functionName,
                    MethodAttributes.Public |
                    MethodAttributes.Static |
                    MethodAttributes.HideBySig);

            _ = writerFunc.AddParam<NetworkWriter>("writer");
            _ = writerFunc.AddParam(typeReference, "value");
            writerFunc.Body.InitLocals = true;

            Register(typeReference, writerFunc);
            return writerFunc;
        }

        MethodDefinition GenerateClassOrStructWriterFunction(TypeReference typeReference, SequencePoint sequencePoint)
        {
            MethodDefinition writerFunc = GenerateWriterFunc(typeReference);

            ILProcessor worker = writerFunc.Body.GetILProcessor();

            if (!typeReference.Resolve().IsValueType)
                WriteNullCheck(worker, sequencePoint);

            if (!WriteAllFields(typeReference, worker, sequencePoint))
                return writerFunc;

            worker.Append(worker.Create(OpCodes.Ret));
            return writerFunc;
        }

        private void WriteNullCheck(ILProcessor worker, SequencePoint sequencePoint)
        {
            // if (value == null)
            // {
            //     writer.WriteBoolean(false);
            //     return;
            // }
            //

            Instruction labelNotNull = worker.Create(OpCodes.Nop);
            worker.Append(worker.Create(OpCodes.Ldarg_1));
            worker.Append(worker.Create(OpCodes.Brtrue, labelNotNull));
            worker.Append(worker.Create(OpCodes.Ldarg_0));
            worker.Append(worker.Create(OpCodes.Ldc_I4_0));
            worker.Append(worker.Create(OpCodes.Call, GetWriteFunc<bool>(sequencePoint)));
            worker.Append(worker.Create(OpCodes.Ret));
            worker.Append(labelNotNull);

            // write.WriteBoolean(true);
            worker.Append(worker.Create(OpCodes.Ldarg_0));
            worker.Append(worker.Create(OpCodes.Ldc_I4_1));
            worker.Append(worker.Create(OpCodes.Call, GetWriteFunc<bool>(sequencePoint)));
        }

        /// <summary>
        /// Find all fields in type and write them
        /// </summary>
        /// <param name="variable"></param>
        /// <param name="worker"></param>
        /// <returns>false if fail</returns>
        bool WriteAllFields(TypeReference variable, ILProcessor worker, SequencePoint sequencePoint)
        {
            uint fields = 0;
            foreach (FieldDefinition field in variable.FindAllPublicFields())
            {
                MethodReference writeFunc = GetWriteFunc(field.FieldType, sequencePoint);
                // need this null check till later PR when GetWriteFunc throws exception instead
                if (writeFunc == null) { return false; }

                FieldReference fieldRef = module.ImportReference(field);

                fields++;
                worker.Append(worker.Create(OpCodes.Ldarg_0));
                worker.Append(worker.Create(OpCodes.Ldarg_1));
                worker.Append(worker.Create(OpCodes.Ldfld, fieldRef));
                worker.Append(worker.Create(OpCodes.Call, writeFunc));
            }

            return true;
        }

        MethodDefinition GenerateCollectionWriter(TypeReference variable, TypeReference elementType, Expression<Action> writerFunction, SequencePoint sequencePoint)
        {

            MethodDefinition writerFunc = GenerateWriterFunc(variable);

            MethodReference elementWriteFunc = GetWriteFunc(elementType, sequencePoint);

            // need this null check till later PR when GetWriteFunc throws exception instead
            if (elementWriteFunc == null)
            {
                logger.Error($"Cannot generate writer for {variable}. Use a supported type or provide a custom writer", variable, sequencePoint);
                return writerFunc;
            }

            MethodReference collectionWriter = module.ImportReference(writerFunction).GetElementMethod();

            var methodRef = new GenericInstanceMethod(collectionWriter);
            methodRef.GenericArguments.Add(elementType);

            // generates
            // reader.WriteArray<T>(array);

            ILProcessor worker = writerFunc.Body.GetILProcessor();
            worker.Append(worker.Create(OpCodes.Ldarg_0)); // writer
            worker.Append(worker.Create(OpCodes.Ldarg_1)); // collection

            worker.Append(worker.Create(OpCodes.Call, methodRef)); // WriteArray

            worker.Append(worker.Create(OpCodes.Ret));

            return writerFunc;
        }

        /// <summary>
        /// Save a delegate for each one of the writers into <see cref="Writer{T}.Write"/>
        /// </summary>
        /// <param name="worker"></param>
        internal void InitializeWriters(ILProcessor worker)
        {
            var initializer = new WriterInitializer(module, worker);

            foreach (SerializeMethod serializeMethod in writeFuncs.Values)
            {
                initializer.SetWriter(serializeMethod.reference, serializeMethod.priority);
            }
        }
    }

    public sealed class WriterInitializer
    {
        readonly ILProcessor worker;

        readonly TypeReference genericWriterClassRef;
        readonly MethodReference setWriterRef;

        readonly TypeReference networkWriterRef;
        readonly TypeReference actionRef;
        readonly MethodReference actionConstructorRef;

        public WriterInitializer(ModuleDefinition module, ILProcessor worker)
        {
            this.worker = worker;

            genericWriterClassRef = module.ImportReference(typeof(Writer<>));
            System.Reflection.MethodInfo setWriterMethod = typeof(Writer<>).GetMethod(nameof(Writer<int>.SetWriter));
            setWriterRef = module.ImportReference(setWriterMethod);

            networkWriterRef = module.ImportReference(typeof(NetworkWriter));
            actionRef = module.ImportReference(typeof(Action<,>));
            actionConstructorRef = module.ImportReference(typeof(Action<,>).GetConstructors()[0]);
        }

        public void SetWriter(MethodReference methodReference, int priority)
        {
            TypeReference dataType = methodReference.Parameters[1].ParameterType;

            // create a Action<NetworkWriter, T> delegate
            worker.Append(worker.Create(OpCodes.Ldnull));
            worker.Append(worker.Create(OpCodes.Ldftn, methodReference));
            GenericInstanceType actionGenericInstance = actionRef.MakeGenericInstanceType(networkWriterRef, dataType);
            MethodReference actionRefInstance = actionConstructorRef.MakeHostInstanceGeneric(actionGenericInstance);
            worker.Append(worker.Create(OpCodes.Newobj, actionRefInstance));

            // call Write<T>.SetWriter(Action, int) to save it
            GenericInstanceType genericInstance = genericWriterClassRef.MakeGenericInstanceType(dataType);
            MethodReference specializedField = setWriterRef.MakeHostInstanceGeneric(genericInstance);

            worker.Append(worker.Create(OpCodes.Ldc_I4, priority));
            worker.Append(worker.Create(OpCodes.Call, specializedField));
        }
    }

}
