using System;
using System.Linq.Expressions;
using Mirage.CodeGen;
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
        protected override Expression<Action> SegmentExpression => () => CollectionExtensions.WriteArraySegment<byte>(default, default);
        protected override Expression<Action> NullableExpression => () => SystemTypesExtensions.WriteNullable<byte>(default, default);

        protected override MethodReference GetGenericFunction()
        {
            var genericType = module.ImportReference(typeof(GenericTypesSerializationExtensions)).Resolve();
            var method = genericType.GetMethod(nameof(GenericTypesSerializationExtensions.Write));
            return module.ImportReference(method);
        }

        protected override MethodReference GetNetworkBehaviourFunction(TypeReference typeReference)
        {
            var writeMethod = GenerateWriterFunc(typeReference);
            var worker = writeMethod.worker;

            worker.Append(worker.Create(OpCodes.Ldarg_0));
            worker.Append(worker.Create(OpCodes.Ldarg_1));
            worker.Append(worker.Create(OpCodes.Call, (NetworkWriter writer) => writer.WriteNetworkBehaviour(default)));
            worker.Append(worker.Create(OpCodes.Ret));

            return writeMethod.definition;
        }

        protected override MethodReference GenerateEnumFunction(TypeReference typeReference)
        {
            var writerMethod = GenerateWriterFunc(typeReference);

            var worker = writerMethod.worker;

            var underlyingWriter = TryGetFunction(typeReference.Resolve().GetEnumUnderlyingType(), null);

            worker.Append(worker.Create(OpCodes.Ldarg_0));
            worker.Append(worker.Create(OpCodes.Ldarg_1));
            worker.Append(worker.Create(OpCodes.Call, underlyingWriter));

            worker.Append(worker.Create(OpCodes.Ret));
            return writerMethod.definition;
        }

        private struct WriteMethod
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
            var functionName = "_Write_" + typeReference.FullName;
            // create new writer for this type
            var definition = module.GeneratedClass().AddMethod(functionName,
                    MethodAttributes.Public |
                    MethodAttributes.Static |
                    MethodAttributes.HideBySig);

            var writerParameter = definition.AddParam<NetworkWriter>("writer");
            var typeParameter = definition.AddParam(typeReference, "value");
            definition.Body.InitLocals = true;

            Register(typeReference, definition);

            var worker = definition.Body.GetILProcessor();
            return new WriteMethod(definition, writerParameter, typeParameter, worker);
        }

        protected override MethodReference GenerateClassOrStructFunction(TypeReference typeReference)
        {
            var writerFunc = GenerateWriterFunc(typeReference);

            var worker = writerFunc.definition.Body.GetILProcessor();

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

            var labelNotNull = worker.Create(OpCodes.Nop);
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
        private void WriteAllFields(TypeReference type, WriteMethod writerFunc)
        {
            // create copy here because we might add static packer field
            var fields = type.FindAllPublicFields();
            foreach (var fieldDef in fields)
            {
                // note:
                // - fieldDef to get attributes
                // - fieldType (made non-generic if possible) used to get type (eg if MyMessage<int> and field `T Value` then get writer for int)
                // - fieldRef (imported) to emit IL codes

                var fieldType = fieldDef.GetFieldTypeIncludingGeneric(type);
                var fieldRef = module.ImportField(fieldDef, type);

                var valueSerialize = ValueSerializerFinder.GetSerializer(module, fieldDef, fieldType, this, null);
                valueSerialize.AppendWriteField(module, writerFunc.worker, writerFunc.writerParameter, writerFunc.typeParameter, fieldRef);
            }
        }

        protected override MethodReference GenerateCollectionFunction(TypeReference typeReference, TypeReference elementType, Expression<Action> genericExpression)
        {
            // make sure element has a writer
            // collection writers use the generic writer, so this will make sure one exists
            _ = GetFunction_Throws(elementType);

            var writerMethod = GenerateWriterFunc(typeReference);

            var collectionWriter = module.ImportReference(genericExpression).GetElementMethod();

            var methodRef = new GenericInstanceMethod(collectionWriter);
            methodRef.GenericArguments.Add(elementType);

            // generates
            // reader.WriteArray<T>(array);

            var worker = writerMethod.worker;
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
            var genericWriterClassRef = module.ImportReference(typeof(Writer<>));

            var writerProperty = typeof(Writer<>).GetProperty(nameof(Writer<int>.Write));
            var fieldRef = module.ImportReference(writerProperty.GetSetMethod());
            var networkWriterRef = module.ImportReference(typeof(NetworkWriter));
            var actionRef = module.ImportReference(typeof(Action<,>));
            var actionConstructorRef = module.ImportReference(typeof(Action<,>).GetConstructors()[0]);

            foreach (var writerMethod in funcs.Values)
            {

                var dataType = writerMethod.Parameters[1].ParameterType;

                // create a Action<NetworkWriter, T> delegate
                worker.Append(worker.Create(OpCodes.Ldnull));
                worker.Append(worker.Create(OpCodes.Ldftn, writerMethod));
                var actionGenericInstance = actionRef.MakeGenericInstanceType(networkWriterRef, dataType);
                var actionRefInstance = actionConstructorRef.MakeHostInstanceGeneric(actionGenericInstance);
                worker.Append(worker.Create(OpCodes.Newobj, actionRefInstance));

                // save it in Writer<T>.write
                var genericInstance = genericWriterClassRef.MakeGenericInstanceType(dataType);
                var specializedField = fieldRef.MakeHostInstanceGeneric(genericInstance);
                worker.Append(worker.Create(OpCodes.Call, specializedField));
            }
        }
    }
}
