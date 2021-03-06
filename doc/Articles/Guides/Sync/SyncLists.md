# SyncLists
SyncLists are array based lists similar to C\# [List\<T\>](https://docs.microsoft.com/en-us/dotnet/api/system.collections.generic.list-1?view=netframework-4.7.2) that synchronize their contents from the server to the clients.

A <xref:Mirage.SyncList`1> can contain any [supported Mirage type](../DataTypes.md).

## Usage
Add a field of type `SyncList<T>` on any <xref:Mirage.NetworkBehaviour> where `T` can be any supported Mirage type and initialize it.

> [!IMPORTANT]
> You need to initialize the SyncList immediately after definition in order for them to work. You can mark them as `readonly` to enforce proper usage.

### Basic example
```cs
using Mirage;

[System.Serializable]
public struct Item
{
    public string name;
    public int amount;
    public Color32 color;
}

public class Player : NetworkBehaviour
{
    readonly SyncList<Item> inventory = new SyncList<Item>();

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

            // during next synchronization,  all clients will see the item
            inventory.Add(item);
        }
    }
}
```

## Callbacks
You can detect when a SyncList changes on the client and/or server. This is especially useful for refreshing your UI, character appearance etc.

There are different callbacks for different operations, such as `OnChange` (any change to the list), `OnInsert` (adding new element) etc. Please check the [SyncList API reference](xref:Mirage.SyncList`1) for the complete list of callbacks.

Depending on where you want to invoke the callbacks, you can use these methods to register them:
- `Awake` for both client and server
- `NetIdentity.OnStartServer` event for server-only
- `NetIdentity.OnStartClient` event for cleint-only

> [!NOTE]
> By the time you subscribe, the list will already be initialized, so you will not get a call for the initial data, only updates.

### Example
```cs
using Mirage;

public class Player : NetworkBehaviour {
    readonly SyncList<Item> inventory = new SyncList<Item>();
    readonly SyncList<Item> hotbar = new SyncList<Item>();

    // this will hook the callback on both server and client
    void Awake()
    {
        inventory.OnChange += UpdateInventory;
        NetIdentity.OnStartClient.AddListener(OnStartClient);
    }

    // hotbar changes will only be invoked on clients
    void OnStartClient() {
        hotbar.OnChange += UpdateHotbar;
    }

    void UpdateInventory()
    {
        // here you can refresh your UI for instance
    }

    void UpdateHotbar()
    {
        // here you can refresh your UI for instance
    }
}
```

By default, `SyncList` uses a List to store its data. If you want to use a different list implementation, add a constructor and pass the list implementation to the parent constructor. For example:

```cs
public SyncList<Item> myList = new SyncList<Item>(new MyIList<Item>());
```