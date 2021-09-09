using Mono.Cecil;
using Mono.Cecil.Cil;

namespace Mirage.Weaver.SyncVars
{
    internal class FunctionSerializer : ValueSerializer
    {
        public override bool IsIntType => false;

        readonly MethodReference writeFunction;
        readonly MethodReference readFunction;

        public FunctionSerializer(MethodReference writeFunction, MethodReference readFunction)
        {
            this.writeFunction = writeFunction;
            this.readFunction = readFunction;
        }

        public override void AppendWrite(ModuleDefinition module, ILProcessor worker, ParameterDefinition writerParameter, ParameterDefinition typeParameter, FieldDefinition fieldDefinition)
        {
            AppendWrite_Field(module, worker, writerParameter, typeParameter, fieldDefinition);
        }

        void AppendWrite_Field(ModuleDefinition module, ILProcessor worker, ParameterDefinition writerParameter, ParameterDefinition typeParameter, FieldDefinition fieldDefinition)
        {
            // make generic and import field
            FieldReference fieldRef = module.ImportReference(fieldDefinition.MakeHostGenericIfNeeded());

            worker.Append(worker.Create(OpCodes.Ldarg, writerParameter));
            worker.Append(worker.Create(OpCodes.Ldarg, typeParameter));
            worker.Append(worker.Create(OpCodes.Ldfld, fieldDefinition));
            worker.Append(worker.Create(OpCodes.Call, writeFunction));
        }

        public override void AppendRead(ModuleDefinition module, ILProcessor worker, ParameterDefinition readerParameter, FoundSyncVar syncVar)
        {
            // add `reader` to stack
            worker.Append(worker.Create(OpCodes.Ldarg, readerParameter));
            // call read function
            worker.Append(worker.Create(OpCodes.Call, readFunction));
        }
    }
}
