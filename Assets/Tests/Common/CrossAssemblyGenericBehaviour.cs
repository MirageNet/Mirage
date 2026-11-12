namespace Mirage.Tests.Runtime.ClientServer.GenericBehaviours.CrossAssembly
{
    /// <summary>
    /// Base for CrossAssemblyGenericBehaviourTest in Mirage.Tests.Runtime. It is in this assembly so that the Weaver has to
    /// call base methods of a generic type from another assembly, with type args from the derived assembly
    /// </summary>
    public class CrossAssemblyGenericBase<T> : NetworkBehaviour
    {
        [SyncVar] public int baseValue;
        public int baseRpcValue;

        [ClientRpc]
        public void RpcBase(int value)
        {
            baseRpcValue = value;
        }
    }

    /// <summary>
    /// Generic middle with no SyncVars or RPCs, so the Weaver has to match the derived type's args to the base's generic params
    /// </summary>
    public class CrossAssemblyGenericMiddle<T> : CrossAssemblyGenericBase<T>
    {
    }
}
