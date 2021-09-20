using System;
using System.Linq.Expressions;
using Mirage.Serialization;
using Mirage.Weaver.Serialization;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Cecil.Rocks;
using UnityEngine;

namespace Mirage.Weaver
{
    public class Readers : SerializeFunctionBase
    {
        public Readers(ModuleDefinition module, IWeaverLogger logger) : base(module, logger) { }

        protected override string FunctionTypeLog => "read function";

        protected override Expression<Action> ArrayExpression => () => CollectionExtensions.ReadArray<byte>(default);
        protected override Expression<Action> ListExpression => () => CollectionExtensions.ReadList<byte>(default);
        protected override Expression<Action> NullableExpression => () => SystemTypesExtensions.ReadNullable<byte>(default);

        protected override MethodReference GetNetworkBehaviourFunction(TypeReference typeReference)
        {
            ReadMethod readMethod = GenerateReaderFunction(typeReference);
            ILProcessor worker = readMethod.worker;

            worker.Append(worker.Create(OpCodes.Ldarg_0));
            worker.Append(worker.Create<NetworkReader>(OpCodes.Call, (reader) => reader.ReadNetworkBehaviour()));
            worker.Append(worker.Create(OpCodes.Castclass, typeReference));
            worker.Append(worker.Create(OpCodes.Ret));
            return readMethod.definition;
        }

        protected override MethodReference GenerateEnumFunction(TypeReference typeReference)
        {
            ReadMethod readMethod = GenerateReaderFunction(typeReference);

            ILProcessor worker = readMethod.worker;

            worker.Append(worker.Create(OpCodes.Ldarg_0));

            TypeReference underlyingType = typeReference.Resolve().GetEnumUnderlyingType();
            MethodReference underlyingFunc = TryGetFunction(underlyingType, null);

            worker.Append(worker.Create(OpCodes.Call, underlyingFunc));
            worker.Append(worker.Create(OpCodes.Ret));
            return readMethod.definition;
        }

        protected override MethodReference GenerateSegmentFunction(TypeReference typeReference, TypeReference elementType)
        {
            var genericInstance = (GenericInstanceType)typeReference;

            ReadMethod readMethod = GenerateReaderFunction(typeReference);

            ILProcessor worker = readMethod.worker;

            // $array = reader.Read<[T]>()
            ArrayType arrayType = elementType.MakeArrayType();
            worker.Append(worker.Create(OpCodes.Ldarg_0));
            worker.Append(worker.Create(OpCodes.Call, GetFunction_Thorws(arrayType)));

            // return new ArraySegment<T>($array)
            MethodReference arraySegmentConstructor = module.ImportReference(() => new ArraySegment<object>());
            worker.Append(worker.Create(OpCodes.Newobj, arraySegmentConstructor.MakeHostInstanceGeneric(genericInstance)));
            worker.Append(worker.Create(OpCodes.Ret));
            return readMethod.definition;
        }

        struct ReadMethod
        {
            public readonly MethodDefinition definition;
            public readonly ParameterDefinition readParameter;
            public readonly ILProcessor worker;

            public ReadMethod(MethodDefinition definition, ParameterDefinition readParameter, ILProcessor worker)
            {
                this.definition = definition;
                this.readParameter = readParameter;
                this.worker = worker;
            }
        }
        private ReadMethod GenerateReaderFunction(TypeReference variable)
        {
            string functionName = "_Read_" + variable.FullName;

            // create new reader for this type
            MethodDefinition definition = module.GeneratedClass().AddMethod(functionName,
                    MethodAttributes.Public |
                    MethodAttributes.Static |
                    MethodAttributes.HideBySig,
                    variable);

            ParameterDefinition readParameter = definition.AddParam<NetworkReader>("reader");
            definition.Body.InitLocals = true;
            Register(variable, definition);

            ILProcessor worker = definition.Body.GetILProcessor();
            return new ReadMethod(definition, readParameter, worker);
        }

        protected override MethodReference GenerateCollectionFunction(TypeReference typeReference, TypeReference elementType, Expression<Action> genericExpression)
        {
            // generate readers for the element
            _ = GetFunction_Thorws(elementType);

            ReadMethod readMethod = GenerateReaderFunction(typeReference);

            MethodReference listReader = module.ImportReference(genericExpression);

            var methodRef = new GenericInstanceMethod(listReader.GetElementMethod());
            methodRef.GenericArguments.Add(elementType);

            // generates
            // return reader.ReadList<T>()

            ILProcessor worker = readMethod.worker;
            worker.Append(worker.Create(OpCodes.Ldarg_0)); // reader
            worker.Append(worker.Create(OpCodes.Call, methodRef)); // Read

            worker.Append(worker.Create(OpCodes.Ret));

            return readMethod.definition;
        }

        protected override MethodReference GenerateClassOrStructFunction(TypeReference typeReference)
        {
            ReadMethod readMethod = GenerateReaderFunction(typeReference);

            // create local for return value
            VariableDefinition variable = readMethod.definition.AddLocal(typeReference);

            ILProcessor worker = readMethod.worker;


            TypeDefinition td = typeReference.Resolve();

            if (!td.IsValueType)
                GenerateNullCheck(worker);

            CreateNew(variable, worker, td);
            ReadAllFields(typeReference, readMethod);

            worker.Append(worker.Create(OpCodes.Ldloc, variable));
            worker.Append(worker.Create(OpCodes.Ret));
            return readMethod.definition;
        }

        private void GenerateNullCheck(ILProcessor worker)
        {
            // if (!reader.ReadBoolean())
            //   return null
            worker.Append(worker.Create(OpCodes.Ldarg_0));
            worker.Append(worker.Create(OpCodes.Call, TryGetFunction<bool>(null)));

            Instruction labelEmptyArray = worker.Create(OpCodes.Nop);
            worker.Append(worker.Create(OpCodes.Brtrue, labelEmptyArray));
            // return null
            worker.Append(worker.Create(OpCodes.Ldnull));
            worker.Append(worker.Create(OpCodes.Ret));
            worker.Append(labelEmptyArray);
        }

        // Initialize the local variable with a new instance
        void CreateNew(VariableDefinition variable, ILProcessor worker, TypeDefinition td)
        {
            TypeReference type = variable.VariableType;
            if (type.IsValueType)
            {
                // structs are created with Initobj
                worker.Append(worker.Create(OpCodes.Ldloca, variable));
                worker.Append(worker.Create(OpCodes.Initobj, type));
            }
            else if (td.IsDerivedFrom<ScriptableObject>())
            {
                MethodReference createScriptableObjectInstance = worker.Body.Method.Module.ImportReference(() => ScriptableObject.CreateInstance<ScriptableObject>());
                var genericInstanceMethod = new GenericInstanceMethod(createScriptableObjectInstance.GetElementMethod());
                genericInstanceMethod.GenericArguments.Add(type);
                worker.Append(worker.Create(OpCodes.Call, genericInstanceMethod));
                worker.Append(worker.Create(OpCodes.Stloc, variable));
            }
            else
            {
                // classes are created with their constructor
                MethodDefinition ctor = Resolvers.ResolveDefaultPublicCtor(type);
                if (ctor == null)
                {
                    throw new SerializeFunctionException($"{type.Name} can't be deserialized because it has no default constructor", type);
                }

                MethodReference ctorRef = worker.Body.Method.Module.ImportReference(ctor);

                worker.Append(worker.Create(OpCodes.Newobj, ctorRef));
                worker.Append(worker.Create(OpCodes.Stloc, variable));
            }
        }

        void ReadAllFields(TypeReference type, ReadMethod readMethod)
        {
            ILProcessor worker = readMethod.worker;
            // create copy here because we might add static packer field
            System.Collections.Generic.IEnumerable<FieldDefinition> fields = type.FindAllPublicFields();
            foreach (FieldDefinition field in fields)
            {
                // load this, write value, store value

                // mismatched ldloca/ldloc for struct/class combinations is invalid IL, which causes crash at runtime
                OpCode opcode = type.IsValueType ? OpCodes.Ldloca : OpCodes.Ldloc;
                worker.Append(worker.Create(opcode, 0));

                ValueSerializer valueSerialize = ValueSerializerFinder.GetSerializer(module, field, null, this);
                valueSerialize.AppendRead(module, worker, readMethod.readParameter, field.FieldType);

                FieldReference fieldRef = module.ImportReference(field);
                worker.Append(worker.Create(OpCodes.Stfld, fieldRef));
            }
        }

        /// <summary>
        /// Save a delegate for each one of the readers into <see cref="Reader{T}.Read"/>
        /// </summary>
        /// <param name="worker"></param>
        internal void InitializeReaders(ILProcessor worker)
        {
            TypeReference genericReaderClassRef = module.ImportReference(typeof(Reader<>));

            System.Reflection.PropertyInfo readProperty = typeof(Reader<>).GetProperty(nameof(Reader<object>.Read));
            MethodReference fieldRef = module.ImportReference(readProperty.GetSetMethod());
            TypeReference networkReaderRef = module.ImportReference(typeof(NetworkReader));
            TypeReference funcRef = module.ImportReference(typeof(Func<,>));
            MethodReference funcConstructorRef = module.ImportReference(typeof(Func<,>).GetConstructors()[0]);

            foreach (MethodReference readFunc in funcs.Values)
            {
                TypeReference dataType = readFunc.ReturnType;

                // create a Func<NetworkReader, T> delegate
                worker.Append(worker.Create(OpCodes.Ldnull));
                worker.Append(worker.Create(OpCodes.Ldftn, readFunc));
                GenericInstanceType funcGenericInstance = funcRef.MakeGenericInstanceType(networkReaderRef, dataType);
                MethodReference funcConstructorInstance = funcConstructorRef.MakeHostInstanceGeneric(funcGenericInstance);
                worker.Append(worker.Create(OpCodes.Newobj, funcConstructorInstance));

                // save it in Reader<T>.Read
                GenericInstanceType genericInstance = genericReaderClassRef.MakeGenericInstanceType(dataType);
                MethodReference specializedField = fieldRef.MakeHostInstanceGeneric(genericInstance);
                worker.Append(worker.Create(OpCodes.Call, specializedField));
            }
        }
    }
}
