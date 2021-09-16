using Mirage;
using UnityEngine;

namespace BadAttributeUseageTests.NormalClassClientRpc
{
    class NormalClassClientRpc
    {
        [ClientRpc]
        void ThisCantBeOutsideNetworkBehaviour() { }
    }
}
