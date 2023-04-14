---
sidebar_position: 2
---
# Sync Dictionary
[`SyncDictionary`](/docs/reference/Mirage.Collections/SyncDictionary-2) is an associative array containing an unordered list of key, value pairs. Keys and values can be any of [Mirage supported types](/docs/guides/data-types).

SyncDictionary works much like [SyncLists](/docs/guides/sync/sync-list): when you make a change on the server, the change is propagated to all clients and the appropriate callback is called.

## Usage
Add a field of type [SyncDictionary](/docs/reference/Mirage.Collections/SyncDictionary-2) on any [NetworkBehaviour](/docs/reference/Mirage/NetworkBehaviour) where `TKey` and `TValue` can be any supported Mirage type and initialize it.

:::caution IMPORTANT
You need to initialize the SyncDictionary immediately after the definition for them to work. You can mark them as `readonly` to enforce proper usage.
:::

### Basic example
```cs
using UnityEngine;
using Mirage;
using Mirage.Collections;

[System.Serializable]
public struct Item
{
    public string name;
    public int hitPoints;
    public int durability;
}

public class Player : NetworkBehaviour
{
    public readonly SyncDictionary<stirng, Item> equipment = new SyncDictionary<string, Item>();

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
```

## Callbacks
You can detect when a SyncDictionary changes on the client and/or server. This is especially useful for refreshing your UI, character appearance, etc.

There are different callbacks for different operations, such as `OnChange` (any change to the dictionary), `OnInsert` (adding a new element), etc. Please check the [SyncDictionary API reference](/docs/reference/Mirage.Collections/SyncDictionary-2) for the complete list of callbacks.

Depending on where you want to invoke the callbacks, you can use these methods to register them:
- `Awake` for both client and server
- `Identity.OnStartServer` event for server-only
- `Identity.OnStartClient` event for client-only

:::note
By the time you subscribe, the dictionary will already be initialized, so you will not get a call for the initial data, only updates.
:::

### Example
```cs
using Mirage;
using Mirage.Collections;

public class Player : NetworkBehaviour 
{
    public readonly SyncDictionary<stirng, Item> equipment = new SyncDictionary<string, Item>();
    public readonly SyncDictionary<stirng, Item> hotbar = new SyncDictionary<string, Item>();

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
```

By default, `SyncDictionary` uses a [`Dictionary`](https://docs.microsoft.com/en-us/dotnet/api/system.collections.generic.dictionary-2?view=netstandard-2.0) to store its data. If you want to use a different dictionary implementation, add a constructor and pass the dictionary implementation to the parent constructor. For example:

```cs
public SyncDictionary<string, Item> myDict = new SyncIDictionary<string, Item>(new MyDictionary<string, Item>());
```