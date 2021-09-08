using Mirage.Serialization;
using Mono.Cecil;
using UnityEngine;

namespace Mirage.Weaver.SyncVars
{
    internal static class QuaternionFinder
    {
        public static int? GetBitCount(FieldDefinition syncVar)
        {
            CustomAttribute attribute = syncVar.GetCustomAttribute<QuaternionPackAttribute>();
            if (attribute == null)
                return default;

            if (!syncVar.FieldType.Is<Quaternion>())
            {
                throw new QuaternionPackException($"{syncVar.FieldType} is not a supported type for [QuaternionPack]", syncVar);
            }

            int bitCount = (int)attribute.ConstructorArguments[0].Value;

            if (bitCount <= 0)
                throw new QuaternionPackException("BitCount should be above 0", syncVar);

            // no reason for 20, but seems higher than anyone should need
            if (bitCount > 20)
                throw new QuaternionPackException("BitCount should be below 20", syncVar);

            return bitCount;
        }
    }
}
