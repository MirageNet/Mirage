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
            // make generic and import field

            // if param is null then load arg0 instead
            WriteParamOfArg0(worker, writerParameter);
            WriteParamOfArg0(worker, typeParameter);
            worker.Append(worker.Create(OpCodes.Ldfld, ImportField(module, fieldDefinition)));
            worker.Append(worker.Create(OpCodes.Call, writeFunction));

        }
        static void WriteParamOfArg0(ILProcessor worker, ParameterDefinition parameter)
        {
            if (parameter == null)
            {
                worker.Append(worker.Create(OpCodes.Ldarg_0));
            }
            else
            {
                worker.Append(worker.Create(OpCodes.Ldarg, parameter));
            }
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
