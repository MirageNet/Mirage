using Mirage;
using UnityEngine;

namespace BadAttributeUseageTests.NormalClassServerRpc
{
    class NormalClassServerRpc
    {
        [ServerRpc]
        void ThisCantBeOutsideNetworkBehaviour() { }
    }
}
