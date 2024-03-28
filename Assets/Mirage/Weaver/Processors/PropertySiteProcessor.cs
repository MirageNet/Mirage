using System.Collections.Generic;
using Mirage.CodeGen;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace Mirage.Weaver
{
    /// <summary>
    /// Replaces SyncVar fields with their property getter/setting
    /// </summary>
    public class PropertySiteProcessor
    {
        // setter functions that replace [SyncVar] member variable references. dict<field, replacement>
        public Dictionary<FieldReference, MethodDefinition> Setters = new Dictionary<FieldReference, MethodDefinition>(new FieldReferenceComparator());
        // getter functions that replace [SyncVar] member variable references. dict<field, replacement>
        public Dictionary<FieldReference, MethodDefinition> Getters = new Dictionary<FieldReference, MethodDefinition>(new FieldReferenceComparator());

        public void Process(ModuleDefinition moduleDef)
        {
            // replace all field access with property access for syncvars
            CodePass.ForEachInstruction(moduleDef, WeavedMethods, ProcessInstruction);
        }

        private static bool WeavedMethods(MethodDefinition md)
        {
            if (md.Name == ".cctor")
                return false;
            if (md.Name == NetworkBehaviourProcessor.ProcessedFunctionName)
                return false;

            // dont use network get/set inside unity consturctors
            // this is because they will try to set dirtyBit and throw unity errors
            if (md.DeclaringType.IsDerivedFrom<UnityEngine.Object>() && md.IsConstructor)
                return false;

            // note: Constructor for non-unity types should be safe to use, for example get/set a sync var on a NB from without a struct

            return true;
        }

        private Instruction ProcessInstruction(MethodDefinition md, Instruction instr, SequencePoint sequencePoint)
        {
            if (instr.OpCode == OpCodes.Stfld && instr.Operand is FieldReference opFieldst)
            {
                FieldReference resolved = opFieldst.Resolve();
                if (resolved == null)
                {
                    resolved = opFieldst.DeclaringType.Resolve().GetField(opFieldst.Name);
                }

                // this instruction sets the value of a field. cache the field reference.
                ProcessInstructionSetterField(instr, resolved);
            }

            if (instr.OpCode == OpCodes.Ldfld && instr.Operand is FieldReference opFieldld)
            {
                FieldReference resolved = opFieldld.Resolve();
                if (resolved == null)
                {
                    resolved = opFieldld.DeclaringType.Resolve().GetField(opFieldld.Name);
                }

                // this instruction gets the value of a field. cache the field reference.
                ProcessInstructionGetterField(instr, resolved);
            }

            if (instr.OpCode == OpCodes.Ldflda && instr.Operand is FieldReference opFieldlda)
            {
                FieldReference resolved = opFieldlda.Resolve();
                if (resolved == null)
                {
                    resolved = opFieldlda.DeclaringType.Resolve().GetField(opFieldlda.Name);
                }

                // loading a field by reference,  watch out for initobj instruction
                // see https://github.com/vis2k/Mirror/issues/696
                return ProcessInstructionLoadAddress(md, instr, resolved);
            }

            return instr;
        }

        // replaces syncvar write access with the NetworkXYZ.get property calls
        private void ProcessInstructionSetterField(Instruction i, FieldReference opField)
        {
            // does it set a field that we replaced?
            if (Setters.TryGetValue(opField, out var replacement))
            {
                if (opField.DeclaringType.IsGenericInstance || opField.DeclaringType.HasGenericParameters) // We're calling to a generic class
                {
                    var newField = i.Operand as FieldReference;
                    var genericType = (GenericInstanceType)newField.DeclaringType;
                    i.OpCode = OpCodes.Callvirt;
                    i.Operand = replacement.MakeHostInstanceGeneric(genericType);
                }
                else
                {
                    //replace with property
                    i.OpCode = OpCodes.Call;
                    i.Operand = replacement;
                }
            }
        }

        // replaces syncvar read access with the NetworkXYZ.get property calls
        private void ProcessInstructionGetterField(Instruction i, FieldReference opField)
        {
            // does it set a field that we replaced?
            if (Getters.TryGetValue(opField, out var replacement))
            {
                if (opField.DeclaringType.IsGenericInstance || opField.DeclaringType.HasGenericParameters) // We're calling to a generic class
                {
                    var newField = i.Operand as FieldReference;
                    var genericType = (GenericInstanceType)newField.DeclaringType;
                    i.OpCode = OpCodes.Callvirt;
                    i.Operand = replacement.MakeHostInstanceGeneric(genericType);
                }
                else
                {
                    //replace with property
                    i.OpCode = OpCodes.Call;
                    i.Operand = replacement;
                }
            }
        }

        private Instruction ProcessInstructionLoadAddress(MethodDefinition md, Instruction instr, FieldReference opField)
        {
            // does it set a field that we replaced?
            if (Setters.TryGetValue(opField, out var replacement))
            {
                // we have a replacement for this property
                // is the next instruction a initobj?
                var nextInstr = instr.Next;

                if (nextInstr.OpCode == OpCodes.Initobj)
                {
                    // we need to replace this code with:
                    //     var tmp = new MyStruct();
                    //     this.set_Networkxxxx(tmp);
                    var worker = md.Body.GetILProcessor();
                    var tmpVariable = md.AddLocal(opField.FieldType);

                    worker.InsertBefore(instr, worker.Create(OpCodes.Ldloca, tmpVariable));
                    worker.InsertBefore(instr, worker.Create(OpCodes.Initobj, opField.FieldType));
                    worker.InsertBefore(instr, worker.Create(OpCodes.Ldloc, tmpVariable));
                    var newInstr = worker.Create(OpCodes.Call, replacement);
                    worker.InsertBefore(instr, newInstr);

                    worker.Remove(instr);
                    worker.Remove(nextInstr);

                    return newInstr;
                }
            }

            return instr;
        }
    }
}
