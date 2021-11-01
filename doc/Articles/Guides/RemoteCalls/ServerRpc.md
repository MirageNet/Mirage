
## Server RPC Calls

Server RPC Calls are sent from character objects on the client to character objects on the server. For security, Server RPC Calls can only be sent from YOUR character object by default, so you cannot control the objects of other players.  You can bypass the authority check using `[ServerRpc(requireAuthority = false)]`.

To make a function into a Server RPC Calls, add the [ServerRpc] custom attribute to it. This function will now be run on the server when it is called on the client. Any parameters of [allowed data type](../DataTypes.md) will be automatically passed to the server with the Server RPC Call.

Server RPC Calls functions cannot be static. 

``` cs
public class Player : NetworkBehaviour
{
    void Update()
    {
        if (!isLocalPlayer) return;

        if (Input.GetKey(KeyCode.X))
            DropCube();
    }

    // assigned in inspector
    public GameObject cubePrefab;

    [ServerRpc]
    void DropCube()
    {
        if (cubePrefab != null)
        {
            Vector3 spawnPos = transform.position + transform.forward * 2;
            Quaternion spawnRot = transform.rotation;
            GameObject cube = Instantiate(cubePrefab, spawnPos, spawnRot);
            NetworkServer.Spawn(cube);
        }
    }
}
```

Be careful of sending ServerRpcs from the client every frame! This can cause a lot of network traffic.

### Returning values

ServerRpcs can return values.  It can take a long time for the server to reply, so they must return a UniTask which the client can await.
To return a value,  add a return value using `UniTask<MyReturnType>` where `MyReturnType` is any [supported Mirage type](../DataTypes.md).  In the server you can make your method async,  or you can use `UniTask.FromResult(myresult);`.  For example:

```cs
public class Shop: NetworkBehavior {

    [ServerRpc]
    public UniTask<int> GetPrice(string item) 
    {
        switch (item) 
        {
             case "turnip":
                 return UniTask.FromResult(10);
             case "apple":
                return UniTask.FromResult(3);
             default:
                return UniTask.FromResult(int.MaxValue);
        }
    }

    [Client]
    public async UniTaskVoid DisplayTurnipPrice() 
    {
        // call the RPC and wait for the response without blocking the main thread
        int price = await GetPrice("turnip");
        Debug.Log($"Turnips price {price}");
    }
}
```

### ServerRpc and Authority

It is possible to invoke ServerRpcs on non-character objects if any of the following are true:

- The object was spawned with client authority
- The object has client authority set with `NetworkIdentity.AssignClientAuthority`
- the Server RPC Call has the `requireAuthority` option set false.  
    - You can include an optional `INetworkPlayer sender = null` parameter in the Server RPC Call method signature and Mirage will fill in the sending client for you.
    - Do not try to set a value for this optional parameter...it will be ignored.

Server RPC Calls sent from these object are run on the server instance of the object, not on the associated character object for the client.

```cs
public enum DoorState : byte
{
    Open, Closed
}

public class Door : NetworkBehaviour
{
    [SyncVar]
    public DoorState doorState;

    [ServerRpc(requireAuthority = false)]
    public void CmdSetDoorState(DoorState newDoorState, INetworkPlayer sender = null)
    {
        if (sender.identity.GetComponent<Player>().hasDoorKey)
            doorState = newDoorState;
    }
}
```
