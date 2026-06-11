using UnityEngine;
using Mirage;
using Mirage.Collections;
using System.Collections.Generic;

namespace Mirage.Snippets.Sync.Lists.Basic
{
    // CodeEmbed-Start: SyncListBasicExample
    [System.Serializable]
    public struct Item
    {
        public string name;
        public int amount;
        public Color32 color;
    }

    public class Player : NetworkBehaviour
    {
        private readonly SyncList<Item> inventory = new SyncList<Item>();

        public int coins = 100;

        [ServerRpc]
        public void Purchase(string itemName)
        {
            if (coins > 10)
            {
                coins -= 10;
                Item item = new Item
                {
                    name = "Sword",
                    amount = 3,
                    color = new Color32(125, 125, 125, 255)
                };

                // During next synchronization, all clients will see the item
                inventory.Add(item);
            }
        }
    }
    // CodeEmbed-End: SyncListBasicExample
}

namespace Mirage.Snippets.Sync.Lists.Callbacks
{
    using Mirage.Snippets.Sync.Lists.Basic;

    // CodeEmbed-Start: SyncListCallbackExample
    public class Player : NetworkBehaviour 
    {
        private readonly SyncList<Item> inventory = new SyncList<Item>();
        private readonly SyncList<Item> hotbar = new SyncList<Item>();

        // This will hook the callback on both server and client
        private void Awake()
        {
            inventory.OnChange += UpdateInventory;
            Identity.OnStartClient.AddListener(OnStartClient);
        }

        // Hotbar changes will only be invoked on clients
        private void OnStartClient() 
        {
            hotbar.OnChange += UpdateHotbar;
        }

        private void UpdateInventory()
        {
            // Here you can refresh your UI for instance
        }

        private void UpdateHotbar()
        {
            // Here you can refresh your UI for instance
        }
    }
    // CodeEmbed-End: SyncListCallbackExample
}

namespace Mirage.Snippets.Sync.Lists.Custom
{
    using Mirage.Snippets.Sync.Lists.Basic;

    public class MyIList<T> : List<T> {}

    public class CustomListExample
    {
        // CodeEmbed-Start: SyncListCustomImplementation
        public SyncList<Item> myList = new SyncList<Item>(new MyIList<Item>());
        // CodeEmbed-End: SyncListCustomImplementation
    }
}
