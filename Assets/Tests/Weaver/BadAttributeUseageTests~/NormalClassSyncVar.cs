using Mirage;
using UnityEngine;

namespace BadAttributeUseageTests.NormalClassSyncVar
{
    class NormalClassSyncVar
    {
        [SyncVar]
        int potato;
    }
}
