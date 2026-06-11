using Mirage;

namespace SyncVarTests.SyncVarClassWarning
{
    class SomeCustomClass
    {
        public int Value;
    }

    [WeaverSyncVarSafe]
    class SafeClass
    {
        public int Value;
    }

    class SyncVarClassWarning : NetworkBehaviour
    {
        [SyncVar]
        SomeCustomClass warnedVar { get; set; }

        [SyncVar]
        [WeaverSyncVarSafe]
        SomeCustomClass safePropVar { get; set; }

        [SyncVar]
        SafeClass safeClassVar { get; set; }

        [SyncVar]
        NetworkBehaviour networkBehaviourVar { get; set; }

        [SyncVar]
        string stringVar { get; set; }
    }
}
