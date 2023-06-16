using System.Collections.Generic;
using Mirage.Weaver.SyncVars;
using Mono.Cecil;

namespace Mirage.Weaver.NetworkBehaviours
{
    internal class FoundNetworkBehaviour
    {
        public readonly ModuleDefinition Module;
        public readonly TypeDefinition TypeDefinition;
        public readonly ConstFieldTracker syncVarCounter;

        public FoundNetworkBehaviour(ModuleDefinition module, TypeDefinition td)
        {
            Module = module;
            TypeDefinition = td;

            syncVarCounter = new ConstFieldTracker("SYNC_VAR_COUNT", td, 64, "[SyncVar]");
        }

        public List<FoundSyncVar> SyncVars { get; private set; } = new List<FoundSyncVar>();

        public FoundSyncVar AddSyncVar(FieldDefinition fd)
        {
            var dirtyIndex = syncVarCounter.GetInBase() + SyncVars.Count;
            var syncVar = new FoundSyncVar(Module, this, fd, dirtyIndex);
            SyncVars.Add(syncVar);
            return syncVar;
        }

        public void SetSyncVarCount()
        {
            syncVarCounter.Set(SyncVars.Count);
        }
    }
}
