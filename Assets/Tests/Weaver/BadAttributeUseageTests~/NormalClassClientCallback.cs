using Mirage;
using UnityEngine;

namespace BadAttributeUseageTests.NormalClassClientCallback
{
    class NormalClassClientCallback 
    {
        [Client(error = false)]
        void ThisCantBeOutsideNetworkBehaviour() { }
    }
}
