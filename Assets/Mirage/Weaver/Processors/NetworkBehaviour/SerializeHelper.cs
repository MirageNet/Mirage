using System;
using Mirage.Serialization;
using Mirage.Weaver.SyncVars;
using Mono.Cecil;
using Mono.Cecil.Cil;
using MethodAttributes = Mono.Cecil.MethodAttributes;

namespace Mirage.Weaver.NetworkBehaviours
{
    internal class SerializeHelper
    {
        public const string MethodName = nameof(NetworkBehaviour.SerializeSyncVars);

        readonly ModuleDefinition module;
        readonly FoundNetworkBehaviour behaviour;

        ILProcessor worker;

        public MethodDefinition Method { get; private set; }
        public ParameterDefinition WriterParameter { get; private set; }
        public ParameterDefinition InitializeParameter { get; private set; }
        public VariableDefinition DirtyLocal { get; private set; }
        public VariableDefinition DirtyBitsLocal { get; private set; }

        public SerializeHelper(ModuleDefinition module, FoundNetworkBehaviour behaviour)
        {
            this.module = module;
            this.behaviour = behaviour;
        }

        /// <summary>
        /// Adds Serialize method to current type
        /// </summary>
        /// <returns></returns>
        public ILProcessor AddMethod()
        {
            Method = behaviour.TypeDefinition.AddMethod(MethodName,
                    MethodAttributes.Public | MethodAttributes.Virtual | MethodAttributes.HideBySig,
                    module.ImportReference<bool>());

            WriterParameter = Method.AddParam<NetworkWriter>("writer");
            InitializeParameter = Method.AddParam<bool>("initialize");
            Method.Body.InitLocals = true;
            worker = Method.Body.GetILProcessor();
            return worker;
        }

        public void AddLocals()
        {
            DirtyLocal = Method.AddLocal<bool>();
            DirtyBitsLocal = Method.AddLocal<ulong>();

            // store dirty bit in local variable to avoid calling property multiple times
            worker.Append(worker.Create(OpCodes.Ldarg_0));
            worker.Append(worker.Create(OpCodes.Call, (NetworkBehaviour nb) => nb.SyncVarDirtyBits));
            worker.Append(worker.Create(OpCodes.Stloc, DirtyBitsLocal));
        }

        public void WriteBaseCall()
        {
            // dirty = base.Serialize(...)

            MethodReference baseSerialize = behaviour.TypeDefinition.BaseType.GetMethodInBaseType(MethodName);
            if (baseSerialize != null)
            {
                // base
                worker.Append(worker.Create(OpCodes.Ldarg_0));
                // writer
                worker.Append(worker.Create(OpCodes.Ldarg, WriterParameter));
                // inital?
                worker.Append(worker.Create(OpCodes.Ldarg, InitializeParameter));
                worker.Append(worker.Create(OpCodes.Call, module.ImportReference(baseSerialize)));
                // store to variable
                worker.Append(worker.Create(OpCodes.Stloc, DirtyLocal));
            }
        }

        public void WriteIfInitial(Action Body)
        {
            // Generates: if (initial)
            Instruction endIfLabel = worker.Create(OpCodes.Nop);
            // initial
            worker.Append(worker.Create(OpCodes.Ldarg, InitializeParameter));
            worker.Append(worker.Create(OpCodes.Brfalse, endIfLabel));

            // body
            Body.Invoke();
            // always return true if initial

            // Generates: return true
            worker.Append(worker.Create(OpCodes.Ldc_I4_1));
            worker.Append(worker.Create(OpCodes.Ret));

            // Generates: end if (initial)
            worker.Append(endIfLabel);
        }

        /// <summary>
        /// Writes dirty bit mask for this NB,
        /// <para>Shifts by number of syncvars in base class, then writes number of bits in this class</para>
        /// </summary>
        public void WriteDirtyBitMask()
        {
            MethodReference writeBitsMethod = module.ImportReference(WriterParameter.ParameterType.Resolve().GetMethod(nameof(NetworkWriter.Write)));

            // Generates: writer.Write(dirtyBits >> b, n)
            // where b is syncvars in base, n is syncvars in this

            // load writer
            worker.Append(worker.Create(OpCodes.Ldarg, WriterParameter));
            // load dirty bits
            worker.Append(worker.Create(OpCodes.Ldloc, DirtyBitsLocal));

            // shift if there are syncvars in base class
            int syncVarInBase = behaviour.syncVarCounter.GetInBase();
            if (syncVarInBase > 0)
            {
                // load inBaseCount
                worker.Append(worker.Create(OpCodes.Ldc_I4, syncVarInBase));
                // right shift, dirtyBits >> inBaseCount
                worker.Append(worker.Create(OpCodes.Shr));
            }
            // load syncVarCount
            worker.Append(worker.Create(OpCodes.Ldc_I4, behaviour.SyncVars.Count));
            // call Write
            worker.Append(worker.Create(OpCodes.Call, writeBitsMethod));
        }


        /// <summary>
        /// Generates: if ((dirtyBits & 1uL) != 0uL)
        /// <para>where 1uL is the syncvar's dirty bit</para>
        /// </summary>
        /// <param name="syncvar"></param>
        /// <param name="falseLabel"></param>
        public void WriteIfSyncVarDirty(FoundSyncVar syncvar, Action Body)
        {
            Instruction endIfLabel = worker.Create(OpCodes.Nop);
            // load dirtyBit
            // load syncvarIndex
            // AND operation

            // if zero, jump to label

            worker.Append(worker.Create(OpCodes.Ldloc, DirtyBitsLocal));
            worker.Append(worker.Create(OpCodes.Ldc_I8, syncvar.DirtyBit));
            worker.Append(worker.Create(OpCodes.And));
            worker.Append(worker.Create(OpCodes.Brfalse, endIfLabel));

            Body.Invoke();

            // say that this NB is dirty
            worker.Append(worker.Create(OpCodes.Ldc_I4_1));
            // set dirtyLocal to true
            worker.Append(worker.Create(OpCodes.Stloc, DirtyLocal));

            worker.Append(endIfLabel);
        }


        public void WriteReturnDirty()
        {
            worker.Append(worker.Create(OpCodes.Ldloc, DirtyLocal));
            worker.Append(worker.Create(OpCodes.Ret));
        }
    }
}
