using Mirage;
using UnityEngine;

namespace SyncVarTests.SyncVarsDifferentModule
{
    class SyncVarsDifferentModule : NetworkBehaviour
    {
        [SyncVar(hook = nameof(OnChangeHealth))]
        int health { get; set; }

        [SyncVar]
        TextMesh invalidVar { get; set; }

        public void TakeDamage(int amount)
        {
            if (!IsServer)
                return;

            health -= amount;
        }

        void OnChangeHealth(int oldHealth, int newHealth)
        {
            // do things with your health bar
        }
    }
}
