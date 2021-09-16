using Mirage;
using UnityEngine;

namespace BadAttributeUseageTests.NormalClassClient
{
    class NormalClassClient
    {
        [Client]
        void ThisCantBeOutsideNetworkBehaviour() { }
    }
}
