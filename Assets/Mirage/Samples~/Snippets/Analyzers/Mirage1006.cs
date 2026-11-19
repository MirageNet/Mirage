using Mirage;
using Mirage.Collections;

namespace Mirage.Snippets.Analyzers
{
    namespace M1006.Triggering
    {
        // CodeEmbed-Start: mirage1006-triggering
        public class Player : NetworkBehaviour
        {
            // Warns: SyncVar 'inventory' is an array, prefer SyncList
            [SyncVar]
            public byte[] inventory;

            [Server]
            public void SetSlot(int slot, byte item)
            {
                // this change is not synced, because the setter is not called
                inventory[slot] = item;
            }
        }
        // CodeEmbed-End: mirage1006-triggering
    }

    namespace M1006.Recommended
    {
        // CodeEmbed-Start: mirage1006-recommended
        public class Player : NetworkBehaviour
        {
            public readonly SyncList<byte> inventory = new SyncList<byte>();

            [Server]
            public void SetSlot(int slot, byte item)
            {
                // only this change is sent to clients
                inventory[slot] = item;
            }
        }
        // CodeEmbed-End: mirage1006-recommended
    }

    namespace M1006.InitialOnly
    {
        // CodeEmbed-Start: mirage1006-initial-only
        public class Aircraft : NetworkBehaviour
        {
            // array is set once before spawning, and only sent in the spawn message
            [SyncVar(initialOnly = true)]
            public byte[] cargoDropCounts;
        }
        // CodeEmbed-End: mirage1006-initial-only
    }
}
