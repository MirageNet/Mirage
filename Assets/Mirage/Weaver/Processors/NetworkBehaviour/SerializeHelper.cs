using System;
using Mirage.CodeGen;
using Mirage.Serialization;
using Mirage.Weaver.SyncVars;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace Mirage.Weaver.NetworkBehaviours
{
    internal class SerializeHelper : BaseMethodHelper
    {
        private FoundNetworkBehaviour _behaviour;

        public ParameterDefinition WriterParameter { get; private set; }
        public ParameterDefinition InitializeParameter { get; private set; }
        public VariableDefinition DirtyLocal { get; private set; }
        public VariableDefinition DirtyBitsLocal { get; private set; }

        public SerializeHelper(ModuleDefinition module, FoundNetworkBehaviour behaviour) : base(module, behaviour.TypeDefinition)
        {
            _behaviour = behaviour;
        }

        public override string MethodName => nameof(NetworkBehaviour.SerializeSyncVars);
        protected override Type ReturnValue => typeof(bool);

        protected override void AddParameters()
        {
            WriterParameter = Method.AddParam<NetworkWriter>("writer");
            InitializeParameter = Method.AddParam<bool>("initialize");
        }

        protected override void AddLocals()
        {
            DirtyLocal = Method.AddLocal<bool>();
            DirtyBitsLocal = Method.AddLocal<ulong>();

            // store dirty bit in local variable to avoid calling property multiple times
            Worker.Append(Worker.Create(OpCodes.Ldarg_0));
            Worker.Append(Worker.Create(OpCodes.Call, (NetworkBehaviour nb) => nb.SyncVarDirtyBits));
            Worker.Append(Worker.Create(OpCodes.Stloc, DirtyBitsLocal));
        }

        protected override void WriteBaseCall()
        {
            base.WriteBaseCall();
            // we also need to store return value for Serialize
            Worker.Append(Worker.Create(OpCodes.Stloc, DirtyLocal));
        }

        public void WriteIfInitial(Action Body)
        {
            // Generates: if (initial)
            var endIfLabel = Worker.Create(OpCodes.Nop);
            // initial
            Worker.Append(Worker.Create(OpCodes.Ldarg, InitializeParameter));
            Worker.Append(Worker.Create(OpCodes.Brfalse, endIfLabel));

            // body
            Body.Invoke();
            // always return true if initial

            // Generates: return true
            Worker.Append(Worker.Create(OpCodes.Ldc_I4_1));
            Worker.Append(Worker.Create(OpCodes.Ret));

            // Generates: end if (initial)
            Worker.Append(endIfLabel);
        }

        /// <summary>
        /// Writes dirty bit mask for this NB,
        /// <para>Shifts by number of syncvars in base class, then writes number of bits in this class</para>
        /// </summary>
        public void WriteDirtyBitMask()
        {
            var writeBitsMethod = _module.ImportReference(WriterParameter.ParameterType.Resolve().GetMethod(nameof(NetworkWriter.Write)));

            // Generates: writer.Write(dirtyBits >> b, n)
            // where b is syncvars in base, n is syncvars in this

            // load writer
            Worker.Append(Worker.Create(OpCodes.Ldarg, WriterParameter));
            // load dirty bits
            Worker.Append(Worker.Create(OpCodes.Ldloc, DirtyBitsLocal));

            // shift if there are syncvars in base class
            var syncVarInBase = _behaviour.syncVarCounter.GetInBase();
            if (syncVarInBase > 0)
            {
                // load inBaseCount
                Worker.Append(Worker.Create(OpCodes.Ldc_I4, syncVarInBase));
                // right shift, dirtyBits >> inBaseCount
                Worker.Append(Worker.Create(OpCodes.Shr));
            }
            // load syncVarCount
            Worker.Append(Worker.Create(OpCodes.Ldc_I4, _behaviour.SyncVars.Count));
            // call Write
            Worker.Append(Worker.Create(OpCodes.Call, writeBitsMethod));
        }


        /// <summary>
        /// Generates: if ((dirtyBits & 1uL) != 0uL)
        /// <para>where 1uL is the syncvar's dirty bit</para>
        /// </summary>
        /// <param name="syncvar"></param>
        /// <param name="falseLabel"></param>
        public void WriteIfSyncVarDirty(FoundSyncVar syncvar, Action Body)
        {
            var endIfLabel = Worker.Create(OpCodes.Nop);
            // load dirtyBit
            // load syncvarIndex
            // AND operation

            // if zero, jump to label

            Worker.Append(Worker.Create(OpCodes.Ldloc, DirtyBitsLocal));
            Worker.Append(Worker.Create(OpCodes.Ldc_I8, syncvar.DirtyBit));
            Worker.Append(Worker.Create(OpCodes.And));
            Worker.Append(Worker.Create(OpCodes.Brfalse, endIfLabel));

            Body.Invoke();

            // say that this NB is dirty
            Worker.Append(Worker.Create(OpCodes.Ldc_I4_1));
            // set dirtyLocal to true
            Worker.Append(Worker.Create(OpCodes.Stloc, DirtyLocal));

            Worker.Append(endIfLabel);
        }


        public void WriteReturnDirty()
        {
            Worker.Append(Worker.Create(OpCodes.Ldloc, DirtyLocal));
            Worker.Append(Worker.Create(OpCodes.Ret));
        }
    }
}
