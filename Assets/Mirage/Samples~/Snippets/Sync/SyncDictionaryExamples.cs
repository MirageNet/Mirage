using System.Collections.Generic;
using UnityEngine;
using Mirage;
using Mirage.Collections;

namespace Mirage.Snippets.Sync
{
    // CodeEmbed-Start: SyncDictionaryBasicExample
    [System.Serializable]
    public struct Item
    {
        public string name;
        public int hitPoints;
        public int durability;
    }

    public class Player : NetworkBehaviour
    {
        public readonly SyncDictionary<string, Item> equipment = new SyncDictionary<string, Item>();

        private void Awake() 
        {
            Identity.OnStartServer.AddListener(OnStartServer);
        }

        private void OnStartServer()
        {
            equipment.Add("head", new Item { name = "Helmet", hitPoints = 10, durability = 20 });
            equipment.Add("body", new Item { name = "Epic Armor", hitPoints = 50, durability = 50 });
            equipment.Add("feet", new Item { name = "Sneakers", hitPoints = 3, durability = 40 });
            equipment.Add("hands", new Item { name = "Sword", hitPoints = 30, durability = 15 });
        }
    }
    // CodeEmbed-End: SyncDictionaryBasicExample

    // CodeEmbed-Start: SyncDictionaryCallbackExample
    public class PlayerWithCallbacks : NetworkBehaviour 
    {
        public readonly SyncDictionary<string, Item> equipment = new SyncDictionary<string, Item>();
        public readonly SyncDictionary<string, Item> hotbar = new SyncDictionary<string, Item>();

        // This will hook the callback on both server and client
        private void Awake()
        {
            equipment.OnChange += UpdateEquipment;
            Identity.OnStartClient.AddListener(OnStartClient);
        }

        // Hotbar changes will only be invoked on clients
        private void OnStartClient() 
        {
            hotbar.OnChange += UpdateHotbar;
        }

        private void UpdateEquipment()
        {
            // Here you can refresh your UI for instance
        }

        private void UpdateHotbar()
        {
            // Here you can refresh your UI for instance
        }
    }
    // CodeEmbed-End: SyncDictionaryCallbackExample

    public class MyDictionary<TKey, TValue> : Dictionary<TKey, TValue> {}

    public class CustomDictionaryExample
    {
        // CodeEmbed-Start: SyncDictionaryCustomImplementation
        public SyncIDictionary<string, Item> myDict = new SyncIDictionary<string, Item>(new MyDictionary<string, Item>());
        // CodeEmbed-End: SyncDictionaryCustomImplementation
    }
}
