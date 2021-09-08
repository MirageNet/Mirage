using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using Mirage.Serialization;
using Mono.Cecil;
using Mono.Cecil.Cil;
using UnityEngine;

namespace Mirage.Weaver.SyncVars
{
    internal struct VarIntSettings
    {
        public ulong small;
        public ulong medium;
        public ulong? large;
        public bool throwIfOverLarge;
        public LambdaExpression packMethod;
        public LambdaExpression unpackMethod;
    }
    internal struct FloatPackSettings
    {
        public float max;
        public float? precision;
        public int? bitCount;
    }
    internal struct Vector2PackSettings
    {
        public Vector2 max;
        public Vector2? precision;
        public Vector2Int? bitCount;
    }
    internal struct Vector3PackSettings
    {
        public Vector3 max;
        public Vector3? precision;
        public Vector3Int? bitCount;
    }

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
        public VarIntSettings? VarIntSettings { get; private set; }
        public int? BlockCount { get; private set; }
        public OpCode? BitCountConvert { get; private set; }

        public bool UseZigZagEncoding { get; private set; }
        public int? BitCountMinValue { get; private set; }

        public FloatPackSettings? FloatPackSettings { get; private set; }
        public Vector2PackSettings? Vector2PackSettings { get; private set; }
        public Vector3PackSettings? Vector3PackSettings { get; private set; }
        public int? QuaternionBitCount { get; private set; }

        public FieldDefinition PackerField { get; internal set; }



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

        bool HasIntAttribute => BitCount.HasValue || VarIntSettings.HasValue || BlockCount.HasValue || BitCountMinValue.HasValue;

        /// <summary>
        /// Finds any attribute values needed for this syncvar
        /// </summary>
        /// <param name="module"></param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ProcessAttributes()
        {
            HookMethod = HookMethodFinder.GetHookMethod(FieldDefinition, OriginalType);
            HasHookMethod = HookMethod != null;

            (BitCount, BitCountConvert) = BitCountFinder.GetBitCount(FieldDefinition);
            if (FieldDefinition.HasCustomAttribute<VarIntAttribute>())
            {
                if (HasIntAttribute)
                    throw new VarIntException($"[VarInt] can't be used with [BitCount], [VarIntBlocks] or [BitCountFromRange]", FieldDefinition);

                VarIntSettings = VarIntFinder.GetBitCount(FieldDefinition);
            }

            if (FieldDefinition.HasCustomAttribute<VarIntBlocksAttribute>())
            {
                if (HasIntAttribute)
                    throw new VarIntBlocksException($"[VarIntBlocks] can't be used with [BitCount], [VarInt] or [BitCountFromRange]", FieldDefinition);

                (BlockCount, BitCountConvert) = VarIntBlocksFinder.GetBitCount(FieldDefinition);
            }

            if (FieldDefinition.HasCustomAttribute<BitCountFromRangeAttribute>())
            {
                if (HasIntAttribute)
                    throw new BitCountFromRangeException($"[BitCountFromRange] can't be used with [BitCount], [VarInt] or [VarIntBlocks]", FieldDefinition);

                (BitCount, BitCountConvert, BitCountMinValue) = BitCountFromRangeFinder.GetBitFoundFromRange(FieldDefinition);
            }

            UseZigZagEncoding = ZigZagFinder.HasZigZag(FieldDefinition, BitCount.HasValue);

            FloatPackSettings = FloatPackFinder.GetPackerSettings(FieldDefinition);
            Vector2PackSettings = Vector2Finder.GetPackerSettings(FieldDefinition);
            Vector3PackSettings = Vector3Finder.GetPackerSettings(FieldDefinition);
            QuaternionBitCount = QuaternionFinder.GetBitCount(FieldDefinition);
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
}
