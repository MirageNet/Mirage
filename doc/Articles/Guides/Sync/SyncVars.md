# SyncVars
SyncVars are properties of classes that inherit from <xref:Mirage.NetworkBehaviour>, which are synchronized from the server to clients. When a game object is spawned, or a new player joins a game in progress, they are sent the latest state of all SyncVars on networked objects that are visible to them. Use the [[SyncVar]](xref:Mirage.SyncVarAttribute) custom attribute to specify which variables in your script you want to synchronize.

> [!NOTE]
> The state of SyncVars is applied to game objects on clients before [NetIdentity.OnStartClient](xref:Mirage.NetworkIdentity.OnStartClient) event is invoked, so the state of the object is always up-to-date in subscribed callbacks.


SyncVars can use any [type supported by Mirage](../DataTypes.md). You can have up to 64 SyncVars on a single NetworkBehaviour script, including [SyncLists](SyncLists.md) and other sync types.

The server automatically sends SyncVar updates when the value of a SyncVar changes, so you do not need to track when they change or send information about the changes yourself. Changing a value in the inspector will not trigger an update.

> [!NOTE]
> SyncVars are not sent right away or in the order they are set. They will be sent as a group in the next sync update.

## Example
Let's have a simple `Player` class with the following code:

``` cs
using Mirage;
using UnityEngine;

public class Player : NetworkBehaviour
{
    [SyncVar]
    public int clickCount;

    void Update()
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

In this example, when Player A clicks the left mouse button, he sends a [ServerRpc](../RemoteCalls/ServerRpc.md) to the server where the `clickCount` SyncVar is incremented. All other visible players will be informed about Player A's new `clickCount` value.

## Class inheritance
SyncVars work with class inheritance. Consider this example:

```cs
class Pet : NetworkBehaviour
{
    [SyncVar] 
    string name;
}

class Cat : Pet
{
    [SyncVar]
    public Color32 color;
}
```

You can attach the Cat component to your cat prefab, and it will synchronize both it's `name` and `color`.

> [!WARNING]
> Both `Cat` and `Pet` should be in the same assembly. If they are in separate assemblies, make sure not to change `name` from inside `Cat` directly, add a method to `Pet` instead. 

## SyncVar hook
The `hook` property of SyncVar can be used to specify a function to be called when the SyncVar changes value on the client and server.

**Trivia:**
- The hook callback must have two parameters of the same type as the SyncVar property. One for the old value, one for the new value.
- The hook is always called after the SyncVar value is set. You don't need to set it yourself.
- The hook only fires for changed values, and changing a value in the inspector will not trigger an update.
- Hooks can be virtual methods and overriden in a derived class.

### Example Client Only
Below is a simple example of assigning a random color to each player when they're spawned on the server.  All clients will see all players in the correct colors, even if they join later.

```cs
using UnityEngine;
using Mirage;

public class Player : NetworkBehaviour
{
    [SyncVar(hook = nameof(UpdateColor))]
    Color playerColor = Color.black;

    Renderer renderer;

    // Unity makes a clone of the Material every time renderer.material is used.
    // Cache it here and Destroy it in OnDestroy to prevent a memory leak.
    Material cachedMaterial;

    void Awake()
    {
        renderer = GetComponent<Renderer>();
        NetIdentity.OnStartServer.AddListener(OnStartServer);
    }

    void OnStartServer()
    {
        playerColor = Random.ColorHSV(0f, 1f, 1f, 1f, 0.5f, 1f);
    }

    void UpdateColor(Color oldColor, Color newColor)
    {
        // this is executed on this player for each client
        if (cachedMaterial == null)
        {
            cachedMaterial = renderer.material;
        }

        cachedMaterial.color = newColor;
    }

    void OnDestroy()
    {
        Destroy(cachedMaterial);
    }
}
```

### Example Client & Server
Below is a simple example of assigning a random color to each player when they're spawned on the server. All clients will see all players in the correct colors, even if they join later.
Server will also fire the event.

```cs
using UnityEngine;
using Mirage;

public class Player : NetworkBehaviour
{
    [SyncVar(hook = nameof(UpdateColor), fireOnServer = true)]
    Color playerColor = Color.black;

    Renderer renderer;

    // Unity makes a clone of the Material every time renderer.material is used.
    // Cache it here and Destroy it in OnDestroy to prevent a memory leak.
    Material cachedMaterial;

    void Awake()
    {
        renderer = GetComponent<Renderer>();
        NetIdentity.OnStartServer.AddListener(OnStartServer);
    }

    void OnStartServer()
    {
        playerColor = Random.ColorHSV(0f, 1f, 1f, 1f, 0.5f, 1f);
    }

    void UpdateColor(Color oldColor, Color newColor)
    {
        // this is executed on this player for each client
        if (cachedMaterial == null)
        {
            cachedMaterial = renderer.material;
        }

        cachedMaterial.color = newColor;
    }

    void OnDestroy()
    {
        Destroy(cachedMaterial);
    }
}
```



## SyncVars Initialize Only

Just like regular Syncvars, when an game object is spawned, or a new player joins a game in progress, they are sent the latest state of all SyncVars on networked objects that are visible to them. 
With the InitialOnly flag set to true you will now be able to control the state of the syncvar manually rather than waiting for Mirage to update them. 

> [!NOTE]
> Make sure you manually update your observable clients with the new state.
> Syncvar Hooks become redundant, as you are setting the state of the Syncvar directly.


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
        // update weapon using id from syncvar (sent to client via spawn message
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
        // set weapon id on server so new players get it
        this.weaponId = weaponId;

        // tell current players about it
        ClientRpc_SetSyncVarWeaponId(weaponId);

        // update weapon on server
        UpdateWeapon(weaponId);
    }

    [ClientRpc]
    private void ClientRpc_SetSyncVarWeaponId(int weaponId)
    {
        // set id on client
        this.weaponId = weaponId;

        // update weapon on client
        UpdateWeapon(weaponId);
    }

    public void UpdateWeapon(int weaponId)
    {
        // do stuff to update weapon here
        // for example, its spawning model
    }
}
```
