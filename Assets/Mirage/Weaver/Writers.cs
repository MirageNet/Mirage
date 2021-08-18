using System;
using System.Linq.Expressions;
using Mirage.Serialization;
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
            MethodDefinition writerFunc = GenerateWriterFunc(typeReference);

            ILProcessor worker = writerFunc.Body.GetILProcessor();

            MethodReference underlyingWriter = TryGetFunction(typeReference.Resolve().GetEnumUnderlyingType(), null);

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

        protected override MethodReference GenerateClassOrStructFunction(TypeReference typeReference)
        {
            MethodDefinition writerFunc = GenerateWriterFunc(typeReference);

            ILProcessor worker = writerFunc.Body.GetILProcessor();

            if (!typeReference.Resolve().IsValueType)
                WriteNullCheck(worker);

            if (!WriteAllFields(typeReference, worker))
                return writerFunc;

            worker.Append(worker.Create(OpCodes.Ret));
            return writerFunc;
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
        /// <param name="variable"></param>
        /// <param name="worker"></param>
        /// <returns>false if fail</returns>
        bool WriteAllFields(TypeReference variable, ILProcessor worker)
        {
            uint fields = 0;
            foreach (FieldDefinition field in variable.FindAllPublicFields())
            {
                MethodReference writeFunc = GetFunction_Thorws(field.FieldType);

                FieldReference fieldRef = module.ImportReference(field);

                fields++;
                worker.Append(worker.Create(OpCodes.Ldarg_0));
                worker.Append(worker.Create(OpCodes.Ldarg_1));
                worker.Append(worker.Create(OpCodes.Ldfld, fieldRef));
                worker.Append(worker.Create(OpCodes.Call, writeFunc));
            }

            return true;
        }

        protected override MethodReference GenerateSegmentFunction(TypeReference variable, TypeReference elementType)
        {
            Expression<Action> segmentExpression = () => CollectionExtensions.WriteArraySegment<byte>(default, default);
            return GenerateCollectionFunction(variable, elementType, segmentExpression);
        }
        protected override MethodReference GenerateCollectionFunction(TypeReference variable, TypeReference elementType, Expression<Action> writerFunction)
        {
            // make sure element has a writer
            // collection writers use the generic writer, so this will make sure one exists
            _ = GetFunction_Thorws(elementType);

            MethodDefinition writerFunc = GenerateWriterFunc(variable);

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
