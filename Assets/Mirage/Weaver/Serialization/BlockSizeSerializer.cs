using Mirage.Serialization;
using Mirage.Weaver.SyncVars;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace Mirage.Weaver.Serialization
{
    internal class BlockSizeSerializer : ValueSerializer
    {
        public override bool IsIntType => true;

        readonly int blockSize;
        readonly OpCode? typeConverter;

        public BlockSizeSerializer(int blockSize, OpCode? typeConverter)
        {
            this.blockSize = blockSize;
            this.typeConverter = typeConverter;
        }

        public override void AppendWriteField(ModuleDefinition module, ILProcessor worker, ParameterDefinition writerParameter, ParameterDefinition typeParameter, FieldDefinition fieldDefinition)
        {
            MethodReference writeWithBlockSize = module.ImportReference(() => VarIntBlocksPacker.Pack(default, default, default));

            worker.Append(worker.Create(OpCodes.Ldarg, writerParameter));
            worker.Append(worker.Create(OpCodes.Ldarg_0));
            worker.Append(worker.Create(OpCodes.Ldfld, ImportField(module, fieldDefinition)));
            worker.Append(worker.Create(OpCodes.Conv_U8));
            worker.Append(worker.Create(OpCodes.Ldc_I4, blockSize));
            worker.Append(worker.Create(OpCodes.Call, writeWithBlockSize));
        }

        public override void AppendWriteParameter(ModuleDefinition module, ILProcessor worker, VariableDefinition writer, ParameterDefinition valueParameter)
        {
            throw new System.NotImplementedException();
        }

        public override void AppendRead(ModuleDefinition module, ILProcessor worker, ParameterDefinition readerParameter, TypeReference fieldType)
        {
            MethodReference writeWithBlockSize = module.ImportReference(() => VarIntBlocksPacker.Unpack(default, default));

            worker.Append(worker.Create(OpCodes.Ldarg, readerParameter));
            worker.Append(worker.Create(OpCodes.Ldc_I4, blockSize));
            worker.Append(worker.Create(OpCodes.Call, writeWithBlockSize));

            // convert result to correct size if needed
            if (typeConverter.HasValue)
            {
                worker.Append(worker.Create(typeConverter.Value));
            }
        }
    }
}
