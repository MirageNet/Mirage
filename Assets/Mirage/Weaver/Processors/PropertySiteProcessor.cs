using System;
using System.Linq;
using System.Threading;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace Mirage.Weaver
{
    internal class SiteProcessorException : WeaverException
    {
        public SiteProcessorException(string message, MemberReference memberReference, SequencePoint sequencePoint) : base(message, memberReference, sequencePoint)
        {
        }
    }

    public class PropertySiteProcessor
    {
        private ModuleDefinition _module;
        private readonly IWeaverLogger _logger;

        public PropertySiteProcessor(ModuleDefinition module, IWeaverLogger logger)
        {
            _module = module;
            _logger = logger;
        }

        private bool TryGetMethodFromAttributeGen(FieldDefinition fieldDef, out MethodReference mf)
        {
            mf = null;

            if (!fieldDef.TryGetCustomAttribute<SyncVarAttribute>(out var attribute))
                // not syncvar
                return false;

            var fieldName = fieldDef.Name;
            var typeDef = fieldDef.DeclaringType;

            var typeNameSyncVar = $"{typeDef.FullName}_SyncVar";
            var searchModule = fieldDef.Module;
            Console.WriteLine($"[PropertySiteProcessor]\n" +
                $"Current:{_module.Name}\n" +
                $"Ref:{searchModule}\n" +
                $"Search:{typeNameSyncVar}\n");
            var typeSyncVar = searchModule.Types.FirstOrDefault(x => x.FullName == typeNameSyncVar);
            if (typeSyncVar == null)
            {
                _logger.Error($"Could not find type named {typeNameSyncVar} in {searchModule.Name}. SyncVar:{fieldDef.Name}");
                return false;
            }

            var methodName = $"{SyncVarProcessor.SetNetworkName}{fieldName}";
            var setMethod = typeSyncVar.Methods.FirstOrDefault(x => x.Name == methodName);
            if (setMethod == null)
            {
                _logger.Error($"Could not find method named {methodName} in {typeSyncVar.FullName} in {searchModule.Name}. SyncVar:{fieldDef.Name}. Methods:[{string.Join(",", typeSyncVar.Methods.Select(x => x.Name))}]");
                return false;
            }

            mf = _module.ImportReference(setMethod);
            return true;
        }


        private bool TryGetMethodFromAttribute(FieldDefinition fieldDef, string attributeFieldName, out MethodReference methodReference)
        {
            methodReference = null;

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

        private static bool TrySlowResolve(FieldReference fieldRef, out FieldDefinition resolved, int retries = 10)
        {
            resolved = fieldRef.Resolve();
            if (resolved != null)
                return true;

            // if we fail, then try again 10 times (1 second)
            // we might be reading a new field from an assembly that is still being compiled by another ILPP
            for (var i = 0; i < 10; i++)
            {
                Console.WriteLine($"[WeaverResolve] Waiting to resolve field {fieldRef.FullName}");
                Thread.Sleep(100);

                resolved = fieldRef.Resolve();
                if (resolved != null)
                    return true;
            }

            return false;
        }

        private Instruction ProcessInstruction(MethodDefinition md, Instruction instr, SequencePoint sequencePoint)
        {
            if (instr.OpCode == OpCodes.Stfld && instr.Operand is FieldReference opFieldst)
            {
                // try get the field normally
                var resolved = opFieldst.Resolve();

                if (resolved == null && opFieldst.DeclaringType is GenericInstanceType)
                {
                    // try get field in generic type
                    resolved = opFieldst.DeclaringType.Resolve().GetField(opFieldst.Name);
                }

                // still null? wait and try get it again, it might be in another asmdef
                if (resolved == null && !TrySlowResolve(opFieldst, out resolved, retries: 0))
                {
                    _logger.Error($"Failed to Resolve field: {opFieldst.FullName}", md, md.DebugInformation.GetSequencePoint(instr));
                    return instr;
                }

                // this instruction sets the value of a field. cache the field reference.
                ProcessInstructionSetterField(md, instr, resolved);
            }

            if (instr.OpCode == OpCodes.Ldfld && instr.Operand is FieldReference opFieldld)
            {
                var resolved = opFieldld.Resolve();
                if (resolved == null)
                {
                    resolved = opFieldld.DeclaringType.Resolve().GetField(opFieldld.Name);
                }

                // this instruction gets the value of a field. cache the field reference.
                ProcessInstructionGetterField(instr, resolved);
            }

            if (instr.OpCode == OpCodes.Ldflda && instr.Operand is FieldReference opFieldlda)
            {
                var resolved = opFieldlda.Resolve();
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

        private void ProcessInstructionSetterField(MethodDefinition md, Instruction i, FieldDefinition opField)
        {
            if (opField.DeclaringType.IsGenericInstance || opField.DeclaringType.HasGenericParameters) // We're calling to a generic class
            {
                if (!TryGetMethodFromAttribute(opField, nameof(SyncVarAttribute.NetworkSet), out var replacement))
                    return;

                var newField = i.Operand as FieldReference;
                var genericType = (GenericInstanceType)newField.DeclaringType;
                i.OpCode = OpCodes.Callvirt;
                i.Operand = replacement.MakeHostInstanceGeneric(genericType);
            }
            else
            {
                if (!TryGetMethodFromAttributeGen(opField, out var replacement))
                    return;

                // old:
                // ld type
                // ld newValue
                // stIn Field

                // new:
                // ld type
                // ld type
                // ldAdd Field
                // ld newValue
                // call Method

                var worker = md.Body.GetILProcessor();
                var loadNewValue = i.Previous;
                worker.InsertBefore(loadNewValue, worker.Create(OpCodes.Dup));
                worker.InsertBefore(loadNewValue, worker.Create(OpCodes.Ldflda, _module.ImportReference(opField)));

                //replace with property
                i.OpCode = OpCodes.Call;
                i.Operand = replacement;
            }
        }

        private void ProcessInstructionGetterField(Instruction i, FieldDefinition opField)
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

        private Instruction ProcessInstructionLoadAddress(MethodDefinition md, Instruction instr, FieldDefinition opField)
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
