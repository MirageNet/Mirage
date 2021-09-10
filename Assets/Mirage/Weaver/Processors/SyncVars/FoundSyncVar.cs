using Mirage.Serialization;
using Mirage.Weaver.NetworkBehaviours;
using Mono.Cecil;
using UnityEngine;

namespace Mirage.Weaver.SyncVars
{
    internal class FoundSyncVar
    {
        public readonly ModuleDefinition Module;
        public readonly FoundNetworkBehaviour Behaviour;
        public readonly FieldDefinition FieldDefinition;
        public readonly int DirtyIndex;
        public long DirtyBit => 1L << DirtyIndex;

        /// <summary>
        /// Flag to say if the sync var was successfully processed or not.
        /// We can check this else where in the code to so we dont throw extra errors when syncvar is invalid
        /// </summary>
        public bool HasProcessed { get; set; } = false;

        public FoundSyncVar(ModuleDefinition module, FoundNetworkBehaviour behaviour, FieldDefinition fieldDefinition, int dirtyIndex)
        {
            Module = module;
            Behaviour = behaviour;
            FieldDefinition = fieldDefinition;
            DirtyIndex = dirtyIndex;
        }

        public ValueSerializer _valueSerializer;
        public ValueSerializer ValueSerializer => _valueSerializer;

        public string OriginalName { get; private set; }
        public TypeReference OriginalType { get; private set; }
        public bool IsWrapped { get; private set; }


        public bool HasHookMethod { get; private set; }
        public MethodDefinition HookMethod { get; private set; }

        /// <summary>
        /// Changing the type of the field to the wrapper type, if one exists
        /// </summary>
        public void SetWrapType()
        {
            OriginalName = FieldDefinition.Name;
            OriginalType = FieldDefinition.FieldType;

            if (CheckWrapType(OriginalType, out TypeReference wrapType))
            {
                IsWrapped = true;
                FieldDefinition.FieldType = wrapType;
            }
        }

        private bool CheckWrapType(TypeReference originalType, out TypeReference wrapType)
        {
            TypeReference typeReference = originalType;

            if (typeReference.Is<NetworkIdentity>())
            {
                // change the type of the field to a wrapper NetworkIdentitySyncvar
                wrapType = Module.ImportReference<NetworkIdentitySyncvar>();
                return true;
            }
            if (typeReference.Is<GameObject>())
            {
                wrapType = Module.ImportReference<GameObjectSyncvar>();
                return true;
            }

            if (typeReference.Resolve().IsDerivedFrom<NetworkBehaviour>())
            {
                wrapType = Module.ImportReference<NetworkBehaviorSyncvar>();
                return true;
            }

            wrapType = null;
            return false;
        }

        bool HasIntAttribute => ValueSerializer != null && ValueSerializer.IsIntType;

        /// <summary>
        /// Finds any attribute values needed for this syncvar
        /// </summary>
        /// <param name="module"></param>
        public void ProcessAttributes()
        {
            HookMethod = HookMethodFinder.GetHookMethod(FieldDefinition, OriginalType);
            HasHookMethod = HookMethod != null;

            if (FieldDefinition.HasCustomAttribute<BitCountAttribute>())
                _valueSerializer = BitCountFinder.GetSerializer(FieldDefinition);

            if (FieldDefinition.HasCustomAttribute<VarIntAttribute>())
            {
                if (HasIntAttribute)
                    throw new VarIntException($"[VarInt] can't be used with [BitCount], [VarIntBlocks] or [BitCountFromRange]", FieldDefinition);

                _valueSerializer = VarIntFinder.GetSerializer(this);
            }

            if (FieldDefinition.HasCustomAttribute<VarIntBlocksAttribute>())
            {
                if (HasIntAttribute)
                    throw new VarIntBlocksException($"[VarIntBlocks] can't be used with [BitCount], [VarInt] or [BitCountFromRange]", FieldDefinition);

                _valueSerializer = VarIntBlocksFinder.GetSerializer(FieldDefinition);
            }

            if (FieldDefinition.HasCustomAttribute<BitCountFromRangeAttribute>())
            {
                if (HasIntAttribute)
                    throw new BitCountFromRangeException($"[BitCountFromRange] can't be used with [BitCount], [VarInt] or [VarIntBlocks]", FieldDefinition);

                _valueSerializer = BitCountFromRangeFinder.GetSerializer(FieldDefinition);
            }

            ZigZagFinder.CheckZigZag(FieldDefinition, ref _valueSerializer);

            if (FieldDefinition.HasCustomAttribute<FloatPackAttribute>())
                _valueSerializer = FloatPackFinder.GetSerializer(this);

            if (FieldDefinition.HasCustomAttribute<Vector2PackAttribute>())
                _valueSerializer = Vector2Finder.GetSerializer(this);

            if (FieldDefinition.HasCustomAttribute<Vector3PackAttribute>())
                _valueSerializer = Vector3Finder.GetSerializer(this);

            if (FieldDefinition.HasCustomAttribute<QuaternionPackAttribute>())
                _valueSerializer = QuaternionFinder.GetSerializer(this);
        }

        public void FindSerializeFunctions(Writers writers, Readers readers)
        {
            // dont need to find function is type already has serializer
            if (_valueSerializer != null) { return; }

            try
            {
                MethodReference writeFunction = writers.GetFunction_Thorws(FieldDefinition.FieldType);
                MethodReference readFunction = readers.GetFunction_Thorws(FieldDefinition.FieldType);
                _valueSerializer = new FunctionSerializer(writeFunction, readFunction);
            }
            catch (SerializeFunctionException e)
            {
                throw new SyncVarException($"{FieldDefinition.Name} is an unsupported type. {e.Message}", FieldDefinition);
            }
        }
    }
}
