using System;
using System.Linq.Expressions;
using Mirage.Serialization;
using Mirage.Weaver.Serialization;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Cecil.Rocks;

namespace Mirage.Weaver
{

    public class Writers : SerializeFunctionBase
    {
        public Writers(ModuleDefinition module, IWeaverLogger logger) : base(module, logger) { }

        protected override string FunctionTypeLog => "write function";
        protected override Expression<Action> ArrayExpression => () => CollectionExtensions.WriteArray<byte>(default, default);
        protected override Expression<Action> ListExpression => () => CollectionExtensions.WriteList<byte>(default, default);
        protected override Expression<Action> NullableExpression => () => SystemTypesExtensions.WriteNullable<byte>(default, default);

        protected override MethodReference GetNetworkBehaviourFunction(TypeReference typeReference)
        {
            MethodReference writeFunc = module.ImportReference<NetworkWriter>((nw) => nw.WriteNetworkBehaviour(default));
            Register(typeReference, writeFunc);
            return writeFunc;
        }

        protected override MethodReference GenerateEnumFunction(TypeReference typeReference)
        {
            WriteMethod writerMethod = GenerateWriterFunc(typeReference);

            ILProcessor worker = writerMethod.worker;

            MethodReference underlyingWriter = TryGetFunction(typeReference.Resolve().GetEnumUnderlyingType(), null);

            worker.Append(worker.Create(OpCodes.Ldarg_0));
            worker.Append(worker.Create(OpCodes.Ldarg_1));
            worker.Append(worker.Create(OpCodes.Call, underlyingWriter));

            worker.Append(worker.Create(OpCodes.Ret));
            return writerMethod.definition;
        }
        struct WriteMethod
        {
            public readonly MethodDefinition definition;
            public readonly ParameterDefinition writerParameter;
            public readonly ParameterDefinition typeParameter;
            public readonly ILProcessor worker;

            public WriteMethod(MethodDefinition definition, ParameterDefinition writerParameter, ParameterDefinition typeParameter, ILProcessor worker)
            {
                this.definition = definition;
                this.writerParameter = writerParameter;
                this.typeParameter = typeParameter;
                this.worker = worker;
            }
        }

        private WriteMethod GenerateWriterFunc(TypeReference typeReference)
        {
            string functionName = "_Write_" + typeReference.FullName;
            // create new writer for this type
            MethodDefinition definition = module.GeneratedClass().AddMethod(functionName,
                    MethodAttributes.Public |
                    MethodAttributes.Static |
                    MethodAttributes.HideBySig);

            ParameterDefinition writerParameter = definition.AddParam<NetworkWriter>("writer");
            ParameterDefinition typeParameter = definition.AddParam(typeReference, "value");
            definition.Body.InitLocals = true;

            Register(typeReference, definition);

            ILProcessor worker = definition.Body.GetILProcessor();
            return new WriteMethod(definition, writerParameter, typeParameter, worker);
        }

        protected override MethodReference GenerateClassOrStructFunction(TypeReference typeReference)
        {
            WriteMethod writerFunc = GenerateWriterFunc(typeReference);

            ILProcessor worker = writerFunc.definition.Body.GetILProcessor();

            if (!typeReference.Resolve().IsValueType)
                WriteNullCheck(worker);

            WriteAllFields(typeReference, writerFunc);

            worker.Append(worker.Create(OpCodes.Ret));
            return writerFunc.definition;
        }

        private void WriteNullCheck(ILProcessor worker)
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
            worker.Append(worker.Create(OpCodes.Call, TryGetFunction<bool>(null)));
            worker.Append(worker.Create(OpCodes.Ret));
            worker.Append(labelNotNull);

            // write.WriteBoolean(true);
            worker.Append(worker.Create(OpCodes.Ldarg_0));
            worker.Append(worker.Create(OpCodes.Ldc_I4_1));
            worker.Append(worker.Create(OpCodes.Call, TryGetFunction<bool>(null)));
        }

        /// <summary>
        /// Find all fields in type and write them
        /// </summary>
        /// <param name="type"></param>
        /// <par
        /// <returns>false if fail</returns>
        void WriteAllFields(TypeReference type, WriteMethod writerFunc)
        {
            // create copy here because we might add static packer field
            System.Collections.Generic.IEnumerable<FieldDefinition> fields = type.FindAllPublicFields();
            foreach (FieldDefinition field in fields)
            {
                ValueSerializer valueSerialize = ValueSerializerFinder.GetSerializer(module, field, this, null);
                valueSerialize.AppendWriteField(module, writerFunc.worker, writerFunc.writerParameter, writerFunc.typeParameter, field);
            }
        }

        protected override MethodReference GenerateSegmentFunction(TypeReference typeReference, TypeReference elementType)
        {
            Expression<Action> segmentExpression = () => CollectionExtensions.WriteArraySegment<byte>(default, default);
            return GenerateCollectionFunction(typeReference, elementType, segmentExpression);
        }
        protected override MethodReference GenerateCollectionFunction(TypeReference typeReference, TypeReference elementType, Expression<Action> genericExpression)
        {
            // make sure element has a writer
            // collection writers use the generic writer, so this will make sure one exists
            _ = GetFunction_Thorws(elementType);

            WriteMethod writerMethod = GenerateWriterFunc(typeReference);

            MethodReference collectionWriter = module.ImportReference(genericExpression).GetElementMethod();

            var methodRef = new GenericInstanceMethod(collectionWriter);
            methodRef.GenericArguments.Add(elementType);

            // generates
            // reader.WriteArray<T>(array);

            ILProcessor worker = writerMethod.worker;
            worker.Append(worker.Create(OpCodes.Ldarg_0)); // writer
            worker.Append(worker.Create(OpCodes.Ldarg_1)); // collection

            worker.Append(worker.Create(OpCodes.Call, methodRef)); // WriteArray

            worker.Append(worker.Create(OpCodes.Ret));

            return writerMethod.definition;
        }

        /// <summary>
        /// Save a delegate for each one of the writers into <see cref="Writer{T}.Write"/>
        /// </summary>
        /// <param name="worker"></param>
        internal void InitializeWriters(ILProcessor worker)
        {
            TypeReference genericWriterClassRef = module.ImportReference(typeof(Writer<>));

            System.Reflection.PropertyInfo writerProperty = typeof(Writer<>).GetProperty(nameof(Writer<int>.Write));
            MethodReference fieldRef = module.ImportReference(writerProperty.GetSetMethod());
            TypeReference networkWriterRef = module.ImportReference(typeof(NetworkWriter));
            TypeReference actionRef = module.ImportReference(typeof(Action<,>));
            MethodReference actionConstructorRef = module.ImportReference(typeof(Action<,>).GetConstructors()[0]);

            foreach (MethodReference writerMethod in funcs.Values)
            {

                TypeReference dataType = writerMethod.Parameters[1].ParameterType;

                // create a Action<NetworkWriter, T> delegate
                worker.Append(worker.Create(OpCodes.Ldnull));
                worker.Append(worker.Create(OpCodes.Ldftn, writerMethod));
                GenericInstanceType actionGenericInstance = actionRef.MakeGenericInstanceType(networkWriterRef, dataType);
                MethodReference actionRefInstance = actionConstructorRef.MakeHostInstanceGeneric(actionGenericInstance);
                worker.Append(worker.Create(OpCodes.Newobj, actionRefInstance));

                // save it in Writer<T>.write
                GenericInstanceType genericInstance = genericWriterClassRef.MakeGenericInstanceType(dataType);
                MethodReference specializedField = fieldRef.MakeHostInstanceGeneric(genericInstance);
                worker.Append(worker.Create(OpCodes.Call, specializedField));
            }
        }
    }
}
