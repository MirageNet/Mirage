---
sidebar_position: 3
---
# Sync List
SyncLists are array based lists similar to C\# [List<T\>](https://docs.microsoft.com/en-us/dotnet/api/system.collections.generic.list-1?view=netstandard-2.0) that synchronize their contents from the server to the clients.

A [SyncList](/docs/reference/Mirage.Collections/SyncList-1) can contain any [supported Mirage type](/docs/guides/data-types).

## Usage
Add a field of type [SyncList](/docs/reference/Mirage.Collections/SyncList-1) on any [NetworkIdentity](/docs/reference/Mirage/NetworkIdentity) where `T` can be any supported Mirage type and initialize it.

:::caution IMPORTANT
You need to initialize the SyncList immediately after definition in order for them to work. You can mark them as `readonly` to enforce proper usage.
:::

### Basic example
```cs
using Mirage;
using Mirage.Collections;

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
```

## Callbacks
You can detect when a `SyncList` changes on the client and/or server. This is especially useful for refreshing your UI, character appearance etc.

There are different callbacks for different operations, such as `OnChange` (any change to the list), `OnInsert` (adding new element) etc. Please check the [SyncList API reference](/docs/reference/Mirage.Collections/SyncList-1) for the complete list of callbacks.

Depending on where you want to invoke the callbacks, you can use these methods to register them:
- `Awake` for both client and server
- `Identity.OnStartServer` event for server-only
- `Identity.OnStartClient` event for cleint-only

:::note
By the time you subscribe, the list will already be initialized, so you will not get a call for the initial data, only updates.
:::

### Example
```cs
using Mirage;
using Mirage.Collections;

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
```

By default, `SyncList` uses a [`List`](https://docs.microsoft.com/en-us/dotnet/api/system.collections.generic.list-1?view=netstandard-2.0) to store its data. If you want to use a different list implementation, add a constructor and pass the list implementation to the parent constructor. For example:

```cs
public SyncList<Item> myList = new SyncList<Item>(new MyIList<Item>());
```