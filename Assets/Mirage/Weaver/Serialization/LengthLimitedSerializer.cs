using System;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace Mirage.Weaver.Serialization
{
    internal class LengthLimitedSerializer : ValueSerializer
    {
        public override bool IsIntType => false;

        private readonly MethodReference writeFunction;
        private readonly MethodReference readFunction;
        private readonly int maxLength;

        public LengthLimitedSerializer(MethodReference writeFunction, MethodReference readFunction, int maxLength)
        {
            if (writeFunction == null && readFunction == null)
                throw new ArgumentNullException(nameof(writeFunction), "At least one of writeFunction or readFunction must be set");

            this.writeFunction = writeFunction;
            this.readFunction = readFunction;
            this.maxLength = maxLength;
        }

        public override void AppendWriteField(ModuleDefinition module, ILProcessor worker, ParameterDefinition writerParameter, ParameterDefinition typeParameter, FieldReference fieldReference)
        {
            worker.Append(LoadParamOrArg0(worker, writerParameter));
            worker.Append(LoadParamOrArg0(worker, typeParameter));
            worker.Append(worker.Create(OpCodes.Ldfld, fieldReference));
            worker.Append(worker.Create(OpCodes.Ldc_I4, maxLength));
            worker.Append(worker.Create(OpCodes.Call, writeFunction));
        }

        public override void AppendWriteParameter(ModuleDefinition module, ILProcessor worker, VariableDefinition writer, ParameterDefinition valueParameter)
        {
            worker.Append(worker.Create(OpCodes.Ldloc, writer));
            worker.Append(worker.Create(OpCodes.Ldarg, valueParameter));
            worker.Append(worker.Create(OpCodes.Ldc_I4, maxLength));
            worker.Append(worker.Create(OpCodes.Call, writeFunction));
        }

        public override void AppendRead(ModuleDefinition module, ILProcessor worker, ParameterDefinition readerParameter, TypeReference fieldType)
        {
            worker.Append(worker.Create(OpCodes.Ldarg, readerParameter));
            worker.Append(worker.Create(OpCodes.Ldc_I4, maxLength));
            worker.Append(worker.Create(OpCodes.Call, readFunction));
        }
    }
}
