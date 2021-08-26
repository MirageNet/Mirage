using System;
using System.Runtime.CompilerServices;
using Mirage.Serialization;
using Mono.Cecil;
using Mono.Cecil.Cil;
using UnityEngine;

namespace Mirage.Weaver.SyncVars
{
    internal struct FloatPackerSettings
    {
        public float max;
        public float? precision;
        public int? bitCount;
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
        public OpCode? BitCountConvert { get; private set; }

        public bool UseZigZagEncoding { get; private set; }
        public int? BitCountMinValue { get; private set; }

        public FloatPackerSettings? FloatPackerSettings { get; private set; }
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
            UseZigZagEncoding = ZigZagFinder.HasZigZag(FieldDefinition, BitCount.HasValue);

            // do this if check here so it doesn't override fields unless attribute exists
            if (FieldDefinition.HasCustomAttribute<BitCountFromRangeAttribute>())
                (BitCount, BitCountConvert, BitCountMinValue) = BitCountFromRangeFinder.GetBitFoundFromRange(FieldDefinition, BitCount.HasValue);

            FloatPackerSettings = FloatPackerFinder.GetPackerSettings(FieldDefinition);
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

    internal static class FloatPackerFinder
    {
        public static FloatPackerSettings? GetPackerSettings(FieldDefinition syncVar)
        {
            CustomAttribute attribute = syncVar.GetCustomAttribute<FloatPackerAttribute>();
            if (attribute == null)
                return default;

            if (!syncVar.FieldType.Is<float>())
            {
                throw new FloatPackerException($"{syncVar.FieldType} is not a supported type for [FloatPacker]", syncVar);
            }

            var settings = new FloatPackerSettings();
            settings.max = (float)attribute.ConstructorArguments[0].Value;
            if (settings.max <= 0)
            {
                throw new FloatPackerException($"Max must be above 0, max:{settings.max}", syncVar);
            }

            CustomAttributeArgument arg1 = attribute.ConstructorArguments[1];
            if (arg1.Type.Is<float>())
            {
                float precision = (float)arg1.Value;
                if (precision < 0)
                {
                    throw new FloatPackerException($"Precsion must be positive, precision:{precision}", syncVar);
                }
                // todo is there a better way to check if Precsion is too small?
                double expectedBitCount = Math.Floor(Math.Log(2 * settings.max / precision, 2)) + 1;
                if (expectedBitCount > 30)
                {
                    throw new FloatPackerException($"Precsion is too small, precision:{precision}", syncVar);
                }
                settings.precision = precision;
            }
            else
            {
                int bitCount = (int)arg1.Value;
                if (bitCount > 30)
                {
                    throw new FloatPackerException($"BitCount must be between 1 and 30 (inclusive), bitCount:{bitCount}", syncVar);
                }
                if (bitCount < 1)
                {
                    throw new FloatPackerException($"BitCount must be between 1 and 30 (inclusive), bitCount:{bitCount}", syncVar);
                }
                settings.bitCount = bitCount;
            }


            // validate bitcount, and other settings
            // 

            return settings;
        }
    }
}
