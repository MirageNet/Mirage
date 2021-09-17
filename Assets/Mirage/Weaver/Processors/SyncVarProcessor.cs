using System;
using Mirage.Weaver.NetworkBehaviours;
using Mirage.Weaver.Serialization;
using Mirage.Weaver.SyncVars;
using Mono.Cecil;
using Mono.Cecil.Cil;
using FieldAttributes = Mono.Cecil.FieldAttributes;
using MethodAttributes = Mono.Cecil.MethodAttributes;
using PropertyAttributes = Mono.Cecil.PropertyAttributes;

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
        private readonly PropertySiteProcessor propertySiteProcessor;

        private FoundNetworkBehaviour behaviour;

        public SyncVarProcessor(ModuleDefinition module, Readers readers, Writers writers, PropertySiteProcessor propertySiteProcessor)
        {
            this.module = module;
            this.readers = readers;
            this.writers = writers;
            this.propertySiteProcessor = propertySiteProcessor;
        }

        public void ProcessSyncVars(TypeDefinition td, IWeaverLogger logger)
        {
            behaviour = new FoundNetworkBehaviour(module, td);
            // the mapping of dirtybits to sync-vars is implicit in the order of the fields here. this order is recorded in m_replacementProperties.
            // start assigning syncvars at the place the base class stopped, if any

            // get numbers of syncvars in parent class, it will be added to syncvars in this class for total
            behaviour.GetSyncVarCountFromBase();

            // find syncvars
            // use ToArray to create copy, ProcessSyncVar might add new fields
            foreach (FieldDefinition fd in td.Fields.ToArray())
            {
                // try/catch for each field, and log once
                // we dont want to spam multiple logs for a single field
                try
                {
                    if (IsValidSyncVar(fd))
                    {
                        FoundSyncVar syncVar = behaviour.AddSyncVar(fd);
                        ProcessSyncVar(syncVar);
                        syncVar.HasProcessed = true;
                    }
                }
                catch (ValueSerializerException e)
                {
                    logger.Error(e.Message, fd);
                }
                catch (SyncVarException e)
                {
                    logger.Error(e);
                }
                catch (SerializeFunctionException e)
                {
                    // use field as member referecne
                    logger.Error(e.Message, fd);
                }
            }

            behaviour.SetSyncVarCount();

            GenerateSerialization();
            GenerateDeserialization();
        }

        bool IsValidSyncVar(FieldDefinition field)
        {
            if (!field.HasCustomAttribute<SyncVarAttribute>())
            {
                return false;
            }

            if (field.FieldType.IsGenericParameter)
            {
                throw new SyncVarException($"{field.Name} cannot be synced since it's a generic parameter", field);
            }

            if ((field.Attributes & FieldAttributes.Static) != 0)
            {
                throw new SyncVarException($"{field.Name} cannot be static", field);
            }

            if (field.FieldType.IsArray)
            {
                // todo should arrays really be blocked?
                throw new SyncVarException($"{field.Name} has invalid type. Use SyncLists instead of arrays", field);
            }

            if (SyncObjectProcessor.ImplementsSyncObject(field.FieldType))
            {
                throw new SyncVarException($"{field.Name} has [SyncVar] attribute. ISyncObject should not be marked with SyncVar", field);
            }

            return true;
        }

        void ProcessSyncVar(FoundSyncVar syncVar)
        {
            // process attributes first before creating setting, otherwise it wont know about hook
            syncVar.SetWrapType();
            syncVar.ProcessAttributes(writers, readers);

            FieldDefinition fd = syncVar.FieldDefinition;

            string originalName = fd.Name;
            Weaver.DebugLog(fd.DeclaringType, $"Sync Var {fd.Name} {fd.FieldType}");


            MethodDefinition get = GenerateSyncVarGetter(syncVar);
            MethodDefinition set = GenerateSyncVarSetter(syncVar);

            //NOTE: is property even needed? Could just use a setter function?
            //create the property
            var propertyDefinition = new PropertyDefinition("Network" + originalName, PropertyAttributes.None, syncVar.OriginalType)
            {
                GetMethod = get,
                SetMethod = set
            };

            propertyDefinition.DeclaringType = fd.DeclaringType;
            //add the methods and property to the type.
            fd.DeclaringType.Properties.Add(propertyDefinition);
            propertySiteProcessor.Setters[fd] = set;

            if (syncVar.IsWrapped)
            {
                propertySiteProcessor.Getters[fd] = get;
            }
        }

        MethodDefinition GenerateSyncVarGetter(FoundSyncVar syncVar)
        {
            FieldDefinition fd = syncVar.FieldDefinition;
            TypeReference originalType = syncVar.OriginalType;
            string originalName = syncVar.OriginalName;

            //Create the get method
            MethodDefinition get = fd.DeclaringType.AddMethod(
                    "get_Network" + originalName, MethodAttributes.Public |
                    MethodAttributes.SpecialName |
                    MethodAttributes.HideBySig,
                    originalType);

            ILProcessor worker = get.Body.GetILProcessor();
            WriteLoadField(worker, syncVar);

            worker.Append(worker.Create(OpCodes.Ret));

            get.SemanticsAttributes = MethodSemanticsAttributes.Getter;

            return get;
        }

        MethodDefinition GenerateSyncVarSetter(FoundSyncVar syncVar)
        {
            FieldDefinition fd = syncVar.FieldDefinition;
            TypeReference originalType = syncVar.OriginalType;
            string originalName = syncVar.OriginalName;

            //Create the set method
            MethodDefinition set = fd.DeclaringType.AddMethod("set_Network" + originalName, MethodAttributes.Public |
                    MethodAttributes.SpecialName |
                    MethodAttributes.HideBySig);
            ParameterDefinition valueParam = set.AddParam(originalType, "value");
            set.SemanticsAttributes = MethodSemanticsAttributes.Setter;

            ILProcessor worker = set.Body.GetILProcessor();

            // if (!SyncVarEqual(value, ref playerData))
            Instruction endOfMethod = worker.Create(OpCodes.Nop);

            // this
            worker.Append(worker.Create(OpCodes.Ldarg_0));
            // new value to set
            worker.Append(worker.Create(OpCodes.Ldarg, valueParam));
            // reference to field to set
            // make generic version of SetSyncVar with field type
            WriteLoadField(worker, syncVar);

            MethodReference syncVarEqual = module.ImportReference<NetworkBehaviour>(nb => nb.SyncVarEqual<object>(default, default));
            var syncVarEqualGm = new GenericInstanceMethod(syncVarEqual.GetElementMethod());
            syncVarEqualGm.GenericArguments.Add(originalType);
            worker.Append(worker.Create(OpCodes.Call, syncVarEqualGm));

            worker.Append(worker.Create(OpCodes.Brtrue, endOfMethod));

            // T oldValue = value
            VariableDefinition oldValue = set.AddLocal(originalType);
            WriteLoadField(worker, syncVar);
            worker.Append(worker.Create(OpCodes.Stloc, oldValue));

            // fieldValue = value
            WriteStoreField(worker, valueParam, syncVar);

            // this.SetDirtyBit(dirtyBit)
            worker.Append(worker.Create(OpCodes.Ldarg_0));
            worker.Append(worker.Create(OpCodes.Ldc_I8, syncVar.DirtyBit));
            worker.Append(worker.Create<NetworkBehaviour>(OpCodes.Call, nb => nb.SetDirtyBit(default)));

            if (syncVar.HasHookMethod)
            {
                //if (base.isLocalClient && !getSyncVarHookGuard(dirtyBit))
                Instruction label = worker.Create(OpCodes.Nop);
                worker.Append(worker.Create(OpCodes.Ldarg_0));
                worker.Append(worker.Create(OpCodes.Call, (NetworkBehaviour nb) => nb.IsLocalClient));
                worker.Append(worker.Create(OpCodes.Brfalse, label));
                worker.Append(worker.Create(OpCodes.Ldarg_0));
                worker.Append(worker.Create(OpCodes.Ldc_I8, syncVar.DirtyBit));
                worker.Append(worker.Create<NetworkBehaviour>(OpCodes.Call, nb => nb.GetSyncVarHookGuard(default)));
                worker.Append(worker.Create(OpCodes.Brtrue, label));

                // setSyncVarHookGuard(dirtyBit, true)
                worker.Append(worker.Create(OpCodes.Ldarg_0));
                worker.Append(worker.Create(OpCodes.Ldc_I8, syncVar.DirtyBit));
                worker.Append(worker.Create(OpCodes.Ldc_I4_1));
                worker.Append(worker.Create<NetworkBehaviour>(OpCodes.Call, nb => nb.SetSyncVarHookGuard(default, default)));

                // call hook (oldValue, newValue)
                // Generates: OnValueChanged(oldValue, value)
                WriteCallHookMethodUsingArgument(worker, syncVar.HookMethod, oldValue);

                // setSyncVarHookGuard(dirtyBit, false)
                worker.Append(worker.Create(OpCodes.Ldarg_0));
                worker.Append(worker.Create(OpCodes.Ldc_I8, syncVar.DirtyBit));
                worker.Append(worker.Create(OpCodes.Ldc_I4_0));
                worker.Append(worker.Create<NetworkBehaviour>(OpCodes.Call, nb => nb.SetSyncVarHookGuard(default, default)));

                worker.Append(label);
            }

            worker.Append(endOfMethod);

            worker.Append(worker.Create(OpCodes.Ret));

            return set;
        }

        /// <summary>
        /// Writes Load field to IL worker, eg `this.field`
        /// <para>If syncvar is wrapped will use get_Value method instead</para>
        /// </summary>
        /// <param name="worker"></param>
        /// <param name="syncVar"></param>
        void WriteLoadField(ILProcessor worker, FoundSyncVar syncVar)
        {
            FieldDefinition fd = syncVar.FieldDefinition;
            TypeReference originalType = syncVar.OriginalType;

            worker.Append(worker.Create(OpCodes.Ldarg_0));

            if (syncVar.IsWrapped)
            {
                worker.Append(worker.Create(OpCodes.Ldflda, fd.MakeHostGenericIfNeeded()));
                MethodReference getter = module.ImportReference(fd.FieldType.Resolve().GetMethod("get_Value"));
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
        void WriteStoreField(ILProcessor worker, ParameterDefinition valueParam, FoundSyncVar syncVar)
        {
            FieldDefinition fd = syncVar.FieldDefinition;

            if (syncVar.IsWrapped)
            {
                // there is a wrapper struct, call the setter
                MethodReference setter = module.ImportReference(fd.FieldType.Resolve().GetMethod("set_Value"));

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


        void WriteCallHookMethodUsingArgument(ILProcessor worker, MethodDefinition hookMethod, VariableDefinition oldValue)
        {
            WriteCallHookMethod(worker, hookMethod, oldValue, null);
        }

        void WriteCallHookMethodUsingField(ILProcessor worker, MethodDefinition hookMethod, VariableDefinition oldValue, FoundSyncVar syncVarField)
        {
            if (syncVarField == null)
            {
                throw new ArgumentNullException(nameof(syncVarField));
            }

            WriteCallHookMethod(worker, hookMethod, oldValue, syncVarField);
        }

        void WriteCallHookMethod(ILProcessor worker, MethodDefinition hookMethod, VariableDefinition oldValue, FoundSyncVar syncVarField)
        {
            WriteStartFunctionCall();

            // write args
            WriteOldValue();
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
                OpCode OpCall = hookMethod.IsStatic ? OpCodes.Call : OpCodes.Callvirt;
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

        void GenerateSerialization()
        {
            Weaver.DebugLog(behaviour.TypeDefinition, "  GenerateSerialization");

            // Dont create method if users has manually overridden it
            if (behaviour.HasManualSerializeOverride())
                return;

            // dont create if there are no syncvars
            if (behaviour.SyncVars.Count == 0)
                return;

            var helper = new SerializeHelper(module, behaviour);
            ILProcessor worker = helper.AddMethod();

            helper.AddLocals();
            helper.WriteBaseCall();

            helper.WriteIfInitial(() =>
            {
                foreach (FoundSyncVar syncVar in behaviour.SyncVars)
                {
                    WriteFromField(worker, helper.WriterParameter, syncVar);
                }
            });

            // write dirty bits before the data fields
            helper.WriteDirtyBitMask();

            // generate a writer call for any dirty variable in this class

            // start at number of syncvars in parent
            foreach (FoundSyncVar syncVar in behaviour.SyncVars)
            {
                helper.WriteIfSyncVarDirty(syncVar, () =>
                {
                    // Generates a call to the writer for that field
                    WriteFromField(worker, helper.WriterParameter, syncVar);
                });
            }

            // generate: return dirtyLocal
            helper.WriteReturnDirty();
        }

        void WriteFromField(ILProcessor worker, ParameterDefinition writerParameter, FoundSyncVar syncVar)
        {
            if (!syncVar.HasProcessed) return;

            syncVar.ValueSerializer.AppendWriteField(module, worker, writerParameter, null, syncVar.FieldDefinition);
        }


        void GenerateDeserialization()
        {
            Weaver.DebugLog(behaviour.TypeDefinition, "  GenerateDeSerialization");

            // Dont create method if users has manually overridden it
            if (behaviour.HasManualDeserializeOverride())
                return;

            // dont create if there are no syncvars
            if (behaviour.SyncVars.Count == 0)
                return;


            var helper = new DeserializeHelper(module, behaviour);
            ILProcessor worker = helper.AddMethod();

            helper.AddLocals();
            helper.WriteBaseCall();

            helper.WriteIfInitial(() =>
            {
                foreach (FoundSyncVar syncVar in behaviour.SyncVars)
                {
                    DeserializeField(worker, helper.Method, helper.ReaderParameter, syncVar);
                }
            });

            helper.ReadDirtyBitMask();

            // conditionally read each syncvar
            foreach (FoundSyncVar syncVar in behaviour.SyncVars)
            {
                helper.WriteIfSyncVarDirty(syncVar, () =>
                {
                    DeserializeField(worker, helper.Method, helper.ReaderParameter, syncVar);
                });
            }

            worker.Append(worker.Create(OpCodes.Ret));
        }

        /// <summary>
        /// [SyncVar] int/float/struct/etc.?
        /// </summary>
        /// <param name="fd"></param>
        /// <param name="worker"></param>
        /// <param name="deserialize"></param>
        /// <param name="initialState"></param>
        /// <param name="hookResult"></param>
        void DeserializeField(ILProcessor worker, MethodDefinition deserialize, ParameterDefinition readerParameter, FoundSyncVar syncVar)
        {
            TypeReference originalType = syncVar.OriginalType;

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
            if (syncVar.HasHookMethod)
            {
                oldValue = deserialize.AddLocal(originalType);
                WriteLoadField(worker, syncVar);

                worker.Append(worker.Create(OpCodes.Stloc, oldValue));
            }

            // read value and store in syncvar BEFORE calling the hook
            ReadToField(worker, readerParameter, syncVar);

            if (syncVar.HasHookMethod)
            {
                // call hook
                // but only if SyncVar changed. otherwise a client would
                // get hook calls for all initial values, even if they
                // didn't change from the default values on the client.
                // see also: https://github.com/vis2k/Mirror/issues/1278

                // Generates: if (!SyncVarEqual)
                Instruction syncVarEqualLabel = worker.Create(OpCodes.Nop);

                // 'this.' for 'this.SyncVarEqual'
                worker.Append(worker.Create(OpCodes.Ldarg_0));
                // 'oldValue'
                worker.Append(worker.Create(OpCodes.Ldloc, oldValue));
                // 'newValue'
                WriteLoadField(worker, syncVar);
                // call the function
                MethodReference syncVarEqual = module.ImportReference<NetworkBehaviour>(nb => nb.SyncVarEqual<object>(default, default));
                var syncVarEqualGm = new GenericInstanceMethod(syncVarEqual.GetElementMethod());
                syncVarEqualGm.GenericArguments.Add(originalType);
                worker.Append(worker.Create(OpCodes.Call, syncVarEqualGm));
                worker.Append(worker.Create(OpCodes.Brtrue, syncVarEqualLabel));

                // call the hook
                // Generates: OnValueChanged(oldValue, this.syncVar)
                WriteCallHookMethodUsingField(worker, syncVar.HookMethod, oldValue, syncVar);

                // Generates: end if (!SyncVarEqual)
                worker.Append(syncVarEqualLabel);
            }
        }

        void ReadToField(ILProcessor worker, ParameterDefinition readerParameter, FoundSyncVar syncVar)
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
