---
sidebar_position: 2
---
# Sync Var
[`SyncVars`](/docs/reference/Mirage/SyncVarAttribute) are properties of classes that inherit from [NetworkBehaviour](/docs/reference/Mirage/NetworkBehaviour), which are synchronized from the server to clients. When a game object is spawned, or a new player joins a game in progress, they are sent the latest state of all SyncVars on networked objects that are visible to them. Use the [[SyncVar]](/docs/reference/Mirage/SyncVarAttribute) custom attribute to specify which variables in your script you want to synchronize.

:::note
The state of SyncVars is applied to game objects on clients before [Identity.OnStartClient](/docs/reference/Mirage/NetworkIdentity#onstartclient) event is invoked, so the state of the object is always up-to-date in subscribed callbacks.
:::


SyncVars can use any [type supported by Mirage](/docs/guides/serialization/data-types). You can have up to 64 SyncVars on a single NetworkBehaviour script, including [SyncLists](/docs/guides/sync/sync-objects/sync-list) and other sync types.

The server automatically sends SyncVar updates when the value of a SyncVar changes, so you do not need to track when they change or send information about the changes yourself. Changing a value in the inspector will not trigger an update.

:::note
SyncVars are not sent right away or in the order they are set. They will be sent as a group in the next sync update.
:::

## Example
Let's have a simple `Player` class with the following code:

``` cs
using Mirage;
using UnityEngine;

public class Player : NetworkBehaviour
{
    [SyncVar]
    public int clickCount;

    private void Update()
    {
        if (IsLocalPlayer && Input.GetMouseButtonDown(0))
        {
            ServerRpc_IncreaseClicks();
        }
    }

    [ServerRpc]
    public void ServerRpc_IncreaseClicks()
    {
        // This is executed on the server
        clickCount++;
    }
}
```

In this example, when Player A clicks the left mouse button, he sends a [ServerRpc](/docs/guides/remote-actions/server-rpc) to the server where the `clickCount` SyncVar is incremented. All other visible players will be informed about Player A's new `clickCount` value.

## Class inheritance
SyncVars work with class inheritance. Consider this example:

```cs
private class Pet : NetworkBehaviour
{
    [SyncVar] 
    private string name;
}

private class Cat : Pet
{
    [SyncVar]
    private Color32 color;
}
```

You can attach the Cat component to your cat prefab, and it will synchronize both its `name` and `color`.

:::caution
Both `Cat` and `Pet` should be in the same assembly. If they are in separate assemblies, make sure not to change `name` from inside `Cat` directly, add a method to `Pet` instead. 
:::

## SyncVar hook
The `hook` option of SyncVar attribute can be used to specify a function to be called when the SyncVar changes value on the client and server.

For more information on SyncVar hooks see [Sync Var Hooks](/docs/guides/sync/sync-var-hooks)

### Example Client Only
Below is a simple example of assigning a random color to each player when they're spawned on the server.  All clients will see all players in the correct colors, even if they join later.

```cs
using UnityEngine;
using Mirage;

public class Player : NetworkBehaviour
{
    [SyncVar(hook = nameof(UpdateColor))]
    private Color playerColor = Color.black;

    private Renderer renderer;

    // Unity makes a clone of the Material every time renderer.material is used.
    // Cache it here and Destroy it in OnDestroy to prevent a memory leak.
    private Material cachedMaterial;

    private void Awake()
    {
        renderer = GetComponent<Renderer>();
        Identity.OnStartServer.AddListener(OnStartServer);
    }

    private void OnStartServer()
    {
        playerColor = Random.ColorHSV(0f, 1f, 1f, 1f, 0.5f, 1f);
    }

    private void UpdateColor(Color oldColor, Color newColor)
    {
        // this is executed on this player for each client
        if (cachedMaterial == null)
        {
            cachedMaterial = renderer.material;
        }

        cachedMaterial.color = newColor;
    }

    private void OnDestroy()
    {
        Destroy(cachedMaterial);
    }
}
```

### Example Client & Server
Below is a simple example of assigning a random color to each player when they're spawned on the server. All clients will see all players in the correct colors, even if they join later, the server will also fire the event.

```cs
using UnityEngine;
using Mirage;

public class Player : NetworkBehaviour
{
    [SyncVar(hook = nameof(UpdateColor), invokeHookOnServer = true)]
    private Color playerColor = Color.black;

    private Renderer renderer;

    // Unity makes a clone of the Material every time renderer.material is used.
    // Cache it here and Destroy it in OnDestroy to prevent a memory leak.
    private Material cachedMaterial;

    private void Awake()
    {
        renderer = GetComponent<Renderer>();
        Identity.OnStartServer.AddListener(OnStartServer);
    }

    private void OnStartServer()
    {
        playerColor = Random.ColorHSV(0f, 1f, 1f, 1f, 0.5f, 1f);
    }

    private void UpdateColor(Color oldColor, Color newColor)
    {
        // this is executed on this player for each client
        if (cachedMaterial == null)
        {
            cachedMaterial = renderer.material;
        }

        cachedMaterial.color = newColor;
    }

    private void OnDestroy()
    {
        Destroy(cachedMaterial);
    }
}
```

## SyncVar Initialize Only

Just like regular SyncVars, when a game object is spawned, or a new player joins a game in progress, they are sent the latest state of all SyncVars on networked objects that are visible to them. 
With the `initialOnly` flag set to true you will now be able to control the state of the SyncVar manually rather than waiting for Mirage to update them. 

:::note
Make sure you manually update your observable clients with the new state.  
Syncvar Hooks become redundant, as you are setting the state of the Syncvar directly.
:::

### Example

``` cs
using Mirage;
using UnityEngine;

public class Player : NetworkBehaviour
{
    [SyncVar(initialOnly = true)]
    private int weaponId;

    private void Awake()
    {
        Identity.OnStartClient.AddListener(OnStartClient);
    }

    private void OnStartClient()
    {
        // Update weapon using id from syncvar (sent to client via spawn message
        UpdateWeapon(weaponId);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            // Client Request weapon change
            ServerRpc_SetSyncVarWeaponId(7);
        }
    }

    [ServerRpc]
    private void ServerRpc_SetSyncVarWeaponId(int weaponId)
    {
        // Set weapon id on server so new players get it
        this.weaponId = weaponId;

        // Tell current players about it
        ClientRpc_SetSyncVarWeaponId(weaponId);

        // Update weapon on server
        UpdateWeapon(weaponId);
    }

    [ClientRpc]
    private void ClientRpc_SetSyncVarWeaponId(int weaponId)
    {
        // Set id on client
        this.weaponId = weaponId;

        // Update weapon on client
        UpdateWeapon(weaponId);
    }

    public void UpdateWeapon(int weaponId)
    {
        // Do stuff to update weapon here
        // For example, its spawning model
    }
}
```
