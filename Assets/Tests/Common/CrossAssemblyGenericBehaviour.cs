using UnityEngine;

namespace Mirage.Tests.Runtime.ClientServer.GenericBehaviours.CrossAssembly
{
    /// <summary>
    /// Non-generic base for CrossAssemblySyncVarFieldTest in Mirage.Tests.Runtime.
    /// Field access to these SyncVars from that assembly is replaced with this assembly's generated property methods
    /// </summary>
    public class CrossAssemblyBase : NetworkBehaviour
    {
        [SyncVar] public int baseValue;
        // struct, so `= default` from the other assembly uses ldflda/initobj instead of stfld
        [SyncVar] public Vector3 baseVector;
    }

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
