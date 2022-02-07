using System;
using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace Mirage.Weaver
{
    public static class ConstructorExtensions
    {
        /// <summary>
        /// Adds the body to the types constructor
        /// </summary>
        /// <param name="typeDefinition"></param>
        /// <param name="body"></param>
        /// <param name="logger"></param>
        public static void AddToConstructor(this TypeDefinition typeDefinition, IWeaverLogger logger, Action<ILProcessor> body)
        {
            // find instance constructor
            MethodDefinition ctor = typeDefinition.GetMethod(".ctor");

            if (ctor == null)
            {
                logger.Error($"{typeDefinition.Name} has invalid constructor", typeDefinition);
                return;
            }

            Instruction ret = ctor.Body.Instructions[ctor.Body.Instructions.Count - 1];
            if (ret.OpCode == OpCodes.Ret)
            {
                // remove Ret so we can emit body
                ctor.Body.Instructions.RemoveAt(ctor.Body.Instructions.Count - 1);
            }
            else
            {
                logger.Error($"{typeDefinition.Name} has invalid constructor", ctor, ctor.DebugInformation.SequencePoints.FirstOrDefault());
                return;
            }

            ILProcessor worker = ctor.Body.GetILProcessor();
            body.Invoke(worker);

            // re-add Ret after body
            worker.Emit(OpCodes.Ret);
        }
    }
}
