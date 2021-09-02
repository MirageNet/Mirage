using Mirage.Serialization;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace Mirage.Weaver.SyncVars
{
    internal static class BitCountFinder
    {
        public static (int? bitCount, OpCode? ConvertCode) GetBitCount(FieldDefinition syncVar)
        {
            CustomAttribute attribute = syncVar.GetCustomAttribute<BitCountAttribute>();
            if (attribute == null)
                return default;

            int bitCount = (int)attribute.ConstructorArguments[0].Value;

            if (bitCount <= 0)
                throw new BitCountException("BitCount should be above 0", syncVar);

            int maxSize = BitPackHelper.GetTypeMaxSize(syncVar.FieldType, syncVar);

            if (bitCount > maxSize)
                throw new BitCountException($"BitCount can not be above target type size, bitCount:{bitCount}, max size:{maxSize}, type:{syncVar.FieldType.Name}", syncVar);

            return (bitCount, BitPackHelper.GetConvertType(syncVar.FieldType));
        }
    }
}
