using System.Collections.Generic;
using Mirage.Weaver.SyncVars;
using Mono.Cecil;

namespace Mirage.Weaver.NetworkBehaviours
{
    internal class FoundNetworkBehaviour
    {
        // ulong = 64 bytes
        const int SyncVarLimit = 64;
        const string SyncVarCountField = "SYNC_VAR_COUNT";

        public readonly ModuleDefinition Module;
        public readonly TypeDefinition TypeDefinition;

        public FoundNetworkBehaviour(ModuleDefinition module, TypeDefinition td)
        {
            Module = module;
            TypeDefinition = td;
        }

        public int SyncVarInBase { get; private set; }
        public List<FoundSyncVar> SyncVars { get; private set; } = new List<FoundSyncVar>();

        public void GetSyncVarCountFromBase()
        {
            SyncVarInBase = TypeDefinition.BaseType.Resolve().GetConst<int>(SyncVarCountField);
        }

        public FoundSyncVar AddSyncVar(FieldDefinition fd)
        {
            int dirtyIndex = SyncVarInBase + SyncVars.Count;
            var syncVar = new FoundSyncVar(Module, this, fd, dirtyIndex);
            SyncVars.Add(syncVar);
            return syncVar;
        }

        public void SetSyncVarCount()
        {
            int totalSyncVars = SyncVarInBase + SyncVars.Count;

            if (totalSyncVars >= SyncVarLimit)
            {
                throw new NetworkBehaviourException($"{TypeDefinition.Name} has too many SyncVars. Consider refactoring your class into multiple components", TypeDefinition);
            }
            TypeDefinition.SetConst(SyncVarCountField, totalSyncVars);
        }

        public bool HasManualSerializeOverride()
        {
            return TypeDefinition.GetMethod(SerializeHelper.MethodName) != null;
        }
        public bool HasManualDeserializeOverride()
        {
            return TypeDefinition.GetMethod(DeserializeHelper.MethodName) != null;
        }
    }
}
