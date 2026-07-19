using Mirage;
using UnityEngine;

namespace SyncVarTests.SyncVarNullable
{
    class SyncVarNullable : NetworkBehaviour
    {
        [SyncVar]
        Color? controlColor;

        [SyncVar]
        int? nullableInt;

        public void ClearColor()
        {
            controlColor = null;
        }

        public void ClearInt()
        {
            nullableInt = null;
        }
    }
}
