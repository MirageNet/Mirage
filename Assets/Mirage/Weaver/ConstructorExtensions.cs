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
            var ctor = typeDefinition.GetMethod(".ctor");

            if (ctor == null)
            {
                logger.Error($"{typeDefinition.Name} has invalid constructor", typeDefinition);
                return;
            }

            var ret = ctor.Body.Instructions[ctor.Body.Instructions.Count - 1];
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

            var worker = ctor.Body.GetILProcessor();
            body.Invoke(worker);

            // re-add Ret after body
            worker.Emit(OpCodes.Ret);
        }

        /// <summary>
        /// Adds code to static Constructor
        /// <para>
        /// If Constructor is missing a new one will be created
        /// </para>
        /// </summary>
        /// <param name="body">code to write</param>
        public static void AddToStaticConstructor(this TypeDefinition typeDefinition, Action<ILProcessor> body)
        {
            var cctor = typeDefinition.GetMethod(".cctor");
            if (cctor != null)
            {
                // remove the return opcode from end of function. will add our own later.
                if (cctor.Body.Instructions.Count != 0)
                {
                    var retInstr = cctor.Body.Instructions[cctor.Body.Instructions.Count - 1];
                    if (retInstr.OpCode == OpCodes.Ret)
                    {
                        cctor.Body.Instructions.RemoveAt(cctor.Body.Instructions.Count - 1);
                    }
                    else
                    {
                        throw new NetworkBehaviourException($"{typeDefinition.Name} has invalid static constructor", cctor, cctor.GetSequencePoint(retInstr));
                    }
                }
            }
            else
            {
                // make one!
                cctor = typeDefinition.AddMethod(".cctor", MethodAttributes.Private |
                        MethodAttributes.HideBySig |
                        MethodAttributes.SpecialName |
                        MethodAttributes.RTSpecialName |
                        MethodAttributes.Static);
            }

            var worker = cctor.Body.GetILProcessor();

            // add new code to bottom of constructor
            // todo should we be adding new code to top of function instead? incase user has early return in custom constructor?
            body.Invoke(worker);

            // re-add return bececause we removed it earlier
            worker.Append(worker.Create(OpCodes.Ret));

            // in case class had no cctor, it might have BeforeFieldInit, so injected cctor would be called too late
            typeDefinition.Attributes &= ~TypeAttributes.BeforeFieldInit;
        }
    }
}
