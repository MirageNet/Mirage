using System;
using System.Linq;
using Mirage.CodeGen;
using Mirage.Weaver.NetworkBehaviours;
using Mirage.Weaver.Serialization;
using Mirage.Weaver.SyncVars;
using Mono.Cecil;
using Mono.Cecil.Cil;
using UnityEngine;

namespace Mirage.Weaver
{
    /// <summary>
    /// Processes [SyncVar] in NetworkBehaviour
    /// </summary>
    public class SyncVarProcessor
    {
        private readonly ModuleDefinition module;
        private readonly Readers readers;
        private readonly Writers writers;

        private FoundNetworkBehaviour behaviour;

        public SyncVarProcessor(ModuleDefinition module, Readers readers, Writers writers)
        {
            this.module = module;
            this.readers = readers;
            this.writers = writers;
        }

        public void ProcessSyncVars(TypeDefinition td, IWeaverLogger logger)
        {
            behaviour = new FoundNetworkBehaviour(module, td);
            // the mapping of dirtybits to sync-vars is implicit in the order of the fields here. this order is recorded in m_replacementProperties.
            // start assigning syncvars at the place the base class stopped, if any

            // find syncvars
            // use ToArray to create copy
            foreach (var pd in td.Properties.ToArray())
            {
                // try/catch for each property, and log once
                // we dont want to spam multiple logs for a single property
                try
                {
                    if (IsValidSyncVar(pd, out var fd))
                    {
                        ValidateSyncVarType(pd, logger);

                        InjectAndCopyAttributes(pd, fd);

                        var syncVar = behaviour.AddSyncVar(pd, fd);
                        ProcessSyncVar(syncVar);
                        syncVar.HasProcessed = true;
                    }
                }
                catch (ValueSerializerException e)
                {
                    logger.Error(e.Message, pd);
                }
                catch (SyncVarException e)
                {
                    logger.Error(e);
                }
                catch (SerializeFunctionException e)
                {
                    // use property as member reference
                    logger.Error(e.Message, pd);
                }
            }

            behaviour.SetSyncVarCount();

            GenerateSerialization();
            GenerateDeserialization();
        }

        private bool IsValidSyncVar(PropertyDefinition property, out FieldDefinition backingField)
        {
            backingField = null;
            if (!property.HasCustomAttribute<SyncVarAttribute>())
                return false;

            if (property.GetMethod == null || property.SetMethod == null)
                throw new SyncVarException($"{property.Name} must have both get and set accessors", property);

            if (property.GetMethod.IsStatic || property.SetMethod.IsStatic)
                throw new SyncVarException($"{property.Name} cannot be static", property);

            var backingFieldName = $"<{property.Name}>k__BackingField";
            backingField = property.DeclaringType.Fields.FirstOrDefault(f => f.Name == backingFieldName);
            if (backingField == null)
                throw new SyncVarException($"{property.Name} is not an auto-property (no backing field found)", property);

            if (property.PropertyType.IsArray)
                throw new SyncVarException($"{property.Name} has invalid type. Use SyncLists instead of arrays", property);

            if (SyncObjectProcessor.ImplementsSyncObject(property.PropertyType))
                throw new SyncVarException($"{property.Name} has [SyncVar] attribute. ISyncObject should not be marked with SyncVar", property);

            if (!IsSimpleGetter(property, backingField))
                throw new SyncVarException($"{property.Name} does not have a simple auto-property getter", property);

            if (!IsSimpleSetter(property, backingField))
                throw new SyncVarException($"{property.Name} does not have a simple auto-property setter", property);

            return true;
        }

        private void ValidateSyncVarType(PropertyDefinition property, IWeaverLogger logger)
        {
            var type = property.PropertyType;
            if (type.Is<NetworkIdentity>() || type.Is<GameObject>() || type.Is<NetworkBehaviour>())
                return;

            var resolved = type.Resolve();
            if (resolved == null)
                return;

            if (resolved.IsDerivedFrom<NetworkBehaviour>())
                return;

            if (resolved.IsClass && !resolved.IsValueType && resolved.FullName != "System.String")
            {
                if (property.HasCustomAttribute<WeaverSyncVarSafeAttribute>() || resolved.HasCustomAttribute<WeaverSyncVarSafeAttribute>())
                    return;

                logger.Warning($"{property.Name} is a class. SyncVars that are classes can allocate and are difficult to track changes for. Consider using a struct instead, or mark the class or property with [WeaverSyncVarSafe] if it is custom serialized.", property);
            }
        }

        private bool IsSimpleGetter(PropertyDefinition property, FieldDefinition backingField)
        {
            if (property.GetMethod?.Body == null)
                return false;

            var ldfldCount = 0;
            foreach (var inst in property.GetMethod.Body.Instructions)
            {
                if (inst.OpCode == OpCodes.Ldfld)
                {
                    if ((inst.Operand as FieldReference)?.Resolve() != backingField)
                        return false;
                    ldfldCount++;
                }
                else if (inst.OpCode == OpCodes.Stfld || inst.OpCode == OpCodes.Ldsfld || inst.OpCode == OpCodes.Stsfld)
                    return false;
            }

            return ldfldCount == 1;
        }

        private bool IsSimpleSetter(PropertyDefinition property, FieldDefinition backingField)
        {
            if (property.SetMethod?.Body == null)
                return false;

            var stfldCount = 0;
            foreach (var inst in property.SetMethod.Body.Instructions)
            {
                if (inst.OpCode == OpCodes.Stfld)
                {
                    if ((inst.Operand as FieldReference)?.Resolve() != backingField)
                        return false;
                    stfldCount++;
                }
                else if (inst.OpCode == OpCodes.Ldfld || inst.OpCode == OpCodes.Ldsfld || inst.OpCode == OpCodes.Stsfld)
                    return false;
            }

            return stfldCount == 1;
        }

        private void InjectAndCopyAttributes(PropertyDefinition property, FieldDefinition backingField)
        {
            if (!backingField.CustomAttributes.Any(a => a.AttributeType.FullName == typeof(UnityEngine.SerializeField).FullName))
            {
                var serializeFieldConstructor = typeof(UnityEngine.SerializeField).GetConstructor(Type.EmptyTypes);
                var serializeFieldAttribute = new CustomAttribute(module.ImportReference(serializeFieldConstructor));
                backingField.CustomAttributes.Add(serializeFieldAttribute);
            }

            foreach (var attr in property.CustomAttributes)
            {
                var attrType = attr.AttributeType;
                if (attrType.Is<SyncVarAttribute>() || attrType.Namespace == "UnityEngine" || attrType.Namespace == "Mirage.Serialization")
                {
                    if (!backingField.CustomAttributes.Any(a => a.AttributeType.FullName == attrType.FullName))
                        CopyAttribute(attr, backingField);
                }
            }
        }

        private void CopyAttribute(CustomAttribute source, FieldDefinition targetField)
        {
            var newAttr = new CustomAttribute(module.ImportReference(source.Constructor));
            foreach (var arg in source.ConstructorArguments)
                newAttr.ConstructorArguments.Add(new CustomAttributeArgument(module.ImportReference(arg.Type), arg.Value));

            foreach (var field in source.Fields)
                newAttr.Fields.Add(new CustomAttributeNamedArgument(field.Name, new CustomAttributeArgument(module.ImportReference(field.Argument.Type), field.Argument.Value)));

            foreach (var prop in source.Properties)
                newAttr.Properties.Add(new CustomAttributeNamedArgument(prop.Name, new CustomAttributeArgument(module.ImportReference(prop.Argument.Type), prop.Argument.Value)));

            targetField.CustomAttributes.Add(newAttr);
        }

        private void ProcessSyncVar(FoundSyncVar syncVar)
        {
            // process attributes first before creating setting, otherwise it wont know about hook
            syncVar.SetWrapType();
            syncVar.ProcessAttributes(writers, readers);

            var pd = syncVar.PropertyDefinition;
            Weaver.DebugLog(pd.DeclaringType, $"Sync Var {pd.Name} {pd.PropertyType}");

            GenerateSyncVarGetter(syncVar);
            if (syncVar.InitialOnly)
                GenerateSyncVarSetterInitialOnly(syncVar);
            else
                GenerateSyncVarSetter(syncVar);
        }

        private void GenerateSyncVarGetter(FoundSyncVar syncVar)
        {
            var get = syncVar.PropertyDefinition.GetMethod;
            get.Body.Instructions.Clear();
            get.Body.Variables.Clear();

            var worker = get.Body.GetILProcessor();
            WriteLoadField(worker, syncVar);

            worker.Append(worker.Create(OpCodes.Ret));
        }

        private void GenerateSyncVarSetterInitialOnly(FoundSyncVar syncVar)
        {
            var set = syncVar.PropertyDefinition.SetMethod;
            set.Body.Instructions.Clear();
            set.Body.Variables.Clear();

            var valueParam = set.Parameters[0];

            var worker = set.Body.GetILProcessor();
            WriteStoreField(worker, valueParam, syncVar);
            worker.Append(worker.Create(OpCodes.Ret));
        }

        private void GenerateSyncVarSetter(FoundSyncVar syncVar)
        {
            var set = syncVar.PropertyDefinition.SetMethod;
            set.Body.Instructions.Clear();
            set.Body.Variables.Clear();

            var originalType = syncVar.OriginalType;
            var valueParam = set.Parameters[0];

            var worker = set.Body.GetILProcessor();

            // if (!SyncVarEqual(value, ref playerData))
            var endOfMethod = worker.Create(OpCodes.Nop);

            // this
            worker.Append(worker.Create(OpCodes.Ldarg_0));
            // new value to set
            worker.Append(worker.Create(OpCodes.Ldarg, valueParam));
            // reference to field to set
            // make generic version of SetSyncVar with field type
            WriteLoadField(worker, syncVar);

            var syncVarEqual = module.ImportReference<NetworkBehaviour>(nb => nb.SyncVarEqual<object>(default, default));
            var syncVarEqualGm = new GenericInstanceMethod(syncVarEqual.GetElementMethod());
            syncVarEqualGm.GenericArguments.Add(originalType);
            worker.Append(worker.Create(OpCodes.Call, syncVarEqualGm));

            worker.Append(worker.Create(OpCodes.Brtrue, endOfMethod));

            // T oldValue = value
            var oldValue = set.AddLocal(originalType);
            WriteLoadField(worker, syncVar);
            worker.Append(worker.Create(OpCodes.Stloc, oldValue));

            // fieldValue = value
            WriteStoreField(worker, valueParam, syncVar);

            // this.SetDirtyBit(dirtyBit)
            worker.Append(worker.Create(OpCodes.Ldarg_0));
            worker.Append(worker.Create(OpCodes.Ldc_I8, syncVar.DirtyBit));
            worker.Append(worker.Create<NetworkBehaviour>(OpCodes.Call, nb => nb.SetDirtyBit(default)));

            if (syncVar.HasHook)
            {
                //if (base.isLocalClient && !getSyncVarHookGuard(dirtyBit))
                var afterIf = worker.Create(OpCodes.Nop);
                var startIf = worker.Create(OpCodes.Nop);

                // check if there is guard
                worker.Append(worker.Create(OpCodes.Ldarg_0));
                worker.Append(worker.Create(OpCodes.Ldc_I8, syncVar.DirtyBit));
                worker.Append(worker.Create<NetworkBehaviour>(OpCodes.Call, nb => nb.GetSyncVarHookGuard(default)));
                worker.Append(worker.Create(OpCodes.Brtrue, afterIf));

                if (syncVar.InvokeHookOnOwner)
                {
                    worker.Append(worker.Create(OpCodes.Ldarg_0));
                    worker.Append(worker.Create(OpCodes.Call, (NetworkBehaviour nb) => nb.HasAuthority));
                    // if true, go to start of if
                    // this will act as an OR for the IsServer check
                    worker.Append(worker.Create(OpCodes.Brtrue, startIf));
                }

                worker.Append(worker.Create(OpCodes.Ldarg_0));
                if (syncVar.InvokeHookOnServer)
                    // if invokeOnServer, then `IsServer` will also cover the Host case too so we dont need to use an OR here
                    worker.Append(worker.Create(OpCodes.Call, (NetworkBehaviour nb) => nb.IsServer));
                else
                    worker.Append(worker.Create(OpCodes.Call, (NetworkBehaviour nb) => nb.IsHost));
                worker.Append(worker.Create(OpCodes.Brfalse, afterIf));


                worker.Append(startIf);
                // setSyncVarHookGuard(dirtyBit, true)
                worker.Append(worker.Create(OpCodes.Ldarg_0));
                worker.Append(worker.Create(OpCodes.Ldc_I8, syncVar.DirtyBit));
                worker.Append(worker.Create(OpCodes.Ldc_I4_1));
                worker.Append(worker.Create<NetworkBehaviour>(OpCodes.Call,
                    nb => nb.SetSyncVarHookGuard(default, default)));

                // call hook (oldValue, newValue)
                // Generates: OnValueChanged(oldValue, value)
                WriteCallHookMethodUsingArgument(worker, syncVar.Hook, oldValue);

                // setSyncVarHookGuard(dirtyBit, false)
                worker.Append(worker.Create(OpCodes.Ldarg_0));
                worker.Append(worker.Create(OpCodes.Ldc_I8, syncVar.DirtyBit));
                worker.Append(worker.Create(OpCodes.Ldc_I4_0));
                worker.Append(worker.Create<NetworkBehaviour>(OpCodes.Call, nb => nb.SetSyncVarHookGuard(default, default)));

                worker.Append(afterIf);
            }

            worker.Append(endOfMethod);

            worker.Append(worker.Create(OpCodes.Ret));
        }

        /// <summary>
        /// Writes Load field to IL worker, eg `this.field`
        /// <para>If syncvar is wrapped will use get_Value method instead</para>
        /// </summary>
        /// <param name="worker"></param>
        /// <param name="syncVar"></param>
        private void WriteLoadField(ILProcessor worker, FoundSyncVar syncVar)
        {
            var fd = syncVar.FieldDefinition;
            var originalType = syncVar.OriginalType;

            worker.Append(worker.Create(OpCodes.Ldarg_0));

            if (syncVar.IsWrapped)
            {
                worker.Append(worker.Create(OpCodes.Ldflda, fd.MakeHostGenericIfNeeded()));
                var getter = module.ImportReference(fd.FieldType.Resolve().GetMethod("get_Value"));
                worker.Append(worker.Create(OpCodes.Call, getter));

                // When we use NetworkBehaviors, we normally use a derived class,
                // but the NetworkBehaviorSyncVar returns just NetworkBehavior
                // thus we need to cast it to the user specicfied type
                // otherwise IL2PP fails to build.  see #629
                if (getter.ReturnType.FullName != originalType.FullName)
                {
                    worker.Append(worker.Create(OpCodes.Castclass, originalType));
                }
            }
            else
            {
                worker.Append(worker.Create(OpCodes.Ldfld, fd.MakeHostGenericIfNeeded()));
            }
        }

        /// <summary>
        /// Writes Store field to IL worker, eg `this.field = `
        /// <para>If syncvar is wrapped will use set_Value method instead</para>
        /// </summary>
        /// <param name="worker"></param>
        /// <param name="valueParam"></param>
        /// <param name="syncVar"></param>
        private void WriteStoreField(ILProcessor worker, ParameterDefinition valueParam, FoundSyncVar syncVar)
        {
            var fd = syncVar.FieldDefinition;

            if (syncVar.IsWrapped)
            {
                // there is a wrapper struct, call the setter
                var setter = module.ImportReference(fd.FieldType.Resolve().GetMethod("set_Value"));

                worker.Append(worker.Create(OpCodes.Ldarg_0));
                worker.Append(worker.Create(OpCodes.Ldflda, fd.MakeHostGenericIfNeeded()));
                worker.Append(worker.Create(OpCodes.Ldarg, valueParam));
                worker.Append(worker.Create(OpCodes.Call, setter));
            }
            else
            {
                worker.Append(worker.Create(OpCodes.Ldarg_0));
                worker.Append(worker.Create(OpCodes.Ldarg, valueParam));
                worker.Append(worker.Create(OpCodes.Stfld, fd.MakeHostGenericIfNeeded()));
            }
        }

        private void WriteCallHookMethodUsingArgument(ILProcessor worker, SyncVarHook hook, VariableDefinition oldValue)
        {
            WriteCallHook(worker, hook, oldValue, null);
        }

        private void WriteCallHookMethodUsingField(ILProcessor worker, SyncVarHook hook, VariableDefinition oldValue, FoundSyncVar syncVarField)
        {
            if (syncVarField == null)
            {
                throw new ArgumentNullException(nameof(syncVarField));
            }

            WriteCallHook(worker, hook, oldValue, syncVarField);
        }

        private void WriteCallHook(ILProcessor worker, SyncVarHook hook, VariableDefinition oldValue, FoundSyncVar syncVarField)
        {
            if (hook.Method != null)
                WriteCallHookMethod(worker, hook.Method, hook.ArgCount, oldValue, syncVarField);
            if (hook.Event != null)
                WriteCallHookEvent(worker, hook.Event, hook.ArgCount, oldValue, syncVarField);
        }

        private void WriteCallHookMethod(ILProcessor worker, MethodDefinition hookMethod, int argCount, VariableDefinition oldValue, FoundSyncVar syncVarField)
        {
            WriteStartFunctionCall();

            // write args
            if (argCount >= 2)
                WriteOldValue();
            if (argCount >= 1)
                WriteNewValue();

            WriteEndFunctionCall();


            // *** Local functions used to write OpCodes ***
            // Local functions have access to function variables, no need to pass in args

            void WriteOldValue()
            {
                worker.Append(worker.Create(OpCodes.Ldloc, oldValue));
            }

            void WriteNewValue()
            {
                // write arg1 or this.field
                if (syncVarField == null)
                {
                    worker.Append(worker.Create(OpCodes.Ldarg_1));
                }
                else
                {
                    WriteLoadField(worker, syncVarField);
                }
            }

            // Writes this before method if it is not static
            void WriteStartFunctionCall()
            {
                // dont add this (Ldarg_0) if method is static
                if (!hookMethod.IsStatic)
                {
                    // this before method call
                    // eg this.onValueChanged
                    worker.Append(worker.Create(OpCodes.Ldarg_0));
                }
            }

            // Calls method
            void WriteEndFunctionCall()
            {
                // only use Callvirt when not static
                var OpCall = hookMethod.IsStatic ? OpCodes.Call : OpCodes.Callvirt;
                MethodReference hookMethodReference = hookMethod;

                if (hookMethodReference.DeclaringType.HasGenericParameters)
                {
                    // we need to get the Type<T>.HookMethod so convert it to a generic<T>.
                    var genericType = (GenericInstanceType)hookMethod.DeclaringType.ConvertToGenericIfNeeded();
                    hookMethodReference = hookMethod.MakeHostInstanceGeneric(genericType);
                }

                worker.Append(worker.Create(OpCall, module.ImportReference(hookMethodReference)));
            }
        }

        private void WriteCallHookEvent(ILProcessor worker, EventDefinition @event, int argCount, VariableDefinition oldValue, FoundSyncVar syncVarField)
        {
            // get backing field for event, and sure it is generic instance (eg MyType<T>.myEvent
            var eventField = @event.DeclaringType.GetField(@event.Name).MakeHostGenericIfNeeded();

            // get action type with number of args
            Type actionType;
            switch (argCount)
            {
                case 0:
                    actionType = typeof(Action);
                    break;
                case 1:
                    actionType = typeof(Action<>);
                    break;
                case 2:
                    actionType = typeof(Action<,>);
                    break;
                default:
                    throw new ArgumentException("SyncVarHook can only have 0, 1 or 2 arguments");

            }

            // get Invoke method and make it correct type
            var invoke = module.ImportReference(actionType.GetMethod("Invoke"));
            if (@event.EventType.IsGenericInstance)
                invoke = invoke.MakeHostInstanceGeneric((GenericInstanceType)@event.EventType);

            var nopEvent = worker.Create(OpCodes.Nop);
            var nopEnd = worker.Create(OpCodes.Nop);

            // **null check**
            worker.Append(worker.Create(OpCodes.Ldarg_0));
            worker.Append(worker.Create(OpCodes.Ldfld, eventField));
            // dup so we dont need to load field twice
            worker.Append(worker.Create(OpCodes.Dup));

            // jump to nop if null
            worker.Append(worker.Create(OpCodes.Brtrue, nopEvent));
            // pop because we didn't use field on if it was null
            worker.Append(worker.Create(OpCodes.Pop));
            worker.Append(worker.Create(OpCodes.Br, nopEnd));

            // **call invoke**
            worker.Append(nopEvent);

            if (argCount >= 2)
                WriteOldValue();
            if (argCount >= 1)
                WriteNewValue();

            worker.Append(worker.Create(OpCodes.Call, invoke));

            // after if (event!=null)
            worker.Append(nopEnd);


            // *** Local functions used to write OpCodes ***
            // Local functions have access to function variables, no need to pass in args

            void WriteOldValue()
            {
                worker.Append(worker.Create(OpCodes.Ldloc, oldValue));
            }

            void WriteNewValue()
            {
                // write arg1 or this.field
                if (syncVarField == null)
                {
                    worker.Append(worker.Create(OpCodes.Ldarg_1));
                }
                else
                {
                    WriteLoadField(worker, syncVarField);
                }
            }
        }

        private void GenerateSerialization()
        {
            Weaver.DebugLog(behaviour.TypeDefinition, "GenerateSerialization");

            // dont create if there are no syncvars
            if (behaviour.SyncVars.Count == 0)
                return;

            var helper = new SerializeHelper(module, behaviour);

            // Dont create method if users has manually overridden it
            if (helper.HasManualOverride())
                return;

            helper.AddMethod();

            helper.WriteIfInitial(() =>
            {
                foreach (var syncVar in behaviour.SyncVars)
                {
                    WriteFromField(helper.Worker, helper.WriterParameter, syncVar);
                }
            });

            // write dirty bits before the data fields
            helper.WriteDirtyBitMask();

            // generate a writer call for any dirty variable in this class

            // start at number of syncvars in parent
            foreach (var syncVar in behaviour.SyncVars)
            {
                // dont need to write field here if syncvar is InitialOnly
                if (syncVar.InitialOnly) { continue; }

                helper.WriteIfSyncVarDirty(syncVar, () =>
                {
                    // Generates a call to the writer for that field
                    WriteFromField(helper.Worker, helper.WriterParameter, syncVar);
                });
            }

            // generate: return dirtyLocal
            helper.WriteReturnDirty();
        }

        private void WriteFromField(ILProcessor worker, ParameterDefinition writerParameter, FoundSyncVar syncVar)
        {
            if (!syncVar.HasProcessed) return;

            var fieldRef = syncVar.FieldDefinition.MakeHostGenericIfNeeded();
            syncVar.ValueSerializer.AppendWriteField(module, worker, writerParameter, null, fieldRef);
        }

        private void GenerateDeserialization()
        {
            Weaver.DebugLog(behaviour.TypeDefinition, "GenerateDeSerialization");

            // dont create if there are no syncvars
            if (behaviour.SyncVars.Count == 0)
                return;

            var helper = new DeserializeHelper(module, behaviour);

            // Dont create method if users has manually overridden it
            if (helper.HasManualOverride())
                return;

            helper.AddMethod();

            helper.WriteIfInitial(() =>
            {
                // For ititial spawn READ all values first, then invoke any hooks
                var oldValues = new VariableDefinition[behaviour.SyncVars.Count];
                for (var i = 0; i < behaviour.SyncVars.Count; i++)
                {
                    var syncVar = behaviour.SyncVars[i];
                    // StartHook create old value local variable,
                    oldValues[i] = StartHook(helper.Worker, helper.Method, syncVar, syncVar.OriginalType);
                    ReadToField(helper.Worker, helper.ReaderParameter, syncVar);
                }
                for (var i = 0; i < behaviour.SyncVars.Count; i++)
                {
                    var syncVar = behaviour.SyncVars[i];
                    EndHook(helper.Worker, syncVar, syncVar.OriginalType, oldValues[i]);
                }
            });

            helper.ReadDirtyBitMask();

            // conditionally read each syncvar
            foreach (var syncVar in behaviour.SyncVars)
            {
                // dont need to write field here if syncvar is InitialOnly
                if (syncVar.InitialOnly) { continue; }

                helper.WriteIfSyncVarDirty(syncVar, () =>
                {
                    var oldValue = StartHook(helper.Worker, helper.Method, syncVar, syncVar.OriginalType);
                    // read value and store in syncvar BEFORE calling the hook
                    ReadToField(helper.Worker, helper.ReaderParameter, syncVar);
                    EndHook(helper.Worker, syncVar, syncVar.OriginalType, oldValue);
                });
            }

            helper.Worker.Emit(OpCodes.Ret);
        }

        /// <summary>
        /// If syncvar has a hook method, this will create a local variable with the old value of the field
        /// <para>should be called before storing the new value in the field</para>
        /// </summary>
        /// <param name="worker"></param>
        /// <param name="deserialize"></param>
        /// <param name="syncVar"></param>
        /// <param name="originalType"></param>
        /// <returns></returns>
        private VariableDefinition StartHook(ILProcessor worker, MethodDefinition deserialize, FoundSyncVar syncVar, TypeReference originalType)
        {
            /*
             Generates code like:
                // for hook
                int oldValue = a
                Networka = reader.ReadPackedInt32()
                if (!SyncVarEqual(oldValue, ref a))
                    OnSetA(oldValue, Networka)
             */

            // Store old value in local variable, we need it for Hook
            // T oldValue = value
            VariableDefinition oldValue = null;
            if (syncVar.HasHook)
            {
                oldValue = deserialize.AddLocal(originalType);
                WriteLoadField(worker, syncVar);

                worker.Append(worker.Create(OpCodes.Stloc, oldValue));
            }

            return oldValue;
        }

        /// <summary>
        /// If syncvar has a hook method, this will invoke the hook method if it is changed with the old and new values
        /// </summary>
        /// <param name="worker"></param>
        /// <param name="syncVar"></param>
        /// <param name="originalType"></param>
        /// <param name="oldValue"></param>
        private void EndHook(ILProcessor worker, FoundSyncVar syncVar, TypeReference originalType, VariableDefinition oldValue)
        {
            if (syncVar.HasHook)
            {
                // call hook
                // but only if SyncVar changed. otherwise a client would
                // get hook calls for all initial values, even if they
                // didn't change from the default values on the client.

                // Generates: if (!SyncVarEqual)
                var endHookInvoke = worker.Create(OpCodes.Nop);

                // if not invoke on server, then we need to add a if (!isServer) check
                // this is because onDeserialize can be called on server when syncdirection is from Owner
                if (!syncVar.InvokeHookOnServer)
                {
                    worker.Append(worker.Create(OpCodes.Ldarg_0));
                    worker.Append(worker.Create(OpCodes.Call, (NetworkBehaviour nb) => nb.IsServer));
                    // if true, go to start of if
                    // this will act as an OR for the IsServer check
                    worker.Append(worker.Create(OpCodes.Brtrue, endHookInvoke));
                }

                // 'this.' for 'this.SyncVarEqual'
                worker.Append(worker.Create(OpCodes.Ldarg_0));
                // 'oldValue'
                worker.Append(worker.Create(OpCodes.Ldloc, oldValue));
                // 'newValue'
                WriteLoadField(worker, syncVar);
                // call the function
                var syncVarEqual = module.ImportReference<NetworkBehaviour>(nb => nb.SyncVarEqual<object>(default, default));
                var syncVarEqualGm = new GenericInstanceMethod(syncVarEqual.GetElementMethod());
                syncVarEqualGm.GenericArguments.Add(originalType);
                worker.Append(worker.Create(OpCodes.Call, syncVarEqualGm));
                worker.Append(worker.Create(OpCodes.Brtrue, endHookInvoke));

                // call the hook
                // Generates: OnValueChanged(oldValue, this.syncVar)
                WriteCallHookMethodUsingField(worker, syncVar.Hook, oldValue, syncVar);

                // Generates: end if (!SyncVarEqual)
                worker.Append(endHookInvoke);
            }

        }

        private void ReadToField(ILProcessor worker, ParameterDefinition readerParameter, FoundSyncVar syncVar)
        {
            if (!syncVar.HasProcessed) return;

            // load this
            // read value
            // store to field

            worker.Append(worker.Create(OpCodes.Ldarg_0));

            syncVar.ValueSerializer.AppendRead(module, worker, readerParameter, syncVar.FieldDefinition.FieldType);

            worker.Append(worker.Create(OpCodes.Stfld, syncVar.FieldDefinition.MakeHostGenericIfNeeded()));
        }
    }
}
