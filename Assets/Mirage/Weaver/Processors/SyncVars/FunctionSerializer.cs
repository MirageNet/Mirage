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

        public override void AppendWrite(ModuleDefinition module, ILProcessor worker, ParameterDefinition writerParameter, FoundSyncVar syncVar)
        {
            // Generates a writer call for each sync variable
            // writer
            worker.Append(worker.Create(OpCodes.Ldarg, writerParameter));
            // this
            worker.Append(worker.Create(OpCodes.Ldarg_0));
            worker.Append(worker.Create(OpCodes.Ldfld, syncVar.FieldDefinition.MakeHostGenericIfNeeded()));
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
