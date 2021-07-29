using Mirage.Serialization;
using Mono.Cecil;
using Mono.Cecil.Cil;
using UnityEngine;

namespace Mirage.Weaver.SyncVars
{
    internal class FoundSyncVar
    {
        public readonly FieldDefinition FieldDefinition;
        public readonly int DirtyIndex;
        public long DirtyBit => 1L << DirtyIndex;

        public FoundSyncVar(FieldDefinition fieldDefinition, int dirtyIndex)
        {
            FieldDefinition = fieldDefinition;
            DirtyIndex = dirtyIndex;
        }

        public string OriginalName { get; private set; }
        public TypeReference OriginalType { get; private set; }
        public bool IsWrapped { get; private set; }


        public bool HasHookMethod { get; private set; }
        public MethodDefinition HookMethod { get; private set; }

        public int? BitCount { get; private set; }
        public OpCode? BitCountConvert { get; internal set; }

        public MethodReference WriteFunction { get; private set; }
        public MethodReference ReadFunction { get; private set; }

        /// <summary>
        /// Changing the type of the field to the wrapper type, if one exists
        /// </summary>
        public void SetWrapType(ModuleDefinition module)
        {
            OriginalName = FieldDefinition.Name;
            OriginalType = FieldDefinition.FieldType;

            if (CheckWrapType(module, OriginalType, out TypeReference wrapType))
            {
                IsWrapped = true;
                FieldDefinition.FieldType = wrapType;
            }
        }

        private static bool CheckWrapType(ModuleDefinition module, TypeReference originalType, out TypeReference wrapType)
        {
            TypeReference typeReference = originalType;

            if (typeReference.Is<NetworkIdentity>())
            {
                // change the type of the field to a wrapper NetworkIdentitySyncvar
                wrapType = module.ImportReference<NetworkIdentitySyncvar>();
                return true;
            }
            if (typeReference.Is<GameObject>())
            {
                wrapType = module.ImportReference<GameObjectSyncvar>();
                return true;
            }

            if (typeReference.Resolve().IsDerivedFrom<NetworkBehaviour>())
            {
                wrapType = module.ImportReference<NetworkBehaviorSyncvar>();
                return true;
            }

            wrapType = null;
            return false;
        }


        /// <summary>
        /// Finds any attribute values needed for this syncvar
        /// </summary>
        /// <param name="module"></param>
        public void ProcessAttributes()
        {
            HookMethod = HookMethodFinder.GetHookMethod(FieldDefinition, OriginalType);
            HasHookMethod = HookMethod != null;

            (BitCount, BitCountConvert) = BitCountFinder.GetBitCount(FieldDefinition);
        }

        public void FindSerializeFunctions(Writers writers, Readers readers)
        {
            try
            {
                WriteFunction = writers.GetFunction_Thorws(FieldDefinition.FieldType);
                ReadFunction = readers.GetFunction_Thorws(FieldDefinition.FieldType);
            }
            catch (SerializeFunctionException e)
            {
                throw new SyncVarException($"{FieldDefinition.Name} is an unsupported type. {e.Message}", FieldDefinition);
            }
        }
    }
    public static class BitCountFinder
    {
        public static (int? bitCount, OpCode? ConvertCode) GetBitCount(FieldDefinition syncVar)
        {
            CustomAttribute attribute = syncVar.GetCustomAttribute<BitCountAttribute>();
            if (attribute == null)
                return default;

            int bitCount = (int)attribute.ConstructorArguments[0].Value;

            if (bitCount <= 0)
                throw new BitCountException("BitCountAttribute bitcount should be above 0", syncVar);

            int maxSize = GetTypeMaxSize(syncVar);

            if (bitCount > maxSize)
                throw new BitCountException("BitCountAttribute bitcount should be less than or equal to type size", syncVar);

            return (bitCount, GetConvertType(syncVar));
        }

        static int GetTypeMaxSize(FieldDefinition syncVar)
        {
            TypeReference type = syncVar.FieldType;
            if (type.Is<byte>()) return 8;
            if (type.Is<ushort>()) return 16;
            if (type.Is<short>()) return 16;
            if (type.Is<uint>()) return 32;
            if (type.Is<int>()) return 32;
            if (type.Is<ulong>()) return 64;
            if (type.Is<long>()) return 64;

            throw new BitCountException($"{type.FullName} is not a supported type for [BitCount]", syncVar);
        }

        /// <summary>
        /// Read returns a ulong, so if field is a smaller type it must be converted to a int32. all smaller types are padded to anyway
        /// </summary>
        /// <param name="syncVar"></param>
        /// <returns></returns>
        static OpCode? GetConvertType(FieldDefinition syncVar)
        {
            TypeReference type = syncVar.FieldType;
            // todo convert we can use Conv_I4 for all these types, or do we need Conv_I2, Conv_I1 instead?
            if (type.Is<byte>()) return OpCodes.Conv_I4;
            if (type.Is<ushort>()) return OpCodes.Conv_I4;
            if (type.Is<short>()) return OpCodes.Conv_I4;
            if (type.Is<uint>()) return OpCodes.Conv_I4;
            if (type.Is<int>()) return OpCodes.Conv_I4;

            return default;
        }
    }
}
