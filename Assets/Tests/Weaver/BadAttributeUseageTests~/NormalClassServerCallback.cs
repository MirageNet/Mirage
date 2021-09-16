using Mirage;
using UnityEngine;

namespace BadAttributeUseageTests.NormalClassServerCallback
{
    class NormalClassServerCallback
    {
        [Server(error = false)]
        void ThisCantBeOutsideNetworkBehaviour() { }
    }
}
