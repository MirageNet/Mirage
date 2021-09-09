using System;
using System.Linq.Expressions;
using Mirage.Serialization;
using Mono.Cecil;
using Mono.Cecil.Cil;
using UnityEngine;

namespace Mirage.Weaver.SyncVars
{
    internal static class QuaternionFinder
    {
        public static ValueSerializer GetSerializer(FoundSyncVar syncVar)
        {
            FieldDefinition fieldDefinition = syncVar.FieldDefinition;
            CustomAttribute attribute = fieldDefinition.GetCustomAttribute<QuaternionPackAttribute>();
            if (attribute == null)
                return default;

            if (!fieldDefinition.FieldType.Is<Quaternion>())
            {
                throw new QuaternionPackException($"{fieldDefinition.FieldType} is not a supported type for [QuaternionPack]", fieldDefinition);
            }

            int bitCount = (int)attribute.ConstructorArguments[0].Value;

            if (bitCount <= 0)
                throw new QuaternionPackException("BitCount should be above 0", fieldDefinition);

            // no reason for 20, but seems higher than anyone should need
            if (bitCount > 20)
                throw new QuaternionPackException("BitCount should be below 20", fieldDefinition);


            Expression<Action<QuaternionPacker>> packMethod = (QuaternionPacker p) => p.Pack(default, default);
            Expression<Action<QuaternionPacker>> unpackMethod = (QuaternionPacker p) => p.Unpack(default(NetworkReader));
            FieldDefinition packerField = CreatePackerField(syncVar, bitCount);

            return new PackerSerializer(packerField, packMethod, unpackMethod, false);
        }

        private static FieldDefinition CreatePackerField(FoundSyncVar syncVar, int bitCount)
        {
            FieldDefinition packerField = syncVar.Behaviour.AddPackerField<QuaternionPacker>(syncVar.FieldDefinition.Name);

            NetworkBehaviourProcessor.AddToStaticConstructor(syncVar.Behaviour.TypeDefinition, (worker) =>
            {
                worker.Append(worker.Create(OpCodes.Ldc_I4, bitCount));
                MethodReference packerCtor = syncVar.Module.ImportReference(() => new QuaternionPacker(default(int)));
                worker.Append(worker.Create(OpCodes.Newobj, packerCtor));
                worker.Append(worker.Create(OpCodes.Stsfld, packerField));
            });

            return packerField;
        }
    }
}
