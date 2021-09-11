using System;
using Mirage.Weaver.SyncVars;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace Mirage.Weaver.Serialization
{
    internal class FunctionSerializer : ValueSerializer
    {
        public override bool IsIntType => false;

        readonly MethodReference writeFunction;
        readonly MethodReference readFunction;

        public FunctionSerializer(MethodReference writeFunction, MethodReference readFunction)
        {
            if (writeFunction == null && readFunction == null) throw new ArgumentNullException(nameof(writeFunction), "Atleast one of Writer or Reader should be set");

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

        public void AppendWriteRpc(ILProcessor worker, VariableDefinition writer, int argIndex)
        {
            // use built-in writer func on writer object
            // NetworkWriter object
            worker.Append(worker.Create(OpCodes.Ldloc, writer));
            // add argument to call
            worker.Append(worker.Create(OpCodes.Ldarg, argIndex));
            // call writer extension method
            worker.Append(worker.Create(OpCodes.Call, writeFunction));
        }

        public override void AppendRead(ModuleDefinition module, ILProcessor worker, ParameterDefinition readerParameter, FoundSyncVar syncVar)
        {
            // add `reader` to stack
            worker.Append(worker.Create(OpCodes.Ldarg, readerParameter));
            // call read function
            worker.Append(worker.Create(OpCodes.Call, readFunction));

            // todo check if we need this? it was in Rpc ReadArguments
            //// conversion.. is this needed?
            //if (param.ParameterType.Is<float>())
            //{
            //    worker.Append(worker.Create(OpCodes.Conv_R4));
            //}
            //else if (param.ParameterType.Is<double>())
            //{
            //    worker.Append(worker.Create(OpCodes.Conv_R8));
            //}
        }
    }
}
