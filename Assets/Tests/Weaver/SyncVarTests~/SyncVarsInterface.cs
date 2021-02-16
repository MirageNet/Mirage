using Mirage;

namespace SyncVarTests.SyncVarsInterface
{
    class SyncVarsInterface : NetworkBehaviour
    {
        interface IMySyncVar
        {
            void interfaceMethod();
        }
        [SyncVar]
        IMySyncVar invalidVar;
    }
}
