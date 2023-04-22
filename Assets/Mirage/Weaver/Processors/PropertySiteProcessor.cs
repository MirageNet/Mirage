using Mono.Cecil;
using Mono.Cecil.Cil;

namespace Mirage.Weaver
{
    public class PropertySiteProcessor
    {
        private ModuleDefinition _module;

        public PropertySiteProcessor(ModuleDefinition module)
        {
            _module = module;
        }

        private bool TryGetMethodFromAttribute(FieldReference field, string attributeFieldName, out MethodReference methodReference)
        {
            methodReference = null;

            var fieldDef = field.Resolve();
            if (!fieldDef.TryGetCustomAttribute<SyncVarAttribute>(out var attribute))
                // not syncvar
                return false;

            var setter = attribute.GetField<string>(attributeFieldName, null);
            if (string.IsNullOrEmpty(setter))
                // no setter
                return false;

            // we might be in another asmdef
            // find setter
            var type = fieldDef.DeclaringType;
            var method = type.GetMethod(setter);
            methodReference = _module.ImportReference(method);
            return true;
        }

        public void Process()
        {
            // replace all field access with property access for syncvars
            CodePass.ForEachInstruction(_module, WeavedMethods, ProcessInstruction);
        }

        private static bool WeavedMethods(MethodDefinition md) =>
                        md.Name != ".cctor" &&
                        md.Name != NetworkBehaviourProcessor.ProcessedFunctionName &&
                        !md.IsConstructor;

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

        private void ProcessInstructionSetterField(Instruction i, FieldReference opField)
        {
            if (!TryGetMethodFromAttribute(opField, nameof(SyncVarAttribute.NetworkSet), out var replacement))
                return;

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

        private void ProcessInstructionGetterField(Instruction i, FieldReference opField)
        {
            if (!TryGetMethodFromAttribute(opField, nameof(SyncVarAttribute.NetworkGet), out var replacement))
                return;

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

        private Instruction ProcessInstructionLoadAddress(MethodDefinition md, Instruction instr, FieldReference opField)
        {
            if (!TryGetMethodFromAttribute(opField, nameof(SyncVarAttribute.NetworkSet), out var replacement))
                return instr;

            // we have a replacement for this property
            // is the next instruction a initobj?
            var nextInstr = instr.Next;
            if (nextInstr.OpCode != OpCodes.Initobj)
                return instr;

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
}
