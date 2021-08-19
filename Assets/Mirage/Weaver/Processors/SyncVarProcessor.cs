using System.Collections.Generic;
using System.Linq;
using Mirage.Serialization;
using Mono.Cecil;
using Mono.Cecil.Cil;
using UnityEngine;
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
        private class FoundSyncVar
        {
            public readonly FieldDefinition fieldDefinition;

            public FoundSyncVar(FieldDefinition fieldDefinition)
            {
                this.fieldDefinition = fieldDefinition;
            }
        }
        private readonly List<FoundSyncVar> syncVars = new List<FoundSyncVar>();

        // store the unwrapped types for every field
        private readonly Dictionary<FieldDefinition, TypeReference> originalTypes = new Dictionary<FieldDefinition, TypeReference>();
        private readonly ModuleDefinition module;
        private readonly Readers readers;
        private readonly Writers writers;
        private readonly PropertySiteProcessor propertySiteProcessor;
        private readonly IWeaverLogger logger;

        // ulong = 64 bytes
        const int SyncVarLimit = 64;
        private const string SyncVarCountField = "SYNC_VAR_COUNT";

        static string HookParameterMessage(string hookName, TypeReference ValueType)
            => string.Format("void {0}({1} oldValue, {1} newValue)", hookName, ValueType);

        public SyncVarProcessor(ModuleDefinition module, Readers readers, Writers writers, PropertySiteProcessor propertySiteProcessor, IWeaverLogger logger)
        {
            this.module = module;
            this.readers = readers;
            this.writers = writers;
            this.propertySiteProcessor = propertySiteProcessor;
            this.logger = logger;
        }

        public void ProcessSyncVars(TypeDefinition td)
        {
            // the mapping of dirtybits to sync-vars is implicit in the order of the fields here. this order is recorded in m_replacementProperties.
            // start assigning syncvars at the place the base class stopped, if any

            // get numbers of syncvars in parent class, it will be added to syncvars in this class for total
            int baseSyncVarCount = td.BaseType.Resolve().GetConst<int>(SyncVarCountField);

            // find syncvars
            foreach (FieldDefinition fd in td.Fields)
            {
                if (IsValidSyncVar(fd))
                {
                    syncVars.Add(new FoundSyncVar(fd));
                }
            }

            if ((baseSyncVarCount + syncVars.Count) >= SyncVarLimit)
            {
                logger.Error($"{td.Name} has too many SyncVars. Consider refactoring your class into multiple components", td);
            }

            td.SetConst(SyncVarCountField, baseSyncVarCount + syncVars.Count);

            for (int i = 0; i < syncVars.Count; i++)
            {
                ProcessSyncVar(syncVars[i].fieldDefinition, 1L << (baseSyncVarCount + i));
            }

            GenerateSerialization(td);
            GenerateDeSerialization(td);
        }

        bool IsValidSyncVar(FieldDefinition field)
        {
            if (!field.HasCustomAttribute<SyncVarAttribute>())
            {
                return false;
            }

            if (field.FieldType.IsGenericParameter)
            {
                logger.Error($"{field.Name} cannot be synced since it's a generic parameter", field);
                return false;
            }

            if ((field.Attributes & FieldAttributes.Static) != 0)
            {
                logger.Error($"{field.Name} cannot be static", field);
                return false;
            }

            if (field.FieldType.IsArray)
            {
                // todo should arrays really be blocked?
                logger.Error($"{field.Name} has invalid type. Use SyncLists instead of arrays", field);
                return false;
            }

            if (SyncObjectProcessor.ImplementsSyncObject(field.FieldType))
            {
                logger.Warning($"{field.Name} has [SyncVar] attribute. ISyncObject should not be marked with SyncVar", field);
                return false;
            }

            return true;
        }

        void ProcessSyncVar(FieldDefinition fd, long dirtyBit)
        {
            string originalName = fd.Name;
            Weaver.DebugLog(fd.DeclaringType, $"Sync Var {fd.Name} {fd.FieldType}");

            TypeReference originalType = fd.FieldType;
            fd.FieldType = WrapType(fd);

            MethodDefinition get = GenerateSyncVarGetter(fd, originalName, originalType);
            MethodDefinition set = GenerateSyncVarSetter(fd, originalName, dirtyBit, originalType);

            //NOTE: is property even needed? Could just use a setter function?
            //create the property
            var propertyDefinition = new PropertyDefinition("Network" + originalName, PropertyAttributes.None, originalType)
            {
                GetMethod = get,
                SetMethod = set
            };

            propertyDefinition.DeclaringType = fd.DeclaringType;
            //add the methods and property to the type.
            fd.DeclaringType.Properties.Add(propertyDefinition);
            propertySiteProcessor.Setters[fd] = set;

            if (IsWrapped(fd.FieldType))
            {
                propertySiteProcessor.Getters[fd] = get;
            }
        }

        MethodDefinition GenerateSyncVarGetter(FieldDefinition fd, string originalName, TypeReference originalType)
        {
            //Create the get method
            MethodDefinition get = fd.DeclaringType.AddMethod(
                    "get_Network" + originalName, MethodAttributes.Public |
                    MethodAttributes.SpecialName |
                    MethodAttributes.HideBySig,
                    originalType);

            ILProcessor worker = get.Body.GetILProcessor();
            LoadField(fd, originalType, worker);

            worker.Append(worker.Create(OpCodes.Ret));

            get.SemanticsAttributes = MethodSemanticsAttributes.Getter;

            return get;
        }

        MethodDefinition GenerateSyncVarSetter(FieldDefinition fd, string originalName, long dirtyBit, TypeReference originalType)
        {

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
            LoadField(fd, originalType, worker);

            MethodReference syncVarEqual = module.ImportReference<NetworkBehaviour>(nb => nb.SyncVarEqual<object>(default, default));
            var syncVarEqualGm = new GenericInstanceMethod(syncVarEqual.GetElementMethod());
            syncVarEqualGm.GenericArguments.Add(originalType);
            worker.Append(worker.Create(OpCodes.Call, syncVarEqualGm));

            worker.Append(worker.Create(OpCodes.Brtrue, endOfMethod));

            // T oldValue = value;
            VariableDefinition oldValue = set.AddLocal(originalType);
            LoadField(fd, originalType, worker);
            worker.Append(worker.Create(OpCodes.Stloc, oldValue));

            // fieldValue = value;
            StoreField(fd, valueParam, worker);

            // this.SetDirtyBit(dirtyBit)
            worker.Append(worker.Create(OpCodes.Ldarg_0));
            worker.Append(worker.Create(OpCodes.Ldc_I8, dirtyBit));
            worker.Append(worker.Create<NetworkBehaviour>(OpCodes.Call, nb => nb.SetDirtyBit(default)));

            MethodDefinition hookMethod = GetHookMethod(fd, originalType);

            if (hookMethod != null)
            {
                //if (base.isLocalClient && !getSyncVarHookGuard(dirtyBit))
                Instruction label = worker.Create(OpCodes.Nop);
                worker.Append(worker.Create(OpCodes.Ldarg_0));
                worker.Append(worker.Create(OpCodes.Call, (NetworkBehaviour nb) => nb.IsLocalClient));
                worker.Append(worker.Create(OpCodes.Brfalse, label));
                worker.Append(worker.Create(OpCodes.Ldarg_0));
                worker.Append(worker.Create(OpCodes.Ldc_I8, dirtyBit));
                worker.Append(worker.Create<NetworkBehaviour>(OpCodes.Call, nb => nb.GetSyncVarHookGuard(default)));
                worker.Append(worker.Create(OpCodes.Brtrue, label));

                // setSyncVarHookGuard(dirtyBit, true);
                worker.Append(worker.Create(OpCodes.Ldarg_0));
                worker.Append(worker.Create(OpCodes.Ldc_I8, dirtyBit));
                worker.Append(worker.Create(OpCodes.Ldc_I4_1));
                worker.Append(worker.Create<NetworkBehaviour>(OpCodes.Call, nb => nb.SetSyncVarHookGuard(default, default)));

                // call hook (oldValue, newValue)
                // Generates: OnValueChanged(oldValue, value);
                WriteCallHookMethodUsingArgument(worker, hookMethod, oldValue);

                // setSyncVarHookGuard(dirtyBit, false);
                worker.Append(worker.Create(OpCodes.Ldarg_0));
                worker.Append(worker.Create(OpCodes.Ldc_I8, dirtyBit));
                worker.Append(worker.Create(OpCodes.Ldc_I4_0));
                worker.Append(worker.Create<NetworkBehaviour>(OpCodes.Call, nb => nb.SetSyncVarHookGuard(default, default)));

                worker.Append(label);
            }

            worker.Append(endOfMethod);

            worker.Append(worker.Create(OpCodes.Ret));

            return set;
        }

        private void LoadField(FieldDefinition fd, TypeReference originalType, ILProcessor worker)
        {
            worker.Append(worker.Create(OpCodes.Ldarg_0));

            if (IsWrapped(fd.FieldType))
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

        private void StoreField(FieldDefinition fd, ParameterDefinition valueParam, ILProcessor worker)
        {
            if (IsWrapped(fd.FieldType))
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

        // Get hook method if any
        MethodDefinition GetHookMethod(FieldDefinition syncVar, TypeReference originalType)
        {
            CustomAttribute syncVarAttr = syncVar.GetCustomAttribute<SyncVarAttribute>();

            if (syncVarAttr == null)
                return null;

            string hookFunctionName = syncVarAttr.GetField<string>("hook", null);

            if (hookFunctionName == null)
                return null;

            return FindHookMethod(syncVar, hookFunctionName, originalType);
        }

        MethodDefinition FindHookMethod(FieldDefinition syncVar, string hookFunctionName, TypeReference originalType)
        {
            List<MethodDefinition> methods = syncVar.DeclaringType.GetMethods(hookFunctionName);

            var methodsWith2Param = new List<MethodDefinition>(methods.Where(m => m.Parameters.Count == 2));

            if (methodsWith2Param.Count == 0)
            {
                logger.Error($"Could not find hook for '{syncVar.Name}', hook name '{hookFunctionName}'. " +
                    $"Method signature should be {HookParameterMessage(hookFunctionName, originalType)}",
                    syncVar);

                return null;
            }

            foreach (MethodDefinition method in methodsWith2Param)
            {
                if (MatchesParameters(method, originalType))
                {
                    return method;
                }
            }

            logger.Error($"Wrong type for Parameter in hook for '{syncVar.Name}', hook name '{hookFunctionName}'. " +
                     $"Method signature should be {HookParameterMessage(hookFunctionName, originalType)}",
                   syncVar);

            return null;
        }

        static bool MatchesParameters(MethodDefinition method, TypeReference originalType)
        {
            // matches void onValueChange(T oldValue, T newValue)
            return method.Parameters[0].ParameterType.FullName == originalType.FullName &&
                   method.Parameters[1].ParameterType.FullName == originalType.FullName;
        }

        private static bool IsWrapped(TypeReference typeReference)
        {
            return typeReference.Is<NetworkIdentitySyncvar>() ||
                typeReference.Is<GameObjectSyncvar>() ||
                typeReference.Is<NetworkBehaviorSyncvar>();
        }

        private TypeReference WrapType(FieldDefinition syncvar)
        {
            TypeReference typeReference = syncvar.FieldType;

            originalTypes[syncvar] = typeReference;
            if (typeReference.Is<NetworkIdentity>())
            {
                // change the type of the field to a wrapper NetworkIDentitySyncvar
                return module.ImportReference<NetworkIdentitySyncvar>();
            }
            if (typeReference.Is<GameObject>())
                return module.ImportReference<GameObjectSyncvar>();

            if (typeReference.Resolve().IsDerivedFrom<NetworkBehaviour>())
                return module.ImportReference<NetworkBehaviorSyncvar>();

            return typeReference;
        }

        private TypeReference UnwrapType(FieldDefinition syncvar)
        {
            return originalTypes[syncvar];
        }

        void WriteCallHookMethodUsingArgument(ILProcessor worker, MethodDefinition hookMethod, VariableDefinition oldValue)
        {
            WriteCallHookMethod(worker, hookMethod, oldValue, null);
        }

        void WriteCallHookMethodUsingField(ILProcessor worker, MethodDefinition hookMethod, VariableDefinition oldValue, FieldDefinition newValue)
        {
            if (newValue == null)
            {
                logger.Error("NewValue field was null when writing SyncVar hook");
            }

            WriteCallHookMethod(worker, hookMethod, oldValue, newValue);
        }

        void WriteCallHookMethod(ILProcessor worker, MethodDefinition hookMethod, VariableDefinition oldValue, FieldDefinition newValue)
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
                if (newValue == null)
                {
                    worker.Append(worker.Create(OpCodes.Ldarg_1));
                }
                else
                {
                    LoadField(newValue, oldValue.VariableType, worker);
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
                OpCode opcode = hookMethod.IsStatic ? OpCodes.Call : OpCodes.Callvirt;
                MethodReference hookMethodReference = hookMethod;

                if (hookMethodReference.DeclaringType.HasGenericParameters)
                {
                    // we need to get the Type<T>.HookMethod so convert it to a generic<T>.
                    var genericType = (GenericInstanceType)hookMethod.DeclaringType.ConvertToGenericIfNeeded();
                    hookMethodReference = hookMethod.MakeHostInstanceGeneric(genericType);
                }

                worker.Append(worker.Create(opcode, module.ImportReference(hookMethodReference)));
            }
        }

        void GenerateSerialization(TypeDefinition netBehaviourSubclass)
        {
            Weaver.DebugLog(netBehaviourSubclass, "  GenerateSerialization");

            const string SerializeMethodName = nameof(NetworkBehaviour.SerializeSyncVars);
            if (netBehaviourSubclass.GetMethod(SerializeMethodName) != null)
                return;

            if (syncVars.Count == 0)
            {
                // no synvars,  no need for custom OnSerialize
                return;
            }

            MethodDefinition serialize = netBehaviourSubclass.AddMethod(SerializeMethodName,
                    MethodAttributes.Public | MethodAttributes.Virtual | MethodAttributes.HideBySig,
                    module.ImportReference<bool>());

            ParameterDefinition writerParameter = serialize.AddParam<NetworkWriter>("writer");
            ParameterDefinition initializeParameter = serialize.AddParam<bool>("initialize");
            ILProcessor worker = serialize.Body.GetILProcessor();

            serialize.Body.InitLocals = true;

            // loc_0,  this local variable is to determine if any variable was dirty
            VariableDefinition dirtyLocal = serialize.AddLocal<bool>();

            MethodReference baseSerialize = netBehaviourSubclass.BaseType.GetMethodInBaseType(SerializeMethodName);
            if (baseSerialize != null)
            {
                // base
                worker.Append(worker.Create(OpCodes.Ldarg_0));
                // writer
                worker.Append(worker.Create(OpCodes.Ldarg, writerParameter));
                // forceAll
                worker.Append(worker.Create(OpCodes.Ldarg, initializeParameter));
                worker.Append(worker.Create(OpCodes.Call, module.ImportReference(baseSerialize)));
                // set dirtyLocal to result of base.OnSerialize()
                worker.Append(worker.Create(OpCodes.Stloc, dirtyLocal));
            }

            // Generates: if (forceAll);
            Instruction initialStateLabel = worker.Create(OpCodes.Nop);
            // forceAll
            worker.Append(worker.Create(OpCodes.Ldarg, initializeParameter));
            worker.Append(worker.Create(OpCodes.Brfalse, initialStateLabel));

            foreach (FoundSyncVar syncVar in syncVars)
            {
                WriteVariable(worker, writerParameter, syncVar.fieldDefinition);
            }

            // always return true if forceAll

            // Generates: return true
            worker.Append(worker.Create(OpCodes.Ldc_I4_1));
            worker.Append(worker.Create(OpCodes.Ret));

            // Generates: end if (forceAll);
            worker.Append(initialStateLabel);

            // write dirty bits before the data fields
            // Generates: writer.WritePackedUInt64 (base.get_syncVarDirtyBits ());
            // writer
            worker.Append(worker.Create(OpCodes.Ldarg, writerParameter));
            // base
            worker.Append(worker.Create(OpCodes.Ldarg_0));
            worker.Append(worker.Create(OpCodes.Call, (NetworkBehaviour nb) => nb.SyncVarDirtyBits));
            MethodReference writeUint64Func = writers.TryGetFunction<ulong>(null);
            worker.Append(worker.Create(OpCodes.Call, writeUint64Func));

            // generate a writer call for any dirty variable in this class

            // start at number of syncvars in parent
            int dirtyBit = netBehaviourSubclass.BaseType.Resolve().GetConst<int>(SyncVarCountField);
            foreach (FoundSyncVar syncVar in syncVars)
            {
                Instruction varLabel = worker.Create(OpCodes.Nop);

                // Generates: if ((base.get_syncVarDirtyBits() & 1uL) != 0uL)
                // base
                worker.Append(worker.Create(OpCodes.Ldarg_0));
                worker.Append(worker.Create(OpCodes.Call, (NetworkBehaviour nb) => nb.SyncVarDirtyBits));
                // 8 bytes = long
                worker.Append(worker.Create(OpCodes.Ldc_I8, 1L << dirtyBit));
                worker.Append(worker.Create(OpCodes.And));
                worker.Append(worker.Create(OpCodes.Brfalse, varLabel));

                // Generates a call to the writer for that field
                WriteVariable(worker, writerParameter, syncVar.fieldDefinition);

                // something was dirty
                worker.Append(worker.Create(OpCodes.Ldc_I4_1));
                // set dirtyLocal to true
                worker.Append(worker.Create(OpCodes.Stloc, dirtyLocal));

                worker.Append(varLabel);
                dirtyBit += 1;
            }

            // generate: return dirtyLocal
            worker.Append(worker.Create(OpCodes.Ldloc, dirtyLocal));
            worker.Append(worker.Create(OpCodes.Ret));
        }

        private void WriteVariable(ILProcessor worker, ParameterDefinition writerParameter, FieldDefinition syncVar)
        {
            // Generates a writer call for each sync variable
            // writer
            worker.Append(worker.Create(OpCodes.Ldarg, writerParameter));
            // this
            worker.Append(worker.Create(OpCodes.Ldarg_0));
            worker.Append(worker.Create(OpCodes.Ldfld, syncVar.MakeHostGenericIfNeeded()));
            MethodReference writeFunc = writers.TryGetFunction(syncVar.FieldType, null);
            if (writeFunc != null)
            {
                worker.Append(worker.Create(OpCodes.Call, writeFunc));
            }
            else
            {
                logger.Error($"{syncVar.Name} has unsupported type. Use a supported Mirage type instead", syncVar);
            }
        }

        void GenerateDeSerialization(TypeDefinition netBehaviourSubclass)
        {
            Weaver.DebugLog(netBehaviourSubclass, "  GenerateDeSerialization");

            const string DeserializeMethodName = nameof(NetworkBehaviour.DeserializeSyncVars);
            if (netBehaviourSubclass.GetMethod(DeserializeMethodName) != null)
                return;

            if (syncVars.Count == 0)
            {
                // no synvars,  no need for custom OnDeserialize
                return;
            }

            MethodDefinition serialize = netBehaviourSubclass.AddMethod(DeserializeMethodName,
                    MethodAttributes.Public | MethodAttributes.Virtual | MethodAttributes.HideBySig);

            ParameterDefinition readerParam = serialize.AddParam<NetworkReader>("reader");
            ParameterDefinition initializeParam = serialize.AddParam<bool>("initialState");
            ILProcessor serWorker = serialize.Body.GetILProcessor();
            // setup local for dirty bits
            serialize.Body.InitLocals = true;
            VariableDefinition dirtyBitsLocal = serialize.AddLocal<long>();

            MethodReference baseDeserialize = netBehaviourSubclass.BaseType.GetMethodInBaseType(DeserializeMethodName);
            if (baseDeserialize != null)
            {
                // base
                serWorker.Append(serWorker.Create(OpCodes.Ldarg_0));
                // reader
                serWorker.Append(serWorker.Create(OpCodes.Ldarg, readerParam));
                // initialState
                serWorker.Append(serWorker.Create(OpCodes.Ldarg, initializeParam));
                serWorker.Append(serWorker.Create(OpCodes.Call, module.ImportReference(baseDeserialize)));
            }

            // Generates: if (initialState);
            Instruction initialStateLabel = serWorker.Create(OpCodes.Nop);

            serWorker.Append(serWorker.Create(OpCodes.Ldarg, initializeParam));
            serWorker.Append(serWorker.Create(OpCodes.Brfalse, initialStateLabel));

            foreach (FoundSyncVar syncVar in syncVars)
            {
                DeserializeField(syncVar.fieldDefinition, serWorker, serialize);
            }

            serWorker.Append(serWorker.Create(OpCodes.Ret));

            // Generates: end if (initialState);
            serWorker.Append(initialStateLabel);

            // get dirty bits
            serWorker.Append(serWorker.Create(OpCodes.Ldarg, readerParam));
            serWorker.Append(serWorker.Create(OpCodes.Call, readers.TryGetFunction<ulong>(null)));
            serWorker.Append(serWorker.Create(OpCodes.Stloc, dirtyBitsLocal));

            // conditionally read each syncvar
            // start at number of syncvars in parent
            int dirtyBit = netBehaviourSubclass.BaseType.Resolve().GetConst<int>(SyncVarCountField);
            foreach (FoundSyncVar syncVar in syncVars)
            {
                Instruction varLabel = serWorker.Create(OpCodes.Nop);

                // check if dirty bit is set
                serWorker.Append(serWorker.Create(OpCodes.Ldloc, dirtyBitsLocal));
                serWorker.Append(serWorker.Create(OpCodes.Ldc_I8, 1L << dirtyBit));
                serWorker.Append(serWorker.Create(OpCodes.And));
                serWorker.Append(serWorker.Create(OpCodes.Brfalse, varLabel));

                DeserializeField(syncVar.fieldDefinition, serWorker, serialize);

                serWorker.Append(varLabel);
                dirtyBit += 1;
            }

            serWorker.Append(serWorker.Create(OpCodes.Ret));
        }

        /// <summary>
        /// [SyncVar] int/float/struct/etc.?
        /// </summary>
        /// <param name="syncVar"></param>
        /// <param name="serWorker"></param>
        /// <param name="deserialize"></param>
        /// <param name="initialState"></param>
        /// <param name="hookResult"></param>
        void DeserializeField(FieldDefinition syncVar, ILProcessor serWorker, MethodDefinition deserialize)
        {
            TypeReference originalType = UnwrapType(syncVar);
            MethodDefinition hookMethod = GetHookMethod(syncVar, originalType);

            /*
             Generates code like:
                // for hook
                int oldValue = a;
                Networka = reader.ReadPackedInt32();
                if (!SyncVarEqual(oldValue, ref a))
                {
                    OnSetA(oldValue, Networka);
                }
             */
            MethodReference readFunc = readers.TryGetFunction(syncVar.FieldType, null);
            if (readFunc == null)
            {
                logger.Error($"{syncVar.Name} has unsupported type. Use a supported Mirage type instead", syncVar);
                return;
            }

            // T oldValue = value;
            VariableDefinition oldValue = deserialize.AddLocal(originalType);
            LoadField(syncVar, originalType, serWorker);

            serWorker.Append(serWorker.Create(OpCodes.Stloc, oldValue));

            // read value and store in syncvar BEFORE calling the hook
            // -> this makes way more sense. by definition, the hook is
            //    supposed to be called after it was changed. not before.
            // -> setting it BEFORE calling the hook fixes the following bug:
            //    https://github.com/vis2k/Mirror/issues/1151 in host mode
            //    where the value during the Hook call would call Cmds on
            //    the host server, and they would all happen and compare
            //    values BEFORE the hook even returned and hence BEFORE the
            //    actual value was even set.
            // put 'this.' onto stack for 'this.syncvar' below
            serWorker.Append(serWorker.Create(OpCodes.Ldarg_0));
            // reader. for 'reader.Read()' below
            serWorker.Append(serWorker.Create(OpCodes.Ldarg_1));
            // reader.Read()
            serWorker.Append(serWorker.Create(OpCodes.Call, readFunc));
            // syncvar
            serWorker.Append(serWorker.Create(OpCodes.Stfld, syncVar.MakeHostGenericIfNeeded()));

            if (hookMethod != null)
            {
                // call hook
                // but only if SyncVar changed. otherwise a client would
                // get hook calls for all initial values, even if they
                // didn't change from the default values on the client.
                // see also: https://github.com/vis2k/Mirror/issues/1278

                // Generates: if (!SyncVarEqual);
                Instruction syncVarEqualLabel = serWorker.Create(OpCodes.Nop);

                // 'this.' for 'this.SyncVarEqual'
                serWorker.Append(serWorker.Create(OpCodes.Ldarg_0));
                // 'oldValue'
                serWorker.Append(serWorker.Create(OpCodes.Ldloc, oldValue));
                // 'newValue'
                LoadField(syncVar, originalType, serWorker);
                // call the function
                MethodReference syncVarEqual = module.ImportReference<NetworkBehaviour>(nb => nb.SyncVarEqual<object>(default, default));
                var syncVarEqualGm = new GenericInstanceMethod(syncVarEqual.GetElementMethod());
                syncVarEqualGm.GenericArguments.Add(originalType);
                serWorker.Append(serWorker.Create(OpCodes.Call, syncVarEqualGm));
                serWorker.Append(serWorker.Create(OpCodes.Brtrue, syncVarEqualLabel));

                // call the hook
                // Generates: OnValueChanged(oldValue, this.syncVar);
                WriteCallHookMethodUsingField(serWorker, hookMethod, oldValue, syncVar);

                // Generates: end if (!SyncVarEqual);
                serWorker.Append(syncVarEqualLabel);
            }
        }
    }
}
