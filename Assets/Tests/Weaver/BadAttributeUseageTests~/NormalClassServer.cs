using Mirage;
using UnityEngine;

namespace BadAttributeUseageTests.NormalClassServer
{
    class NormalClassServer
    {
        [Server]
        void ThisCantBeOutsideNetworkBehaviour() { }
    }
}
