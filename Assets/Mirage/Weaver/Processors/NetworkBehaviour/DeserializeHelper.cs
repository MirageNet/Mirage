using System;
using Mirage.CodeGen;
using Mirage.Serialization;
using Mirage.Weaver.SyncVars;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace Mirage.Weaver.NetworkBehaviours
{
    internal class DeserializeHelper : BaseMethodHelper
    {
        private FoundNetworkBehaviour _behaviour;

        public ParameterDefinition ReaderParameter { get; private set; }
        public ParameterDefinition InitializeParameter { get; private set; }
        /// <summary>
        /// IMPORTANT: this mask is only for this NB, it is not shifted based on base class
        /// </summary>
        public VariableDefinition DirtyBitsLocal { get; private set; }

        public DeserializeHelper(ModuleDefinition module, FoundNetworkBehaviour behaviour) : base(module, behaviour.TypeDefinition)
        {
            _behaviour = behaviour;
        }

        public override string MethodName => nameof(NetworkBehaviour.DeserializeSyncVars);

        protected override void AddParameters()
        {
            ReaderParameter = Method.AddParam<NetworkReader>("reader");
            InitializeParameter = Method.AddParam<bool>("initialState");
        }

        protected override void AddLocals()
        {
            DirtyBitsLocal = Method.AddLocal<ulong>();
        }


        public void WriteIfInitial(Action body)
        {
            // Generates: if (initial)
            var initialStateLabel = Worker.Create(OpCodes.Nop);

            Worker.Append(Worker.Create(OpCodes.Ldarg, InitializeParameter));
            Worker.Append(Worker.Create(OpCodes.Brfalse, initialStateLabel));

            body.Invoke();

            Worker.Append(Worker.Create(OpCodes.Ret));

            // Generates: end if (initial)
            Worker.Append(initialStateLabel);
        }

        /// <summary>
        /// Writes Reads dirty bit mask for this NB,
        /// <para>Shifts by number of syncvars in base class, then writes number of bits in this class</para>
        /// </summary>
        public void ReadDirtyBitMask()
        {
            var readBitsMethod = _module.ImportReference(ReaderParameter.ParameterType.Resolve().GetMethod(nameof(NetworkReader.Read)));

            // Generates: reader.Read(n)
            // n is syncvars in this

            // get dirty bits
            Worker.Append(Worker.Create(OpCodes.Ldarg, ReaderParameter));
            Worker.Append(Worker.Create(OpCodes.Ldc_I4, _behaviour.SyncVars.Count));
            Worker.Append(Worker.Create(OpCodes.Call, readBitsMethod));
            Worker.Append(Worker.Create(OpCodes.Stloc, DirtyBitsLocal));

            SetDeserializeMask();
        }

        private void SetDeserializeMask()
        {
            // Generates: SetDeserializeMask(mask, n)
            // n is syncvars in base class
            Worker.Append(Worker.Create(OpCodes.Ldarg_0));
            Worker.Append(Worker.Create(OpCodes.Ldloc, DirtyBitsLocal));
            Worker.Append(Worker.Create(OpCodes.Ldc_I4, _behaviour.syncVarCounter.GetInBase()));
            Worker.Append(Worker.Create<NetworkBehaviour>(OpCodes.Call, nb => nb.SetDeserializeMask(default, default)));
        }

        internal void WriteIfSyncVarDirty(FoundSyncVar syncVar, Action body)
        {
            var endIf = Worker.Create(OpCodes.Nop);

            // we dont shift read bits, so we have to shift dirty bit here
            var syncVarIndex = syncVar.DirtyBit >> _behaviour.syncVarCounter.GetInBase();

            // check if dirty bit is set
            Worker.Append(Worker.Create(OpCodes.Ldloc, DirtyBitsLocal));
            Worker.Append(Worker.Create(OpCodes.Ldc_I8, syncVarIndex));
            Worker.Append(Worker.Create(OpCodes.And));
            Worker.Append(Worker.Create(OpCodes.Brfalse, endIf));

            body.Invoke();

            Worker.Append(endIf);
        }
    }
}
