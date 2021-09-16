using Mirage.Serialization;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace Mirage.Weaver.Serialization
{
    internal class BitCountSerializer : ValueSerializer
    {
        public override bool IsIntType => true;

        readonly int bitCount;
        readonly OpCode? typeConverter;
        readonly bool useZigZag;
        readonly int? minValue;

        public BitCountSerializer(int bitCount, OpCode? typeConverter, bool useZigZag = false)
        {
            this.bitCount = bitCount;
            this.typeConverter = typeConverter;
            this.useZigZag = useZigZag;
            minValue = null;
        }

        public BitCountSerializer(int bitCount, OpCode? typeConverter, int? minValue)
        {
            this.bitCount = bitCount;
            this.typeConverter = typeConverter;
            useZigZag = false;
            this.minValue = minValue;
        }

        /// <summary>
        /// Creates new BitCountSerializer with zigzag enabled
        /// </summary>
        /// <returns></returns>
        internal BitCountSerializer CopyWithZigZag()
        {
            return new BitCountSerializer(bitCount, typeConverter, true);
        }

        public override void AppendWriteField(ModuleDefinition module, ILProcessor worker, ParameterDefinition writerParameter, ParameterDefinition typeParameter, FieldDefinition fieldDefinition)
        {
            MethodReference writeWithBitCount = module.ImportReference(typeof(NetworkWriter).GetMethod(nameof(NetworkWriter.Write)));

            worker.Append(LoadParamOrArg0(worker, writerParameter));
            worker.Append(LoadParamOrArg0(worker, typeParameter));
            worker.Append(worker.Create(OpCodes.Ldfld, ImportField(module, fieldDefinition)));

            if (useZigZag)
            {
                WriteZigZag(module, worker, fieldDefinition.FieldType);
            }
            if (minValue.HasValue)
            {
                WriteSubtractMinValue(worker);
            }

            worker.Append(worker.Create(OpCodes.Conv_U8));
            worker.Append(worker.Create(OpCodes.Ldc_I4, bitCount));
            worker.Append(worker.Create(OpCodes.Call, writeWithBitCount));
        }


        public override void AppendWriteParameter(ModuleDefinition module, ILProcessor worker, VariableDefinition writer, ParameterDefinition valueParameter)
        {
            MethodReference writeWithBitCount = module.ImportReference(typeof(NetworkWriter).GetMethod(nameof(NetworkWriter.Write)));

            worker.Append(worker.Create(OpCodes.Ldloc, writer));
            worker.Append(worker.Create(OpCodes.Ldarg, valueParameter));

            if (useZigZag)
            {
                WriteZigZag(module, worker, valueParameter.ParameterType);
            }
            if (minValue.HasValue)
            {
                WriteSubtractMinValue(worker);
            }

            worker.Append(worker.Create(OpCodes.Conv_U8));
            worker.Append(worker.Create(OpCodes.Ldc_I4, bitCount));
            worker.Append(worker.Create(OpCodes.Call, writeWithBitCount));
        }
        void WriteZigZag(ModuleDefinition module, ILProcessor worker, TypeReference fieldType)
        {
            bool useLong = fieldType.Is<long>();
            MethodReference encode = useLong
                ? module.ImportReference((long v) => ZigZag.Encode(v))
                : module.ImportReference((int v) => ZigZag.Encode(v));

            worker.Append(worker.Create(OpCodes.Call, encode));
        }
        void WriteSubtractMinValue(ILProcessor worker)
        {
            worker.Append(worker.Create(OpCodes.Ldc_I4, minValue.Value));
            worker.Append(worker.Create(OpCodes.Sub));
        }

        public override void AppendRead(ModuleDefinition module, ILProcessor worker, ParameterDefinition readerParameter, TypeReference fieldType)
        {
            MethodReference readWithBitCount = module.ImportReference(readerParameter.ParameterType.Resolve().GetMethod(nameof(NetworkReader.Read)));

            // add `reader` to stack
            worker.Append(worker.Create(OpCodes.Ldarg, readerParameter));
            // add `bitCount` to stack
            worker.Append(worker.Create(OpCodes.Ldc_I4, bitCount));
            // call `reader.read(bitCount)` function
            worker.Append(worker.Create(OpCodes.Call, readWithBitCount));

            // convert result to correct size if needed
            if (typeConverter.HasValue)
            {
                worker.Append(worker.Create(typeConverter.Value));
            }

            if (useZigZag)
            {
                ReadZigZag(module, worker, fieldType);
            }
            if (minValue.HasValue)
            {
                ReadAddMinValue(worker);
            }
        }

        void ReadZigZag(ModuleDefinition module, ILProcessor worker, TypeReference fieldType)
        {
            bool useLong = fieldType.Is<long>();
            MethodReference encode = useLong
                ? module.ImportReference((ulong v) => ZigZag.Decode(v))
                : module.ImportReference((uint v) => ZigZag.Decode(v));

            worker.Append(worker.Create(OpCodes.Call, encode));
        }
        void ReadAddMinValue(ILProcessor worker)
        {
            worker.Append(worker.Create(OpCodes.Ldc_I4, minValue.Value));
            worker.Append(worker.Create(OpCodes.Add));
        }
    }
}
