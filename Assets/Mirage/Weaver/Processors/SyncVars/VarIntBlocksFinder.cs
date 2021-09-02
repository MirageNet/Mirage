using Mirage.Serialization;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace Mirage.Weaver.SyncVars
{
    internal static class VarIntBlocksFinder
    {
        public static (int? blockSize, OpCode? ConvertCode) GetBitCount(FieldDefinition syncVar)
        {
            CustomAttribute attribute = syncVar.GetCustomAttribute<VarIntBlocksAttribute>();
            if (attribute == null)
                return default;

            int blockSize = (int)attribute.ConstructorArguments[0].Value;

            if (blockSize <= 0)
                throw new VarIntBlocksException("Blocksize should be above 0", syncVar);

            // 32 is reasonable max value
            if (blockSize > 32)
                throw new VarIntBlocksException("Blocksize should be below 32", syncVar);

            return (blockSize, BitPackHelper.GetConvertType(syncVar.FieldType));
        }
    }
}
