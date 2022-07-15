using System;
using Mirage.Serialization;
using Mirage.Weaver.SyncVars;
using Mono.Cecil;
using Mono.Cecil.Cil;
using MethodAttributes = Mono.Cecil.MethodAttributes;

namespace Mirage.Weaver.NetworkBehaviours
{
    internal class DeserializeHelper
    {
        public const string MethodName = nameof(NetworkBehaviour.DeserializeSyncVars);
        private readonly ModuleDefinition module;
        private readonly FoundNetworkBehaviour behaviour;
        private ILProcessor worker;

        public MethodDefinition Method { get; private set; }
        public ParameterDefinition ReaderParameter { get; private set; }
        public ParameterDefinition InitializeParameter { get; private set; }
        /// <summary>
        /// IMPORTANT: this mask is only for this NB, it is not shifted based on base class
        /// </summary>
        public VariableDefinition DirtyBitsLocal { get; private set; }

        public DeserializeHelper(ModuleDefinition module, FoundNetworkBehaviour behaviour)
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
                    MethodAttributes.Public | MethodAttributes.Virtual | MethodAttributes.HideBySig);

            ReaderParameter = Method.AddParam<NetworkReader>("reader");
            InitializeParameter = Method.AddParam<bool>("initialState");

            Method.Body.InitLocals = true;
            worker = Method.Body.GetILProcessor();
            return worker;
        }

        public void AddLocals()
        {
            DirtyBitsLocal = Method.AddLocal<ulong>();
        }

        public void WriteBaseCall()
        {
            var baseDeserialize = behaviour.TypeDefinition.BaseType.GetMethodInBaseType(MethodName);
            if (baseDeserialize != null)
            {
                // base
                worker.Append(worker.Create(OpCodes.Ldarg_0));
                // reader
                worker.Append(worker.Create(OpCodes.Ldarg, ReaderParameter));
                // initialState
                worker.Append(worker.Create(OpCodes.Ldarg, InitializeParameter));
                worker.Append(worker.Create(OpCodes.Call, module.ImportReference(baseDeserialize)));
            }
        }

        public void WriteIfInitial(Action body)
        {
            // Generates: if (initial)
            var initialStateLabel = worker.Create(OpCodes.Nop);

            worker.Append(worker.Create(OpCodes.Ldarg, InitializeParameter));
            worker.Append(worker.Create(OpCodes.Brfalse, initialStateLabel));

            body.Invoke();

            worker.Append(worker.Create(OpCodes.Ret));

            // Generates: end if (initial)
            worker.Append(initialStateLabel);
        }

        /// <summary>
        /// Writes Reads dirty bit mask for this NB,
        /// <para>Shifts by number of syncvars in base class, then writes number of bits in this class</para>
        /// </summary>
        public void ReadDirtyBitMask()
        {
            var readBitsMethod = module.ImportReference(ReaderParameter.ParameterType.Resolve().GetMethod(nameof(NetworkReader.Read)));

            // Generates: reader.Read(n)
            // n is syncvars in this

            // get dirty bits
            worker.Append(worker.Create(OpCodes.Ldarg, ReaderParameter));
            worker.Append(worker.Create(OpCodes.Ldc_I4, behaviour.SyncVars.Count));
            worker.Append(worker.Create(OpCodes.Call, readBitsMethod));
            worker.Append(worker.Create(OpCodes.Stloc, DirtyBitsLocal));
        }

        internal void WriteIfSyncVarDirty(FoundSyncVar syncVar, Action body)
        {
            var endIf = worker.Create(OpCodes.Nop);

            // we dont shift read bits, so we have to shift dirty bit here
            var syncVarIndex = syncVar.DirtyBit >> behaviour.syncVarCounter.GetInBase();

            // check if dirty bit is set
            worker.Append(worker.Create(OpCodes.Ldloc, DirtyBitsLocal));
            worker.Append(worker.Create(OpCodes.Ldc_I8, syncVarIndex));
            worker.Append(worker.Create(OpCodes.And));
            worker.Append(worker.Create(OpCodes.Brfalse, endIf));

            body.Invoke();

            worker.Append(endIf);
        }
    }
}
