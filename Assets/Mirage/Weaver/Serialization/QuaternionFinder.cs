using System;
using System.Linq.Expressions;
using Mirage.Serialization;
using Mono.Cecil;
using Mono.Cecil.Cil;
using UnityEngine;

namespace Mirage.Weaver.Serialization
{
    internal class QuaternionFinder : PackerFinderBase<QuaternionPackAttribute, int>
    {
        protected override bool IsIntType => false;

        protected override int GetSettings(TypeReference fieldType, CustomAttribute attribute)
        {
            if (!fieldType.Is<Quaternion>())
            {
                throw new QuaternionPackException($"{fieldType} is not a supported type for [QuaternionPack]");
            }

            int bitCount = (int)attribute.ConstructorArguments[0].Value;

            if (bitCount <= 0)
                throw new QuaternionPackException("BitCount should be above 0");

            // no reason for 20, but seems higher than anyone should need
            if (bitCount > 20)
                throw new QuaternionPackException("BitCount should be below 20");

            return bitCount;
        }

        protected override LambdaExpression GetPackMethod(TypeReference fieldType)
        {
            Expression<Action<QuaternionPacker>> packMethod = (QuaternionPacker p) => p.Pack(default, default);
            return packMethod;
        }

        protected override LambdaExpression GetUnpackMethod(TypeReference fieldType)
        {
            Expression<Action<QuaternionPacker>> unpackMethod = (QuaternionPacker p) => p.Unpack(default(NetworkReader));
            return unpackMethod;
        }

        protected override FieldDefinition CreatePackerField(ModuleDefinition module, string fieldName, TypeDefinition holder, int settings)
        {
            FieldDefinition packerField = AddPackerField<QuaternionPacker>(holder, fieldName);

            NetworkBehaviourProcessor.AddToStaticConstructor(holder, (worker) =>
            {
                worker.Append(worker.Create(OpCodes.Ldc_I4, settings));
                MethodReference packerCtor = module.ImportReference(() => new QuaternionPacker(default(int)));
                worker.Append(worker.Create(OpCodes.Newobj, packerCtor));
                worker.Append(worker.Create(OpCodes.Stsfld, packerField));
            });

            return packerField;
        }
    }
}
