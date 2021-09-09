using System.Linq.Expressions;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace Mirage.Weaver.SyncVars
{
    internal class PackerSerializer : ValueSerializer
    {
        readonly FieldDefinition packerField;

        readonly LambdaExpression packMethod;
        readonly LambdaExpression unpackMethod;

        public override bool IsIntType { get; }

        public PackerSerializer(FieldDefinition packerField, LambdaExpression packMethod, LambdaExpression unpackMethod, bool isIntType)
        {
            this.packerField = packerField;

            this.packMethod = packMethod;
            this.unpackMethod = unpackMethod;

            IsIntType = isIntType;
        }

        public override void AppendWrite(ModuleDefinition module, ILProcessor worker, ParameterDefinition writerParameter, ParameterDefinition typeParameter, FieldDefinition fieldDefinition)
        {
            // if PackerField is null it means there was an error earlier, so we dont need to do anything here
            if (packerField == null) { return; }

            // Generates: packer.pack(writer, field)
            worker.Append(worker.Create(OpCodes.Ldarg_0));
            worker.Append(worker.Create(OpCodes.Ldfld, packerField.MakeHostGenericIfNeeded()));
            worker.Append(worker.Create(OpCodes.Ldarg, writerParameter));
            worker.Append(worker.Create(OpCodes.Ldarg_0));
            worker.Append(worker.Create(OpCodes.Ldfld, ImportField(module, fieldDefinition)));
            worker.Append(worker.Create(OpCodes.Call, module.ImportReference(packMethod)));
        }

        public override void AppendRead(ModuleDefinition module, ILProcessor worker, ParameterDefinition readerParameter, FoundSyncVar syncVar)
        {
            // if PackerField is null it means there was an error earlier, so we dont need to do anything here
            if (packerField == null) { return; }

            // Generates: ... = packer.unpack(reader)
            worker.Append(worker.Create(OpCodes.Ldarg_0));
            worker.Append(worker.Create(OpCodes.Ldfld, packerField.MakeHostGenericIfNeeded()));
            worker.Append(worker.Create(OpCodes.Ldarg, readerParameter));
            worker.Append(worker.Create(OpCodes.Call, module.ImportReference(unpackMethod)));
        }
    }
}
